using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Linq;

public class FieldDisplay : MonoBehaviour
{
    public int index = -1;
    public ulong clientOwner;
    public int owner = -1;
    private FieldManager gameManager;
    private turnArray turnArray;
    private TeamPicker teamPicker;
    private ShpereSpawn shpereSpawn;
    public bool canBuy = false;
    public int money = -20;
    public bool firstBuy = false;
    public GameObject playerPrefab;
    private bool spawnOnce = true;
    public ulong sphereID;

    public int upgrade = -1; //



    public GameObject purchaseDialogPrefab;
    public bool purchaseDecisionMade = false;
    public bool purchaseResult = false;

    public int secondUp;
    public int thirdUp;
    public int fourthUp;


    private void Start()
    {
        gameManager = FindObjectOfType<FieldManager>();
        turnArray = FindObjectOfType<turnArray>();
        teamPicker = FindObjectOfType<TeamPicker>();
    }

    private void Update()
    {
        try { owner = gameManager.fieldOwners[index]; } catch(Exception ex) { print(ex.ToString()); }
    }

    private void OnMouseOver()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        if (Input.GetMouseButtonDown(0) && NetworkManager.Singleton.LocalClientId == turnArray.currentPlayerId.Value)
        {

            if (canBuy == false) return;

            if (gameManager.fieldOwners[index] == -1)
            {
                if (firstBuy == true) return;
                //Buy(money);

                if (AskToBuyField(money) == true)
                {
                    PlayersData playerForClient = FindObjectsOfType<PlayersData>().FirstOrDefault(player => player.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId);
                    if (!playerForClient.IsLocalPlayer || !playerForClient.IsOwner || playerForClient.Score.Value < -money) return;
                    else
                    {
                        Debug.Log(playerForClient.name + " " + playerForClient.Score.Value);
                        Buy(money);
                    }
                }
                //else
                //{
                //    if(purchaseResult == false && purchaseDecisionMade == true)
                //    turnArray.StartAuctionServerRpc(index);
                //}

                //firstBuy = true;
            }
            else
            {
                if (turnArray.currentPlayerId.Value != (ulong)gameManager.fieldOwners[index]) return;

                PlayersData playerForClient = FindObjectsOfType<PlayersData>().FirstOrDefault(player => player.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId);

                switch (gameManager.fieldUpgrades[index])
                {
                    case 1: money = secondUp; break;
                    case 2: money = thirdUp; break;
                    case 3: money = fourthUp; break;
                }

                if (!playerForClient.IsLocalPlayer || !playerForClient.IsOwner || playerForClient.Score.Value < Mathf.Abs(money)) return;
                else
                {
                    if (firstBuy == true)
                    {
                        if (playerForClient.Score.Value >= Mathf.Abs(money * 2)) Buy(money * 2);
                        firstBuy = false;
                        // Buy(money * 2); firstBuy = false;
                    }
                    else {
                        Buy(money);
                    }
                    canBuy = false;
                }
                firstBuy = false;
                //if (firstBuy == true)
                //{
                //    Buy(money * 2); firstBuy = false;
                //}
                //else Buy(money);
                //canBuy = false;
            }
        }
    }
    private void OnMouseExit()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public void Buy(int cost)
    {
        if (gameManager.fieldUpgrades[index] >= 4) return; //

        if (NetworkManager.Singleton.IsHost)
        {           
            gameManager.BuyFieldServerRpc(index, NetworkManager.Singleton.LocalClientId);
            teamPicker.Change(cost);
            if (owner == -1)
            {
                owner = gameManager.fieldOwners[index];
            }

            upgrade = gameManager.fieldUpgrades[index]; //
        }
        else
        {
            gameManager.BuyFieldServerRpc(index, NetworkManager.Singleton.LocalClientId);
            teamPicker.Change(cost);
            if (owner == -1)
            {
                owner = gameManager.fieldOwners[index];
            }

            upgrade = gameManager.fieldUpgrades[index]; //
        }
    }

    public void UpdateOwner(ulong newOwner)
    {
        clientOwner = newOwner;
    }

    //
    public void UpdateUpgrade(int upgradeF)
    {
        upgrade = upgradeF;
    }
    //




    private bool AskToBuyField(int cost)
    {
        Transform spawnPanel = GameObject.Find("spawnPanel").transform;
        GameObject dialogInstance = Instantiate(purchaseDialogPrefab, spawnPanel);
        dialogInstance.transform.SetParent(spawnPanel);
        PurchaseDialog dialog = dialogInstance.GetComponent<PurchaseDialog>();

        dialog.Setup($"Do you want to buy this field for ${cost}?", OnPurchaseConfirmed, OnPurchaseDeclined);

        StartCoroutine(WaitForPurchaseDecision());

        return purchaseResult;
    }

    private IEnumerator WaitForPurchaseDecision()
    {
        while (!purchaseDecisionMade)
        {
            yield return null;
        }
        Destroy(GameObject.FindWithTag("PurchaseDialog"));
    }

    private void OnPurchaseConfirmed()
    {
        purchaseResult = true;
        purchaseDecisionMade = true;
        PlayersData playerForClient = FindObjectsOfType<PlayersData>().FirstOrDefault(player => player.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId);
        if (!playerForClient.IsLocalPlayer || !playerForClient.IsOwner || playerForClient.Score.Value < Mathf.Abs(money)) return;
        else
        {
            Debug.Log(playerForClient.name + " " + playerForClient.Score.Value);
            Buy(money);
        }
        //Buy(money);
        firstBuy = true;
    }

    private void OnPurchaseDeclined()
    {
        purchaseResult = false;
        purchaseDecisionMade = true;
        turnArray.StartAuctionServerRpc(index);
    }
}

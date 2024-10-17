using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Linq;
using System;

public class turnArray : NetworkBehaviour
{
    public NetworkList<int> numberList;
    public NetworkVariable<ulong> currentPlayerId;
    public Text numberListText;
    public Button generateNumberButton;

    public Text timerText;
    public NetworkVariable<float> timer;
    private Coroutine timerCoroutine;

    //
    public GameObject auctionUIPrefab;
    private GameObject auctionUIInstance;
    public NetworkVariable<ulong> leadingPlayerId = new NetworkVariable<ulong>();
    public NetworkVariable<int> highestBid = new NetworkVariable<int>(0);
    private Coroutine auctionCoroutine;
    //


    private void Awake()
    {
        numberList = new NetworkList<int>();
        currentPlayerId = new NetworkVariable<ulong>();

        timer = new NetworkVariable<float>(50f);
    }

    private void Start()
    {
        numberList.OnListChanged += OnNumberListChanged;
        currentPlayerId.OnValueChanged += OnCurrentPlayerIdChanged;

        timer.OnValueChanged += OnTimerChanged;

        if (generateNumberButton != null)
        {
            generateNumberButton.onClick.AddListener(OnGenerateNumberButtonClicked);
        }

        //if (IsHost) StartTimer();

        UpdateUI();
    }


    private void OnTimerChanged(float previousValue, float newValue)
    {
        UpdateTimer();
    }

    public void StartTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        timerCoroutine = StartCoroutine(TimerCoroutine());
    }


    public void UpdateTimer()
    {
        timerText.text = "Time Remaining: " + Mathf.Ceil(timer.Value).ToString() + "s";
    }

    private IEnumerator TimerCoroutine()
    {
        while (timer.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            timer.Value -= 1f;
        }
        if (IsHost)
        {
            //OnGenerateNumberButtonClicked();
            UpdateTurnOrderServerRpc();
            int generatedNumber = Convert.ToInt32(currentPlayerId.Value.ToString()); // Generate a random number between 1 and 100
            GenerateNumberServerRpc(generatedNumber);
            timer.Value = 50f;
            StartTimer();
        }
    }


    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            currentPlayerId.Value = NetworkManager.Singleton.LocalClientId;
        }
    }

    public void OnGenerateNumberButtonClicked()
    {
        if (NetworkManager.Singleton.LocalClientId == currentPlayerId.Value)
        {
            int generatedNumber = Convert.ToInt32(currentPlayerId.Value.ToString()); // Generate a random number between 1 and 100
            GenerateNumberServerRpc(generatedNumber);
            UpdateTurnOrderServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void GenerateNumberServerRpc(int number, ServerRpcParams rpcParams = default)
    {
        numberList.Add(number);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateTurnOrderServerRpc(ServerRpcParams rpcParams = default)
    {
        var clientIds = NetworkManager.Singleton.ConnectedClientsIds.ToList();
        int currentIndex = clientIds.IndexOf(currentPlayerId.Value);
        int nextIndex = (currentIndex + 1) % clientIds.Count;
        currentPlayerId.Value = clientIds[nextIndex];

        UpdateCanBuyClientRpc(currentPlayerId.Value);
    }

    [ClientRpc]
    private void UpdateCanBuyClientRpc(ulong currentPlayerId)
    {
        FieldDisplay[] fieldDisplay = FindObjectsOfType<FieldDisplay>();
        foreach (var item in fieldDisplay)
        {
            if (item.owner != -1)
            {
                ulong ownerId = ulong.Parse(item.owner.ToString());
                item.canBuy = ownerId == currentPlayerId;
                item.firstBuy = false;
            }
        }
    }

    private void OnNumberListChanged(NetworkListEvent<int> changeEvent)
    {
        UpdateUI();
    }

    private void OnCurrentPlayerIdChanged(ulong oldId, ulong newId)
    {
        UpdateUI();

        if (IsHost) timer.Value = 50f;
    }

    public void UpdateUI()
    {
        numberListText.text = "Numbers: ";
        for (int i = 0; i < numberList.Count; i++)
        {
            numberListText.text += numberList[i].ToString() + " ";
        }
        generateNumberButton.interactable = NetworkManager.Singleton.LocalClientId == currentPlayerId.Value;
    }







    [ServerRpc(RequireOwnership = false)]
    public void StartAuctionServerRpc(int fieldIndex)
    {
        if (auctionCoroutine != null)
        {
            StopCoroutine(auctionCoroutine);
        }

        leadingPlayerId.Value = 0;
        highestBid.Value = 0;

        SpawnAuctionUIClientRpc();

        auctionCoroutine = StartCoroutine(AuctionCountdownCoroutine(fieldIndex));
    }

    [ClientRpc]
    private void SpawnAuctionUIClientRpc()
    {
        if (auctionUIInstance == null)
        {
            Transform spawnPanel = GameObject.Find("spawnPanel").transform;
            auctionUIInstance = Instantiate(auctionUIPrefab, spawnPanel);
            auctionUIInstance.transform.SetParent(spawnPanel);
            var auctionUI = auctionUIInstance.GetComponent<AuctionManager>();
            if (auctionUI != null)
            {
                auctionUI.Initialize(this);
            }
        }
    }

    private IEnumerator AuctionCountdownCoroutine(int fieldIndex)
    {
        float auctionDuration = 15f;

        while (auctionDuration > 0f)
        {
            auctionDuration -= Time.deltaTime;
            yield return null;
        }

        if (leadingPlayerId.Value >= 0 && highestBid.Value != 0)
        {
            AssignFieldToWinner(fieldIndex, leadingPlayerId.Value);
        }

        DestroyAuctionUIClientRpc();
    }

    [ClientRpc]
    private void DestroyAuctionUIClientRpc()
    {
        if (auctionUIInstance != null)
        {
            Destroy(auctionUIInstance);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RaiseBidServerRpc(int newBid, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;


        //PlayerData[] playerForClient = FindObjectsOfType<PlayerData>();
        //foreach (var player in playerForClient)
        //{
        //    NetworkObject cur = player.GetComponent<NetworkObject>();
        //    if (cur.IsLocalPlayer && cur.IsOwner && cur.OwnerClientId == clientId)
        //    {

        //    }
        //}
        if (newBid > highestBid.Value)
        {
            highestBid.Value = newBid;
            leadingPlayerId.Value = clientId;

            UpdateAuctionUIClientRpc(clientId, newBid);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PassServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (clientId == currentPlayerId.Value)
        {
            UpdateAuctionUIClientRpc(clientId, 0);

            DisableBidUIClientRpc(clientId);
        }
    }

    private void AssignFieldToWinner(int fieldIndex, ulong winnerId)
    {
        AssignFieldToWinnerClientRpc(fieldIndex, winnerId);
    }

    [ClientRpc]
    private void AssignFieldToWinnerClientRpc(int fieldIndex, ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
            FieldDisplay[] fieldDisplays = FindObjectsOfType<FieldDisplay>();

            foreach (var field in fieldDisplays)
            {
                if (field.index == fieldIndex)
                {
                    field.Buy(-highestBid.Value);
                    field.firstBuy = true;
                    break;
                }
            }
        }
    }

    [ClientRpc]
    private void UpdateAuctionUIClientRpc(ulong leadingPlayerId, int highestBid)
    {
        if (auctionUIInstance != null)
        {
            var auctionUI = auctionUIInstance.GetComponent<AuctionManager>();
            if (auctionUI != null)
            {
                auctionUI.UpdateLeadingPlayer(leadingPlayerId, highestBid);
            }
        }
    }

    [ClientRpc]
    private void DisableBidUIClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId && auctionUIInstance != null)
        {
            var auctionUI = auctionUIInstance.GetComponent<AuctionManager>();
            if (auctionUI != null)
            {
                auctionUI.DisableUI();
            }
        }
    }
}

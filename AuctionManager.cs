using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Linq;
using System;

public class AuctionManager : NetworkBehaviour
{
    public Text leadingPlayerText;
    public Text highestBidText;
    public InputField bidInputField;
    public Button raiseBidButton;
    public Button passButton;

    private turnArray turnArrayScript;

    public void Initialize(turnArray turnArray)
    {
        turnArrayScript = turnArray;

        raiseBidButton.onClick.AddListener(OnRaiseBidButtonClicked);
        passButton.onClick.AddListener(OnPassButtonClicked);

        UpdateLeadingPlayer(turnArray.leadingPlayerId.Value, turnArray.highestBid.Value);
    }

    public void UpdateLeadingPlayer(ulong leadingPlayerId, int highestBid)
    {
        leadingPlayerText.text = "Leading Player: " + leadingPlayerId.ToString();
        highestBidText.text = "Highest Bid: " + highestBid.ToString();
    }

    private void OnRaiseBidButtonClicked()
    {
        PlayersData playerForClient = FindObjectsOfType<PlayersData>().FirstOrDefault(player => player.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId);
        Debug.Log(playerForClient.name + " " + playerForClient.Score.Value + " " + turnArrayScript.highestBid.Value);

        if (Convert.ToInt32(bidInputField.text) > playerForClient.Score.Value) return;

        if (int.TryParse(bidInputField.text, out int newBid))
        {
            if (newBid > turnArrayScript.highestBid.Value && playerForClient.IsLocalPlayer && playerForClient.IsOwner && playerForClient.Score.Value > turnArrayScript.highestBid.Value)
            {
                turnArrayScript.RaiseBidServerRpc(newBid);

                bidInputField.text = string.Empty;
            }
            else
            {
                Debug.Log("Bid must be higher than the current highest bid.");
            }
        }
        else
        {
            Debug.Log("Invalid bid. Please enter a number.");
        }
    }

    private void OnPassButtonClicked()
    {
        turnArrayScript.PassServerRpc();

        DisableUI();
    }

    public void DisableUI()
    {
        bidInputField.interactable = false;
        raiseBidButton.interactable = false;
        passButton.interactable = false;
    }
}

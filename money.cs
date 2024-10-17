using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class money : NetworkBehaviour
{
    public Text playerNameText;
    public Text playerMoneyText;
    public Button updateMoneyButton;

    private NetworkVariable<ulong> playerId = new NetworkVariable<ulong>();
    private NetworkVariable<int> playerMoney = new NetworkVariable<int>();

    private void Start()
    {
        transform.SetParent(GameObject.Find("ButtonHolder").transform);
        playerMoney.OnValueChanged += OnMoneyChanged;
        playerId.OnValueChanged += OnPlayerIdChanged;

        if (IsOwner)
        {
            updateMoneyButton.onClick.AddListener(OnUpdateMoneyButtonClicked);
        }
        else
        {
            updateMoneyButton.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        playerMoney.OnValueChanged -= OnMoneyChanged;
        playerId.OnValueChanged -= OnPlayerIdChanged;
    }

    public void SetPlayerInfo(ulong clientId, int money)
    {
        if (IsOwner)
        {
            playerId.Value = clientId;
            playerMoney.Value = money;
        }
    }

    private void OnMoneyChanged(int oldValue, int newValue)
    {
        playerMoneyText.text = newValue.ToString();
    }

    private void OnPlayerIdChanged(ulong oldValue, ulong newValue)
    {
        playerNameText.text = "PlayerName" + playerId + ": " + newValue;
    }

    public void OnUpdateMoneyButtonClicked()
    {
        if (IsOwner)
        {
            UpdatePlayerMoneyServerRpc(playerMoney.Value + 10);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerMoneyServerRpc(int newAmount)
    {
       playerMoney.Value = newAmount;
    }
}

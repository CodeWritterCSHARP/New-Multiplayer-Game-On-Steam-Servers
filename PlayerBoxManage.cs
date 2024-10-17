using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBoxManage : NetworkBehaviour
{
    public GameObject playerBoxPrefab;
    public Transform playerBoxContainer;

    private Dictionary<ulong, GameObject> playerBoxes = new Dictionary<ulong, GameObject>();
    private Dictionary<ulong, NetworkVariable<PlayerMoneyData>> playerDataDict = new Dictionary<ulong, NetworkVariable<PlayerMoneyData>>();

    private void Start()
    {
        if (playerBoxContainer == null) playerBoxContainer = this.transform;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedCallback;
        }

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            AddPlayer(client.ClientId);
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null && IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedCallback;
        }
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        AddPlayer(clientId);
    }

    private void OnClientDisconnectedCallback(ulong clientId)
    {
        RemovePlayer(clientId);
    }

    private void AddPlayer(ulong clientId)
    {
        if (!IsHost) return;

        var playerData = new PlayerMoneyData { clientId = clientId, money = 120 };
        var networkPlayerData = new NetworkVariable<PlayerMoneyData>(playerData, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        playerDataDict[clientId] = networkPlayerData;

        SpawnPlayerBoxClientRpc(clientId, playerData);

        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            GameObject playerBox = playerBoxes[clientId];
            playerBox.GetComponentInChildren<Button>().onClick.AddListener(OnAddMoneyClicked);
        }
    }

    private void RemovePlayer(ulong clientId)
    {
        if (playerBoxes.TryGetValue(clientId, out var playerBox))
        {
            Destroy(playerBox);
            playerBoxes.Remove(clientId);
        }

        if (playerDataDict.ContainsKey(clientId))
        {
            playerDataDict.Remove(clientId);
        }
    }

    [ClientRpc]
    private void SpawnPlayerBoxClientRpc(ulong clientId, PlayerMoneyData playerData)
    {
        GameObject playerBox = Instantiate(playerBoxPrefab, playerBoxContainer);
        playerBoxes[clientId] = playerBox;
        UpdatePlayerBoxUI(clientId, playerBox, playerData);
    }

    private void UpdatePlayerBoxUI(ulong clientId, GameObject playerBox, PlayerMoneyData playerData)
    {
        Text playerInfoText = playerBox.GetComponentInChildren<Text>();
        playerInfoText.text = $"Player ID: {clientId}\nMoney: {playerData.money}";
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddMoneyServerRpc(ulong clientId, int amount)
    {
        if (playerDataDict.TryGetValue(clientId, out var playerData))
        {
            var updatedData = playerData.Value;
            updatedData.money += amount;
            playerData.Value = updatedData;

            UpdatePlayerBoxClientRpc(clientId, updatedData);
        }
        else
        {
            Debug.LogError($"Player data not found for client ID {clientId}");
        }
    }

    [ClientRpc]
    private void UpdatePlayerBoxClientRpc(ulong clientId, PlayerMoneyData updatedData)
    {
        if (playerBoxes.TryGetValue(clientId, out var playerBox))
        {
            UpdatePlayerBoxUI(clientId, playerBox, updatedData);
        }
    }

    private void OnAddMoneyClicked()
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        AddMoneyServerRpc(clientId, 10);
    }
}

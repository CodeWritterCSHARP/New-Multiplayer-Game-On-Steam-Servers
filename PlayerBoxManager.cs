using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBoxManager : NetworkBehaviour
{
    public GameObject playerBoxPrefab;
    public GridLayoutGroup gridLayoutGroup;
    private bool playerBoxesSpawned = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsHost && !playerBoxesSpawned)
        {
            InstantiatePlayerBoxesForAllPlayers();
        }
    }

    private void InstantiatePlayerBoxesForAllPlayers()
    {
        playerBoxesSpawned = true;
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            GameObject playerBox = Instantiate(playerBoxPrefab, gridLayoutGroup.transform);
            playerBox.transform.SetParent(gridLayoutGroup.transform);
            playerBox.GetComponent<NetworkObject>().SpawnWithOwnership(client.ClientId);
            money playerBoxUI = playerBox.GetComponent<money>();
            playerBoxUI.SetPlayerInfo(client.ClientId, 120);
        }
    }
}

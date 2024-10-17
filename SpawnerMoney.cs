using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class SpawnerMoney : NetworkBehaviour
{
    public GameObject playerPrefab;
    public GridLayoutGroup grid;

    private void Update()
    {
        if (!IsHost) return;
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.R))
        {
            SpawnPlayerServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnPlayerServerRpc(ServerRpcParams rpcParams = default)
    {
        SpawnPlayerClientRpc();
    }

    [ClientRpc]
    void SpawnPlayerClientRpc()
    {
        Debug.Log("Spawn");
        if (grid == null) grid = GameObject.Find("ButtonHolder").GetComponent<GridLayoutGroup>();
        GameObject player = Instantiate(playerPrefab, grid.transform);
        player.name = NetworkManager.Singleton.LocalClientId.ToString();
        player.GetComponent<Text>().text = NetworkManager.Singleton.LocalClientId.ToString() + ":100";
        var playerNetworkObject = player.GetComponent<NetworkObject>();
        playerNetworkObject.Spawn();
        playerNetworkObject.transform.SetParent(grid.transform);
    }
}

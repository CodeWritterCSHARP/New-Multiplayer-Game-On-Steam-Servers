using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class SpawnDice : NetworkBehaviour
{
    [SerializeField] private float xMin, xMax, zMin, zMax;
    public GameObject playerPrefab;
    [SerializeField] private List<GameObject> spawnedPrefabs = new List<GameObject>();
    [SerializeField] private turnArray turnArray;
    [SerializeField] private GameObject btn;

    private void Awake()
    {
        if (turnArray == null) turnArray = FindObjectOfType<turnArray>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && NetworkManager.Singleton.LocalClientId == turnArray.currentPlayerId.Value && !btn.activeSelf)
        {
            SpawnPlayerServerRpc(); SpawnPlayerServerRpc();
        }
    }

    public void sp() => SpawnPlayerServerRpc();

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ServerRpcParams rpcParams = default)
    {
        SpawnPlayerOnClients();
    }

    void SpawnPlayerOnClients()
    {
        Vector3 pos = new Vector3(Random.Range(xMin, xMax), -1, Random.Range(zMin, zMax));

        GameObject player = Instantiate(playerPrefab, pos, Quaternion.identity);
        spawnedPrefabs.Add(player);
        player.GetComponent<DiceBehaviour>().parent = this;
        var playerNetworkObject = player.GetComponent<NetworkObject>();
        playerNetworkObject.Spawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyServerRpc()
    {
        GameObject toDestroy = spawnedPrefabs[0];
        toDestroy.GetComponent<NetworkObject>().Despawn();
        spawnedPrefabs.Remove(toDestroy);
        Destroy(toDestroy);
        CheckList();
    }

    private void CheckList()
    {
        for (int i = 0; i < spawnedPrefabs.Count; i++)
        {
            if(spawnedPrefabs[i] == null) spawnedPrefabs.Remove(spawnedPrefabs[i]);
        }
    }
}

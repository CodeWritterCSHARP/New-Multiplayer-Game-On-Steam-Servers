using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CubesThrowing : NetworkBehaviour
{
    [SerializeField] private Transform plane;
    [SerializeField] private GameObject cube;
    [SerializeField] private float xMin, xMax, zMin, zMax;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
       // if (!IsOwner) { this.enabled = false; GetComponent<movement>().enabled = false; }
    }

    private void Awake()
    {
        try { plane = GameObject.FindGameObjectWithTag("Plane").transform; } catch { Debug.Log("Not found plane"); }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsOwner) { SpawnServerRpc(); SpawnServerRpc(); }
    }

    [ServerRpc]
    private void SpawnServerRpc()
    {
        GameObject cur = Instantiate(cube, new Vector3(Random.Range(xMin, xMax), -1, Random.Range(zMin, zMax)), Quaternion.identity);
        cur.GetComponent<NetworkObject>().Spawn();
    }
}

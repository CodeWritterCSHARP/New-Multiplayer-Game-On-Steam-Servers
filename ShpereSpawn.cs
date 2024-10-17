using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class ShpereSpawn : NetworkBehaviour
{
    public string name;
    public ulong owner;

    public void Spawn() => SpawnPlayerServerRpc();

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ServerRpcParams rpcParams = default)
    {
        SpawnSphereClientRpc();
    }

    [ClientRpc]
    private void SpawnSphereClientRpc()
    {
        GameObject sphere = GameObject.Find(name);

        Color color = Color.white;
        switch (owner)
        {
            case 0: color = Color.green; break;
            case 1: color = Color.yellow; break;
            case 2: color = Color.blue; break;
            case 3: color = Color.red; break;
            default: break;
        }

        var renderer = sphere.GetComponent<MeshRenderer>();

        if (renderer != null)
        {
            renderer.material.color = color;
        }
        else
        {
            Debug.LogError("MeshRenderer not found on the spawned sphere object.");
        }

        //StartCoroutine(DelayedColorChange(sphereNetworkObject.NetworkObjectId, color));
    }

    public void ChangeColor()
    {

    }

    private IEnumerator DelayedColorChange(ulong networkObjectId, Color color)
    {
        yield return new WaitForSeconds(0.1f);
        ChangeColorClientRpc(networkObjectId, color);
    }

    [ClientRpc]
    private void ChangeColorClientRpc(ulong networkObjectId, Color color)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
        {
            GameObject sphere = networkObject.gameObject;
            var renderer = sphere.GetComponent<MeshRenderer>();

            if (renderer != null)
            {
                renderer.material.color = color;
            }
            else
            {
                Debug.LogError("MeshRenderer not found on the spawned sphere object.");
            }
        }
        else
        {
            Debug.LogError("NetworkObject not found on the client.");
        }
    }
}

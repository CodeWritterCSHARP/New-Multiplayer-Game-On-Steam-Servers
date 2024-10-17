using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSpawner : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterDatabase characterDatabase;

    private bool isStarted = false;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) { Debug.Log("ret"); return; }

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += Load;
    }

    private void Load(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
      //  if (isStarted) return;
        if (IsHost && sceneName == "Gameplay")
        {
            foreach (ulong id in clientsCompleted)
            {
                for (int i = 0; i < DataTransfer.Instance.playersPrefabs.Count; i++)
                {
                    if (id == DataTransfer.Instance.playersPrefabs.ElementAt(i).Key)
                    {
                        GameObject character = DataTransfer.Instance.GetByID(id);
                        var spawnPos = new Vector3(-10, - 9.5f, -10);
                        var characterInstance = Instantiate(character, spawnPos, Quaternion.identity);
                        characterInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
                    }
                }

                //if (id == DataTransfer.Instance.clientId[])
                //{
                //    GameObject character = DataTransfer.Instance.GetByID();
                //    var spawnPos = new Vector3(UnityEngine.Random.Range(-5f, 5f), 0f, UnityEngine.Random.Range(-3f, 3f));
                //    var characterInstance = Instantiate(character, spawnPos, Quaternion.identity);
                //    characterInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
                //}

                //if (character != null)
                //{
                //    Debug.Log("spawn");
                //    var spawnPos = new Vector3(UnityEngine.Random.Range(-5f, 5f), 0f, UnityEngine.Random.Range(-3f, 3f));
                //    var characterInstance = Instantiate(character.GameplayPrefab, spawnPos, Quaternion.identity);
                //    characterInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
                //}
                //else Debug.Log("null");
                //foreach (var client in ServerManager.Instance.ClientData)
                //{
                //    Debug.Log("spawn");
                //    var character = characterDatabase.GetCharacterById(client.Value.characterId);
                //    if (character != null)
                //    {
                //        var spawnPos = new Vector3(UnityEngine.Random.Range(-5f, 5f), 0f, UnityEngine.Random.Range(-3f, 3f));
                //        var characterInstance = Instantiate(character.GameplayPrefab, spawnPos, Quaternion.identity);
                //        characterInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
                //    }
                //    else Debug.Log("null");
                //}
            }
          //  isStarted = true;
        }
    }
}

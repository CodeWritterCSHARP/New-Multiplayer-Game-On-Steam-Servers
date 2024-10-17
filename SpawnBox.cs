using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class SpawnBox : NetworkBehaviour
{
    public GameObject uiPrefab;
    private Dictionary<ulong, GameObject> playerUIElements = new Dictionary<ulong, GameObject>();

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDestroy()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        SpawnPlayerUI(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (playerUIElements.TryGetValue(clientId, out GameObject uiElement))
        {
            Destroy(uiElement);
            playerUIElements.Remove(clientId);
        }
    }

    private void SpawnPlayerUI(ulong clientId)
    {
        GameObject uiInstance = Instantiate(uiPrefab);
        Text uiText = uiInstance.GetComponentInChildren<Text>();
        uiInstance.name = clientId.ToString();
        if (uiText != null)
        {
            uiText.text = "Client ID: " + clientId;
        }

        uiInstance.transform.SetParent(GameObject.Find("ButtonHolder").transform, false);

        playerUIElements[clientId] = uiInstance;
    }
}

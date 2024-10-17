using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataTransfer : MonoBehaviour
{
    public static DataTransfer Instance { get; private set; }

    public GameObject[] characters = new GameObject[4];

    public Dictionary<ulong, int> playersPrefabs = new Dictionary<ulong, int>();

    public List<ulong> clientId = new List<ulong>();
    public List<int> characterId = new List<int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject GetByID(ulong clientId)
    {
        if(playersPrefabs.ContainsKey(clientId)) 
        return characters[playersPrefabs[clientId]-1];
        return null;
    }
}

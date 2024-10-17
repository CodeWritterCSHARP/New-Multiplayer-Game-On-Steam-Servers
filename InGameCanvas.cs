using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameCanvas : MonoBehaviour
{
    [SerializeField] private Transform scoreBoard;
    [SerializeField] private GameObject palyerScoreTemp;

    private void OnEnable()
    {
        NetworkPlayers.OnPlayerSpawn += OnPlayerSpawned;
    }

    private void OnDisable()
    {
        NetworkPlayers.OnPlayerSpawn -= OnPlayerSpawned;
    }

    private void OnPlayerSpawned(GameObject player)
    {
        Debug.Log("Spawn");
        GameObject playerUi = Instantiate(palyerScoreTemp, scoreBoard);
        playerUi.GetComponent<PlayerScore>().TrackPlayer(player);
    }
}

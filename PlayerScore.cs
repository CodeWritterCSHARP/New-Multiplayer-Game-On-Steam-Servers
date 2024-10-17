using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using System;

public class PlayerScore : MonoBehaviour
{
    [SerializeField] private Text NameUI;
    [SerializeField] private Text ScoreUI;

    public void TrackPlayer(GameObject player)
    {
        player.GetComponent<PlayersData>().Name.OnValueChanged += OnNameChanged;
        player.GetComponent<PlayersData>().Score.OnValueChanged += OnScoreChanged;
        OnScoreChanged(100, player.GetComponent<PlayersData>().Score.Value);
        OnNameChanged("", player.GetComponent<PlayersData>().Name.Value);
    }

    private void OnScoreChanged(int previousValue, int newValue)
    {
        ScoreUI.text = newValue.ToString();
    }

    private void OnNameChanged(FixedString128Bytes previousValue, FixedString128Bytes newValue)
    {
        NameUI.text = newValue.ToString();
    }
}

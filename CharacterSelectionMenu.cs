using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class CharacterSelectionMenu : NetworkBehaviour
{
    public GameObject[] characterPrefabs; 
    public Button selectButton; 

    private List<PlayerController> players = new List<PlayerController>(); 

    private int selectedCharacterIndex = 0; 

    public static CharacterSelectionMenu Instance;

    void Start()
    {
        selectButton.onClick.AddListener(SelectCharacter);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void SelectCharacter()
    {
        selectedCharacterIndex = (selectedCharacterIndex + 1) % characterPrefabs.Length;

        foreach (var player in players)
        {
            player.SelectedCharacter.Value = selectedCharacterIndex;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        foreach (var player in GameObject.FindObjectsOfType<PlayerController>())
        {
            players.Add(player);
        }
    }
}

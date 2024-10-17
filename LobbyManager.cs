using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private InputField lobbyIDInput;
    [SerializeField] private Text lobbyID;

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject inLobbyMenu;

 //   public Dictionary<ulong, ClientData> clientData { get; private set; }
    public static LobbyManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(Instance);
        else Instance = this;
    }

    private void OnEnable()
    {
        SteamMatchmaking.OnLobbyCreated += LobbyCreated;
        SteamMatchmaking.OnLobbyEntered += LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested += GameLobbyJoinRequested;
    }


    private void OnDisable()
    {
        SteamMatchmaking.OnLobbyCreated -= LobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested -= GameLobbyJoinRequested;
    }

    private async void GameLobbyJoinRequested(Lobby lobby, SteamId steamId)
    {
        await lobby.Join(); 
    }

    private void LobbyEntered(Lobby lobby)
    {
        LobbySaver.instance.currentlobby = lobby;
        Debug.Log("Entered");
        lobbyID.text = lobby.Id.ToString();
        CheckUI();

        if (NetworkManager.Singleton.IsHost) return;
        NetworkManager.Singleton.gameObject.GetComponent<FacepunchTransport>().targetSteamId = lobby.Owner.Id;
        NetworkManager.Singleton.StartClient();
    }

    private void LobbyCreated(Result result, Lobby lobby)
    {
        if(result == Result.OK)
        {
            lobby.SetPublic();
            lobby.SetJoinable(true);
            NetworkManager.Singleton.StartHost();
        }
    }

    public async void HostLobby()
    {
        await SteamMatchmaking.CreateLobbyAsync(4);
        Debug.Log("Created");
    }

    public async void JoinLobbyWithID()
    {
        ulong ID;
        if (!ulong.TryParse(lobbyIDInput.text, out ID)) return;

        Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();
        foreach (Lobby lobby in lobbies)
        {
            if(lobby.Id == ID)
            {
                await lobby.Join();
                return;
            }
        }
    }

    public void CopyID()
    {
        TextEditor textEditor = new TextEditor();
        textEditor.text = lobbyID.text;
        textEditor.SelectAll();
        textEditor.Copy();
    }

    public void LeaveLobby()
    {
        LobbySaver.instance.currentlobby?.Leave();
        LobbySaver.instance.currentlobby = null;
        NetworkManager.Singleton.Shutdown();
        CheckUI();
    }

    private void CheckUI()
    {
        if(LobbySaver.instance.currentlobby == null)
        {
            mainMenu.SetActive(true);
            inLobbyMenu.SetActive(false);
        }
        else
        {
            mainMenu.SetActive(false);
            inLobbyMenu.SetActive(true);
        }
    }

    public void StartGameServer()
    {
        if(NetworkManager.Singleton.IsHost) NetworkManager.Singleton.SceneManager.LoadScene("Gameplay1", LoadSceneMode.Single);
    }

    private void OnClientDisc()
    {
    }

    public void SetChar(ulong clientId, int charId)
    {
        //if(clientData.TryGetValue(clientId, out ClientData data))
        //{
        //    data.characterId = charId;
        //}
    }
}

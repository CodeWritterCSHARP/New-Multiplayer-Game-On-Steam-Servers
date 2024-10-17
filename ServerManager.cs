using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class ServerManager : NetworkBehaviour
{
    [SerializeField] private InputField lobbyIDInput;
    [SerializeField] private Text lobbyID;
    [Header("Settings")]
    [SerializeField] private string characterSelectSceneName = "CharacterSelect";
    [SerializeField] private string gameplaySceneName = "Gameplay";
    public GameObject startBtn;
    [SerializeField] private GameObject inLobbyMenu;
    [SerializeField] private GameObject gameName;
    [SerializeField] private GameObject hostName;
    [SerializeField] private GameObject inputJoin;

    [SerializeField] private GameObject targetObject;
    [SerializeField] private GameObject rawImage;
    [SerializeField] private GameObject canvas;

    public static ServerManager Instance { get; private set; }

    private bool gameHasStarted;
    public Dictionary<ulong, ClientData> ClientData { get; private set; }

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

    public void LeaveLobby()
    {
        LobbySaver.instance.currentlobby?.Leave();
        LobbySaver.instance.currentlobby = null;
        NetworkManager.Singleton.Shutdown();
        CheckUI();
        inLobbyMenu.SetActive(false);
        startBtn.SetActive(false);
        gameName.SetActive(true);
        hostName.SetActive(true);
        inputJoin.SetActive(true);
        FindObjectOfType<MainMenuDisplay>().Back();
        NetworkManager.Singleton.OnServerStarted -= OnNetworkReady;
        NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        //string currentSceneName = SceneManager.GetActiveScene().name;
        //SceneManager.LoadScene(currentSceneName);
    }

    private void CheckUI()
    {
        if (LobbySaver.instance.currentlobby == null)
        {
            inLobbyMenu.SetActive(false);
        }
        else
        {
            inLobbyMenu.SetActive(true);
        }
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

    public async void JoinLobbyWithID()
    {
        ulong ID;
        if (!ulong.TryParse(lobbyIDInput.text, out ID)) return;

        Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();
        foreach (Lobby lobby in lobbies)
        {
            if (lobby.Id == ID)
            {
                await lobby.Join();
                return;
            }
        }
        startBtn.SetActive(false);
        gameName.SetActive(false);
        hostName.SetActive(false);
        inputJoin.SetActive(false);
        inLobbyMenu.SetActive(true);
    }

    public void CopyID()
    {
        TextEditor textEditor = new TextEditor();
        textEditor.text = lobbyID.text;
        textEditor.SelectAll();
        textEditor.Copy();
    }

    private void Start()
    {
        targetObject.GetComponent<VideoPlayer>().frame = 0;
    }

    private async void GameLobbyJoinRequested(Lobby lobby, SteamId steamId)
    {
        await lobby.Join();
        startBtn.SetActive(false);
        gameName.SetActive(false);
        hostName.SetActive(false);
        inputJoin.SetActive(false);
        inLobbyMenu.SetActive(true);
    }

    private void LobbyEntered(Lobby lobby)
    {
        LobbySaver.instance.currentlobby = lobby;
        Debug.Log("Entered");
        lobbyID.text = lobby.Id.ToString();

        if (NetworkManager.Singleton.IsHost) return;
        NetworkManager.Singleton.gameObject.GetComponent<FacepunchTransport>().targetSteamId = lobby.Owner.Id;
        NetworkManager.Singleton.StartClient();
        CheckUI();
        hostName.SetActive(false);
        gameName.SetActive(false);
        inputJoin.SetActive(false);
        inLobbyMenu.SetActive(true);
    }

    private void LobbyCreated(Result result, Lobby lobby)
    {
        if (result == Result.OK)
        {
            lobby.SetPublic();
            lobby.SetJoinable(true);
            NetworkManager.Singleton.StartHost();
        }
    }

    public async void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        await SteamMatchmaking.CreateLobbyAsync(4);
        Debug.Log("Created");
        startBtn.SetActive(true);
        gameName.SetActive(false);
        hostName.SetActive(false);
        inputJoin.SetActive(false);
        ClientData = new Dictionary<ulong, ClientData>();
        inLobbyMenu.SetActive(true);
        NetworkManager.Singleton.StartHost();
    }

    public void StartServer()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnServerStarted += OnNetworkReady;

        ClientData = new Dictionary<ulong, ClientData>();

        NetworkManager.Singleton.StartServer();
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (ClientData.Count >= 4 || gameHasStarted)
        {
            response.Approved = false;
            return;
        }

        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Pending = false;

        ClientData[request.ClientNetworkId] = new ClientData(request.ClientNetworkId);

        Debug.Log($"Added client {request.ClientNetworkId}");
    }

    public void OnNetworkReady()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;

        if (NetworkManager.Singleton.IsHost) { TriggerSetActive(true); Invoke("LoadNewScene", 2.4f); }
        else if(NetworkManager.Singleton.IsClient) SetActiveClientRpc(true);
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (ClientData.ContainsKey(clientId))
        {
            if (ClientData.Remove(clientId))
            {
                Debug.Log($"Removed client {clientId}");
            }
        }
    }

    public void SetCharacter(ulong clientId, int characterId)
    {
        if (ClientData.TryGetValue(clientId, out ClientData data))
        {
            data.characterId = characterId;
        }
    }

    public void StartGame()
    {

        gameHasStarted = true;
        NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }

    public void LoadNewScene()
    {
        if (NetworkManager.Singleton.IsHost) NetworkManager.Singleton.SceneManager.LoadScene(characterSelectSceneName, LoadSceneMode.Single);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetActiveServerRpc(bool isActive)
    {
        Debug.Log("Started Host");
        targetObject.GetComponent<VideoPlayer>().frame = 0;
        targetObject.SetActive(isActive); rawImage.SetActive(isActive); canvas.SetActive(false);
        SetActiveClientRpc(isActive);
    }

    [ClientRpc]
    private void SetActiveClientRpc(bool isActive)
    {
        Debug.Log("Started Client");
        targetObject.GetComponent<VideoPlayer>().frame = 0;
        targetObject.SetActive(isActive); rawImage.SetActive(isActive); canvas.SetActive(false);
    }
    public void TriggerSetActive(bool isActive) => SetActiveServerRpc(isActive);
}

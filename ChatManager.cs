using System;
using System.IO;
using System.Linq;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    [SerializeField] private InputField messageInput;
    [SerializeField] private Text messageTemplate;
    [SerializeField] private int lineCount = 0;

    private void Start() => messageTemplate.text = "";

    private void OnEnable()
    {
        SteamMatchmaking.OnChatMessage += ChatSent;
        SteamMatchmaking.OnLobbyEntered += LobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += LobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += LobbyMemberLeave;
    }

    private void OnDisable()
    {
        SteamMatchmaking.OnChatMessage -= ChatSent;
        SteamMatchmaking.OnLobbyEntered -= LobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= LobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= LobbyMemberLeave;
        messageTemplate.text = "";
    }

    private void LobbyMemberLeave(Lobby lobby, Friend friend) => AddMessage(friend.Name + " leaved the lobby");

    private void LobbyMemberJoined(Lobby lobby, Friend friend) => AddMessage(friend.Name + " joined the lobby");

    private void LobbyEntered(Lobby lobby) => AddMessage("you entered the lobby");

    private void ChatSent(Lobby lobby, Friend friend, string message)
    {
        AddMessage(friend.Name + ": " + message);
    }

    private static string DeleteLines(string s, int linesToRemove)
    {
        return s.Split(Environment.NewLine.ToCharArray(), linesToRemove + 1).Skip(linesToRemove).FirstOrDefault();
    }

    private void AddMessage(string message)
    {
        messageTemplate.text += message + Environment.NewLine;
        lineCount++;
        if (lineCount >= 10)
        {
            string myString = messageTemplate.text;
            myString = DeleteLines(myString, 2);
            lineCount = 0;
            using (StringReader reader = new StringReader(myString))
            {
                string line;
                while ((line = reader.ReadLine()) != null) lineCount++;
            }
            messageTemplate.text = myString;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) ToggleChatBox();
    }

    private void ToggleChatBox()
    {
        if (messageInput.gameObject.activeSelf)
        {
            if (!String.IsNullOrEmpty(messageInput.text))
            {
                LobbySaver.instance.currentlobby?.SendChatString(messageInput.text);
                messageInput.text = "";
            }
            messageInput.gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
        }
        else
        {
            messageInput.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(messageInput.gameObject);
        }
    }
}

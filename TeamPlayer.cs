using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TeamPlayer : NetworkBehaviour
{
    public Text teamIndexText;
    private NetworkVariable<byte> teamIndex = new NetworkVariable<byte>(100);

    private void Start()
    {
       // teamIndexText = GameObject.Find("Team").GetComponent<Text>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetTeamServerRpc(byte newTeamIndex)
    {
        teamIndex.Value += newTeamIndex;
    }

    private void OnEnable()
    {
        teamIndex.OnValueChanged += OnTeamChange;
    }

    private void OnDisable()
    {
        teamIndex.OnValueChanged -= OnTeamChange;
    }

    private void OnTeamChange(byte oldTeamIndex, byte newTeamIndex)
    {
        if (!IsClient) return;
        this.name = newTeamIndex.ToString();
    }
}

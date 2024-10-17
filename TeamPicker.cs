using Unity.Netcode;
using UnityEngine;

public class TeamPicker : MonoBehaviour
{
    public void Change(int value)
    {
        NetworkObject[] players = FindObjectsOfType<NetworkObject>();
        foreach (var player in players)
        {
            if (player.IsLocalPlayer && player.IsOwner)
            {
                player.GetComponent<PlayersData>().UpdateScoreServerRpc(value);
            }
        }
    }
}

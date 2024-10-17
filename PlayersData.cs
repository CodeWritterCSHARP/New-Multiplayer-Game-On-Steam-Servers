using Unity.Netcode;
using Unity.Collections;
using Steamworks;

public class PlayersData : NetworkBehaviour
{
    public NetworkVariable<int> Score = new NetworkVariable<int>();
    public NetworkVariable<FixedString128Bytes> Name = new NetworkVariable<FixedString128Bytes>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsHost) return;
        Score.Value = 100;
        GetNameClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { GetComponent<NetworkObject>().OwnerClientId }
            }
        }
        );
    }

    [ClientRpc]
    void GetNameClientRpc(ClientRpcParams clientRpcParams = default)
    {
        GetNameServerRpc(SteamClient.Name);
    }

    [ServerRpc(RequireOwnership = false)]
    void GetNameServerRpc(string Name)
    {
        this.Name.Value = Name;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateScoreServerRpc(int amount)
    {
        Score.Value += amount;
    }
}

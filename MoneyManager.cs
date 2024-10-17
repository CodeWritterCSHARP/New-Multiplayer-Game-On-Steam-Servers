using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MoneyManager : NetworkBehaviour
{
    private struct PlayerMoney : INetworkSerializable
    {
        public ulong clientId;
        public int money;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref money);
        }
    }

    private static readonly List<PlayerMoney> playerMoneyList = new List<PlayerMoney>();

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseMoneyServerRpc(ulong clientId)
    {
        PlayerMoney player = playerMoneyList.Find(x => x.clientId == clientId);
        player.money += 10;
        UpdateMoneyClientRpc(player);
    }

    [ClientRpc]
    private void UpdateMoneyClientRpc(PlayerMoney player)
    {
        MoneyUIManager moneyUIManager = FindObjectOfType<MoneyUIManager>();

        if (moneyUIManager != null)
        {
            moneyUIManager.UpdateMoney(player.clientId, player.money);
        }
        else
        {
            Debug.LogError("MoneyUIManager not founded.");
        }
    }

    public void OnButtonPress()
    {
        if (IsLocalPlayer)
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            IncreaseMoneyServerRpc(clientId);
        }
    }
}

using Unity.Netcode;
using UnityEngine;

public struct PlayerMoneyData : INetworkSerializable
{
    public ulong clientId;
    public int money;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref money);
    }
}

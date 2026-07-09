using System;
using Unity.Collections;
using Unity.Netcode;

[Serializable]
public struct LobbyPlayerState : INetworkSerializable, IEquatable<LobbyPlayerState>
{
    public ulong ClientId;
    public bool HasSubmittedName;
    public FixedString64Bytes DisplayName;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref HasSubmittedName);
        serializer.SerializeValue(ref DisplayName);
    }

    public bool Equals(LobbyPlayerState other)
    {
        return ClientId == other.ClientId
            && HasSubmittedName == other.HasSubmittedName
            && DisplayName.Equals(other.DisplayName);
    }

    public override string ToString()
    {
        return HasSubmittedName ? DisplayName.ToString() : "Connecting...";
    }
}

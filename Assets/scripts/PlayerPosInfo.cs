using Fusion;

public struct PlayerPosInfo : INetworkStruct
{
    public NetworkString<_16> playerId; // 16字节够了
    public double longitude;
    public double latitude;
    public Camp camp;
    public SurvivorType survivorType;
}

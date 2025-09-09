using Fusion;
using UnityEngine;

/// <summary>
/// 挂在玩家NetworkObject上，负责GPS同步与Marker更新
/// </summary>
public class NetworkedPlayerGPSController : NetworkBehaviour
{
    [Header("地图Marker")]
    public OnlineMapsMarker marker;

    [Header("Marker贴图")]
    public Texture2D survivorTexture;
    public Texture2D ghostTexture;
    public Texture2D bombTexture;

    [Networked]
    public Vector2 GPSPosition { get; set; }

    [Networked]
    public Camp Camp { get; set; }

    private void Start()
    {
        // 初始化Marker贴图
        if (marker != null)
        {
            marker.texture = GetTexture(Camp);
        }
    }

    private void Update()
    {
        // 本地玩家更新GPS
        if (Object.HasInputAuthority)
        {
            if (OnlineMapsLocationService.instance != null)
            {
                Vector2 pos = OnlineMapsLocationService.instance.position;
                GPSPosition = pos;
            }
        }
        // 所有玩家刷新Marker位置
        if (marker != null)
        {
            Vector2 pos = GPSPosition;
            marker.SetPosition(pos.x, pos.y);
        }
        OnlineMaps.instance.Redraw();
    }

    private Texture2D GetTexture(Camp camp)
    {
        switch (camp)
        {
            case Camp.Survivor:
                return survivorTexture;
            case Camp.Ghost:
                return ghostTexture;
            case Camp.BombExpert:
                return bombTexture;
        }
        return survivorTexture;
    }
}
using Fusion;
using UnityEngine;

/// <summary>
/// 显示其他玩家的Marker
/// </summary>
public class MarkerDisplay : NetworkBehaviour
{
    [Header("地图Marker")]
    public OnlineMapsMarker marker;

    [Header("Marker贴图")]
    public Texture2D[] survivorTextures; // 0-2
    public Texture2D ghostTexture;
    public Texture2D bombTexture;

    [Networked]
    public Vector2 GPSPosition { get; set; }

    [Networked]
    public Camp Camp { get; set; }

    [Networked]
    public SurvivorType SurvivorType { get; set; }

    void Update()
    {
        // 如果是自己，跳过
        if (Object.HasInputAuthority)
            return;

        if (marker == null)
            return;

        // 更新位置
        marker.SetPosition(GPSPosition.x, GPSPosition.y);

        // 更新贴图
        marker.texture = GetTexture(Camp, SurvivorType);

        OnlineMaps.instance.Redraw();
    }

    private Texture2D GetTexture(Camp camp, SurvivorType survivorType)
    {
        switch (camp)
        {
            case Camp.Survivor:
                return survivorTextures[(int)survivorType];
            case Camp.Ghost:
                return ghostTexture;
            case Camp.BombExpert:
                return bombTexture;
        }
        return survivorTextures[0];
    }
}
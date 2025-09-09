using Fusion;
using UnityEngine;

/// <summary>
/// 挂在玩家 Prefab 上的 NetworkBehaviour，负责同步角色、GPS 和玩家 ID
/// </summary>
public class PlayerNetwork : NetworkBehaviour
{
    // 角色：0=Survivor, 1=Ghost, 2=BombExpert
    [Networked] public RoleType Role { get; set; }
    // 玩家 ID 或昵称
    [Networked] public string PlayerID { get; set; }
    // GPS 坐标
    [Networked] public float Lat { get; set; }
    [Networked] public float Lon { get; set; }

    public override void Spawned()
    {
        // 本地玩家生成完毕，可在此初始化自己的 UI 或 Marker
        if (Object.HasInputAuthority)
        {
            // 例如：UIManager.Instance.ShowMyRole(Role);
        }
    }

    public override void Render()
    {
        // 每帧客户端渲染；在此更新其他玩家的地图 Marker
        if (!Object.HasInputAuthority)
        {
            // MapController.Instance.UpdateMarker(Object.Id, Lat, Lon, Role);
        }
    }
}

// 角色类型枚举
public enum RoleType : byte
{
    Survivor   = 0,
    Ghost      = 1,
    BombExpert = 2
}

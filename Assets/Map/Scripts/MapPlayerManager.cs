using UnityEngine;
using System.Collections.Generic;

public class MapPlayerManager : MonoBehaviour
{
    public Texture2D[] survivorTextures;
    public Texture2D ghostTexture;
    public Texture2D bombexpertTexture;

    // 所有玩家的 marker
    private Dictionary<string, MarkerMover> playerMarkers = new Dictionary<string, MarkerMover>();

    // 新玩家数据到来/玩家位置或身份发生变化时，调用这个函数
    public void OnPlayerInfoReceived(string playerId, double lng, double lat, Camp camp, SurvivorType type)
    {
        if (!playerMarkers.TryGetValue(playerId, out var mover))
        {
            // 新玩家，创建 marker
            GameObject markerObj = new GameObject($"PlayerMarker_{playerId}");
            mover = markerObj.AddComponent<MarkerMover>();
            mover.longitude = lng;
            mover.latitude = lat;
            
            // 贴图先随便用一个，等下立刻更新
            Texture2D initialTexture = GetTexture(camp, type);
            mover.CreateMarker(initialTexture);

            // 身份和icon管理
            var identity = markerObj.AddComponent<PlayerIdentity>();
            identity.Initialize(camp, type);

            var iconSwitcher = markerObj.AddComponent<MarkerIconSwitcher>();
            // 贴图赋值
            iconSwitcher.survivorTextures = survivorTextures;
            iconSwitcher.ghostTexture = ghostTexture;
            iconSwitcher.bombexpertTexture = bombexpertTexture;
            iconSwitcher.Initialize(mover.marker, identity);

            playerMarkers.Add(playerId, mover);
        }
        else
        {
            // 已有玩家，更新位置与身份
            mover.longitude = lng;
            mover.latitude = lat;
            var identity = mover.GetComponent<PlayerIdentity>();
            identity.ChangeCamp(camp, type);
        }
    }

    // 玩家下线时调用
    public void RemovePlayerMarker(string playerId)
    {
        if (playerMarkers.TryGetValue(playerId, out var mover))
        {
            if (mover.marker != null) OnlineMapsMarkerManager.RemoveItem(mover.marker);
            Destroy(mover.gameObject);
            playerMarkers.Remove(playerId);
        }
    }

    // 获取身份对应贴图
    private Texture2D GetTexture(Camp camp, SurvivorType type)
    {
        switch (camp)
        {
            case Camp.Survivor:
                return survivorTextures[(int)type];
            case Camp.Ghost:
                return ghostTexture;
            case Camp.BombExpert:
                return bombexpertTexture;
        }
        return null;
    }
}

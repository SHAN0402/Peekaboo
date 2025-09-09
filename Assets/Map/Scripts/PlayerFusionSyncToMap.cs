using UnityEngine;
using Fusion;

public class PlayerFusionSyncToMap : MonoBehaviour
{
    public MapPlayerManager mapPlayerManager; // Inspector拖入

    void Update()
    {
        // Debug：输出同步表条目数
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[PlayerFusionSyncToMap] GameManager.Instance == null，跳过同步！");
            return;
        }

        var allPlayerPos = GameManager.Instance.allPlayerPos;
        Debug.Log($"[PlayerFusionSyncToMap] 当前同步表玩家数: {allPlayerPos.Count}");

        int pushCount = 0;

        foreach (var kv in allPlayerPos)
        {
            var info = kv.Value;
            Debug.Log($"[PlayerFusionSyncToMap] 推送 Player: playerId={info.playerId} lat={info.latitude} lon={info.longitude} camp={info.camp}");

            mapPlayerManager.OnPlayerInfoReceived(
                info.playerId.ToString(),
                info.longitude,
                info.latitude,
                info.camp,
                info.survivorType
            );
            pushCount++;
        }

        Debug.Log($"[PlayerFusionSyncToMap] 本帧推送 marker 数量：{pushCount}");
    }
}
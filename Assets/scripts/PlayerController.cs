using UnityEngine;
using Fusion;
using System.Collections;
using PlayFab;

public enum Camp { Survivor, Ghost, BombExpert }
public enum InfectionStatus { None, Infecting, Infected }

// 需要有你的 PlayerPosInfo 结构体、以及 GameManager.Instance 和 allPlayerPos

public class PlayerController : NetworkBehaviour
{
    public string playerId;
    [Networked] public NetworkString<_32> PlayFabId { get; set; }
    [Networked] public string playerName { get; set; }
    [Networked] public Camp currentCamp { get; set; } = Camp.Survivor;
    [Networked] public InfectionStatus infectionStatus { get; set; } = InfectionStatus.None;
    [Networked] public bool canInfectOthers { get; set; } = false;
    [Networked] public bool isOriginalGhost { get; set; } = false;
    [Networked] public bool isEliminated { get; set; } = false;

    [Networked] public float gpsLatitude { get; set; }
    [Networked] public float gpsLongitude { get; set; }

    private bool hasCheckedCamp = false;

    public bool isInfectable =>
        (currentCamp == Camp.Survivor || currentCamp == Camp.BombExpert) &&
        infectionStatus == InfectionStatus.None && !isEliminated;

    public Vector2 GetGPSPosition() => new Vector2(gpsLatitude, gpsLongitude);

    public void UpdateGPS(float lat, float lon)
    {
        if (!HasStateAuthority) return;

        gpsLatitude = lat;
        gpsLongitude = lon;

        RpcUpdateGPS(lat, lon);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RpcUpdateGPS(float lat, float lon)
    {
        gpsLatitude = lat;
        gpsLongitude = lon;
        Debug.Log($"[RpcUpdateGPS] 接收到来自 {playerName} 的 GPS：Lat={lat}, Lon={lon}");
    }

    public override void Spawned()
    {
        // 确保每个人都有唯一 playerId
        if (string.IsNullOrEmpty(playerId))
        {
            playerId = Object.InputAuthority.ToString();
            Debug.Log("[PlayerController] 自动分配唯一 playerId: " + playerId);
        }

        if (Object.HasInputAuthority)
        {
            Debug.Log($"[PlayerController] {playerName} 加入游戏，准备初始化 GPS...");

            PlayFabId = PlayFabSettings.staticPlayer.PlayFabId;
            Debug.Log("[PlayerController] 设置本地玩家 PlayFabId: " + PlayFabId);

            if (!gameObject.TryGetComponent<GPSUploader>(out _))
                gameObject.AddComponent<GPSUploader>();

            StartCoroutine(InitGPS());

#if UNITY_EDITOR
            UpdateGPS(Random.Range(30f, 31f), Random.Range(120f, 121f));
#endif
        }
    }

    public void Update()
    {
        // 阵营检测（原逻辑）
        if (!hasCheckedCamp && currentCamp == Camp.BombExpert)
        {
            hasCheckedCamp = true;
        }

        // 同步位置数据到 GameManager（唯一playerId作为key）
        if (Object.HasStateAuthority && GameManager.Instance != null)
        {
            var info = new PlayerPosInfo
            {
                playerId = playerId,
                longitude = gpsLongitude,
                latitude = gpsLatitude,
                camp = currentCamp,
                survivorType = SurvivorType.Survivor1 // 可按实际填写类型
            };
            GameManager.Instance.allPlayerPos.Set((NetworkString<_16>)playerId, info);
        }
    }

    // 离开时移除自己的同步表项
    void OnDestroy()
    {
        if (GameManager.Instance == null)
            return;

        if (!Object.HasStateAuthority)
            return;

        if (string.IsNullOrEmpty(playerId))
            return;

        var netId = (NetworkString<_16>)playerId;
        if (GameManager.Instance.allPlayerPos.ContainsKey(netId))
        {
            GameManager.Instance.allPlayerPos.Remove(netId);
        }
    }

    private IEnumerator InitGPS()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("用户未启用 GPS");
            yield break;
        }

        Input.location.Start();
        int maxWait = 10;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("无法获取 GPS 位置");
            yield break;
        }

        float lat = Input.location.lastData.latitude;
        float lon = Input.location.lastData.longitude;
        UpdateGPS(lat, lon);

        Debug.Log($"[PlayerController] 成功上传 GPS：Lat = {lat}, Lon = {lon}");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcStartInfectionProcess(float requiredTime)
    {
        if (!isInfectable || isEliminated) return;

        infectionStatus = InfectionStatus.Infecting;
        Debug.Log($"{playerName} 正在被感染（{requiredTime} 秒）...");

        if (HasStateAuthority)
            StartCoroutine(DelayedInfect(requiredTime));
    }

    private IEnumerator DelayedInfect(float delay)
    {
        yield return new WaitForSeconds(delay);
        CompleteInfection();
    }

    private void CompleteInfection()
    {
        if (infectionStatus != InfectionStatus.Infecting || isEliminated) return;

        infectionStatus = InfectionStatus.Infected;
        currentCamp = Camp.Ghost;
        canInfectOthers = true;

        if (GetComponent<InfectionScanner>() == null)
        {
            InfectionScanner scanner = gameObject.AddComponent<InfectionScanner>();
            scanner.self = this;
        }

        OnInfected();
        GameManager.Instance.ReportInfection(this);
        SendInfectEvent();
    }

    private void OnInfected()
    {
        Debug.Log($"{playerName} 被感染成功！现在是 Wanderer（鬼）");
    }

    private void SendInfectEvent()
    {
        Debug.Log($"[SendInfectEvent] 玩家 {playerId} 感染完成，阵营：{currentCamp}");
    }
}

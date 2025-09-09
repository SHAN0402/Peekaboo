using UnityEngine;
using Fusion;

public class Bomb : NetworkBehaviour
{
    [Header("爆炸倒计时（秒）")]
    public float explosionDelay = 5f;

    private PlayerController carrierPlayer;
    private PlayerController bombPlacerOwner;

    private float timer = 0f;
    private bool isActive = false;
    private bool hasExploded = false;

    void Start()
    {
        Debug.Log($"[Bomb] Start() 调用 → InstanceID = {GetInstanceID()}");
    }

    public override void Spawned()
    {
        Debug.Log($"[Bomb] Spawned() 调用 → HasStateAuthority = {HasStateAuthority}");
    }

    void Update()
    {
        if (!Runner.IsServer) return;

        Debug.Log("[Bomb] Update() 正在运行");

        if (isActive && carrierPlayer != null)
        {
            timer += Time.deltaTime;
            if (timer >= explosionDelay)
            {
                Explode();
                return;
            }

            if (carrierPlayer.Object != null && carrierPlayer.Object.HasStateAuthority)
            {
                Vector2 gpsPos = carrierPlayer.GetGPSPosition();
                transform.position = new Vector3(gpsPos.x, gpsPos.y + 1.5f, 0);
            }
            else
            {
                Debug.LogWarning("[Bomb] 无法获取 carrierPlayer 的位置或权限");
            }
        }
        else
        {
            TryDetectNearbyPlayers();
        }
    }

    public void Place(Vector2 gpsPos, PlayerController placer)
    {
        transform.position = new Vector3(gpsPos.x, gpsPos.y, 0);
        bombPlacerOwner = placer;
        carrierPlayer = null;
        isActive = false;
        timer = 0f;

        Debug.Log($"[Bomb] 已放置 → 坐标: ({gpsPos.x:F5}, {gpsPos.y:F5})，放置者: {placer.playerName}");
    }

    private void TryDetectNearbyPlayers()
    {
        Debug.Log("[Bomb] 开始检测附近玩家");

        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        Debug.Log($"[Bomb] 当前场景内发现玩家数量: {allPlayers.Length}");

        foreach (var p in allPlayers)
        {
            if (p == null) continue;

            float dist = Vector2.Distance(transform.position, p.GetGPSPosition());

            if (bombPlacerOwner != null && p.Object.InputAuthority == bombPlacerOwner.Object.InputAuthority)
            {
                Debug.Log($"[Bomb] {p.playerName} 被跳过（放置者）");
                continue;
            }

            if (p.isEliminated)
            {
                Debug.Log($"[Bomb] {p.playerName} 被跳过（淘汰）");
                continue;
            }

            if (p.currentCamp == Camp.BombExpert)
            {
                Debug.Log($"[Bomb] {p.playerName} 被跳过（炸弹专家）");
                continue;
            }

            Debug.Log($"检查 {p.playerName} | 距离 = {dist:F4} | 状态 = OK");

            if (dist <= 0.000435f)
            {
                Debug.Log($" [Bomb] 成功吸附玩家 {p.playerName}，距离 = {dist:F2}");
                TryActivateBy(p);
                break;
            }
        }
    }

    public void TryActivateBy(PlayerController target)
    {
        if (isActive || target == null) return;

        carrierPlayer = target;
        isActive = true;
        timer = 0f;

        Debug.Log($"[Bomb] 启动计时器，已吸附到 {target.playerName}，爆炸将在 {explosionDelay} 秒后发生");
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Debug.Log($"[Bomb] 爆炸！携带者: {carrierPlayer?.playerName ?? "None"}，位置: ({transform.position.x:F5}, {transform.position.y:F5})");

        if (carrierPlayer != null && bombPlacerOwner != null)
        {
            Debug.Log("[Bomb] 汇报击杀 → 调用 GameManager.ReportBombKill");
            GameManager.Instance?.ReportBombKill(carrierPlayer, bombPlacerOwner);
        }
        else
        {
            Debug.LogWarning("[Bomb] 爆炸时无法识别携带者或放置者，跳过击杀汇报");
        }

        Runner.Despawn(Object);
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class BombPlacer : NetworkBehaviour
{
    public GameObject bombPrefab;             // 炸弹预制体
    public int maxBomb = 3;                   // 最多放置炸弹数
    public float cooldowntime = 3f;           // 放置冷却时间

    private float gameTime = 0f;
    private int i = 0;

    [SerializeField]
    private List<NetworkObject> activeBombs = new List<NetworkObject>();

    private PlayerController self;
    private bool isEnabled = false;
    private bool initialized = false;

    public override void Spawned()
    {
        self = GetComponent<PlayerController>();
        Debug.Log($"[BombPlacer] Spawned → InputAuthority = {Object.InputAuthority}, StateAuthority = {HasStateAuthority}");

        //  只有本地控制玩家才绑定 UI 按钮
        if (Object.HasInputAuthority)
        {
            GameObject bombBtn = GameObject.Find("BombThrow");
            if (bombBtn != null && bombBtn.TryGetComponent(out Button btn))
            {
                btn.onClick.RemoveAllListeners(); // 避免重复绑定
                btn.onClick.AddListener(() =>
                {
                    Debug.Log("[BombPlacer] 客户端点击了炸弹按钮！");
                    RequestPlaceBomb();
                });
                Debug.Log("[BombPlacer] 成功绑定炸弹按钮事件");
            }
            else
            {
                Debug.LogWarning("[BombPlacer] 找不到 BombThrow 或 Button 组件");
            }
        }
    }

    void Update()
    {
        if (!HasStateAuthority) return;

        // 第一次进入，确认阵营信息后初始化
        if (!initialized)
        {
            if (self == null) self = GetComponent<PlayerController>();

            if (self != null && self.currentCamp == Camp.BombExpert)
            {
                isEnabled = true;
                initialized = true;
                Debug.Log($"[BombPlacer] 初始化完成，{self.playerName} 是炸弹专家");
            }
            else
            {
                Debug.Log($"[BombPlacer] 等待阵营同步中... 当前阵营 = {self?.currentCamp}");
                return;
            }
        }

        if (!isEnabled) return;

        gameTime += Time.deltaTime;
        activeBombs.RemoveAll(bomb => bomb == null || !bomb.IsValid);

        //  可选：按 B 键也能放置
        if (Input.GetKeyDown(KeyCode.B))
        {
            i++;
            Debug.Log($"[BombPlacer] B 键按下第 {i} 次 → 请求放炸弹");
            RequestPlaceBomb();
        }
    }

    // 客户端调用这个方法来请求服务器放置炸弹
    private void RequestPlaceBomb()
    {
        if (HasStateAuthority)
        {
            Debug.Log("[BombPlacer] 本地就是服务器，直接执行放置");
            BombButtonClicked();
        }
        else
        {
            Debug.Log("[BombPlacer] 客户端发送 RPC 请求服务器放炸弹");
            RpcRequestPlaceBomb();
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RpcRequestPlaceBomb()
    {
        Debug.Log("[BombPlacer] 服务器收到客户端的放炸弹请求");
        BombButtonClicked();
    }

    public void BombButtonClicked()
    {
        if (!HasStateAuthority || !isEnabled) return;

        if (gameTime < cooldowntime)
        {
            Debug.Log($"[BombPlacer] 冷却中，还有 {(cooldowntime - gameTime):F1}s");
        }
        else if (activeBombs.Count >= maxBomb)
        {
            Debug.Log($"[BombPlacer] 达到最大炸弹数 {maxBomb}");
        }
        else
        {
            PlaceBomb();
        }
    }

    private void PlaceBomb()
    {
        Vector2 gpsPos = self.GetGPSPosition();
        Vector3 spawnPos = new Vector3(gpsPos.x, gpsPos.y, 0);

        NetworkObject bomb = Runner.Spawn(
            bombPrefab.GetComponent<NetworkObject>(),
            spawnPos,
            Quaternion.identity);

        if (bomb != null)
        {
            Bomb bombScript = bomb.GetComponent<Bomb>();
            bombScript.Place(gpsPos, self);
            activeBombs.Add(bomb);

            Debug.Log($"[BombPlacer] 炸弹已放置：玩家 = {self.playerName}，坐标 = ({gpsPos.x:F5}, {gpsPos.y:F5})，当前炸弹数 = {activeBombs.Count}");
            gameTime = 0f;
        }
        else
        {
            Debug.LogError("[BombPlacer] Runner.Spawn 失败，炸弹未生成");
        }
    }
}

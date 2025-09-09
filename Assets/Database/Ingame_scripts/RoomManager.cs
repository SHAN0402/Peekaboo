using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Fusion;
using Fusion.Sockets;
using Fusion.Photon.Realtime;          // AuthenticationValues / CustomAuth
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;

public class RoomManager : MonoBehaviour, INetworkRunnerCallbacks
{
    private int playerCount = 0;
    Dictionary<PlayerRef, string> idMap = new();


    public string RoomCode { get; private set; }  // 或 public string RoomCode;

    public static RoomManager Instance { get; private set; }
    [SerializeField] NetworkPrefabRef playerPrefab;
    [SerializeField] int mapSceneIndex = 20;
    //[SerializeField] int lobbySceneIndex = 10; // Lobby 场景索引
    [SerializeField] private NetworkPrefabRef gameManagerPrefab; // 拖入 GameManager prefab
    private TMP_InputField joinInputField;
    private NetworkRunner runner;
    public NetworkRunner Runner => runner;

    // [SerializeField] int mapSceneIndex = 10;
    // [SerializeField] private GameObject lobbyRoot;

    void Awake()
    {
        // 保证唯一
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void BindUI(TMP_InputField codeInput, Button joinBtn, Button hostBtn, GameObject loadingPanel)
    {
        joinInputField = codeInput;

        if (joinBtn != null)
        {
            joinBtn.onClick.RemoveAllListeners();
            joinBtn.onClick.AddListener(() =>
            {
                if (codeInput != null)
                {
                    string code = codeInput.text.Trim().ToUpper();
                    if (!string.IsNullOrEmpty(code))
                    {
                        loadingPanel.SetActive(true);
                        Join();  // 会自动读取上面赋值的 joinInputField
                    }

                }

            });

        }


        if (hostBtn != null)
        {
            hostBtn.onClick.RemoveAllListeners();
            hostBtn.onClick.AddListener(HostAndJoinWithRandomCode);
        }
        ;

    }

    /* ---------- 对外入口 ---------- */

    public void HostAndJoinWithRandomCode()
    {
        // 不要await，直接fire and forget
        _ = HostAndJoinWithRandomCodeAsync();
    }

    public async Task HostAndJoinWithRandomCodeAsync()
    {
        string code = GenerateRoomCode(6);
        //UI
        // RoomUICache.RoomCode = code;
        Debug.Log($"[RoomManager] Host 创建房间，房号为 {code}");
        RoomCode = code; // 更新房间号缓存




        await JoinWithCode(code); // 复用已有逻辑
    }

    private string GenerateRoomCode(int len = 6)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Text.StringBuilder sb = new();
        var rng = new System.Random();
        for (int i = 0; i < len; i++) sb.Append(chars[rng.Next(chars.Length)]);
        return sb.ToString();
    }

    public void Join()
    {
        string code = joinInputField.text.Trim().ToUpper();
        _ = JoinWithCode(code);
    }

    [SerializeField] private GameObject runnerPrefab;
    private GameObject instantiatedRunner;

    public async Task JoinWithCode(string roomCode)
    {
        if (string.IsNullOrWhiteSpace(roomCode)) return;
        if (string.IsNullOrEmpty(PlayFabSettings.staticPlayer?.PlayFabId))
        {
            Debug.LogError("PlayFab 未登录成功");
            return;
        }

        Debug.Log($"JoinWithCode called, code = {roomCode}, playfabId = {PlayFabSettings.staticPlayer?.PlayFabId}");


        // 防止重复点击
        if (runner != null && runner.IsRunning)
        {
            Debug.LogWarning("Runner 正在运行，忽略重复 Join");
            //await CleanRunner();  
            return;
        }

        // 1) 获得 (或创建) 唯一 Runner
        //runner = runnerPrefab.GetComponent<NetworkRunner>();// ?? gameObject.AddComponent<NetworkRunner>();
        instantiatedRunner = Instantiate(runnerPrefab);
        runner = instantiatedRunner.GetComponent<NetworkRunner>();

        runner.AddCallbacks(this);
        // 如不用输入可省略 ProvideInput
        // runner.ProvideInput = true;

        // 2) PlayFab 拿 Photon Token
        string token = await GetPhotonTokenFromPlayFab();

        // 3) 一次 StartGame → HostOrClient
        var auth = new AuthenticationValues { AuthType = CustomAuthenticationType.Custom };
        auth.AddAuthParameter("username", PlayFabSettings.staticPlayer?.PlayFabId);
        auth.AddAuthParameter("token", token);

        var sceneRef = SceneRef.FromIndex(mapSceneIndex);
        NetworkSceneInfo info = new NetworkSceneInfo();
        info.AddSceneRef(sceneRef, LoadSceneMode.Single);

        var args = new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient,       // 核心：自动 Host / Client
            SessionName = roomCode,
            Scene = info, //SceneRef.FromIndex(gameSceneIndex),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            AuthValues = auth
        };

        Debug.Log($"starting args: {args.GameMode}, {args.Scene}");
        Debug.Log("Calling StartGame...");
        //runner.StartGame(args);
        //if(runner2.IsRunning) await runner2.Shutdown();
        //var result = await NetworkRunner.GetRunnerForScene(SceneManager.GetActiveScene()).StartGame(args);
        if (runner.IsRunning) await CleanRunner();
        var result = await runner.StartGame(args);



        Debug.Log($"StartGame result: {result.Ok}, Reason: {result.ShutdownReason}");
        if (!result.Ok)
        {
            Debug.LogError($"StartGame 失败: {result.ShutdownReason}");
            await CleanRunner();                       // 必要时清理
        }
    }

    /* ---------- 遇到错误时彻底清理 ---------- */
    private async Task CleanRunner()
    {
        if (runner)
        {
            await runner.Shutdown();
            Destroy(instantiatedRunner);
            runner = null;
        }
    }

    /* ---------- Host 负责生成玩家对象 ---------- */
    public void OnPlayerJoined(NetworkRunner r, PlayerRef player)
    {
        if (!r.IsServer) return;

        // 给玩家编号
        playerCount++;
        string playerName = $"Player_{playerCount}";
        string playerId = $"P{playerCount}";

        var obj = r.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player, (runner, obj) =>
        {
            var pc = obj.GetComponent<PlayerController>();
            pc.playerName = playerName;
            pc.playerId = playerId;

            // Host 生成自己时也加GPSUploader防止同步失效（客户端自动有，不必改）
            if (runner.LocalPlayer == player && pc.GetComponent<GPSUploader>() == null)
            {
                pc.gameObject.AddComponent<GPSUploader>();
            }

            Debug.Log($"[RoomManager] 玩家加入：{playerName}");

            if (runner.IsServer)
                StartCoroutine(WaitAndLogId(player, pc));
            // Debug.Log($"[RoomManager] 玩家PlayFabid：{pc.PlayFabId.ToString()}");
        });

        // 创建 GameManager（仅一次）
        if (GameManager.Instance == null)
        {
            r.Spawn(gameManagerPrefab, Vector3.zero, Quaternion.identity);
            Debug.Log("[RoomManager] GameManager 已生成");
        }
    }

    IEnumerator WaitAndLogId(PlayerRef player, PlayerController pc)
    {
        float timeout = 5f; // 最多等5秒
        while (string.IsNullOrEmpty(pc.PlayFabId.ToString()) && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (!string.IsNullOrEmpty(pc.PlayFabId.ToString()))
        {
            idMap[player] = pc.PlayFabId.ToString();
            Debug.Log($"Confirmed [{pc.playerName}]'s PlayFabId: {pc.PlayFabId}");
        }
        else
        {
            Debug.LogWarning($"Failed to get PlayFabId for {pc.playerName}");
        }
    }

    /* ---------- 必要输入（此示例不发送任何数据） ---------- */
    public struct EmptyInput : INetworkInput { }
    public void OnInput(NetworkRunner r, NetworkInput input) => input.Set(new EmptyInput());

    /* ---------- 其余回调留空 ---------- */
    public void OnPlayerLeft(NetworkRunner r, PlayerRef p) { }
    public void OnInputMissing(NetworkRunner r, PlayerRef p, NetworkInput i) { }
    public void OnShutdown(NetworkRunner r, ShutdownReason s) { }
    public void OnConnectRequest(NetworkRunner r, NetworkRunnerCallbackArgs.ConnectRequest req, byte[] tok) { }
    public void OnConnectedToServer()
    {
        throw new NotImplementedException();
    }
    public void OnConnectFailed(NetworkRunner r, NetAddress addr, NetConnectFailedReason rea) { }
    public void OnUserSimulationMessage(NetworkRunner r, SimulationMessagePtr msg) { }
    public void OnCustomAuthenticationResponse(NetworkRunner r, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner r, HostMigrationToken t) { }
    public void OnReliableDataReceived(NetworkRunner r, NetworkObject obj, PlayerRef p) { }
    public void OnReliableDataProgress(NetworkRunner r, PlayerRef p, ReliableKey key, float prog) { }
    public void OnSceneLoadDone(NetworkRunner r) { }
    public void OnSceneLoadStart(NetworkRunner r) { }
    public void OnSessionListUpdated(NetworkRunner r, List<SessionInfo> list) { }
    public void OnObjectEnterAOI(NetworkRunner r, NetworkObject obj, PlayerRef p) { }
    public void OnObjectExitAOI(NetworkRunner r, NetworkObject obj, PlayerRef p) { }

    /* ---------- PlayFab 获取 Photon Token ---------- */
    private static async Task<string> GetPhotonTokenFromPlayFab()
    {
        var tcs = new TaskCompletionSource<string>();
        var req = new GetPhotonAuthenticationTokenRequest
        {
            PhotonApplicationId = "71f9ab24-630a-416a-9567-c6092473291b"
        };
        PlayFabClientAPI.GetPhotonAuthenticationToken(
            req,
            res => tcs.SetResult(res.PhotonCustomAuthenticationToken),
            err => tcs.SetException(new Exception(err.GenerateErrorReport()))
        );
        return await tcs.Task;
    }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        throw new NotImplementedException();
    }

    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("[Fusion] Connected to server.");
    }
    
    public void StartGame()
    {
        if (runner != null && runner.IsServer)
        {
            runner.SceneManager.LoadScene(
                SceneRef.FromIndex(mapSceneIndex),
                new NetworkLoadSceneParameters()
        );
            Debug.Log("[RoomManager] Host starts loading Map");
        }
        else
        {
            Debug.Log("[RoomManager] Only Host can start the game!");
    }
}

}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

/// <summary>
/// Host 统一：收集每个玩家的结算增量（delta），调用 CloudScript 累加写入。
/// 只允许上传一次；支持可选重试。
/// </summary>
public class GameResultManager : MonoBehaviour
{
    public static GameResultManager Instance;

    [Tooltip("上传后是否清空缓存结果")]
    [SerializeField] private bool clearAfterUpload = true;

    private readonly List<PlayerGameResult> _results = new();
    private bool _uploaded = false;
    private bool _isUploading = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 添加或累加一位玩家的增量结果。
    /// 示例：AddOrUpdateResult(id, 500, 500, winDelta:1, roundDelta:1);
    /// </summary>
    public void AddOrUpdateResult(string playFabId, int coinsDelta, int expDelta, int winDelta, int roundDelta)
    {
        if (string.IsNullOrEmpty(playFabId))
        {
            Debug.LogWarning("[GameResultManager] playFabId 为空，忽略。");
            return;
        }

        var entry = _results.Find(r => r.playFabId == playFabId);
        if (entry == null)
        {
            _results.Add(new PlayerGameResult
            {
                playFabId = playFabId,
                coinsDelta = coinsDelta,
                expDelta = expDelta,
                winDelta = winDelta,
                roundDelta = roundDelta
            });
        }
        else
        {
            entry.coinsDelta += coinsDelta;
            entry.expDelta   += expDelta;
            entry.winDelta   += winDelta;
            entry.roundDelta += roundDelta;
        }
    }

    /// <summary>
    /// 执行 CloudScript → 服务器端累加更新。
    /// </summary>
    public void UploadResultsViaCloudScript()
    {
        if (_uploaded)
        {
            Debug.LogWarning("[GameResultManager] 已上传过，本局不再重复。");
            return;
        }
        if (_isUploading)
        {
            Debug.LogWarning("[GameResultManager] 正在上传中，忽略重复调用。");
            return;
        }
        if (_results.Count == 0)
        {
            Debug.LogWarning("[GameResultManager] 没有结果可上传。");
            return;
        }
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            Debug.LogError("[GameResultManager] 未登录 PlayFab，无法上传。");
            return;
        }

        var payload = _results.Select(r => new Dictionary<string, object>
        {
            { "playFabId",  r.playFabId },
            { "coinsDelta", r.coinsDelta },
            { "expDelta",   r.expDelta },
            { "winDelta",   r.winDelta },
            { "roundDelta", r.roundDelta }
        }).ToList();

        var req = new ExecuteCloudScriptRequest
        {
            FunctionName = "ApplyGameResults",
            FunctionParameter = new Dictionary<string, object> {
                { "updates", payload }
            },
            GeneratePlayStreamEvent = true
        };

        _isUploading = true;
        Debug.Log($"[GameResultManager] 开始上传，共 {_results.Count} 条。");

        PlayFabClientAPI.ExecuteCloudScript(req, OnCloudSuccess, OnCloudFail);

        void OnCloudSuccess(ExecuteCloudScriptResult res)
        {
            _isUploading = false;

            if (res.Error != null)
            {
                Debug.LogError("[GameResultManager] CloudScript 逻辑错误: " + res.Error.Message);
                return;
            }

            _uploaded = true;
            Debug.Log("[GameResultManager] CloudScript ApplyGameResults 成功。");
            if (clearAfterUpload)
                _results.Clear();
        }

        void OnCloudFail(PlayFabError err)
        {
            _isUploading = false;
            Debug.LogError("[GameResultManager] ExecuteCloudScript 请求失败: " + err.GenerateErrorReport());
        }
    }

    /// <summary>
    /// 可选：失败后允许手动再试，仅在尚未成功时。
    /// </summary>
    public void RetryUpload()
    {
        if (_uploaded)
        {
            Debug.Log("[GameResultManager] 已成功上传，无需重试。");
            return;
        }
        UploadResultsViaCloudScript();
    }

    /// <summary>
    /// 调试查看缓存。
    /// </summary>
    public void DebugPrintCache()
    {
        Debug.Log("[GameResultManager] 缓存结果：\n" +
                  string.Join("\n", _results.Select(r =>
                      $"{r.playFabId} ΔCoins={r.coinsDelta}, ΔExp={r.expDelta}, ΔWin={r.winDelta}, ΔRound={r.roundDelta}")));
    }
}

public class PlayerGameResult
{
    public string playFabId;
    public int coinsDelta;
    public int expDelta;
    public int winDelta;
    public int roundDelta;
}

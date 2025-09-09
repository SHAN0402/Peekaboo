 using UnityEngine;
using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;


public class GameManager : NetworkBehaviour
{
    // Toggle single player auto win
    public bool enableDebugSoloWin = false;

    [Header("Result UI References")]
    private GameObject resultPanel;
    private TMP_Text    roleText;
    private TMP_Text    expText;
    private TMP_Text    coinText;
    private TMP_Text    displayText;
    private Button homeBtn;

    public static GameManager Instance;

    private Dictionary<string, PopUpEasy> popupPanels = new Dictionary<string, PopUpEasy>();

    [Networked, Capacity(32)]
    public NetworkDictionary<NetworkString<_16>, PlayerPosInfo> allPlayerPos => default;

    [Header("炸弹人胜利判断")]
    public int bombKillCount = 0;
    public int bombKillThreshold = 2;
    
    [Networked] public float gameTimer { get; set; }
    public float gameDurationLimit = 1800f;

    private bool gameEnded = false;
    private bool hasInitializedCamps = false;

    [Header("阵营人数")]
    public int survivorCount = 0;
    public int ghostCount = 0;
    public int bombExpertCount = 0;

    public GameObject bombPrefab;
    private float checkInitTimer = 0f;
    private float gpsDebugTimer = 0f;

    #region resultpanel
    bool resultUIBound = false;

    [Header("Hide Phase")]
    private float hidePhaseDuration = 120f; // HideTime
    private float hidePhaseTimer = 0f;
    public bool isHidePhase = true;
    private bool hasShowHidePopup = false;

    private ResultUIBinder FindResultUIBinder()
    {
        foreach (var binder in Resources.FindObjectsOfTypeAll<ResultUIBinder>())
        {
            if (binder.gameObject.name == "ResultPanel")
            {
                Debug.Log("[GameManager] 找到 ResultUIBinder 对象");
                return binder;
            }
        }

        return null;
    }

    public void BindResultUI(GameObject panel, TMP_Text role, TMP_Text exp, TMP_Text coin, TMP_Text disp, Button home)
    {
        resultPanel = panel;
        roleText = role;
        expText = exp;
        coinText = coin;
        displayText = disp;
        homeBtn = home;

        if (homeBtn != null)
        {
            homeBtn.onClick.RemoveAllListeners();
            homeBtn.onClick.AddListener(OnHomeClicked);
        }

        resultUIBound = true;

        Debug.Log($"[BindResultUI] resultPanel 绑定成功: {resultPanel != null}, 子组件 Text 是否为 null: {roleText == null}, {expText == null}, {coinText == null}");
    }

    private void ShowResultPanel(PlayerController localPlayer, Camp winnerCamp)
    {
        if (!resultUIBound) return;
        bool isWin = localPlayer.currentCamp == winnerCamp;

        roleText.text = "ROLE: " + localPlayer.currentCamp.ToString();
        expText.text = "EXP: " + (isWin ? 500 : 100).ToString();
        coinText.text = "GOLD: " + (isWin ? 100 : 50).ToString();
        displayText.text = isWin ? "Victory! Your skills turned the tide." : "This time, the odds weren't yours.";

        resultPanel.SetActive(true);
    }

    public void OnHomeClicked()
    {
        resultPanel.SetActive(false);
        SceneManager.LoadScene("Main");
    }
    #endregion resultpanel

    public override void Spawned()
    {
        if (Instance == null)
            Instance = this;

        var binder = FindResultUIBinder();
        if (binder != null)
        {
            BindResultUI(
                binder.panel,
                binder.roleText,
                binder.expText,
                binder.coinText,
                binder.displayText,
                binder.homeButton
            );
        }
        else
        {
            Debug.LogError("[GameManager] 未找到 ResultUIBinder");
        }


        foreach (var panel in FindObjectsOfType<PopUpEasy>(true))
        {
            popupPanels[panel.gameObject.name] = panel;
            panel.HidePopup();
        }

        if (Runner.IsServer)
        {
            Invoke(nameof(InitIfPossible), 1.5f);
            Debug.Log("[GameManager] Spawned, 尝试延迟初始化阵营");
        }
    }

    void Start()
    {
        foreach (var panel in popupPanels.Values)
            panel.HidePopup();
    }

    void Update()
    {
        //Hide CountDown
        if (isHidePhase)
        {
            hidePhaseTimer -= Time.deltaTime;

            if (!hasShowHidePopup)
            {
                RpcShowHidePopupToAll();
                hasShowHidePopup = true;
            }
            if (hidePhaseTimer <= 0)
            {
                isHidePhase = false;
                gameTimer = 0f;

                RpcShowGameStartPopupToAll();
                Debug.Log("[GameManager] 躲藏阶段结束，游戏正式开始！");
            }
        }


        // Test for result panel
        /* if (Input.GetKeyDown(KeyCode.R))
    {
        Debug.Log("[GameManager] R pressed");

        foreach (var pc in FindObjectsOfType<PlayerController>())
        {
            Debug.Log($"Checking {pc.playerName} input authority: {pc.Object?.HasInputAuthority}");
            ResolveGame(Camp.Ghost, "所有幸存者+炸弹人被感染");
        }
    }
    */

        if (!HasStateAuthority || gameEnded) return;

        gameTimer += Time.deltaTime;
        checkInitTimer += Time.deltaTime;
        gpsDebugTimer += Time.deltaTime;

        if (checkInitTimer > 2f)
        {
            checkInitTimer = 0f;
            InitIfPossible();
        }

        if (gpsDebugTimer >= 5f)
        {
            gpsDebugTimer = 0f;
            LogPlayerGPS();
        }

        if (gameTimer >= gameDurationLimit)
        {
            CheckAndResolveGameResult();
        }

        if (enableDebugSoloWin && Runner.IsServer && !gameEnded && GetActivePlayers().Count == 1)
        {
            Debug.Log("Testing solo win started");
            ResolveGame(Camp.Survivor, "Test win");
        }
    }

    public void RpcShowHidePopupToAll()
    {
        if (popupPanels.TryGetValue("TimeToHide", out var hidePanel))//gua Panel
            hidePanel.ShowPopup();
        else
            Debug.LogWarning("cant find TimeToHide popup panel");
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RpcShowGameStartPopupToAll()
        {       
            ShowGameStartPopup2Sec_Local();
        }

    private void ShowGameStartPopup2Sec_Local()
    {
        if (popupPanels.TryGetValue("GameStartPanel", out var popup))
        {
            popup.ShowPopup();
            StartCoroutine(HidePopupAfterDelay(popup, 2f));
        }
        else
        {
            Debug.LogWarning("没有找到 GameStart 弹窗！");
        }
    }

private IEnumerator HidePopupAfterDelay(PopUpEasy popup, float delay)
{
    yield return new WaitForSeconds(delay);
    popup.HidePopup();
}

    private void InitIfPossible()
    {
        if (hasInitializedCamps) return;

        List<PlayerController> players = GetActivePlayers();
        Debug.Log($"[GameManager] 当前在线有效玩家数量：{players.Count}");

        if (players.Count >= 3)
        {
            InitPlayerCamps(players);
            hasInitializedCamps = true;
            Debug.Log("[GameManager] 阵营初始化完成（来自 InitIfPossible）");
            Debug.Log("[GameManager] 阵营初始化完成（来自 InitIfPossible）");
        }
        else
        {
            Debug.Log("[GameManager] 玩家不足 2 人，等待中...");
        }
    }

    public void InitPlayerCamps(List<PlayerController> players)
    {
        int ghostIndex = Random.Range(0, players.Count);
        int bombIndex;
        do
        {
            bombIndex = Random.Range(0, players.Count);
        } while (bombIndex == ghostIndex);

        for (int i = 0; i < players.Count; i++)
        {
            PlayerController player = players[i];

            if (i == ghostIndex)
            {
                player.currentCamp = Camp.Ghost;
                player.infectionStatus = InfectionStatus.Infected;
                player.canInfectOthers = true;
                player.isOriginalGhost = true;

                Debug.Log($"[Init] 玩家 {player.playerName} 被选为初始鬼");
                RpcShowPopupToClient("Ghost", player.playerName);
            }


            else if (i == bombIndex)
            {
                player.currentCamp = Camp.BombExpert;
                player.infectionStatus = InfectionStatus.None;
                player.canInfectOthers = false;
                player.isOriginalGhost = false;

                Debug.Log($"[Init] 玩家 {player.playerName} 被选为炸弹人");
                RpcShowPopupToClient("BombExpert", player.playerName);
            }
            else
            {
                player.currentCamp = Camp.Survivor;
                player.infectionStatus = InfectionStatus.None;
                player.canInfectOthers = false;
                player.isOriginalGhost = false;

                Debug.Log($"[Init] 玩家 {player.playerName} 是普通幸存者");
                RpcShowPopupToClient("Survivor", player.playerName);
            }
            
            //RpcShowPopupAndPlayAudio(player.currentCamp.ToString(), player.playerName);
        }


        isHidePhase = true;
        hidePhaseTimer = 120f;
        Debug.Log("[GameManager] 阵营初始化完成，进入躲藏阶段");
        hasShowHidePopup = false;



        CountCamps();
        gameTimer = 0f;
        gameEnded = false;
        bombKillCount = 0;
        
    }

    //Role Musik
    /*public void RpcShowPopupAndPlayAudio(string role, string targetPlayerName)
    {
        foreach (var localPlayer in FindObjectsOfType<PlayerController>())
        {
            if (localPlayer.Object != null && localPlayer.Object.HasInputAuthority)
            {
                if (localPlayer.playerName == targetPlayerName)
                {
                    switch (role)
                    {
                        case "Ghost":
                        // Play ghost audio:
                            InGameAudio.Instance?.PlayOneShot(InGameAudio.Instance.ghostRoleClip);
                            break;
                        case "BombExpert":
                            InGameAudio.Instance?.PlayOneShot(InGameAudio.Instance.bombRoleClip);
                            break;
                        case "Survivor":
                            InGameAudio.Instance?.PlayOneShot(InGameAudio.Instance.survivorRoleClip);
                            break;
                    }
                }
            }
        }
    }*/

    public void CountCamps()
    {
        survivorCount = 0;
        ghostCount = 0;
        bombExpertCount = 0;

        List<PlayerController> players = GetActivePlayers();
        foreach (PlayerController p in players)
        {
            if (p.isEliminated) continue;

            if (p.infectionStatus == InfectionStatus.Infected)
            {
                ghostCount++;
            }
            else
            {
                switch (p.currentCamp)
                {
                    case Camp.Survivor:
                        survivorCount++;
                        break;
                    case Camp.BombExpert:
                        bombExpertCount++;
                        break;
                    case Camp.Ghost:
                        ghostCount++;
                        break;
                }
            }
        }

        Debug.Log($"【GameManager】当前阵营人数统计：Survivor: {survivorCount}, Ghost: {ghostCount}, BombExpert: {bombExpertCount}");
        CheckAndResolveGameResult();
    }

    public void ReportInfection(PlayerController victim)
{
    victim.infectionStatus = InfectionStatus.Infected;
    victim.canInfectOthers = true;
    victim.currentCamp = Camp.Ghost;

    Debug.Log($"【感染汇报】{victim.playerName} 被感染，转入鬼阵营");
    RpcShowStatusPopupToClient("Infection", victim.playerName); // 新增
    CountCamps();
}

   public void ReportBombKill(PlayerController victim, PlayerController killer)
{
    if (killer != null && killer.currentCamp == Camp.BombExpert)
    {
        bombKillCount++;
        victim.isEliminated = true;

        Debug.Log($"炸弹人击杀 +1（当前 {bombKillCount}/{bombKillThreshold}）");
        Debug.Log($"炸弹人击杀 +1（当前 {bombKillCount}/{bombKillThreshold}）");
        RpcShowStatusPopupToClient("StepOnBomb", victim.playerName); // 新增

        if (bombKillCount >= bombKillThreshold)
        {
            Debug.Log("炸弹人击杀两人，直接胜利！");
            Debug.Log("炸弹人击杀两人，直接胜利！");
            CheckAndResolveGameResult();
        }
    }
}

    public void CheckAndResolveGameResult()
    {
        if (gameEnded) return;

        if (survivorCount <= 0 && bombExpertCount <= 0)
        {
            Debug.Log("所有幸存者和炸弹人已被感染！鬼阵营胜利！");
            Debug.Log("所有幸存者和炸弹人已被感染！鬼阵营胜利！");
            ResolveGame(Camp.Ghost, "所有幸存者+炸弹人被感染");
        }
        else if (bombKillCount >= bombKillThreshold)
        {
            Debug.Log("炸弹人已炸死两人！炸弹人胜利！");
            Debug.Log("炸弹人已炸死两人！炸弹人胜利！");
            ResolveGame(Camp.BombExpert, "炸弹人炸死两人");
        }
        else if (gameTimer >= gameDurationLimit && survivorCount > 0)
        {
            Debug.Log("游戏时间到，幸存者存活！好人胜利！");
            Debug.Log("游戏时间到，幸存者存活！好人胜利！");
            ResolveGame(Camp.Survivor, "游戏时间到，好人存活");
        }
    }

    private void ResolveGame(Camp winnerCamp, string reason)
    {
        if (gameEnded) return;

    gameEnded = true;
    Debug.Log($"游戏结束！胜利阵营：{winnerCamp}（原因：{reason}）");
        gameEnded = true;
        Debug.Log($"游戏结束！胜利阵营：{winnerCamp}（原因：{reason}）");

        foreach (var player in GetActivePlayers())
        {
            string playfabId = player.PlayFabId.ToString();
            if (!string.IsNullOrEmpty(playfabId))
            {
                if (player.currentCamp == winnerCamp)
                {
                    GameResultManager.Instance.AddOrUpdateResult(playfabId, 500, 100, 1, 1);
                }
                else // lose
                {
                    GameResultManager.Instance.AddOrUpdateResult(playfabId, 100, 50, 0, 1);
                }
            }
            RpcShowVictoryPopupToClient(winnerCamp.ToString(), player.playerName);
        }

        if (Runner.IsServer)
        {
            GameResultManager.Instance.UploadResultsViaCloudScript();
            Debug.Log("[GameManager] 所有胜利玩家奖励已上传 PlayFab");
        }
    }




    private List<PlayerController> GetActivePlayers()
    {
        List<PlayerController> result = new List<PlayerController>();
        foreach (var playerRef in Runner.ActivePlayers)
        {
            foreach (var pc in FindObjectsOfType<PlayerController>())
            {
                if (pc.Object != null && pc.Object.InputAuthority == playerRef)
                {
                    result.Add(pc);
                    break;
                }
            }
        }
        return result;
    }

    private void LogPlayerGPS()
    {
        var players = GetActivePlayers();
        Debug.Log($"[GPS] 当前在线玩家数量：{players.Count}");

        foreach (var p in players)
        {
            bool hasAuthority = p.Object != null && p.Object.HasStateAuthority;

            if (Mathf.Abs(p.gpsLatitude) > 0.000001f || Mathf.Abs(p.gpsLongitude) > 0.000001f)
            if (Mathf.Abs(p.gpsLatitude) > 0.000001f || Mathf.Abs(p.gpsLongitude) > 0.000001f)
            {
                Debug.Log($"[GPS] 玩家：{p.playerName}, Lat: {p.gpsLatitude:F6}, Lon: {p.gpsLongitude:F6}, Authority: {hasAuthority}");
            }
            else
            {
                Debug.Log($"[GPS] 玩家：{p.playerName} 尚未上传 GPS，Authority: {hasAuthority}");
            }

            if (!hasAuthority)
            {
                Debug.LogWarning($"[GPS] 玩家 {p.playerName} 无 StateAuthority，Host 端无法获得其 GPS 数据（Fusion 不同步）");
                Debug.LogWarning($"[GPS] 玩家 {p.playerName} 无 StateAuthority，Host 端无法获得其 GPS 数据（Fusion 不同步）");
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcShowPopupToClient(string role, string targetPlayerName)
    {
        foreach (var localPlayer in FindObjectsOfType<PlayerController>())
        {
            if (localPlayer.Object != null && localPlayer.Object.HasInputAuthority)
            {
                Debug.Log($"[Client] 正在检查本地玩家：{localPlayer.playerName}, Authority: true");

                if (localPlayer.playerName == targetPlayerName)
                {
                    Debug.Log($"[Client] 弹窗触发：角色={role}，自己是 {localPlayer.playerName}");
                    Debug.Log($"[Client] 弹窗触发：角色={role}，自己是 {localPlayer.playerName}");

                    switch (role)
                    {
                        case "Ghost":
                            if (popupPanels.TryGetValue("PopUpGhost", out var ghostPanel))
                                ghostPanel.ShowPopup();
                            break;
                        case "BombExpert":
                            if (popupPanels.TryGetValue("PopUpBombExpert", out var bombPanel))
                                bombPanel.ShowPopup();
                            break;
                        case "Survivor":
                            if (popupPanels.TryGetValue("PopUpSurvivor", out var survivorPanel))
                                survivorPanel.ShowPopup();
                            break;
                    }
                }
            }
        }
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcShowVictoryPopupToClient(string winnerCamp, string targetPlayerName)
    {
        Camp winCampEnum = (Camp)System.Enum.Parse(typeof(Camp), winnerCamp);

        foreach (var localPlayer in FindObjectsOfType<PlayerController>())
        {
            if (localPlayer.Object != null && localPlayer.Object.HasInputAuthority)
            {
                if (localPlayer.playerName == targetPlayerName)
                {
                    string popupKey = winnerCamp switch
                    {
                        "Ghost"      => "PopUpWinGhost",
                        "BombExpert" => "PopUpWinBombExpert",
                        _             => "PopUpWinSurvivor"
                    };

                    if (popupPanels.TryGetValue(popupKey, out var winPanel))
                    {
                        winPanel.ShowPopup();

                        // 为关闭按钮添加一次性监听：关闭胜利弹窗→显示结果面板
                        // winPanel is script...
                        Button closeBtn = winPanel.popupPanel.GetComponentInChildren<Button>(true);
                        // Debug.Log(winPanel.gameObject);
                        if (closeBtn != null)
                        {
                            closeBtn.onClick.RemoveAllListeners();
                            closeBtn.onClick.AddListener(() =>
                            {
                                winPanel.HidePopup();
                                ShowResultPanel(localPlayer, winCampEnum);
                            });
                            Debug.Log("Clost Btn attached to result panel.");
                        }
                        else
                        {
                            Debug.Log("No close btn.");
                        }
                    }
                }
            }
        }
    }

    /* [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcShowVictoryPopupToClient(string winnerCamp, string targetPlayerName)
    {
        foreach (var localPlayer in FindObjectsOfType<PlayerController>())
        {
            if (localPlayer.Object != null && localPlayer.Object.HasInputAuthority)
            {
                Debug.Log($"[Client] 正在检查本地玩家：{localPlayer.playerName}, Authority: true");

                if (localPlayer.playerName == targetPlayerName)
                {
                    Debug.Log($"[Client] 弹窗触发：胜利阵营={winnerCamp}，自己是 {localPlayer.playerName}");
                    Debug.Log($"[Client]弹窗触发：胜利阵营={winnerCamp}，自己是 {localPlayer.playerName}");

                    switch (winnerCamp)
                    {
                        case "Ghost":
                            if (popupPanels.TryGetValue("PopUpWinGhost", out var ghostWinPanel))
                                ghostWinPanel.ShowPopup();
                            break;
                        case "BombExpert":
                            if (popupPanels.TryGetValue("PopUpWinBombExpert", out var bombWinPanel))
                                bombWinPanel.ShowPopup();
                            break;
                        case "Survivor":
                            if (popupPanels.TryGetValue("PopUpWinSurvivor", out var survivorWinPanel))
                                survivorWinPanel.ShowPopup();
                            break;
                    }
                }
            }
        }
    }
    */
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
public void RpcShowStatusPopupToClient(string popupType, string targetPlayerName)
{
    foreach (var localPlayer in FindObjectsOfType<PlayerController>())
    {
        if (localPlayer.Object != null && localPlayer.Object.HasInputAuthority)
        {
            if (localPlayer.playerName == targetPlayerName)
            {
                Debug.Log($"[Client]状态弹窗弹出：类型={popupType}，玩家={localPlayer.playerName}");
                if (popupPanels.TryGetValue("PopUp" + popupType, out var statusPanel))
                    statusPanel.ShowPopup();
                else
                    Debug.LogError($"[Client] 没找到弹窗 PopUp{popupType}！");
            }
        }
    }
}

}

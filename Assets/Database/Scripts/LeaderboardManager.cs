using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LeaderboardManager : MonoBehaviour
{
    [Header("Row & Prefab")]
    [SerializeField] GameObject rowPrefab;

    [Header("Panels & Content Parents")]
    [SerializeField] GameObject winRatePanel;
    [SerializeField] GameObject winsPanel;
    [SerializeField] GameObject scorePanel;

    [SerializeField] Transform winRateContent;
    [SerializeField] Transform winsContent;
    [SerializeField] Transform scoreContent;

    [Header("Tab Buttons")]
    [SerializeField] Button winRateBtn;
    [SerializeField] Button winsBtn;
    [SerializeField] Button scoreBtn;

    void Start()
    {
        winRateBtn.onClick.AddListener(() => ShowPanel(PanelType.WinRate));
        winsBtn.onClick.AddListener(() => ShowPanel(PanelType.Wins));
        scoreBtn.onClick.AddListener(() => ShowPanel(PanelType.Score));

        // 默认先显示胜率榜
        ShowPanel(PanelType.WinRate);
    }

    enum PanelType { WinRate, Wins, Score }
    PanelType currentPanel;
    void ShowPanel(PanelType type)
    {
        // 1) 先全部隐藏
        winRatePanel.SetActive(false);
        winsPanel.SetActive(false);
        scorePanel.SetActive(false);

        // 2) 根据类型显示并刷新
        switch (type)
        {
            case PanelType.WinRate:
                winRatePanel.SetActive(true);
                LoadLeaderboard("WinRate", winRateContent);
                break;
            case PanelType.Wins:
                winsPanel.SetActive(true);
                LoadLeaderboard("Wins", winsContent);
                break;
            case PanelType.Score:
                scorePanel.SetActive(true);
                LoadLeaderboard("Score", scoreContent);
                break;
        }

        currentPanel = type;
    }

    void LoadLeaderboard(string statName, Transform contentParent)
    {
        // 清空旧行
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        PlayFabClientAPI.GetLeaderboard(
            new GetLeaderboardRequest
            {
                StatisticName   = statName,
                StartPosition   = 0,
                MaxResultsCount = 50   // 取前 50 名，可自行调整
            },
            result =>
            {
                int index = 1;
                foreach (var entry in result.Leaderboard)
                {
                    var row = Instantiate(rowPrefab, contentParent);
                    var ui = row.GetComponent<LeaderboardUI>();
                    if (currentPanel == PanelType.WinRate)
                    {
                        ui.SetData(
                        index,
                        entry.DisplayName ?? entry.PlayFabId,
                        entry.StatValue.ToString() + "%"
                        );
                        index++;
                    }
                    else
                    {
                        ui.SetData(
                        index,
                        entry.DisplayName ?? entry.PlayFabId,
                        entry.StatValue.ToString()
                        );
                        index++;
                    }
                }
            },
            error => Debug.LogWarning($"GetLeaderboard {statName} failed: {error.GenerateErrorReport()}"));
    }
}

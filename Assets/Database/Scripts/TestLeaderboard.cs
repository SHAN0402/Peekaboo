using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class TestLeaderboard : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UpdateStatistic("Wins", 50);
        // UpdateStatistic("WinRate", Mathf.RoundToInt(((float)wins / totalGames) * 10000));
        UpdateStatistic("WinRate", Mathf.RoundToInt((float) 0.3 * 10000));
        // UpdateStatistic("Score", exp + Mathf.RoundToInt(coins * 0.5f));
        UpdateStatistic("Score", 33);
    }

    void UpdateStatistic(string statName, int value)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = statName,
                    Value = value
                }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request,
            result => Debug.Log($"Updated {statName} to {value}"),
            error => Debug.LogWarning(error.GenerateErrorReport())
        );
    }
}

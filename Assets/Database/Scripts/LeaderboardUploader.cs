using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class LeaderboardUploader : MonoBehaviour
{
    void Start()
    {
        UploadAllLeaderboards();
    }

    public void UploadAllLeaderboards()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            var data = result.Data;

            int coins = data.ContainsKey("Coins") ? int.Parse(data["Coins"].Value) : 0;
            int exp = data.ContainsKey("Exp") ? int.Parse(data["Exp"].Value) : 0;
            int winCount = data.ContainsKey("WinCount") ? int.Parse(data["WinCount"].Value) : 0;
            int totalGames = data.ContainsKey("TotalGames") ? int.Parse(data["TotalGames"].Value) : 0;

            int score = exp + Mathf.RoundToInt(coins * 0.5f);
            int winRate = totalGames > 0 ? Mathf.RoundToInt((float)winCount / totalGames * 100) : 0;

            UploadLeaderboard("Score", score);
            UploadLeaderboard("Wins", winCount);
            UploadLeaderboard("WinRate", winRate);

        }, error =>
        {
            Debug.LogError("获取用户数据失败: " + error.GenerateErrorReport());
        });
    }

    private void UploadLeaderboard(string leaderboardName, int value)
    {
        // int scaledValue = Mathf.FloorToInt(value * 1000);  // 保留3位小数

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = leaderboardName,
                    Value = value
                }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request,
            result => Debug.Log($"成功上传 Leaderboard [{leaderboardName}] → {value}"),
            error  => Debug.LogError($"上传 Leaderboard [{leaderboardName}] 失败: {error.GenerateErrorReport()}")
        );
    }
}

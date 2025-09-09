using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ClientModels;
using System;
using PlayFab;
using UnityEngine.Networking;
using Image = UnityEngine.UI.Image;

// Manages server and local data
public static class PlayFabData
{
    #region User data
    static bool isGettingData = false;

    // local data
    static Dictionary<string, UserDataRecord> _userData;

    static readonly List<(Action<GetUserDataResult> ok, Action<PlayFabError> err)> _queued = new();

    // Updates server and local data
    public static void UpdateData(Dictionary<string, string> newData, Action<UpdateUserDataResult> onSuccess, Action<PlayFabError> onFail)
    {
        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest()
            {
                Data = newData
            },
            // Update server success
            successResult =>
            {
                // Update local
                if (_userData != null)
                {
                    foreach (var key in newData.Keys)
                    {
                        // wrap string to UserDataRecord
                        UserDataRecord value = new UserDataRecord() { Value = newData[key] };
                        if (_userData.ContainsKey(key)) _userData[key] = value;
                        else _userData.Add(key, value);
                    }
                }
                onSuccess(successResult);
            },
            onFail
        );
    }

    // Get data from local or server
    public static void GetData(Action<GetUserDataResult> onSuccess, Action<PlayFabError> onFail, bool forceRefresh = false)
    {
        // Data locally cached (already fetched)
        if (!forceRefresh && _userData != null)
        {
            onSuccess(new GetUserDataResult { Data = _userData });
            return;
        }

        // someone else is already downloading (enqueue callbacks)
        if (isGettingData)
        {
            _queued.Add((onSuccess, onFail));
            return;
        }

        // first caller starts to download
        isGettingData = true;
        _queued.Add((onSuccess, onFail));

        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest(),
            successResult =>
            {
                isGettingData = false;
                _userData = successResult.Data;

                // return results for all callers to handle
                foreach (var (ok, _) in _queued) ok(successResult);
                _queued.Clear();
            },
            error =>
            {
                isGettingData = false;
                foreach (var (_, err) in _queued) err?.Invoke(error);
                _queued.Clear();
            });
    }

    public static void Reset()
    {
        _userData = null;
        isGettingData = false;
        _queued.Clear();
    }
    #endregion User data

    #region Leaderboard
    public static void SendLeaderboard(int score)
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest()
        {
            Statistics = new List<StatisticUpdate>()
            {
                new StatisticUpdate() {
                    StatisticName = "WinRate",
                    Value = score
                }
            }
        },
        successResult => Debug.Log("Leaderboard update successful."),
        error => Debug.Log(error.Error + " : " + error.GenerateErrorReport())
        );
    }

    /* public static void GetLeaderboard()
    {
        PlayFabClientAPI.GetLeaderboard(
            new GetLeaderboardRequest
            {
                StatisticName = "WinRate",
                StartPosition = 0,
                MaxResultsCount = 100
            },
            successResult =>
            {
                Debug.Log("Leaderboard:");
                foreach (var entry in successResult.Leaderboard)
                {
                    Debug.Log(entry.Position + " " + entry.PlayFabId + " " + entry.StatValue);
                }
            },
            error => Debug.Log(error.Error + " : " + error.GenerateErrorReport())
        );
    }
    */
    #endregion Leaderboard

    #region Avatar
    public static IEnumerator LoadAvatarImage(string url, Image avatarImage)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D tex = DownloadHandlerTexture.GetContent(www);
            Sprite spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
            avatarImage.sprite = spr;
            // avatarImage.preserveAspect = true;
        }
        else
        {
            Debug.LogError("Loading avatar failed: " + www.error);
        }
    }
    #endregion Avatar

    #region Outfit
    public static void SaveOutfit(int outfitId)
    {
        var data = new Dictionary<string, string>
        {
            { "outfitId", outfitId.ToString() }
        };

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
        {
            Data = data,
            Permission = UserDataPermission.Public
        },
        result => Debug.Log("Outfit saved."),
        error => Debug.LogError("Save outfit failed: " + error.GenerateErrorReport())
        );
    }

    public static void LoadOutfitFromResources(int outfitId, Image outfitImage)
    {
        Sprite outfitSprite = Resources.Load<Sprite>($"Outfits/Outfit_{outfitId}");
        if (outfitSprite != null)
        {
            outfitImage.sprite = outfitSprite;
            // outfitImage.preserveAspect = true;
            Debug.Log("Outfit loaded");
        }
    }

    #endregion Outfit

    #region Frame

    public static void SaveFrame(int frameId)
    {
        var data = new Dictionary<string, string>
        {
            { "frameId", frameId.ToString() }
        };

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
        {
            Data = data,
            Permission = UserDataPermission.Public
        },
        result => Debug.Log("Frame saved."),
        error => Debug.LogError("Save frame failed: " + error.GenerateErrorReport())
        );
    }

    public static void LoadFrameFromResources(int frameId, Image frameImage)
    {
        Sprite frameSprite = Resources.Load<Sprite>($"Frames/Frame_{frameId}");
        if (frameSprite != null)
        {
            frameImage.sprite = frameSprite;
            // outfitImage.preserveAspect = true;
            Debug.Log("Frame loaded");
        }
    }
    #endregion Frame
}

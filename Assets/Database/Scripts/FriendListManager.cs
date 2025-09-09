using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FriendListManager : MonoBehaviour
{
    [Header("Friends UI")]
    [SerializeField] GameObject rowPrefab;
    [SerializeField] Transform contentParent;
    [SerializeField] Button refreshBtn;
    [SerializeField] Button requestBtn;
    [SerializeField] Button homeBtn;
    [SerializeField] GameObject friendPanel;
    [SerializeField] GameObject requestPanel;

    readonly Dictionary<string, FriendRowUI> rows = new();

    void Awake()
    {
        var img = GetComponent<UnityEngine.UI.Image>();
        if (img) img.raycastTarget = false;   // row no longer swallows clicks
    }

    void Start()
    {
        RefreshFriends();
        refreshBtn.onClick.AddListener(RefreshFriends);
        requestBtn.onClick.AddListener(SwitchToRequest);
        homeBtn.onClick.AddListener(SwitchToMain);
    }

    #region interface
    public void RefreshFriends()
    {
        PlayFabClientAPI.GetFriendsList(
            new GetFriendsListRequest
            {
                ProfileConstraints = new PlayerProfileViewConstraints
                {
                    ShowDisplayName = true,
                    ShowAvatarUrl = true
                }
            },
            OnFriendsReceived,
            e => Debug.LogWarning(e.GenerateErrorReport()));
    }

    public void RemoveFriend(string targetId)
    {
        ExecuteCloud("MutualRemoveFriend", new { targetId },
            () =>
            {
                // Remove local
                PlayFabClientAPI.RemoveFriend(
                    new RemoveFriendRequest { FriendPlayFabId = targetId },
                    _ =>
                    {

                    },
                    e => Debug.LogWarning(e.GenerateErrorReport()));
                    
                    if (rows.TryGetValue(targetId, out var ui))
                        {
                            Destroy(ui.gameObject);
                            rows.Remove(targetId);
                        }
                        // Refresh list
                        RefreshFriends();
                        Debug.Log("Friend removed mutually and refreshed.");
            });
    }

    public void SwitchToRequest()
    {
        // SceneManager.LoadScene("RequestScene");
        friendPanel.SetActive(false);
        requestPanel.SetActive(true);
    }

    public void SwitchToMain()
    {
        SceneManager.LoadScene("Main");
    }
    #endregion interface

    /* ---------- helpers ---------- */

    void OnFriendsReceived(GetFriendsListResult res)
    {
        foreach (Transform c in contentParent) Destroy(c.gameObject);
        rows.Clear();

        foreach (FriendInfo f in res.Friends)
        {
            PlayFabClientAPI.GetUserData(
                new GetUserDataRequest
                {
                    PlayFabId = f.FriendPlayFabId,
                    Keys = new List<string> { "outfitId", "frameId" }
                },
                result =>
                {
                    int outfitId = -1;
                    int frameId = -1;
                    if (result.Data != null && result.Data.TryGetValue("outfitId", out var outfitRecord))
                        int.TryParse(outfitRecord.Value, out outfitId);
                    
                    if (result.Data != null && result.Data.TryGetValue("frameId", out var frameRecord))
                        int.TryParse(frameRecord.Value, out frameId);

                    var go = Instantiate(rowPrefab, contentParent);
                    var ui = go.GetComponent<FriendRowUI>();

                    string display = f.TitleDisplayName
                                ?? f.Username
                                ?? f.FriendPlayFabId;

                    ui.Init(f.FriendPlayFabId, display, f.Profile.AvatarUrl, outfitId, frameId, this);
                    rows.Add(f.FriendPlayFabId, ui);
                },
                error => Debug.LogWarning(error.GenerateErrorReport())
            );
            /* var go = Instantiate(rowPrefab, contentParent);
            var ui = go.GetComponent<FriendRowUI>();

            string display = f.TitleDisplayName
                          ?? f.Username
                          ?? f.FriendPlayFabId;

            ui.Init(f.FriendPlayFabId, display, f.Profile.AvatarUrl, this);
            rows.Add(f.FriendPlayFabId, ui); */
        }

        Debug.Log($"[FriendManager] now showing {rows.Count} friends");
    }
    
    void ExecuteCloud(string fnName, object param, Action onSuccess)
        {
            PlayFabClientAPI.ExecuteCloudScript(
                new ExecuteCloudScriptRequest
                {
                    FunctionName     = fnName,
                    FunctionParameter= param,
                    GeneratePlayStreamEvent = false
                },
                _ => onSuccess?.Invoke(),
                err => Debug.LogWarning(err.GenerateErrorReport()));
        }
}
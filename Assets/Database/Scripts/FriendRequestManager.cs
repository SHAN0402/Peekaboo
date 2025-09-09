using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FriendRequestManager : MonoBehaviour
{
    [Header("Send request")]
    [SerializeField] TMP_InputField friendIdInput;
    [SerializeField] Button addBtn;

    [Header("Requests list")]
    [SerializeField] GameObject rowPrefab;
    [SerializeField] Transform contentParent;

    [Header("Utils")]
    [SerializeField] GameObject friendPanel;
    [SerializeField] GameObject requestPanel;
    [SerializeField] Button friendBtn;
    
    // local requests data
    readonly Dictionary<string, FriendRequestRowUI> rows = new();

    void Awake()
    {
        var img = GetComponent<UnityEngine.UI.Image>();
        if (img) img.raycastTarget = false;   // row no longer swallows clicks
        addBtn.onClick.AddListener(SendRequest);
        friendBtn.onClick.AddListener(SwitchToFriend);
    }
    

    void Start()
    {
        RefreshPending();
    }

    #region intefaces
    public void RefreshPending()
    {
        PlayFabData.GetData(
            result =>
            {
                foreach (Transform c in contentParent) Destroy(c.gameObject);
                rows.Clear();

                if (result.Data != null &&
                    result.Data.TryGetValue("FriendRequests", out var rec) &&
                    !string.IsNullOrEmpty(rec.Value))
                {
                    var list = JsonUtility.FromJson<IdArrayWrapper>($"{{\"Ids\":{rec.Value}}}");
                    foreach (string id in list.Ids)
                    {
                        // 异步获取 username
                        PlayFabClientAPI.GetAccountInfo(
                            new GetAccountInfoRequest { PlayFabId = id },
                            info =>
                            {
                                string username = info.AccountInfo.Username;

                                var go = Instantiate(rowPrefab, contentParent);
                                var ui = go.GetComponent<FriendRequestRowUI>();
                                ui.Init(id, username, Accept, Decline);

                                rows[id] = ui;
                            },
                            error =>
                            {
                                Debug.LogWarning("Failed to get username for ID: " + id + "\n" + error.GenerateErrorReport());
                            });
                    }
                }
            },
            err => Debug.LogWarning(err.GenerateErrorReport()),
            true); // enable forceRefresh from server
    }

    // Username version
    public void SendRequest()
    {
        string username = friendIdInput.text.Trim();
        if (string.IsNullOrEmpty(username)) return;

        PlayFabClientAPI.GetAccountInfo(
            new GetAccountInfoRequest
            {
                Username = username
            },
            result =>
            {
                string targetId = result.AccountInfo.PlayFabId;

                ExecuteCloud("SendFriendRequest",
                    new { targetId },
                    () =>
                    {
                        Debug.Log($"Request sent → {username}（{targetId}）");
                        friendIdInput.text = "";
                    }
                    // TODO: Requires onFail here??
                );

            },
            error => Debug.LogError("Username not found: " + error.GenerateErrorReport())
        );
    }

    // PlayFabId version
    /* public void SendRequest()
    {
        string target = friendIdInput.text.Trim();
        if (string.IsNullOrEmpty(target)) return;

        ExecuteCloud("SendFriendRequest",
            new { targetId = target },
                     () =>
                     {
                         Debug.Log($"Request sent → {target}");
                         friendIdInput.text = "";
                     });
    } */

    public void Accept(string requesterId)
    {
        ExecuteCloud("AcceptFriendRequest",
                     new { requesterId },
                     () =>
                     {
                         Debug.Log($"Accepted → {requesterId}");
                         DestroyRow(requesterId);
                         RefreshFriends();    // Update friend list
                         RefreshPending();
                     });
    }

    public void Decline(string requesterId)
    {
        ExecuteCloud("DeclineFriendRequest",
                     new { requesterId },
                     () =>
                     {
                         Debug.Log($"Declined → {requesterId}");
                         DestroyRow(requesterId);
                         RefreshPending();
                     });
    }

    public void SwitchToFriend()
    {
        requestPanel.SetActive(false);
        friendPanel.SetActive(true);
    }
    #endregion interfaces

    #region helpers
    // Test: friend count
    void RefreshFriends()
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest(),
            res => Debug.Log($"Friend count: {res.Friends.Count}"),
            err => Debug.LogWarning(err.GenerateErrorReport()));
    }

    void DestroyRow(string id)
    {
        if (rows.TryGetValue(id, out var ui))
        {
            Destroy(ui.gameObject);
            rows.Remove(id);
        }
    }

    [Serializable] class IdArrayWrapper { public List<string> Ids; }

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
    #endregion helpers
}

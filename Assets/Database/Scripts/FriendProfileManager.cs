using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.SceneManagement;

public class FriendProfileManager : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] Image avatarImage;
    [SerializeField] Image outfitImage;
    [SerializeField] Image frameImage;
    [SerializeField] TMP_Text displayName;
    [SerializeField] TMP_Text userName;
    [SerializeField] TMP_Text bioText;

    [Header("Util")]
    [SerializeField] GameObject panel;
    [SerializeField] Button closeButton;

    public static FriendProfileManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        closeButton.onClick.AddListener(OnClosePressed);
    }

    public void ShowProfile(string id)
    {
        panel.SetActive(true);
        outfitImage.gameObject.SetActive(false);
        frameImage.gameObject.SetActive(false);
        
        PlayFabClientAPI.GetPlayerProfile(
            new GetPlayerProfileRequest
            {
                PlayFabId = id,
                ProfileConstraints = new PlayerProfileViewConstraints
                {
                    ShowDisplayName = true,
                    ShowAvatarUrl = true
                }
            },
            result =>
            {
                displayName.text = result.PlayerProfile.DisplayName;
                string avatarUrl = result.PlayerProfile.AvatarUrl;
                if (!string.IsNullOrEmpty(avatarUrl))
                    StartCoroutine(PlayFabData.LoadAvatarImage(avatarUrl, avatarImage));
            },
            error => Debug.LogError("Loading friend profile failed: " + error.GenerateErrorReport())
        );

        // ↓ 本地工具函数：从 UserData 中取整型键，失败返回 0
        int GetInt(Dictionary<string, UserDataRecord> dict, string key)
        {
            return dict != null &&
                dict.TryGetValue(key, out var rec) &&
                int.TryParse(rec.Value, out var v) ? v : -1;
        }

        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest {
                PlayFabId = id,
                Keys = new List<string> { "Biography", "outfitId", "frameId" }
            },
            result => {
                var data = result.Data;

                // Biography
                bioText.text = data != null && data.TryGetValue("Biography", out var bio) && !string.IsNullOrWhiteSpace(bio.Value)
                            ? bio.Value
                            : "A quiet player. Nothing to share... yet!";

                // Outfit & frame — 用工具函数统一取值
                int outfitId = GetInt(data, "outfitId");
                int frameId  = GetInt(data, "frameId");

                Debug.Log($"outfitId: {outfitId}  frameId: {frameId}");

                if (outfitId != -1)
                {
                    outfitImage.gameObject.SetActive(true);
                    PlayFabData.LoadOutfitFromResources(outfitId, outfitImage);
                }

                if (frameId != -1)
                {
                    frameImage.gameObject.SetActive(true);
                    PlayFabData.LoadFrameFromResources(frameId,  frameImage);
                }
                
            },
            error => Debug.LogError("Loading friend profile extras failed: " + error.GenerateErrorReport())
        );

        /*
        // Get bio and outfit and frame
        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest
            {
                PlayFabId = id,
                Keys = new List<string> { "Biography", "outfitId", "frameId" }
            },
            result =>
            {
                Debug.Log("outfitId exists: " + result.Data.ContainsKey("outfitId"));
                Debug.Log("frameId exists: " + result.Data.ContainsKey("frameId"));
                if (result.Data != null && result.Data.ContainsKey("Biography"))
                    bioText.text = result.Data["Biography"].Value;
                else
                    bioText.text = "A quiet player. Nothing to share... yet!";
                if (result.Data != null && result.Data.ContainsKey("outfitId"))
                {
                    string outfitId = result.Data.TryGetValue("outfitId", out var outfit) ? outfit.Value : null;
                    Debug.Log("Fetched outfitId: " + outfitId);
                    if (int.TryParse(outfitId, out int id))
                    {
                        PlayFabData.LoadOutfitFromResources(id, outfitImage);
                    }
                    else
                    {
                        PlayFabData.LoadOutfitFromResources(0, outfitImage);
                    }
                }
                else
                {
                    PlayFabData.LoadOutfitFromResources(0, outfitImage);
                }

                if (result.Data != null && result.Data.ContainsKey("frameId"))
                {
                    string frameId = result.Data.TryGetValue("frameId", out var frame) ? frame.Value : null;
                    Debug.Log("Fetched frameId: " + frameId);
                    if (int.TryParse(frameId, out int id))
                    {
                        PlayFabData.LoadFrameFromResources(id, frameImage);
                    }
                    else
                    {
                        PlayFabData.LoadFrameFromResources(0, frameImage);
                    }
                }
                else
                {
                    PlayFabData.LoadFrameFromResources(0, frameImage);
                }
            },
            error =>
            {
                Debug.LogError("Loading friend bio, outfit and frame failed: " + error.GenerateErrorReport());
            }
        );
        */

        // Get username
        PlayFabClientAPI.GetAccountInfo(
            new GetAccountInfoRequest
            {
                PlayFabId = id
            },
            result =>
            {
                userName.text = result.AccountInfo.Username;
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    void OnClosePressed()
    {
        panel.SetActive(false);
    }
}

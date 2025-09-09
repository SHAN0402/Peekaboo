using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.Networking;

public class AvatarManager : MonoBehaviour
{
    [SerializeField] Button selectButton;
    [SerializeField] GameObject previewPanel;
    // [SerializeField] Button a0Button;
    [SerializeField] Button a1Button;
    [SerializeField] Button a2Button;
    [SerializeField] Button a3Button;
    // [SerializeField] Button closeButton;
    [SerializeField] Image avatarImage;

    private readonly string[] avatarUrls = new string[] {
        /*
        "https://raw.githubusercontent.com/ArkeanEon/Peekaboo/main/Avatars/Test/Green.png",
        "https://raw.githubusercontent.com/ArkeanEon/Peekaboo/main/Avatars/Test/Red.png",
        "https://raw.githubusercontent.com/ArkeanEon/Peekaboo/main/Avatars/Test/Blue.png",
        "https://raw.githubusercontent.com/ArkeanEon/Peekaboo/main/Avatars/Test/Yellow.png"
        */

        "https://raw.githubusercontent.com/ArkeanEon/Peekaboo/main/Avatars/Player/default_icon.png",
        "https://raw.githubusercontent.com/ArkeanEon/Peekaboo/main/Avatars/Player/ma_icon.png",
        "https://raw.githubusercontent.com/ArkeanEon/Peekaboo/main/Avatars/Player/tu_icon.png",
        "https://raw.githubusercontent.com/ArkeanEon/Peekaboo/main/Avatars/Player/yang_icon.png"
    };


    // Start is called before the first frame update
    void Start()
    {
        GetAvatar();
        previewPanel.SetActive(false);
        // a0Button.onClick.AddListener(() => OnAvatarSelected(avatarUrls[0]));
        a1Button.onClick.AddListener(() => OnAvatarSelected(avatarUrls[1]));
        a2Button.onClick.AddListener(() => OnAvatarSelected(avatarUrls[2]));
        a3Button.onClick.AddListener(() => OnAvatarSelected(avatarUrls[3]));
        selectButton.onClick.AddListener(OnSelectPressed);
        // closeButton.onClick.AddListener(OnClosePressed);
    }

    void GetAvatar()
    {
        PlayFabClientAPI.GetPlayerProfile(
            new GetPlayerProfileRequest
            {
                PlayFabId = PlayFabSettings.staticPlayer.PlayFabId,
                ProfileConstraints = new PlayerProfileViewConstraints {
                ShowDisplayName = true,
                ShowAvatarUrl = true
                }
            },
            result =>
            {
                var url = result.PlayerProfile.AvatarUrl;
                if (string.IsNullOrEmpty(url))
                    OnAvatarSelected(avatarUrls[0]);
                else    
                    StartCoroutine(PlayFabData.LoadAvatarImage(url, avatarImage));
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    public void OnAvatarSelected(string url)
    {
        PlayFabClientAPI.UpdateAvatarUrl(new UpdateAvatarUrlRequest
        {
            ImageUrl = url
        },
            result =>
            {
                Debug.Log("Avatar update successful: " + url);

                // Download and display
                StartCoroutine(PlayFabData.LoadAvatarImage(url, avatarImage));
                previewPanel.SetActive(false);
            },
            error =>
            {
                Debug.LogError("Avatar update failed: " + error.GenerateErrorReport());
            }
        );
    }

    /* IEnumerator LoadImage(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D tex = DownloadHandlerTexture.GetContent(www);
            Sprite spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
            avatarImage.sprite = spr;
        }
        else
        {
            Debug.LogError("Loading avatar failed: " + www.error);
        }
    } */

    public void OnSelectPressed()
    {
        previewPanel.SetActive(true);
    }

    public void OnClosePressed()
    {
        previewPanel.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;

public class FrameManager : MonoBehaviour
{
    // [SerializeField] Image avatarImage;
    [SerializeField] Image frameImage;
    [SerializeField] Button frameBtn;

    int currentFrameId;

    public static FrameManager Instance { get; private set; }

     private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        frameBtn.onClick.AddListener(OnFramePressed);
        LoadFrame();

    }

    public void LoadFrame()
    {
        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest(),
            result =>
            {
                string frameId = result.Data.TryGetValue("frameId", out var frame) ? frame.Value : null;
                if (int.TryParse(frameId, out int id) && id != -1)
                {
                    frameImage.gameObject.SetActive(true);
                    PlayFabData.LoadFrameFromResources(id, frameImage);
                    currentFrameId = id;
                }
            },
            error => Debug.LogError("Get user data failed: " + error.GenerateErrorReport())
        );
    }

    // Test
    void OnFramePressed()
    {
        currentFrameId = (currentFrameId + 1) % 7;
        PlayFabData.LoadFrameFromResources(currentFrameId, frameImage);
        PlayFabData.SaveFrame(currentFrameId);
    }
}

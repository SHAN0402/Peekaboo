using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;

public class OutfitManager : MonoBehaviour
{
    // [SerializeField] Image avatarImage;
    [SerializeField] Image outfitImage;
    [SerializeField] Button outfitBtn;

    int currentOutfitId;

     public static OutfitManager Instance { get; private set; }

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
        outfitBtn.onClick.AddListener(OnOutfitPressed);
        LoadOutfit();
        
    }

    public void LoadOutfit()
    {
        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest(),
            result =>
            {
                string outfitId = result.Data.TryGetValue("outfitId", out var outfit) ? outfit.Value : null;
                if (int.TryParse(outfitId, out int id) && id != -1)
                {
                    outfitImage.gameObject.SetActive(true);
                    PlayFabData.LoadOutfitFromResources(id, outfitImage);
                    currentOutfitId = id;
                }    
            },
            error => Debug.LogError("Get user data failed: " + error.GenerateErrorReport())
        );
    }

    // Test
    void OnOutfitPressed()
    {
        currentOutfitId = (currentOutfitId + 1) % 7;
        PlayFabData.LoadOutfitFromResources(currentOutfitId, outfitImage);
        PlayFabData.SaveOutfit(currentOutfitId);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

[System.Serializable]
public class OutfitWrapper { public List<int> Outfits = new List<int>(); }

[System.Serializable]
public class FrameWrapper { public List<int> Frames = new List<int>(); }

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private List<int> ownedOutfits = new List<int>();
    private List<int> ownedFrames = new List<int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadInventoryFromPlayFab()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            if (result.Data.TryGetValue("OwnedOutfits", out var outfitData))
            {
                ownedOutfits = JsonUtility.FromJson<OutfitWrapper>($"{{\"Outfits\":{outfitData.Value}}}")?.Outfits ?? new List<int>();
            }

            if (result.Data.TryGetValue("OwnedFrames", out var frameData))
            {
                ownedFrames = JsonUtility.FromJson<FrameWrapper>($"{{\"Frames\":{frameData.Value}}}")?.Frames ?? new List<int>();
            }

            Debug.Log("Inventory loaded.");
        },
        error => Debug.LogWarning("LoadInventoryFromPlayFab failed: " + error.GenerateErrorReport()));
    }

    public void AddOutfit(int newOutfitId, List<int> currentOwnedOutfits)
    {
        if (!currentOwnedOutfits.Contains(newOutfitId))
            currentOwnedOutfits.Add(newOutfitId);

        var wrapper = new OutfitWrapper { Outfits = currentOwnedOutfits };
        string json = JsonUtility.ToJson(wrapper);

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> {
                { "OwnedOutfits", json }
            }
        }, 
        result => Debug.Log("Outfit updated"), 
        error => Debug.LogWarning(error.GenerateErrorReport()));
    }

    public void AddFrame(int newFrameId, List<int> currentOwnedFrames)
    {
        if (!currentOwnedFrames.Contains(newFrameId))
            currentOwnedFrames.Add(newFrameId);

        var wrapper = new FrameWrapper { Frames = currentOwnedFrames };
        string json = JsonUtility.ToJson(wrapper);

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> {
                { "OwnedFrames", json }
            }
        }, 
        result => Debug.Log("Frame updated"), 
        error => Debug.LogWarning(error.GenerateErrorReport()));
    }

    public bool HasOutfit(int id)
    {
        return ownedOutfits.Contains(id);
    }

    public bool HasFrame(int id)
    {
        return ownedFrames.Contains(id);
    }

    public List<int> GetAllOwnedOutfits()
    {
        return new List<int>(ownedOutfits);
    }

    public List<int> GetAllOwnedFrames()
    {
        return new List<int>(ownedFrames);
    }
}

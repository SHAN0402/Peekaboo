using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CommunicationManager : MonoBehaviour
{
    [SerializeField] TMP_InputField valueName;
    [SerializeField] TMP_InputField value;
    [SerializeField] GameObject rowPrefab;
    [SerializeField] Transform contentParent;

    // Start is called before the first frame update
    void Start()
    {
        PlayFabData.GetData(GotDataSuccess, PlayFabFailure);
        // PlayFabData.SendLeaderboard(13);
        // PlayFabData.GetLeaderboard();
    }

    private void GotDataSuccess(GetUserDataResult result)
    {
        // Displays stored data
        foreach (var item in result.Data)
        {
            // Add rows to data panel
            GameObject row = Instantiate(rowPrefab, contentParent);
            RowUI rowUI = row.GetComponent<RowUI>();
            rowUI.SetData(item.Key, item.Value.Value);
        }
    }

    private void PlayFabFailure(PlayFabError error)
    {
        Debug.Log(error.Error + " : " + error.GenerateErrorReport());
    }

    // Locally add data
    public void OnAddPressed()
    {
        PlayFabData.UpdateData(new Dictionary<string, string>()
        {
            {valueName.text, value.text}
        },
        successResult =>
        {
            PlayFabData.GetData(
                successResult =>
                {
                    GameObject row = Instantiate(rowPrefab, contentParent);
                    RowUI rowUI = row.GetComponent<RowUI>();
                    rowUI.SetData(valueName.text, value.text);
                    valueName.text = "";
                    value.text = "";
                },
                PlayFabFailure);
            Debug.Log("Update successful.");
        },
        PlayFabFailure);
    }

    public void OnModifyPressed()
    {
        string key = valueName.text.Trim();
        string newValue = value.text.Trim();

        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("Key is empty – nothing to modify.");
            return;
        }

        // Update PlayFab
        PlayFabData.UpdateData(
            new Dictionary<string, string> { { key, newValue } },
            _ =>
            {
                // Update local UI
                foreach (Transform child in contentParent)
                {
                    var row = child.GetComponent<RowUI>();
                    if (row != null && row.Key == key)
                    {
                        row.UpdateValue(newValue);
                        break;
                    }
                }

                valueName.text = "";
                value.text = "";
                Debug.Log($"Modified '{key}' → '{newValue}'");
            },
            PlayFabFailure);

    }

    public void OnFriendPressed()
    {
        SceneManager.LoadScene("FriendScene");
    }

    public void OnProfilePressed()
    {
        SceneManager.LoadScene("ProfileScene");
    }

    public void OnLeaderboardPressed()
    {
        SceneManager.LoadScene("LeaderboardScene");
    }
}

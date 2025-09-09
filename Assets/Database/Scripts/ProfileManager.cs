using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    [Header("Display name")]
    [SerializeField] TMP_Text nameText;
    
    [Header("Change name")]
    // [SerializeField] GameObject nameInputPanel;
    [SerializeField] TMP_InputField nameInput;
    // [SerializeField] Button confirmButton;
    // [SerializeField] Button changeButton;

    [Header("Utils")]
    [SerializeField] GameObject profilePanel;
    [SerializeField] Button logOutButton;
    [SerializeField] Button exitButton;



    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Current ID: " + PlayerPrefs.GetString("PLAYFAB_ID"));
        nameInput.gameObject.SetActive(false);

        // changeButton.onClick.AddListener(OnChangePressed);
        nameText.GetComponent<Button>().onClick.AddListener(SwitchToNameInput);
        nameInput.onSubmit.AddListener(_ => UpdateName());
        logOutButton.onClick.AddListener(OnLogOutPressed);
        exitButton.onClick.AddListener(OnExitPressed);
        DisplayName();
    }

    #region Name
    void DisplayName()
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(),
            result => {
                string display = result.AccountInfo.TitleInfo.DisplayName;
                nameText.text = string.IsNullOrEmpty(display) ? "Default" : display;
            },
            error => {
                Debug.LogError(error.GenerateErrorReport());
            }
        );
    }

    void SwitchToNameInput()
    {
        nameInput.text = nameText.text;
        // nameInput.text = "";
        nameText.gameObject.SetActive(false);
        nameInput.gameObject.SetActive(true);
        nameInput.ActivateInputField();
    }

    void OnExitPressed()
    {
        profilePanel.SetActive(false);
    }

    void UpdateName()
    {
        string newName = nameInput.text.Trim();

        if (string.IsNullOrEmpty(newName))
        {
            Debug.Log("Name cannot be empty.");
            return;
        }

        if (newName.Length > 20)
        {
            Debug.Log("Name cannot exceed 20 chars.");
            return;
        }

        PlayFabClientAPI.UpdateUserTitleDisplayName(
            new UpdateUserTitleDisplayNameRequest { DisplayName = newName },
            result =>
            {
                Debug.Log("Successful! New name: " + result.DisplayName);
                DisplayName();
                nameInput.gameObject.SetActive(false);
                nameText.gameObject.SetActive(true);
            },
            error =>
            {
                Debug.Log("Failed! " + error.ErrorMessage);
            });
    }
    #endregion Name

    void OnLogOutPressed()
    {
        PlayerPrefs.DeleteKey("PLAYFAB_ID");
        PlayerPrefs.DeleteKey("LAST_USERNAME");
        PlayerPrefs.DeleteKey("LAST_PASSWORD");
        PlayerPrefs.Save();

        PlayFabData.Reset();
        SceneManager.LoadScene("LoginScene");
    }
}

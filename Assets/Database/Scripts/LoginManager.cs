using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;



public class LoginManager : MonoBehaviour
{
    const string LAST_USERNAME_KEY = "LAST_USERNAME", LAST_PASSWORD_KEY = "LAST_PASSWORD",
    PLAYFAB_ID_KEY = "PLAYFAB_ID";

    void Start()
    {
        // registerPanel.SetActive(false);
    }

    #region Register
    [Header("Register UI")]
    [SerializeField] TMP_InputField registerEmail;
    [SerializeField] TMP_InputField registerUsername;
    [SerializeField] TMP_InputField registerPassword;
    [SerializeField] GameObject loginPanel;

    public void ToLoginPanel()
    {
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
    }

    public void OnRegisterPressed()
    {
        Register(registerEmail.text, registerUsername.text, registerPassword.text);
    }

    private void Register(string email, string username, string password)
    {
        PlayFabClientAPI.RegisterPlayFabUser(new RegisterPlayFabUserRequest()
        {
            Email = email,
            Username = username,
            DisplayName = username,
            Password = password,
            RequireBothUsernameAndEmail = true
        },
        successResult =>
        {
            // Initialize user data
            PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    // Win rate locally calculate
                    { "OutfitId", "-1" },
                    { "FrameId", "-1" },
                    { "WinCount", "0" },
                    { "TotalGames", "0" },
                    { "Exp", "0" },
                    { "Coins", "0" } ,
                    { "OwnedOutfits", JsonUtility.ToJson(new OutfitWrapper()) },
                    { "OwnedFrames", JsonUtility.ToJson(new FrameWrapper()) }
                }
            },
            _ =>
            {
                Debug.Log("User default data initialized.");
                Login(username, password);
            },
            PlayFabFailure);
        },
        PlayFabFailure);
    }
    #endregion Register

    #region Login
    [Header("Login UI")]
    [SerializeField] TMP_InputField loginUsername;
    [SerializeField] TMP_InputField loginPassword;
    [SerializeField] GameObject registerPanel;

    public void ToRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
    }

    public void OnLoginPressed()
    {
        Login(loginUsername.text, loginPassword.text);
    }

    private void Login(string username, string password)
    {
        PlayFabClientAPI.LoginWithPlayFab(new LoginWithPlayFabRequest()
        {
            Username = username,
            Password = password,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams()
            {
                GetPlayerProfile = true
            }
        },
        result =>
        {
            PlayerPrefs.SetString(PLAYFAB_ID_KEY, result.PlayFabId);
            PlayerPrefs.SetString(LAST_USERNAME_KEY, username);
            PlayerPrefs.SetString(LAST_PASSWORD_KEY, password);

            SceneManager.LoadScene("Main");
            Debug.Log("Login successful!, id = " + PlayerPrefs.GetString(PLAYFAB_ID_KEY));
        }
        ,
        PlayFabFailure);
    }
    #endregion Login

    // Unused
    #region CustomID
    public void OnCustomIDPressed()
    {
        LoginWithID();
    }
    private void LoginWithID()
    {
        PlayFabClientAPI.LoginWithCustomID(
            new LoginWithCustomIDRequest()
            {
                CustomId = SystemInfo.deviceUniqueIdentifier,
                CreateAccount = true
            },
            successResult =>
            {
                SceneManager.LoadScene("DataScene");
                Debug.Log(successResult.PlayFabId);
                Debug.Log("Login anonymously successful!");
            },
            PlayFabFailure
        );
    }

    private void ShowTutorial()
    {
        Debug.Log("Tutorial begins");
    }

    #endregion CustomID
    private void PlayFabFailure(PlayFabError error)
    {
        Debug.Log(error.Error + " : " + error.GenerateErrorReport());
    }
}

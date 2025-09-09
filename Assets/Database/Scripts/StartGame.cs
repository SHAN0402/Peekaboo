using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class StartGame : MonoBehaviour
{
    [SerializeField] Button LoginBtn;
    [SerializeField] GameObject LoginPanel;
    [SerializeField] GameObject RegisterPanel;
    public void OnLoginPressed()
    {
        // More structured approach: use BootScene and refer to LoginManager
        if (PlayerPrefs.HasKey("LAST_USERNAME") && PlayerPrefs.HasKey("LAST_PASSWORD"))
        {
            string savedUsername = PlayerPrefs.GetString("LAST_USERNAME");
            string savedPassword = PlayerPrefs.GetString("LAST_PASSWORD");

            PlayFabClientAPI.LoginWithPlayFab(new LoginWithPlayFabRequest()
            {
                Username = savedUsername,
                Password = savedPassword,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams()
                {
                    GetPlayerProfile = true
                }
            },
        result =>
        {
            SceneManager.LoadScene("Main");
            Debug.Log("Login successful!, id = " + PlayerPrefs.GetString("PLAYFAB_ID"));
        }
        ,
        error => Debug.Log(error.Error + " : " + error.GenerateErrorReport())
        );
        }
        else
        {
            LoginPanel.SetActive(true);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        LoginPanel.SetActive(false);
        RegisterPanel.SetActive(false);
        LoginBtn.onClick.AddListener(OnLoginPressed);
    }
}

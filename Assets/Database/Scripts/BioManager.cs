using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using System.Data.Common;

public class BioManager : MonoBehaviour
{
    public TMP_Text bioText;
    public TMP_InputField bioInput;
    // public Button confirmButton;

    private const string BIO_KEY = "Biography";

    // Start is called before the first frame update
    void Start()
    {
        bioInput.gameObject.SetActive(false);
        GetBio();

        bioText.GetComponent<Button>().onClick.AddListener(SwitchToInput);
        bioInput.onSubmit.AddListener(_ => UpdateBio());
    }

    void SwitchToInput()
    {
        bioInput.text = bioText.text;
        bioText.gameObject.SetActive(false);
        bioInput.gameObject.SetActive(true);
        bioInput.ActivateInputField();
    }

    void UpdateBio()
    {
        string newBio = bioInput.text.Trim();

        if (newBio.Length > 50)
        {
            Debug.Log("Bio cannot exceed 50 chars");
            return;
        }

        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest()
            {
                Data = new Dictionary<string, string> {
                    { BIO_KEY, newBio}
                },
                Permission = UserDataPermission.Public
            },
            result =>
            {
                GetBio();
                bioInput.gameObject.SetActive(false);
                bioText.gameObject.SetActive(true);
            },
            error => PlayFabFailure(error)
        );
    }

    void GetBio()
    {
        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest(),
            result =>
            {
                if (result.Data != null && result.Data.ContainsKey(BIO_KEY))
                {
                    bioText.text = result.Data[BIO_KEY].Value;
                }
            },
            error => PlayFabFailure(error)
        );
    }

    void PlayFabFailure(PlayFabError error)
    {
        Debug.Log(error.Error + " : " + error.GenerateErrorReport());
    }
}
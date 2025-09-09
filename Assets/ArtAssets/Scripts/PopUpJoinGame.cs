using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopUpJoinGame : MonoBehaviour
{
    [Header("Elements")]
    public GameObject popupPanel;
    public TMP_InputField codeInput;
    public Button joinButton;
    public Button cancelButton;

    void Start()
    {
        popupPanel.SetActive(false);

        //joinButton.onClick.AddListener(OnJoinClicked);
        //cancelButton.onClick.AddListener(HidePopup);
    }

    public void ShowPopup()
    {
        popupPanel.SetActive(true);
    }

    public void HidePopup()
    {
        popupPanel.SetActive(false);
    }

    void OnJoinClicked()
    {
        string roomCode = codeInput.text;

        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogWarning("Please enter room code!");
            return;
        }

        //TODO 等zb的服务器接口
    }
}

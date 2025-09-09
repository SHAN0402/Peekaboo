using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InitRoomManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Button joinBtn;
    [SerializeField] private Button hostBtn;
    // [SerializeField] private GameObject lobbyCanvasRoot;
    // [SerializeField] private Button playBtn;

    void Start()
    {
        loadingPanel.SetActive(false);
        RoomManager.Instance.BindUI(codeInput, joinBtn, hostBtn, loadingPanel);
    }


}
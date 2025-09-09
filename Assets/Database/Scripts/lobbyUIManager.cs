using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;
using System.Linq;
using System;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    [Header("UI 元素")]
    public TMP_Text roomCodeText;      // 房间号显示
    public TMP_Text playerCountText;   // 玩家数显示

    private NetworkRunner runner;

    void Start()
    {
        runner = RoomManager.Instance.Runner;
        // 初始显示一次（有可能 Start 就有值）
        RefreshUI();
    }

    void Update()
    {
        RefreshUI();
    }

    void RefreshUI()
    {
        // 房间号
        if (roomCodeText != null)
            roomCodeText.text = $"{RoomManager.Instance.RoomCode}";

        // 玩家数量
        if (runner != null && runner.IsRunning && playerCountText != null)
        {
            int count = runner.ActivePlayers.Count();
            playerCountText.text = $"Players: {count}/9";
        }
    }
}

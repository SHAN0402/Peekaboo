using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] TMP_Text rank;
    [SerializeField] TMP_Text userName;
    [SerializeField] TMP_Text userScore;

    public void SetData(int rankIndex, string name, string score)
    {
        rank.text = rankIndex.ToString();
        userName.text = name;
        userScore.text = score;
    }
}

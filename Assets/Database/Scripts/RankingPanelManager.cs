using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankingPanelManager : MonoBehaviour
{
    public GameObject panelWins;
    public GameObject panelWinrate;
    public GameObject panelScore;

    void Start()
    {
        ShowPanel("winrate");  //Default winrate
    }

    public void ShowPanel(string type)
    {
        panelWins.SetActive(type == "wins");
        panelWinrate.SetActive(type == "winrate");
        panelScore.SetActive(type == "score");
    }
}

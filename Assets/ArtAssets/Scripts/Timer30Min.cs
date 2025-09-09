using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer30Min : MonoBehaviour
{
    public TMP_Text timerText;
    //public int totalTimeSeconds = 1800;
    public GameObject timeOutPopUp;

    private float timeLeft;

    void Start()
    {
        //timeLeft = totalTimeSeconds;
    }

    void Update()
    {
        if (GameManager.Instance != null)
        {
            float remain = GameManager.Instance.gameDurationLimit - GameManager.Instance.gameTimer;

            if (remain > 0)
            {
                int minutes = Mathf.FloorToInt(remain / 60);
                int seconds = Mathf.FloorToInt(remain % 60);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
            else
            {
                timerText.text = "00:00";
                if (timeOutPopUp != null)
                {
                    timeOutPopUp.SetActive(true);
                }
            }
        }
    }
}

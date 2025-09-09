using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopUpEasy : MonoBehaviour
{
    public GameObject popupPanel;
   

    void Start()
    {
        popupPanel.SetActive(false);

    }

    public void ShowPopup()
    {
        popupPanel.SetActive(true);
    }

    public void HidePopup()
    {
        popupPanel.SetActive(false);
    }

}
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultUIBinder : MonoBehaviour
{
    public TMP_Text roleText;
    public TMP_Text expText;
    public TMP_Text coinText;
    public TMP_Text displayText;
    public Button homeButton;
    public GameObject panel => gameObject;

    void Start()
    {
        gameObject.SetActive(false);
    }

}

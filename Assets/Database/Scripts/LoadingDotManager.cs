using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingDotManager : MonoBehaviour
{
    public TextMeshProUGUI loadingText;
    public CanvasGroup canvasGroup;
    public float interval = 0.5f;
    public float PageDauer = 2f;
    public float fadeSpeed = 1f;

    private float timer;
    private int dotCount = 0;
    private readonly string baseText = "LOADING";

    //private bool fadingIn = true;
    private bool fadingOut = false;

    void Start()
    {
        //canvasGroup.alpha = 0f;
        //fadingIn = true;

        //FadOut in 2s
        Invoke("StartFadeOut", PageDauer);

    }

    void Update()
    {
        //Dots Anima ^^
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            dotCount = (dotCount + 1) % 4;
            loadingText.text = baseText + new string('.', dotCount);
            timer = 0f;
        }

        // FadeIn
        /*if (fadingIn)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            if (canvasGroup.alpha >= 1f)
            {
                canvasGroup.alpha = 1f;
                fadingIn = false;
            }
        }*/

        //FadeOUt
        if (fadingOut)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            if (canvasGroup.alpha <= 0f)
            {
                canvasGroup.alpha = 0f;
                fadingOut = false;
                gameObject.SetActive(false);
            }
        }
    }

    public void StartFadeOut()
    {
        fadingOut = true;
    }
}


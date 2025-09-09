using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class PlayClickSoundOnPress : MonoBehaviour
{

    private Button btn;

    void OnEnable()
    {
        btn = GetComponent<Button>();
        StartCoroutine(DelayBind());


    }


    IEnumerator DelayBind()
    {
        yield return new WaitForSeconds(0.05f);
        if(btn != null)
        {
            btn.onClick.RemoveListener(PlaySound);
            btn.onClick.AddListener(PlaySound);
        }
    }

    void PlaySound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }
        else
        {
            Debug.LogWarning("AudioManager not found!");
        }
    }
}

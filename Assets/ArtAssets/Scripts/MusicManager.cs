using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    public AudioClip bgmA; //Main&all other scenes
    public AudioClip bgmB; // MapInGame

    public AudioSource audioSource;
    public float fadeDuration = 1.5f;
    private int lastSceneIndex = -1;

    void Awake()
    {
        // Instance check
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        int mapSceneIndex = 8;

        //no Chongfu
        if (lastSceneIndex == scene.buildIndex)
            return;

        lastSceneIndex = scene.buildIndex;

        if (scene.buildIndex == mapSceneIndex)
        {
            PlayBGM(bgmB);
        }
        else
        {
            PlayBGM(bgmA);
        }
    }

    void PlayBGM(AudioClip clip)
    {
        if (audioSource.clip == clip && audioSource.isPlaying)
            return;
        audioSource.clip = clip;
        audioSource.Play();
    }

    public IEnumerator FadeIn(AudioClip newClip)
    {
        audioSource.clip = newClip;
        audioSource.volume = 0f;
        audioSource.Play();

        float timer = 0f;
        while (timer < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = 1f;
    }

    public IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        float timer = 0f;
        while (timer < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = 1f;
    }
    
    public void CrossFadeTo(AudioClip newClip)
    {
        StartCoroutine(CrossFadeCoroutine(newClip));
    }
    private IEnumerator CrossFadeCoroutine(AudioClip newClip)
    {
        yield return FadeOut();
        yield return FadeIn(newClip);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGM_Manager : MonoBehaviour
{
    public static BGM_Manager Instance;

    private AudioSource musicSource;

    // default audio for all Scene out of a game --City Beats
    [Header("Default BGM (optional)")]
    public AudioClip defaultBGM;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        //dont change
        DontDestroyOnLoad(gameObject);

        musicSource = GetComponent<AudioSource>();
        if (!musicSource)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        //play default audio
        if (defaultBGM != null)
        {
            PlayBGM(defaultBGM);
        }
    }

    //avoid redudanz
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    // incase change bgm
    public void ChangeBGM(AudioClip newClip, bool loop = true)
    {
        PlayBGM(newClip, loop);
    }

    public void StopBGM()
    {
        musicSource.Stop();
    }


}

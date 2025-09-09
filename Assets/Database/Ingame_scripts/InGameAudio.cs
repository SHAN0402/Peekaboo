using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameAudio : MonoBehaviour
{
    public static InGameAudio Instance;

    public AudioSource audioSource;
    public AudioClip ghostRoleClip;
    public AudioClip bombRoleClip;
    public AudioClip survivorRoleClip;


    private void Awake()
    {
        Instance = this;
    }

    public void PlayOneShot(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip);
    }
}

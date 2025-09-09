using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class SideButtonManager : MonoBehaviour
{
    public GameObject sideButtonServer;
    public GameObject sideButtonClient;
    public NetworkRunner runner;

    void Start()
    {
        StartCoroutine(DelayedCheckHost());
    }

    private IEnumerator DelayedCheckHost()
    {
        yield return new WaitForSeconds(0.3f);

        if (runner != null)
        {
            if (runner.IsServer)
            {
                sideButtonServer.SetActive(true);
                sideButtonClient.SetActive(false);
            }
            else
            {
                sideButtonServer.SetActive(false);
                sideButtonClient.SetActive(true);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ToMap : MonoBehaviour
{
    [SerializeField] Button mapBtn;

    // Start is called before the first frame update
    void Start()
    {
        mapBtn.onClick.AddListener(OnToMapClicked);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnToMapClicked() {
        SceneManager.LoadScene("MapInGame");
    }
}

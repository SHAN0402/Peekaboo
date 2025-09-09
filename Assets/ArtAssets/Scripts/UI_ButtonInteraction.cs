using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;




public class UI_ButtonInteraction : MonoBehaviour
{
    public static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public void RotateButton()
    {
        Vector3 currentRotation = transform.localEulerAngles;
        currentRotation.z = (currentRotation.z + 180f) % 360f;
        transform.localEulerAngles = currentRotation;
    }

    public void TestButtonClick()
    {
        Debug.Log("Button clicked!");
    }

}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingPageController : MonoBehaviour
{
    public GameObject loadingPage;
    public string sceneToLoad;

    public void StartLoading()
    {
        loadingPage.SetActive(true);
        StartCoroutine(LoadSceneDirectly());
    }

    IEnumerator LoadSceneDirectly()
    {
        // 强制等一帧，防止 UI 渲染失败
        yield return null;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncLoad.allowSceneActivation = false;

        // 等加载到 90%
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // 再等一秒保险点（可选）
        yield return new WaitForSeconds(1f);

        asyncLoad.allowSceneActivation = true;
    }
}

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class ServerCommunicator : MonoBehaviour
{
    public void SendPlayFabIdToServer(string playFabId)
    {
        StartCoroutine(SendRequestCoroutine(playFabId));
    }

    IEnumerator SendRequestCoroutine(string playFabId)
    {
        string json = JsonUtility.ToJson(new Payload { playFabId = playFabId });

        using (UnityWebRequest request = new UnityWebRequest("https://yourserver.com/api/updateData", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Successfully sent PlayFabId to server.");
            }
            else
            {
                Debug.LogError("Error sending PlayFabId: " + request.error);
            }
        }
    }

    [System.Serializable]
    private class Payload
    {
        public string playFabId;
    }
}

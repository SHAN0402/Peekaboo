using System.Collections;
using System.Collections.Generic;
using PlayFab;
using TMPro;
using UnityEngine;
using PlayFab.ClientModels;

public class UserDataManager : MonoBehaviour
{
    public TMP_Text goldText;
    public TMP_Text levelText;

    // Start is called before the first frame update
    void Start()
    {
        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest(),
            OnUserDataSuccess,
            err => Debug.LogError("Failed getting userdata" + err.GenerateErrorReport())
            );
    }

    void OnUserDataSuccess(GetUserDataResult result)
    {
        if (result.Data != null)
        {
            if (result.Data.TryGetValue("Coins", out var gold))
            {
                goldText.text = "GOLD: " + gold.Value;
            }
            else
            {
                goldText.text = "GOLD: -1";
            }

            if (result.Data.TryGetValue("Exp", out var expEntry))
            {
                if (int.TryParse(expEntry.Value, out int exp))
                {
                    int level = exp / 500;
                    levelText.text = "LEVEL: " + level.ToString();
                }
                else
                {
                    Debug.LogWarning("[UserDataDisplay] Exp 数据格式错误: " + expEntry.Value);
                    levelText.text = "LEVEL: ?";
                }
            }
            else
            {
                levelText.text = "LEVEL: -1";
            }

            Debug.Log("[UserDataDisplay] 成功读取用户数据");
        }
        else
        {
            Debug.LogWarning("[UserDataDisplay] 用户数据为空");
        }
    }
}

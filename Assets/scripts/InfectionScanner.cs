using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class InfectionScanner : NetworkBehaviour
{
    public PlayerController self;

    [Header("感染判定参数")]
    public float infectionRange = 0.000435f;
    public float ghostContactTime = 5f;
    public float wandererContactTime = 10f;

    private Dictionary<PlayerRef, float> contactTimers = new Dictionary<PlayerRef, float>();
private void Awake()
{
    Debug.LogWarning("[InfectionScanner] Awake 被调用");
}

    public override void Spawned()
    {   
        if (self == null)
        {
            self = GetComponent<PlayerController>();
            Debug.LogWarning("[InfectionScanner] 自动绑定 self 成功：" + self?.playerName);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || self == null || !self.canInfectOthers || Runner == null)
        {
            Debug.LogWarning("[InfectionScanner] 条件不满足，不执行感染逻辑");
            return;
        }

        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        Vector2 selfGPS = self.GetGPSPosition();
        Debug.Log($"[{self.playerName}] 我的位置是 {selfGPS}");

        foreach (PlayerController other in allPlayers)
        {
            if (other == self || !other.isInfectable) continue;

            float distance = Vector2.Distance(selfGPS, other.GetGPSPosition());
            Debug.Log($"[InfectionScanner] {self.playerName} → {other.playerName} 距离 = {distance}");

            PlayerRef otherRef = other.Object.InputAuthority;

            if (GameManager.Instance.isHidePhase)
            {
                Debug.Log("躲藏阶段，不感染！");
                return;
            }

            if (distance <= infectionRange)
            {
                Debug.Log($"[InfectionScanner]  {other.playerName} 在感染范围内");

                if (!contactTimers.ContainsKey(otherRef))
                    contactTimers[otherRef] = 0f;

                contactTimers[otherRef] += Runner.DeltaTime;

                float requiredTime = self.isOriginalGhost ? ghostContactTime : wandererContactTime;

                if (contactTimers[otherRef] >= requiredTime)
                {
                    Debug.Log($"[InfectionScanner] 满足感染时间，发送感染 RPC：{other.playerName}");
                    other.RpcStartInfectionProcess(requiredTime);
                    contactTimers.Remove(otherRef);
                }
            }
            else
            {
                if (contactTimers.ContainsKey(otherRef))
                {
                    Debug.Log($"[InfectionScanner] 离开感染范围，清除计时：{other.playerName}");
                    contactTimers.Remove(otherRef);
                }
            }
        }
    }
}

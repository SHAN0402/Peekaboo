using UnityEngine;
using System;

/*public enum Camp
{
    Survivor,
    Ghost,
    BombExpert
}*/

public enum SurvivorType
{
    Survivor1,
    Survivor2
}

public class PlayerIdentity : MonoBehaviour
{
    public Camp camp = Camp.Survivor;
    
    [HideInInspector]
    public SurvivorType survivorType = SurvivorType.Survivor1;

    public event Action<Camp, SurvivorType> OnIdentityChanged;

    private Camp lastCamp;
    private SurvivorType lastType;

    void Start()
    {
        // 初始化缓存
        lastCamp = camp;
        lastType = survivorType;

        // 运行时先触发一次，确保正确显示
        OnIdentityChanged?.Invoke(camp, survivorType);
    }

    void Update()
    {
        if (camp != lastCamp || survivorType != lastType)
        {
            lastCamp = camp;
            lastType = survivorType;
            Debug.Log($"[PlayerIdentity] Inspector修改，触发事件: {camp} / {survivorType}");
            OnIdentityChanged?.Invoke(camp, survivorType);
        }
    }

    public void Initialize(Camp initialCamp, SurvivorType initialSurvivorType)
    {
        camp = initialCamp;
        survivorType = initialSurvivorType;
        lastCamp = camp;
        lastType = survivorType;
        OnIdentityChanged?.Invoke(camp, survivorType);
    }

    public void ChangeCamp(Camp newCamp, SurvivorType newType = SurvivorType.Survivor1)
    {
        camp = newCamp;
        survivorType = newType;
        lastCamp = camp;
        lastType = survivorType;
        OnIdentityChanged?.Invoke(camp, survivorType);
    }
}

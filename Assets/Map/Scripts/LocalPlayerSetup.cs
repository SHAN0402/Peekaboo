using UnityEngine;

public class LocalPlayerSetup : MonoBehaviour
{
    public MarkerMover mover;
    public MarkerIconSwitcher iconSwitcher;
    public PlayerIdentity playerIdentity;

    public Camp initialCamp = Camp.Survivor;

    void Start()
    {
        SurvivorType randomType = SurvivorType.Survivor1;

        // 如果是生存者就随机
        if (initialCamp == Camp.Survivor)
        {
            int r = Random.Range(0, 2); // 0~2
            randomType = (SurvivorType)r;
            Debug.Log($"随机生存者类型：{randomType}");
        }

        // 初始化身份
        playerIdentity.Initialize(initialCamp, randomType);

        // 根据身份选贴图
        Texture2D initialTexture = iconSwitcher.GetTexture(initialCamp, randomType);

        // 创建Marker
        mover.CreateMarker(initialTexture);

        // 初始化IconSwitcher
        iconSwitcher.Initialize(mover.marker, playerIdentity);
    }
}

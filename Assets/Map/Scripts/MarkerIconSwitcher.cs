using UnityEngine;

public class MarkerIconSwitcher : MonoBehaviour
{
    public Texture2D[] survivorTextures; // index 0=horse,1=niu,2=tu
    public Texture2D ghostTexture;       // ghostdot
    public Texture2D bombexpertTexture;        // ppdot

    private OnlineMapsMarker marker;

    public void Initialize(OnlineMapsMarker m, PlayerIdentity player)
    {
        marker = m;

        // 初始化时显示
        UpdateIcon(player.camp, player.survivorType);

        // 监听事件
        player.OnIdentityChanged += UpdateIcon;
    }

    private void UpdateIcon(Camp camp, SurvivorType survivorType)
    {
        if (marker == null) return;

        switch (camp)
        {
            case Camp.Survivor:
                marker.texture = survivorTextures[(int)survivorType];
                break;
            case Camp.Ghost:
                marker.texture = ghostTexture;
                break;
            case Camp.BombExpert:
                marker.texture = bombexpertTexture;
                break;
        }

        OnlineMaps.instance.Redraw();
    }
    
    public Texture2D GetTexture(Camp camp, SurvivorType survivorType)
    {
        switch (camp)
        {
            case Camp.Survivor:
                return survivorTextures[(int)survivorType];
            case Camp.Ghost:
                return ghostTexture;
            case Camp.BombExpert:
                return bombexpertTexture;
        }
        return null;
    }

}
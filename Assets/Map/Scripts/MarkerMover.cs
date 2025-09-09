using UnityEngine;

public class MarkerMover : MonoBehaviour
{
    [HideInInspector]
    public OnlineMapsMarker marker;

    public double longitude = 11.669;
    public double latitude = 48.262;
    
    public void CreateMarker(Texture2D initialTexture)
    {
        marker = OnlineMapsMarkerManager.CreateItem(longitude, latitude, initialTexture, "本地玩家");
        marker.scale = 0.05f;
    }

    void Update()
    {
        if (marker == null) return;

        // 简单移动示例
        marker.SetPosition(longitude, latitude);

        OnlineMaps.instance.Redraw();
    }
}
using UnityEngine;

public class MarkerGPSUpdater : MonoBehaviour
{
    private OnlineMapsMarker marker;
    private MarkerMover mover;

    void Start()
    {
        // 自动获取MarkerMover
        mover = GetComponent<MarkerMover>();
        if (mover != null)
        {
            marker = mover.marker;
        }
        else
        {
            Debug.LogError("MarkerMover组件没有挂在Player上！");
        }
    }

    void Update()
    {
        if (marker == null) return;
        if (OnlineMapsLocationService.instance == null) return;

        Vector2 pos = OnlineMapsLocationService.instance.position;
        double lon = pos.x;
        double lat = pos.y;

        marker.SetPosition(lon, lat);
        OnlineMaps.instance.Redraw();
    }
}
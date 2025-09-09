using UnityEngine;
using Fusion;

public class GPSUploader : NetworkBehaviour
{
    [Header("Mock Settings")]
    public bool useMock = true;
    public float mockLatitude = 48.2626f;
    public float mockLongitude = 11.6678f;

    private LocationServiceExt locSvc;
    private float uploadInterval = 1f;
    private float timer = 0f;
    private bool gpsReady = false;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            #if UNITY_EDITOR
                useMock = true;
            #else
                useMock = false;
            #endif               
            // —— 仅在模拟模式下为每个玩家赋不同的 mock 经纬度 ——
            if (useMock)
            {
                float baseLat = 48.2626f;
                float baseLon = 11.6678f;
                float latOffset = Random.Range(-0.001f, 0.001f);
                float lonOffset = Random.Range(-0.001f, 0.001f);
                mockLatitude = baseLat + latOffset;
                mockLongitude = baseLon + lonOffset;

                Debug.Log($"[GPSUploader] 模拟模式，随机偏移：lat={mockLatitude}, lon={mockLongitude}");
            }

            locSvc = new LocationServiceExt(useMock);
            locSvc.isEnabledByUser = true;
            locSvc.Start();

            Debug.Log($"[GPSUploader] Spawned() – useMock={useMock}");
            gpsReady = true;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!gpsReady || !Object.HasInputAuthority)
            return;

        timer += Runner.DeltaTime;
        if (timer < uploadInterval)
            return;

        timer = 0f;

        if (locSvc.status != LocationServiceStatusExt.Running)
        {
            Debug.Log($"[GPSUploader] Location not ready: {locSvc.status}");
            return;
        }

        if (useMock)
        {
            locSvc.lastData = new LocationInfoExt
            {
                latitude = mockLatitude,
                longitude = mockLongitude
            };
        }

        var ld = locSvc.lastData;
        Vector2 pos = new Vector2(ld.latitude, ld.longitude);

        if (pos == Vector2.zero)
        {
            Debug.Log("[GPSUploader] Skipping (0,0)");
            return;
        }

        var pc = GetComponent<PlayerController>();
        if (pc != null && Object.HasInputAuthority)
        {
            pc.UpdateGPS(pos.x, pos.y); // 本地更新
            pc.RpcUpdateGPS(pos.x, pos.y); // 发给 Host 同步

            Debug.Log($"[GPSUploader] GPS synced from InputAuthority: lat={pos.x}, lon={pos.y}");
        }
    }
}

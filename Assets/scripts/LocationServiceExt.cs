using UnityEngine;

public struct LocationInfoExt {
    public float latitude;
    public float longitude;
}

public enum LocationServiceStatusExt { Stopped, Initializing, Running, Failed }

public class LocationServiceExt {
    private bool useMock;
    private LocationInfoExt mockData;
    private LocationServiceStatusExt mockStatus;
    private bool mIsEnabledByUser = false;

    public LocationServiceExt(bool useMock) {
        this.useMock = useMock;
        if (useMock) {
            mIsEnabledByUser = true;
            mockStatus = LocationServiceStatusExt.Running;
        }
    }

    public bool isEnabledByUser {
        get => useMock ? mIsEnabledByUser : Input.location.isEnabledByUser;
        set { if (useMock) mIsEnabledByUser = value; }
    }

    public LocationInfoExt lastData {
        get {
            if (useMock) return mockData;
            var ld = Input.location.lastData;
            return new LocationInfoExt { latitude = ld.latitude, longitude = ld.longitude };
        }
        set { if (useMock) mockData = value; }
    }

    public LocationServiceStatusExt status {
        get {
            if (useMock) return mockStatus;
            switch (Input.location.status) {
                case LocationServiceStatus.Initializing: return LocationServiceStatusExt.Initializing;
                case LocationServiceStatus.Running: return LocationServiceStatusExt.Running;
                case LocationServiceStatus.Failed: return LocationServiceStatusExt.Failed;
                case LocationServiceStatus.Stopped: default: return LocationServiceStatusExt.Stopped;
            }
        }
        set { if (useMock) mockStatus = value; }
    }

    public void Start() {
        if (useMock) {
            mockStatus = LocationServiceStatusExt.Running;
        } else {
            Input.location.Start();
        }
    }

    public void Stop() {
        if (useMock) {
            mockStatus = LocationServiceStatusExt.Stopped;
        } else {
            Input.location.Stop();
        }
    }
}

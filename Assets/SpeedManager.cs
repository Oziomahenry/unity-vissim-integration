using UnityEngine;

public class SpeedManager : MonoBehaviour
{
    public static SpeedManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
    }

    /// <summary>
    /// Set the speed of a single vehicle in VISSIM.
    /// </summary>
    /// <param name="vehicleId">Vehicle ID in VISSIM</param>
    /// <param name="speed">Desired speed (m/s)</param>
    public void SetVehicleSpeed(int vehicleId, float speed)
    {
        if (VissimUdpClient.Instance != null)
        {
            VissimUdpClient.Instance.SetVehicleSpeed(vehicleId, speed);
            Debug.Log($"SpeedManager: Set vehicle {vehicleId} speed = {speed}");
        }
        else
        {
            Debug.LogWarning("SpeedManager: VissimUdpClient not initialized");
        }
    }
}

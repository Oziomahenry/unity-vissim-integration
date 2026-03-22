using UnityEngine;

public class BusVolumeManager : MonoBehaviour
{
    public static BusVolumeManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
    }

    /// <summary>
    /// Set the volume of a bus input in VISSIM.
    /// </summary>
    /// <param name="vehInputId">VISSIM Vehicle Input ID for the bus</param>
    /// <param name="volume">Desired volume (vehicles/hour)</param>
    public void SetBusVolume(int vehInputId, int volume)
    {
        if (VissimUdpClient.Instance != null)
        {
            VissimUdpClient.Instance.SetBusVolume(vehInputId, volume);
            Debug.Log($"BusVolumeManager: Set bus input {vehInputId} volume = {volume}");
        }
        else
        {
            Debug.LogWarning("BusVolumeManager: VissimUdpClient not initialized");
        }
    }
}

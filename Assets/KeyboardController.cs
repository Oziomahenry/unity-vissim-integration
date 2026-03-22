using UnityEngine;

/// <summary>
/// Control bus volumes, vehicle speed, and simulation switching with keyboard.
/// </summary>
public class KeyboardController : MonoBehaviour
{
    [Header("Vehicle Settings")]
    public int selectedVehicleId = 101;
    public float speedStep = 5f;

    [Header("Bus Input Settings")]
    public int selectedBusInputId = 5;
    public int volumeStep = 5;

    private float currentSpeed = 0f;
    private int currentBusVolume = 0;

    void Start()
    {
        currentSpeed = 0f;
        currentBusVolume = 0;
    }

    void Update()
    {
        // ========================
        // 🚗 VEHICLE SPEED CONTROL
        // ========================
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSpeed += speedStep;
            SpeedManager.Instance?.SetVehicleSpeed(selectedVehicleId, currentSpeed);
            Debug.Log($"Increased vehicle {selectedVehicleId} speed to {currentSpeed}");
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSpeed = Mathf.Max(0, currentSpeed - speedStep);
            SpeedManager.Instance?.SetVehicleSpeed(selectedVehicleId, currentSpeed);
            Debug.Log($"Decreased vehicle {selectedVehicleId} speed to {currentSpeed}");
        }

        // ========================
        // 🚌 BUS VOLUME CONTROL
        // ========================
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentBusVolume += volumeStep;
            BusVolumeManager.Instance?.SetBusVolume(selectedBusInputId, currentBusVolume);
            Debug.Log($"Increased bus input {selectedBusInputId} volume to {currentBusVolume}");
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentBusVolume = Mathf.Max(0, currentBusVolume - volumeStep);
            BusVolumeManager.Instance?.SetBusVolume(selectedBusInputId, currentBusVolume);
            Debug.Log($"Decreased bus input {selectedBusInputId} volume to {currentBusVolume}");
        }

        // ========================
        // 🔄 SIMULATION SWITCHING
        // ========================
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            VissimUdpClient.Instance?.SelectSimulation(0);
            Debug.Log("Switched to Simulation 0");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            VissimUdpClient.Instance?.SelectSimulation(1);
            Debug.Log("Switched to Simulation 1");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            VissimUdpClient.Instance?.SelectSimulation(2);
            Debug.Log("Switched to Simulation 2");
        }
    }
}

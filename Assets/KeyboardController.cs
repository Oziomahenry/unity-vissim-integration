using UnityEngine;

/// <summary>
/// Control bus volumes and vehicle speed with keyboard.
/// </summary>
public class KeyboardController : MonoBehaviour
{
    [Header("Vehicle Settings")]
    public int selectedVehicleId = 101;   // Vehicle to control
    public float speedStep = 5f;          // Speed increment (m/s)

    [Header("Bus Input Settings")]
    public int selectedBusInputId = 5;    // Bus input to control
    public int volumeStep = 5;            // Bus volume increment (vehicles/hour)

    private float currentSpeed = 0f;
    private int currentBusVolume = 0;

    void Start()
    {
        // Initialize current values (optional)
        currentSpeed = 0f;
        currentBusVolume = 0;
    }

    void Update()
    {
        // ------------------------
        // Vehicle speed control
        // ------------------------
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

        // ------------------------
        // Bus volume control
        // ------------------------
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
    }
}

using UnityEngine;

public class KeyboardController : MonoBehaviour
{
    // ----------------------------
    // VEHICLE SELECTION
    // ----------------------------
    public int[] vehicleIds = { 200, 100, 190, 610, 620 };
    private int currentVehicleIndex = 0;

    public float speedStep = 1f; // smoother control
    private float currentSpeed = 0f;

    // ----------------------------
    // BUS CONTROL
    // ----------------------------
    public int selectedBusInputId = 5;
    public int volumeStep = 5;
    private int currentBusVolume = 0;

    // ----------------------------
    // SIMULATION KEYS
    // ----------------------------
    private KeyCode[] simulationKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };

    void Start()
    {
        if (vehicleIds.Length > 0)
        {
            SetVehicle(currentVehicleIndex);
        }
    }

    void Update()
    {
        HandleVehicleSwitch();
        HandleSpeedControl();
        HandleBusVolumeControl();
        HandleSimulationSwitch();
    }

    // ----------------------------
    // VEHICLE SWITCH
    // ----------------------------
    private void HandleVehicleSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            currentVehicleIndex = (currentVehicleIndex + 1) % vehicleIds.Length;
            SetVehicle(currentVehicleIndex);
        }
    }

    private void SetVehicle(int index)
    {
        int vehicleId = vehicleIds[index];
        Debug.Log("Switched to vehicle: " + vehicleId);
        ControlBoardUI.Instance?.SetVehicle(vehicleId);
    }

    // ----------------------------
    // SPEED CONTROL
    // ----------------------------
    private void HandleSpeedControl()
    {
        int selectedVehicleId = vehicleIds[currentVehicleIndex];

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSpeed += speedStep;
            ApplySpeed(selectedVehicleId);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSpeed = Mathf.Max(0, currentSpeed - speedStep);
            ApplySpeed(selectedVehicleId);
        }
    }

    private void ApplySpeed(int vehicleId)
    {
        SpeedManager.Instance?.SetVehicleSpeed(vehicleId, currentSpeed);
        ControlBoardUI.Instance?.SetSpeed(currentSpeed);
    }

    // ----------------------------
    // BUS VOLUME CONTROL
    // ----------------------------
    private void HandleBusVolumeControl()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentBusVolume += volumeStep;
            ApplyBusVolume();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentBusVolume = Mathf.Max(0, currentBusVolume - volumeStep);
            ApplyBusVolume();
        }
    }

    private void ApplyBusVolume()
    {
        BusVolumeManager.Instance?.SetBusVolume(selectedBusInputId, currentBusVolume);
        ControlBoardUI.Instance?.SetBusVolume(currentBusVolume);
    }

    // ----------------------------
    // SIMULATION SWITCH
    // ----------------------------
    private void HandleSimulationSwitch()
    {
        for (int i = 0; i < simulationKeys.Length; i++)
        {
            if (Input.GetKeyDown(simulationKeys[i]))
            {
                VissimUdpClient.Instance?.SelectSimulation(i);
                ControlBoardUI.Instance?.SetSimulation(i);
            }
        }
    }
}
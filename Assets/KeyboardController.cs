using UnityEngine;

public class KeyboardController : MonoBehaviour
{
    public int selectedVehicleId = 101;
    public float speedStep = 5f;

    public int selectedBusInputId = 5;
    public int volumeStep = 5;

    private float currentSpeed = 0f;
    private int currentBusVolume = 0;

    void Update()
    {
        // SPEED
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSpeed += speedStep;
            SpeedManager.Instance?.SetVehicleSpeed(selectedVehicleId, currentSpeed);
            ControlBoardUI.Instance?.SetSpeed(currentSpeed);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSpeed = Mathf.Max(0, currentSpeed - speedStep);
            SpeedManager.Instance?.SetVehicleSpeed(selectedVehicleId, currentSpeed);
            ControlBoardUI.Instance?.SetSpeed(currentSpeed);
        }

        // BUS VOLUME
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentBusVolume += volumeStep;
            BusVolumeManager.Instance?.SetBusVolume(selectedBusInputId, currentBusVolume);
            ControlBoardUI.Instance?.SetBusVolume(currentBusVolume);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentBusVolume = Mathf.Max(0, currentBusVolume - volumeStep);
            BusVolumeManager.Instance?.SetBusVolume(selectedBusInputId, currentBusVolume);
            ControlBoardUI.Instance?.SetBusVolume(currentBusVolume);
        }

        // SWITCH SIM
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            VissimUdpClient.Instance?.SelectSimulation(0);
            ControlBoardUI.Instance?.SetSimulation(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            VissimUdpClient.Instance?.SelectSimulation(1);
            ControlBoardUI.Instance?.SetSimulation(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            VissimUdpClient.Instance?.SelectSimulation(2);
            ControlBoardUI.Instance?.SetSimulation(2);
        }
    }
}
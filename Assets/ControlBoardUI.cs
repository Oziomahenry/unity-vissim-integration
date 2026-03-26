using UnityEngine;
using TMPro;

public class ControlBoardUI : MonoBehaviour
{
    public static ControlBoardUI Instance;

    [Header("UI Text")]
    public TextMeshProUGUI boardText;

    // --------------------------
    // STATE VARIABLES
    // --------------------------
    private int currentSimulation = 0;
    private int currentVehicleId = 0;
    private float currentSpeed = 0f;
    private int currentBusVolume = 0;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        UpdateBoard("System ready");
    }

    // --------------------------
    // SETTERS
    // --------------------------
    public void SetSimulation(int index)
    {
        currentSimulation = index;
        UpdateBoard("Simulation switched");
    }

    public void SetVehicle(int vehicleId)
    {
        currentVehicleId = vehicleId;
        UpdateBoard("Vehicle switched");
    }

    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
        UpdateBoard("Vehicle speed changed");
    }

    public void SetBusVolume(int volume)
    {
        currentBusVolume = volume;
        UpdateBoard("Bus volume changed");
    }

    // --------------------------
    // UPDATE UI TEXT
    // --------------------------
    private void UpdateBoard(string lastAction)
    {
        if (boardText == null) return;

        boardText.text =
            "=== CONTROL BOARD ===\n\n" +
            $"Simulation: {currentSimulation}\n" +
            $"Vehicle: {currentVehicleId}\n" +
            $"Vehicle Speed: {currentSpeed} m/s\n" +
            $"Bus Volume: {currentBusVolume}\n\n" +
            $"Last Action: {lastAction}";
    }
}

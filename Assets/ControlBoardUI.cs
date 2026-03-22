using UnityEngine;
using TMPro;

public class ControlBoardUI : MonoBehaviour
{
    public static ControlBoardUI Instance;

    [Header("UI Text")]
    public TextMeshProUGUI boardText;

    private int currentSimulation = 0;
    private float currentSpeed = 0f;
    private int currentBusVolume = 0;

    void Awake()
    {
        Instance = this;
        UpdateBoard("System ready");
    }

    // --------------------------
    // UPDATE SIMULATION
    // --------------------------
    public void SetSimulation(int index)
    {
        currentSimulation = index;
        UpdateBoard("Simulation switched");
    }

    // --------------------------
    // UPDATE SPEED
    // --------------------------
    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
        UpdateBoard("Vehicle speed changed");
    }

    // --------------------------
    // UPDATE BUS VOLUME
    // --------------------------
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
        boardText.text =
            "=== CONTROL BOARD ===\n\n" +
            $"Simulation: {currentSimulation}\n" +
            $"Vehicle Speed: {currentSpeed} m/s\n" +
            $"Bus Volume: {currentBusVolume}\n\n" +
            $"Last Action: {lastAction}";
    }
}

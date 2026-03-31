using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class VissimUdpClient : MonoBehaviour
{
    public static VissimUdpClient Instance;

    [Header("Connection")]
    public string host = "127.0.0.1";
    public int port = 1234;

    private UdpClient udp;
    private IPEndPoint serverEndPoint;
    private Thread receiveThread;
    private bool running = false;

    private ConcurrentQueue<StateMessage> messageQueue =
        new ConcurrentQueue<StateMessage>();

    private int currentSim = -1;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Connect();
    }

    void Connect()
    {
        try
        {
            serverEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
            udp = new UdpClient();
            udp.Connect(serverEndPoint);

            running = true;

            receiveThread = new Thread(ReceiveLoop);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            SendRaw("{\"type\":\"hello\"}");

            Debug.Log("UDP connected to VISSIM server.");
        }
        catch (Exception e)
        {
            Debug.LogError("UDP connection failed: " + e.Message);
        }
    }

    void ReceiveLoop()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

        try
        {
            while (running)
            {
                byte[] data = udp.Receive(ref remoteEP);
                string json = Encoding.UTF8.GetString(data);

                ProcessMessage(json);
            }
        }
        catch (SocketException) { }
        catch (Exception e)
        {
            Debug.LogError("UDP receive error: " + e.Message);
        }
    }

    void ProcessMessage(string json)
    {
        try
        {
            StateMessage msg = JsonUtility.FromJson<StateMessage>(json);
            if (msg != null)
                messageQueue.Enqueue(msg);
        }
        catch (Exception e)
        {
            Debug.LogError("JSON parse error: " + e.Message);
        }
    }

    void Update()
    {
        while (messageQueue.TryDequeue(out StateMessage msg))
        {
            if (msg.type != "state") continue;

            // 🔥 scenario change detection
            if (msg.sim != currentSim)
            {
                Debug.Log($"Scenario changed → sc{msg.sim}");

                VehicleManager.Instance?.ClearAllVehicles();
                PedestrianManager.Instance?.ClearAllPedestrians();

                currentSim = msg.sim;
            }

            // VEHICLES
            if (msg.vehicles != null)
            {
                foreach (VehicleState v in msg.vehicles)
                {
                    VehicleManager.Instance?.UpdateVehicle(
                        v.id,
                        v.type,
                        new Vector3(v.x, 0f, v.y),
                        v.speed
                    );
                }
            }

            // PEDESTRIANS
            if (msg.pedestrians != null)
            {
                foreach (PedestrianState p in msg.pedestrians)
                {
                    PedestrianManager.Instance?.UpdatePedestrian(
                        p.id,
                        p.type,
                        new Vector3(p.x, 0f, p.y)
                    );
                }
            }
        }
    }

    // ---------------- SEND ----------------

    public void SelectSimulation(int index)
    {
        SendRaw($"{{\"type\":\"select_sim\",\"index\":{index}}}");
    }

    public void SetBusVolume(int vehInputId, int volume)
    {
        SendRaw($"{{\"type\":\"bus_input\",\"id\":{vehInputId},\"volume\":{volume}}}");
    }

    public void SetVehicleSpeed(int vehicleId, float speed)
    {
        SendRaw($"{{\"type\":\"set_speed\",\"id\":{vehicleId},\"speed\":{speed}}}");
    }

    void SendRaw(string json)
    {
        if (udp == null) return;

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(json);
            udp.Send(data, data.Length);

            Debug.Log("Sent: " + json);
        }
        catch (Exception e)
        {
            Debug.LogWarning("UDP send failed: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        running = false;

        if (udp != null)
            udp.Close();

        Debug.Log("UDP socket closed.");
    }
}
using System.Collections.Generic;
using UnityEngine;

public class VehicleManager : MonoBehaviour
{
    public static VehicleManager Instance;

    [Header("Vehicle Prefabs")]
    public GameObject carPrefab;
    public GameObject lgvPrefab;
    public GameObject hgvPrefab;
    public GameObject bikePrefab;

    [Header("Movement Settings")]
    [Tooltip("Speed factor for Lerp movement of vehicles.")]
    public float lerpSpeed = 10f;

    [Tooltip("Smooth rotation speed when facing movement direction.")]
    public float rotationSpeed = 5f;

    private class VehicleData
    {
        public GameObject obj;
        public Vector3 targetPos;
        public int lastSeq;     // for UDP ordering
        public Vector3 lastPos; // for rotation calculation
    }

    private Dictionary<int, VehicleData> vehicles = new Dictionary<int, VehicleData>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    // --------------------------------------------------
    // CALLED FROM UDP CLIENT
    // --------------------------------------------------
    /// <summary>
    /// Update or spawn a vehicle.
    /// </summary>
    public void UpdateVehicle(int id, int type, Vector3 targetPos, float speed, int seq = 0)
    {
        if (!vehicles.TryGetValue(id, out VehicleData data))
        {
            // Spawn new vehicle
            GameObject obj = Instantiate(GetPrefab(type), targetPos, Quaternion.identity);
            obj.name = $"Vehicle_{id}_Type_{type}";

            data = new VehicleData
            {
                obj = obj,
                targetPos = targetPos,
                lastSeq = seq,
                lastPos = targetPos
            };

            vehicles[id] = data;
            return;
        }

        // Drop old UDP packets
        if (seq < data.lastSeq) return;

        data.lastSeq = seq;
        data.targetPos = targetPos;
    }

    // --------------------------------------------------
    // SMOOTH MOVEMENT LOOP
    // --------------------------------------------------
    void Update()
    {
        var removeKeys = new List<int>();

        foreach (var kv in vehicles)
        {
            VehicleData data = kv.Value;
            if (data.obj == null)
            {
                removeKeys.Add(kv.Key);
                continue;
            }

            // Smooth position
            data.obj.transform.position = Vector3.Lerp(
                data.obj.transform.position,
                data.targetPos,
                Time.deltaTime * lerpSpeed
            );

            // Smooth rotation
            Vector3 direction = data.targetPos - data.obj.transform.position;
            if (direction.magnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                data.obj.transform.rotation = Quaternion.Slerp(
                    data.obj.transform.rotation,
                    targetRotation,
                    Time.deltaTime * rotationSpeed
                );
            }

            data.lastPos = data.obj.transform.position;
        }

        // Cleanup destroyed vehicles
        foreach (int key in removeKeys)
            vehicles.Remove(key);
    }

    // --------------------------------------------------
    // PREFAB SELECTION
    // --------------------------------------------------
    private GameObject GetPrefab(int type)
    {
        switch (type)
        {
            case 100: return carPrefab;
            case 190: return lgvPrefab;
            case 200: return hgvPrefab;
            case 610:
            case 620: return bikePrefab;
            default:
                Debug.LogWarning($"Unknown VehType {type}, using carPrefab");
                return carPrefab;
        }
    }

    // --------------------------------------------------
    // OPTIONAL: Reset all vehicles
    // --------------------------------------------------
    public void ClearAll()
    {
        foreach (var kv in vehicles)
        {
            if (kv.Value.obj != null)
                Destroy(kv.Value.obj);
        }
        vehicles.Clear();
    }
}

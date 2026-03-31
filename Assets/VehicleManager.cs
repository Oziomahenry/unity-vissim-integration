using System.Collections.Generic;
using UnityEngine;

public class VehicleManager : MonoBehaviour
{
    public static VehicleManager Instance;

    // -----------------------------
    // CAR PREFABS (ASSIGN IN INSPECTOR)
    // -----------------------------
    [Header("Vehicle Prefabs")]
    public GameObject carPrefab;        // default car
    public GameObject truckPrefab;      // optional
    public GameObject busPrefab;        // optional

    // -----------------------------
    // MOVEMENT SETTINGS
    // -----------------------------
    [Header("Movement")]
    public float lerpSpeed = 8f;

    // -----------------------------
    // INTERNAL DATA
    // -----------------------------
    private class VehicleData
    {
        public GameObject obj;
        public Vector3 targetPos;
        public float speed;
        public int lastSeq;
    }

    private Dictionary<int, VehicleData> vehicles = new Dictionary<int, VehicleData>();

    // -----------------------------
    // SINGLETON
    // -----------------------------
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // -----------------------------
    // UPDATE VEHICLE FROM UDP
    // -----------------------------
    public void UpdateVehicle(int id, int type, Vector3 targetPos, float speed, int seq = -1)
    {
        if (!vehicles.TryGetValue(id, out VehicleData data))
        {
            GameObject prefab = GetPrefab(type);

            GameObject obj = Instantiate(prefab, targetPos, Quaternion.identity);
            obj.name = $"Vehicle_{id}_Type_{type}";
            obj.transform.SetParent(transform);

            data = new VehicleData
            {
                obj = obj,
                targetPos = targetPos,
                speed = speed,
                lastSeq = seq
            };

            vehicles[id] = data;
            return;
        }

        // Ignore old packets
        if (seq >= 0 && seq < data.lastSeq)
            return;

        data.lastSeq = seq;
        data.targetPos = targetPos;
        data.speed = speed;
    }

    // -----------------------------
    // SMOOTH MOVEMENT
    // -----------------------------
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

            // Smooth movement
            data.obj.transform.position = Vector3.Lerp(
                data.obj.transform.position,
                data.targetPos,
                1 - Mathf.Exp(-lerpSpeed * Time.deltaTime)
            );

            // Optional: rotate toward movement direction
            Vector3 dir = data.targetPos - data.obj.transform.position;
            if (dir != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(dir);
                data.obj.transform.rotation = Quaternion.Slerp(
                    data.obj.transform.rotation,
                    lookRotation,
                    Time.deltaTime * 5f
                );
            }
        }

        foreach (int key in removeKeys)
            vehicles.Remove(key);
    }

    // -----------------------------
    // PREFAB SELECTION
    // -----------------------------
    private GameObject GetPrefab(int type)
    {
        switch (type)
        {
            case 100: // Car
                return carPrefab;

            case 200: // Bus
                return busPrefab != null ? busPrefab : carPrefab;

            case 300: // Truck (optional)
                return truckPrefab != null ? truckPrefab : carPrefab;

            default:
                Debug.LogWarning($"Unknown vehicle type {type}, using car prefab");
                return carPrefab;
        }
    }

    // -----------------------------
    // CLEAR ALL VEHICLES (SCENARIO RESET)
    // -----------------------------
    public void ClearAllVehicles()
    {
        foreach (var kv in vehicles)
        {
            if (kv.Value.obj != null)
                Destroy(kv.Value.obj);
        }

        vehicles.Clear();
    }
}
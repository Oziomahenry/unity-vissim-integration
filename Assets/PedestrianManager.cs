using System.Collections.Generic;
using UnityEngine;

public class PedestrianManager : MonoBehaviour
{
    public static PedestrianManager Instance;

    [Header("Pedestrian Prefabs")]
    public GameObject manPrefab;
    public GameObject womanPrefab;

    [Header("Movement Settings")]
    [Tooltip("Speed factor for Lerp movement of pedestrians.")]
    public float lerpSpeed = 5f;

    private class PedData
    {
        public GameObject obj;
        public Vector3 targetPos;
        public int lastSeq;
    }

    private Dictionary<int, PedData> pedestrians = new Dictionary<int, PedData>();

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
    /// Update or spawn a pedestrian.
    /// </summary>
    /// <param name="id">Unique pedestrian ID</param>
    /// <param name="type">Pedestrian type (matches VISSIM)</param>
    /// <param name="targetPos">Target position from VISSIM</param>
    /// <param name="seq">Optional sequence number for ordering (-1 = ignore)</param>
    public void UpdatePedestrian(int id, int type, Vector3 targetPos, int seq = -1)
    {
        if (!pedestrians.TryGetValue(id, out PedData data))
        {
            // Spawn new pedestrian
            GameObject obj = Instantiate(GetPrefab(type), targetPos, Quaternion.identity);
            obj.name = $"Ped_{id}_Type_{type}";

            data = new PedData
            {
                obj = obj,
                targetPos = targetPos,
                lastSeq = seq
            };

            pedestrians[id] = data;
            return;
        }

        // Ignore out-of-order packets (if sequence used)
        if (seq >= 0 && seq < data.lastSeq)
            return;

        data.lastSeq = seq;
        data.targetPos = targetPos;
    }

    // --------------------------------------------------
    // SMOOTH MOVEMENT
    // --------------------------------------------------
    void Update()
    {
        var removeKeys = new List<int>();

        foreach (var kv in pedestrians)
        {
            PedData data = kv.Value;

            if (data.obj == null)
            {
                // Mark destroyed objects for cleanup
                removeKeys.Add(kv.Key);
                continue;
            }

            // Smoothly move pedestrian toward target position
            data.obj.transform.position = Vector3.Lerp(
                data.obj.transform.position,
                data.targetPos,
                Time.deltaTime * lerpSpeed
            );
        }

        // Remove destroyed pedestrians
        foreach (int key in removeKeys)
            pedestrians.Remove(key);
    }

    // --------------------------------------------------
    // PREFAB SELECTION
    // --------------------------------------------------
    private GameObject GetPrefab(int type)
    {
        switch (type)
        {
            case 100: return manPrefab;     // Man
            case 200: return womanPrefab;   // Woman
            default:
                Debug.LogWarning($"Unknown PedType {type}, using man prefab");
                return manPrefab;
        }
    }

    // --------------------------------------------------
    // OPTIONAL: Reset all pedestrians
    // --------------------------------------------------
    public void ClearAll()
    {
        foreach (var kv in pedestrians)
        {
            if (kv.Value.obj != null)
                Destroy(kv.Value.obj);
        }
        pedestrians.Clear();
    }
}

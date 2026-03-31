using System.Collections.Generic;
using UnityEngine;

public class PedestrianManager : MonoBehaviour
{
    public static PedestrianManager Instance;

    // -----------------------------
    // PREFABS (ASSIGN IN INSPECTOR)
    // -----------------------------
    [Header("Pedestrian Prefabs")]
    public GameObject manPrefab;
    public GameObject womanPrefab;

    // -----------------------------
    // MOVEMENT SETTINGS
    // -----------------------------
    [Header("Movement")]
    public float lerpSpeed = 5f;

    // -----------------------------
    // INTERNAL DATA
    // -----------------------------
    private class PedData
    {
        public GameObject obj;
        public Vector3 targetPos;
        public int lastSeq;
    }

    private Dictionary<int, PedData> pedestrians = new Dictionary<int, PedData>();

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
    // UPDATE FROM UDP
    // -----------------------------
    public void UpdatePedestrian(int id, int type, Vector3 targetPos, int seq = -1)
    {
        if (!pedestrians.TryGetValue(id, out PedData data))
        {
            // Create new pedestrian
            GameObject prefab = GetPrefab(type);

            GameObject obj = Instantiate(prefab, targetPos, Quaternion.identity);
            obj.name = $"Ped_{id}_Type_{type}";
            obj.transform.SetParent(transform);

            data = new PedData
            {
                obj = obj,
                targetPos = targetPos,
                lastSeq = seq
            };

            pedestrians[id] = data;
            return;
        }

        // Ignore old packets if sequence is used
        if (seq >= 0 && seq < data.lastSeq)
            return;

        data.lastSeq = seq;
        data.targetPos = targetPos;
    }

    // -----------------------------
    // SMOOTH MOVEMENT
    // -----------------------------
    void Update()
    {
        var removeKeys = new List<int>();

        foreach (var kv in pedestrians)
        {
            PedData data = kv.Value;

            if (data.obj == null)
            {
                removeKeys.Add(kv.Key);
                continue;
            }

            data.obj.transform.position = Vector3.Lerp(
                data.obj.transform.position,
                data.targetPos,
                1 - Mathf.Exp(-lerpSpeed * Time.deltaTime)
            );
        }

        foreach (int key in removeKeys)
            pedestrians.Remove(key);
    }

    // -----------------------------
    // PREFAB SELECTION
    // -----------------------------
    private GameObject GetPrefab(int type)
    {
        switch (type)
        {
            case 100: // Man
                return manPrefab;

            case 200: // Woman
                return womanPrefab;

            default:
                Debug.LogWarning($"Unknown pedestrian type: {type}. Using man prefab.");
                return manPrefab;
        }
    }

    // -----------------------------
    // CLEAR ALL (USED FOR SCENARIOS)
    // -----------------------------
    public void ClearAllPedestrians()
    {
        foreach (var kv in pedestrians)
        {
            if (kv.Value.obj != null)
                Destroy(kv.Value.obj);
        }

        pedestrians.Clear();
    }
}
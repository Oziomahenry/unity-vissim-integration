using System;
using UnityEngine;   // <-- this is required for Vector3, GameObject, MonoBehaviour, etc.

[System.Serializable]
public class LinkPolygon
{
    public string id;
    public Vector3[] points;
}

[System.Serializable]
public class PedestrianArea
{
    public string id;
    public Vector3[] points;
}
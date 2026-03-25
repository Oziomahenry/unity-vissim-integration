using UnityEngine;
using System.Collections.Generic;

public class VissimStaticLoader : MonoBehaviour
{
    public TextAsset vissimTxt;   // assign in Inspector
    public Material roadMaterial;
    public Material pedestrianMaterial;

    void Start()
    {
        string[] lines = vissimTxt.text.Split('\n');
        bool inLinks = false, inPedAreas = false;

        foreach (string line in lines)
        {
            if (line.StartsWith("Links")) { inLinks = true; inPedAreas = false; continue; }
            if (line.StartsWith("EndLinks")) { inLinks = false; continue; }
            if (line.StartsWith("PedestrianAreas")) { inPedAreas = true; continue; }
            if (line.StartsWith("EndPedestrianAreas")) { inPedAreas = false; continue; }

            if (inLinks && line.StartsWith("\""))
                CreatePolygon(line, roadMaterial);

            if (inPedAreas && line.StartsWith("\""))
                CreatePolygon(line, pedestrianMaterial);
        }
    }

    void CreatePolygon(string line, Material mat)
    {
        // Example: "1",[x,y,z],[x,y,z]...
        string[] parts = line.Split(']');
        List<Vector3> pts = new List<Vector3>();

        foreach (string p in parts)
        {
            if (p.Contains("["))
            {
                string coords = p.Substring(p.IndexOf('[') + 1);
                string[] xyz = coords.Split(',');
                float x = float.Parse(xyz[0]);
                float y = float.Parse(xyz[1]);
                float z = float.Parse(xyz[2]);
                pts.Add(new Vector3(x, z, y)); // swap Y/Z for Unity’s coordinate system
            }
        }

        // Build mesh
        GameObject go = new GameObject("Polygon");
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = mat;

        Mesh mesh = new Mesh();
        mesh.vertices = pts.ToArray();

        // Triangulate polygon (simple fan triangulation)
        List<int> tris = new List<int>();
        for (int i = 1; i < pts.Count - 1; i++)
        {
            tris.Add(0);
            tris.Add(i);
            tris.Add(i + 1);
        }

        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mf.mesh = mesh;
    }
}

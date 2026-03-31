using UnityEngine;
using System.Collections.Generic;

public class VissimStaticLoader1 : MonoBehaviour
{
    public TextAsset vissimTxt;   // assign in Inspector
    public Material roadMaterial;
    public Material pedestrianMaterial;

    void Start()
    {
        if (vissimTxt == null)
        {
            Debug.LogError("No text file assigned!");
            return;
        }

        string[] lines = vissimTxt.text.Split('\n');

        bool inLinks = false;
        bool inPedAreas = false;

        foreach (string raw in lines)
        {
            string line = raw.Trim();

            if (line.StartsWith("Links"))
            {
                inLinks = true;
                inPedAreas = false;
                continue;
            }

            if (line.StartsWith("PedestrianAreas"))
            {
                inPedAreas = true;
                inLinks = false;
                continue;
            }

            // Your file does NOT contain "EndLinks" or "EndPedestrianAreas"
            // so we stop only when the next section begins.

            if (line.StartsWith("\""))
            {
                if (inLinks)
                    CreatePolygon(line, roadMaterial);

                if (inPedAreas)
                    CreatePolygon(line, pedestrianMaterial);
            }
        }
    }

    void CreatePolygon(string line, Material mat)
    {
        // Example line:
        // "1",[111.20,-290.35,0.0],[111.15,-273.35,0.0],...

        List<Vector3> pts = new List<Vector3>();

        int idx = 0;
        while (true)
        {
            int start = line.IndexOf('[', idx);
            if (start == -1) break;

            int end = line.IndexOf(']', start);
            if (end == -1) break;

            string coords = line.Substring(start + 1, end - start - 1);
            string[] xyz = coords.Split(',');

            if (xyz.Length == 3)
            {
                float x = float.Parse(xyz[0]);
                float y = float.Parse(xyz[1]);
                float z = float.Parse(xyz[2]);

                // Convert Vissim XY-plane → Unity XZ-plane
                pts.Add(new Vector3(x, z, y));
            }

            idx = end + 1;
        }

        if (pts.Count < 3)
            return;

        // Create GameObject
        GameObject go = new GameObject("Polygon");
        go.transform.parent = this.transform;

        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = mat;

        Mesh mesh = new Mesh();
        mesh.vertices = pts.ToArray();

        // Simple fan triangulation
        List<int> tris = new List<int>();
        for (int i = 1; i < pts.Count - 1; i++)
        {
            tris.Add(0);
            tris.Add(i);
            tris.Add(i + 1);
        }

        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        mf.mesh = mesh;
    }
}


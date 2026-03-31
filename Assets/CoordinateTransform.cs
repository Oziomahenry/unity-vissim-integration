using UnityEngine;

public static class CoordinateTransform
{
    public static Vector3 ToUnity(Vector3 v, float scale = 1f)
    {
        return new Vector3(v.x * scale, v.z * scale, v.y * scale);
    }
}

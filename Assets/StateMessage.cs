using System;

[Serializable]
public class StateMessage
{
    public string type;
    public int seq;
    public int sim; // 🔥 REQUIRED
    public VehicleState[] vehicles;
    public PedestrianState[] pedestrians;
}

[Serializable]
public class VehicleState
{
    public int id;
    public int type;
    public float x;
    public float y;
    public float speed;
}

[Serializable]
public class PedestrianState
{
    public int id;
    public int type;
    public float x;
    public float y;
    public float speed;
}
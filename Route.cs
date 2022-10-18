using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Route 
{ 
    public Vector3 Start;
    public Vector3 End;
    public List<Vector3> Intersections; 
    public List<WayPoint> wayPoints;
}
[System.Serializable]
public class WayPoint
{
    public int id;
    public int cost;
    public Vector3 point;
}

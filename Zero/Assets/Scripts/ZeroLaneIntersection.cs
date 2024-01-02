using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroLaneIntersection
{

    public string Name;
    public ZeroParallelogram IntersectionPoints;
    public ZeroRoadLane PrimaryLane;
    public ZeroRoadLane IntersectingLane;
    public float PrimaryLengthSoFar;

    public ZeroLaneIntersection(
        string name,
        float primaryLengthSoFar,
        ZeroParallelogram intersectionPoints,
        ZeroRoadLane primaryLane,
        ZeroRoadLane intersectingLane)
    {
        Name = name;
        this.IntersectionPoints = intersectionPoints;
        this.PrimaryLane = primaryLane;
        this.IntersectingLane = intersectingLane;
        this.PrimaryLengthSoFar = primaryLengthSoFar;
    }
}

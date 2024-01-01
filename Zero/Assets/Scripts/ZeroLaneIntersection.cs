using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroLaneIntersection
{

    public string Name;
    public ZeroParallelogram IntersectionPoints;
    public ZeroRoadLane PrimaryLane;
    public ZeroRoadLane IntersectingLane;

    public ZeroLaneIntersection(
        string name,
        ZeroParallelogram intersectionPoints,
        ZeroRoadLane primaryLane,
        ZeroRoadLane intersectingLane)
    {
        this.Name = name;
        IntersectionPoints = intersectionPoints;
        PrimaryLane = primaryLane;
        IntersectingLane = intersectingLane;
    }
}

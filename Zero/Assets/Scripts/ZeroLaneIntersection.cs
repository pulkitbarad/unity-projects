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
        ZeroParallelogram intersectionPoints,
        ZeroRoadLane primaryLane,
        ZeroRoadLane intersectingLane)
    {
        IntersectionPoints = intersectionPoints;
        PrimaryLane = primaryLane;
        IntersectingLane = intersectingLane;
        Name = PrimaryLane.Name + IntersectingLane.Name;
    }
}

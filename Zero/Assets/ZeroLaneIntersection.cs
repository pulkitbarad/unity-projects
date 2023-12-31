using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroLaneIntersection 
{
    public ZeroParallelogram IntersectionPoints;
    public ZeroRoadLane PrimaryLane;
    public ZeroRoadLane IntersectingLane;
    public float DistanceToPrimaryLaneStart;
    public float DistanceToIntersectingLaneStart;

    public ZeroLaneIntersection(
        ZeroParallelogram intersectionPoints,
        ZeroRoadLane primaryLane,
        ZeroRoadLane intersectingLane)
    {
        this.IntersectionPoints = intersectionPoints;
        this.PrimaryLane = primaryLane;
        this.IntersectingLane = intersectingLane;
        this.IntersectingLane = intersectingLane;
    }
}

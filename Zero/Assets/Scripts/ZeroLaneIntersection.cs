using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroLaneIntersection
{

    public string Name;
    public float PrimaryDistance;
    public ZeroParallelogram IntersectionPoints;
    public ZeroRoadLane PrimaryLane;
    public ZeroRoadLane IntersectingLane;

    public ZeroLaneIntersection(
        string name,
        float primaryDistance,
        ZeroParallelogram intersectionPoints,
        ZeroRoadLane primaryLane,
        ZeroRoadLane intersectingLane)
    {
        this.Name = name;
        this.PrimaryDistance = primaryDistance;
        this.IntersectionPoints = intersectionPoints;
        this.PrimaryLane = primaryLane;
        this.IntersectingLane = intersectingLane;
    }

    public override string ToString()
    {
        return "ZeroLaneIntersection("
        + "\n Name:" + this.Name
        + "\n PrimaryDistance:" + this.PrimaryDistance
        + "\n PrimaryLane:" + this.PrimaryLane.ToString()
        + "\n IntersectingLane:" + this.IntersectingLane.ToString()
        + "\n IntersectionPoints:" + this.IntersectionPoints.ToString()
        + ")";
    }
}

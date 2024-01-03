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
        Name = name;
        this.IntersectionPoints = intersectionPoints;
        this.PrimaryLane = primaryLane;
        this.IntersectingLane = intersectingLane;
    }

    public override string ToString()
    {
        return "ZeroLaneIntersection("
        + "\n Name:" + this.Name
        + "\n PrimaryLane:" + this.PrimaryLane.ToString()
        + "\n IntersectingLane:" + this.IntersectingLane.ToString()
        + "\n IntersectionPoints:" + this.IntersectionPoints.ToString()
        + ")";
    }
}

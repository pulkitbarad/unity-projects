using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroLaneIntersection
{

    public string Name;
    public float PrimaryDistance;
    public ZeroCollisionInfo[] IntersectionPoints;
    public ZeroRoadLane PrimaryLane;
    public ZeroRoadLane IntersectingLane;
    public int LaneIntersectionType;

    public ZeroLaneIntersection(
        string name,
        float primaryDistance,
        int intersectionType,
        ZeroCollisionInfo[] intersectionPoints,
        ZeroRoadLane primaryLane,
        ZeroRoadLane intersectingLane)
    {
        this.Name = name;
        this.PrimaryDistance = primaryDistance;
        this.LaneIntersectionType = intersectionType;
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

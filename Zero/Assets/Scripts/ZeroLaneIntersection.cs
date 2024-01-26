using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroLaneIntersection
{
    public float PrimaryRoadLengthSoFar;
    public float L0DistanceFromOrigin;
    public ZeroCollisionInfo[] CollisonPoints;
    public ZeroRoadLane PrimaryLane;
    public ZeroRoadLane CollidingLane;
    // public int LaneIntersectionType;

    public ZeroLaneIntersection(
        ZeroCollisionInfo[] collisionPoints,
        ZeroRoadLane primaryLane,
        ZeroRoadLane collidingLane)
    {
        this.PrimaryRoadLengthSoFar = collisionPoints[0].PrimarySegment.RoadLengthSofar;
        this.L0DistanceFromOrigin = collisionPoints[0].DistanceFromOrigin;
        // this.LaneIntersectionType = intersectionType;
        this.CollisonPoints = collisionPoints;
        this.PrimaryLane = primaryLane;
        this.CollidingLane = collidingLane;
    }

    public override string ToString()
    {
        return "ZeroLaneIntersection("
        + "\n PrimaryRoadLengthSoFar:" + this.PrimaryRoadLengthSoFar
        + "\n L0DistanceFromOrigin:" + this.L0DistanceFromOrigin
        + "\n PrimaryLane:" + this.PrimaryLane.ToString()
        + "\n IntersectingLane:" + this.CollidingLane.ToString()
        + "\n IntersectionPoints:" + this.CollisonPoints.ToString()
        + ")";
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class ZeroRoad
{
    public string Name;
    public float WidthExclSidewalks;
    public float WidthInclSidewalks;
    public float Height;
    public float SidewalkHeight;
    public int VertexCount;
    public int NumberOfLanesExclSidewalks;
    public int NumberOfLanesInclSidewalks;
    public int NumberOfSegmentsPerLane;
    public int LeftSidewalkIndex;
    public int RightSidewalkIndex;
    public bool IsCurved = true;
    public bool HasBusLane = false;
    public Vector3[] ControlPoints;
    public ZeroRoadLane[] Lanes;
    public ZeroRoadLane[] Sidewalks;
    public Dictionary<string, List<ZeroRoadIntersection>> IntersectionsByRoadName = new();

    //Default constructor to generate test data
    public ZeroRoad()
    {

    }
    public ZeroRoad(
        bool isCurved,
        bool hasBusLane,
        int numberOfLanes,
        float height,
        float sidewalkHeight,
        Vector3[] controlPoints)
    {
        this.Name = "R" + ZeroRoadBuilder.BuiltRoadsByName.Count();
        this.IsCurved = isCurved;
        this.NumberOfLanesExclSidewalks = numberOfLanes;
        this.NumberOfLanesInclSidewalks = numberOfLanes + 2;
        this.LeftSidewalkIndex = numberOfLanes;
        this.RightSidewalkIndex = numberOfLanes + 1;
        this.WidthExclSidewalks = numberOfLanes * ZeroRoadBuilder.RoadLaneWidth;
        this.WidthInclSidewalks = (numberOfLanes + 2) * ZeroRoadBuilder.RoadLaneWidth;
        this.Height = height;
        this.SidewalkHeight = sidewalkHeight;
        this.HasBusLane = hasBusLane && numberOfLanes > 1;
        this.ControlPoints = controlPoints;
        InitRoadObject();

        if (!this.IsCurved)
            VertexCount = 4;

        this.Build(controlPoints);
    }

    private void InitRoadObject()
    {
        ZeroRoadBuilder.BuiltRoadsByName[this.Name] = this;
    }

    public void Hide()
    {
        foreach (ZeroRoadLane lane in this.Lanes)
        {
            lane.HideAllSegments();
        }
    }

    public void Build(Vector3[] controlPoints)
    {
        Vector3[] centerVertices =
               ZeroCurvedLine.FindBazierLinePoints(
                   controlPoints);
        this.NumberOfSegmentsPerLane = centerVertices.Length;

        this.Lanes =
           GetLanes(
               centerVertices: centerVertices);
        this.IntersectionsByRoadName = GetRoadIntersections();

        if (IntersectionsByRoadName.Count() > 0)
        {
            foreach (var entry in this.IntersectionsByRoadName)
            {
                foreach (ZeroRoadIntersection intersection in entry.Value)
                {
                    // intersection.RenderSidewalkCorners();
                    // intersection.RenderCrosswalks();
                    intersection.RenderLaneIntersections();
                    // intersection.RenderSidewalks();
                    ZeroController.AppendToDebugLog(
                        intersection.CrosswalksLogPairs());
                    ZeroController.AppendToDebugLog(
                        intersection.SidewalksLogPairs());
                    ZeroController.AppendToDebugLog(
                        intersection.SidewalkCornersLogPairs());
                    ZeroController.AppendToDebugLog(
                        intersection.LaneIntersectionsLogPairs());
                }
            }
        }
    }

    public void LogRoadPositions()
    {
        int i = 0;
        ZeroController.AppendToDebugLog(
            this.ControlPoints
            .Select(e =>
                (this.Name + "Control" + i++.ToString(), e.ToString())).ToArray()
            );
    }

    private string GetVectorString(string name, Vector3 vector)
    {
        return String.Format("{0}={1},{2},{3}", name, vector.x, vector.y, vector.z);
    }

    private ZeroRoadLane[] GetLanes(Vector3[] centerVertices)
    {
        Vector3[][] allLaneVertices = new Vector3[this.NumberOfLanes * 2 + 1][];

        allLaneVertices[this.NumberOfLanes] = centerVertices;
        for (
            int leftIndex = this.NumberOfLanes - 1, rightIndex = this.NumberOfLanes + 1, distMultiplier = 1;
            leftIndex >= 0;
            leftIndex--, rightIndex++, distMultiplier++)
        {
            Vector3[][] parallelLines = GetParallelLines(
                 vertices: centerVertices,
                 distance: distMultiplier * 0.5f * ZeroRoadBuilder.RoadLaneWidth);

            allLaneVertices[leftIndex] = parallelLines[0];
            allLaneVertices[rightIndex] = parallelLines[1];
        }

        ZeroRoadLane[] lanes = new ZeroRoadLane[this.NumberOfLanes + 2];

        int laneIndex = 0;
        for (; laneIndex < this.NumberOfLanes; laneIndex++)
        {
            ZeroRoadLane newLane =
                new(
                    laneIndex: laneIndex,
                    width: ZeroRoadBuilder.RoadLaneWidth,
                    height: ZeroRoadBuilder.RoadLaneHeight,
                    leftVertices: allLaneVertices[laneIndex * 2],
                    centerVertices: allLaneVertices[laneIndex * 2 + 1],
                    rightVertices: allLaneVertices[laneIndex * 2 + 2],
                    parentRoad: this);
            lanes[laneIndex] = newLane;
        }
        ZeroRoadLane[] sidewalks = this.GetSideWalks(centerVertices);
        lanes[laneIndex++] = sidewalks[0];
        lanes[laneIndex++] = sidewalks[1];
        return lanes;
    }

    private ZeroRoadLane[] GetSideWalks(Vector3[] centerVertices)
    {
        ZeroRoadLane[] lanes = new ZeroRoadLane[2];

        float distanceToSideWalkCenter =
            (this.NumberOfLanes + 1) * (0.5f * ZeroRoadBuilder.RoadLaneWidth);

        Vector3[][] sideWalkCenterVertices =
            GetParallelLines(
                vertices: centerVertices,
                distance: distanceToSideWalkCenter);

        Vector3[][] sideWalk1Edges =
            GetParallelLines(
                vertices: sideWalkCenterVertices[0],
                distance: 0.5f * ZeroRoadBuilder.RoadLaneWidth);
        Vector3[][] sideWalk2Edges =
            GetParallelLines(
                vertices: sideWalkCenterVertices[1],
                distance: 0.5f * ZeroRoadBuilder.RoadLaneWidth);

        lanes[0] = new(
            laneIndex: this.LeftSidewalkIndex,
            width: ZeroRoadBuilder.RoadLaneWidth,
            height: this.SidewalkHeight,
            leftVertices: sideWalk1Edges[0],
            centerVertices: sideWalkCenterVertices[0],
            rightVertices: sideWalk1Edges[1],
            parentRoad: this);
        lanes[1] = new(
            laneIndex: this.RightSidewalkIndex,
            width: ZeroRoadBuilder.RoadLaneWidth,
            height: this.SidewalkHeight,
            leftVertices: sideWalk2Edges[0],
            centerVertices: sideWalkCenterVertices[1],
            rightVertices: sideWalk2Edges[1],
            parentRoad: this);
        return lanes;
    }

    private static Vector3[] GetLeftParallelLine(
        Vector3[] vertices,
        float distance)
    {
        return GetParallelLines(vertices, distance)[0];
    }

    private static Vector3[] GetRightParallelLine(
        Vector3[] vertices,
        float distance)
    {
        return GetParallelLines(vertices, distance)[1];
    }

    private static Vector3[][] GetParallelLines(
        Vector3[] vertices,
        float distance)
    {
        Vector3[] leftLine = new Vector3[vertices.Length];
        Vector3[] rightLine = new Vector3[vertices.Length];
        for (int i = 1; i < vertices.Length; i++)
        {
            GetParallelPoints(
               originPoint: vertices[i - 1],
               targetPoint: vertices[i],
               distance: distance,
               out leftLine[i - 1],
               out rightLine[i - 1]);

            if (i == vertices.Length - 1)
            {
                GetParallelPoints(
                    originPoint: vertices[i],
                    targetPoint: vertices[i - 1],
                    distance: distance,
                    out rightLine[i],
                    out leftLine[i]);
            }
        }
        return new Vector3[][] { leftLine, rightLine };
    }
    public static void GetParallelPoints(
        Vector3 originPoint,
        Vector3 targetPoint,
        float distance,
        out Vector3 leftPoint,
        out Vector3 rightPoint)
    {
        Vector3 forward = targetPoint - originPoint;
        Vector3 leftVector;
        if (originPoint.y == targetPoint.y)
            leftVector = Vector3.Cross(forward, Vector3.up).normalized;
        else
        {
            Vector3 updTargetPoint = new(targetPoint.x, originPoint.y, targetPoint.z);
            Vector3 forwardFlat = updTargetPoint - originPoint;
            leftVector = Vector3.Cross(
                    forwardFlat,
                    forward).normalized;
        }
        leftPoint = originPoint + (leftVector * distance);
        rightPoint = originPoint - (leftVector * distance);
    }
    public Dictionary<string, List<ZeroRoadIntersection>> GetRoadIntersections()
    {
        Dictionary<string, List<ZeroRoadIntersection>> intersectionsByRoadName = new();
        Dictionary<string, List<ZeroLaneIntersection>> leftIntersectionsByRoadName =
                     new ZeroCollisionMap(
                        roadName: this.Name,
                        primaryLane: this.Lanes[this.LeftSidewalkIndex],
                        layerMaskName: ZeroRoadBuilder.RoadSidewalkMaskName)
                     .GetLaneIntersectionsByRoadName();
        Dictionary<string, List<ZeroLaneIntersection>> rightIntersectionsByRoadName =
                     new ZeroCollisionMap(
                        roadName: this.Name,
                        primaryLane: this.Lanes[this.RightSidewalkIndex],
                        layerMaskName: ZeroRoadBuilder.RoadSidewalkMaskName)
                     .GetLaneIntersectionsByRoadName();

        if (leftIntersectionsByRoadName.Count() == rightIntersectionsByRoadName.Count())
        {
            List<string> listOfRoads = leftIntersectionsByRoadName.Keys.Intersect(rightIntersectionsByRoadName.Keys).ToList();
            foreach (var intersectingRoadName in listOfRoads)
            {

                ZeroRoad intersectingRoad = ZeroRoadBuilder.BuiltRoadsByName[intersectingRoadName];

                ZeroLaneIntersection[] leftIntersections =
                   leftIntersectionsByRoadName[intersectingRoadName]
                   .OrderBy(e => e.PrimaryDistance)
                   .ThenBy(e => (e.IntersectionPoints[0].CollisionPoint - e.PrimaryLane.Segments[0].SegmentBounds.TopPlane[0]).magnitude)
                   .ToArray();

                ZeroLaneIntersection[] rightIntersections =
                    rightIntersectionsByRoadName[intersectingRoadName]
                    .OrderBy(e => e.PrimaryDistance)
                    .ThenBy(e => (e.IntersectionPoints[0].CollisionPoint - e.PrimaryLane.Segments[0].SegmentBounds.TopPlane[0]).magnitude)
                    .ToArray();

                List<ZeroRoadIntersection> roadIntersections = new();

                if (leftIntersections != null
                    && (leftIntersections.Length == 2 || leftIntersections.Length == 4)
                    && leftIntersections.Length == rightIntersections.Length)
                {
                    if (!intersectionsByRoadName.ContainsKey(intersectingRoadName))
                        intersectionsByRoadName[intersectingRoadName] = new();

                    intersectionsByRoadName[intersectingRoadName].Add(new ZeroRoadIntersection(
                        name: this.Name + intersectingRoadName + "I" + intersectionsByRoadName[intersectingRoadName].Count(),
                        height: Mathf.Max(this.Height, intersectingRoad.Height),
                        laneIntersections: new ZeroLaneIntersection[]{
                            leftIntersections[0],
                            rightIntersections[0],
                            leftIntersections[1],
                            rightIntersections[1]
                        }
                    ));
                    // leftIntersections[0].IntersectionPoints.RenderVertices(Color.white);
                    // rightIntersections[0].IntersectionPoints.RenderVertices(Color.black);
                    // leftIntersections[1].IntersectionPoints.RenderVertices(Color.gray);
                    // rightIntersections[1].IntersectionPoints.RenderVertices(Color.cyan);

                    if (leftIntersections.Length == 4)
                    {
                        intersectionsByRoadName[intersectingRoadName].Add(new ZeroRoadIntersection(
                            name: this.Name + intersectingRoadName + "I" + intersectionsByRoadName[intersectingRoadName].Count(),
                            height: Mathf.Max(this.Height, intersectingRoad.Height),
                            laneIntersections: new ZeroLaneIntersection[]{
                                leftIntersections[2],
                                rightIntersections[2],
                                leftIntersections[3],
                                rightIntersections[3]
                            }
                        ));
                        // leftIntersections[2].IntersectionPoints.RenderVertices(Color.green);
                        // rightIntersections[2].IntersectionPoints.RenderVertices(Color.red);
                        // leftIntersections[3].IntersectionPoints.RenderVertices(Color.blue);
                        // rightIntersections[3].IntersectionPoints.RenderVertices(Color.yellow);
                    }
                }
            }
        }
        return intersectionsByRoadName;
    }
}

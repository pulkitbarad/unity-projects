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
    public bool IsCurved;
    public bool HasBusLane;
    public bool IsRoadAngleChangeValid;
    public Vector3[] ControlPoints;
    public ZeroRoadLane[] Lanes;
    public ZeroRoadLane[] Sidewalks;
    public Dictionary<string, List<ZeroRoadIntersection>> IntersectionsByRoadName;

    public ZeroRoad(
        bool isCurved,
        bool hasBusLane,
        int numberOfLanesExclSidewalks,
        float height,
        float sidewalkHeight,
        bool forceSyncTransform,
        Vector3[] controlPoints)
    {

        this.Name = "R" + ZeroRoadBuilder.BuiltRoadsByName.Count();
        this.IsCurved = isCurved;
        this.NumberOfLanesExclSidewalks = numberOfLanesExclSidewalks;
        this.NumberOfLanesInclSidewalks = numberOfLanesExclSidewalks + 2;
        this.LeftSidewalkIndex = 0;
        this.RightSidewalkIndex = NumberOfLanesInclSidewalks - 1;
        this.WidthExclSidewalks = numberOfLanesExclSidewalks * ZeroRoadBuilder.RoadLaneWidth;
        this.WidthInclSidewalks = (numberOfLanesExclSidewalks + 2) * ZeroRoadBuilder.RoadLaneWidth;
        this.Height = height;
        this.SidewalkHeight = sidewalkHeight;
        this.HasBusLane = hasBusLane && numberOfLanesExclSidewalks > 1;
        InitRoadObject();

        if (!this.IsCurved)
            VertexCount = 4;

        this.Build(controlPoints, forceSyncTransform);
    }

    private void InitRoadObject()
    {
        ZeroRoadBuilder.BuiltRoadsByName[this.Name] = this;
    }

    public void Hide()
    {
        foreach (ZeroRoadLane lane in this.Lanes)
            lane.HideAllSegments();
    }

    public void Build(Vector3[] controlPoints, bool forceSyncTransform)
    {
        IntersectionsByRoadName = new();
        this.ControlPoints = controlPoints;
        (Vector3[], float) bazierResult =
               ZeroCurvedLine.FindBazierLinePoints(
                   controlPoints);

        this.NumberOfSegmentsPerLane = bazierResult.Item1.Length;

        this.Lanes =
           GetLanes(
               centerVertices: bazierResult.Item1);
        this.IsRoadAngleChangeValid = true;

        foreach (var lane in this.Lanes)
            this.IsRoadAngleChangeValid &= lane.IsLaneAngleChangeValid;

        Debug.LogFormat("Road={0} roadAngleChangeValid={1}", this.Name, IsRoadAngleChangeValid);
        if (this.IsRoadAngleChangeValid)
        {
            this.IntersectionsByRoadName = GetRoadIntersectionsByRoad();
            if (IntersectionsByRoadName.Count() > 0)
            {
                foreach (var entry in this.IntersectionsByRoadName)
                {
                    foreach (ZeroRoadIntersection intersection in entry.Value)
                    {
                        Debug.LogFormat("intersecion={0} isValid={1}", intersection.Name, intersection.IsValid);
                        intersection.RenderLaneIntersections();
                        intersection.RenderSidewalks();
                        intersection.RenderCrosswalks();
                        intersection.RenderRoadEdges();
                        intersection.RenderMainSquare();
                    }
                }
            }
        }
        if (forceSyncTransform)
            Physics.SyncTransforms();
    }

    public Dictionary<string, Vector3> GenerateTestData(string testName)
    {
        Dictionary<string, Vector3> testData = new();
        if (testName.Length > 0)
        {
            for (int i = 0; i < this.ControlPoints.Length; i++)
                testData[this.Name + "Control" + i.ToString()] = this.ControlPoints[i];
        }
        foreach (var lane in this.Lanes)
        {
            if (testName == ZeroRoadTest.Test1)
            {
                this.Lanes
                    .Select(
                        e => e.GetSegmentVertexLogs())
                    .SelectMany(e => e)
                    .ToList()
                    .ForEach(e => testData[e.Key] = e.Value);
            }
        }
        foreach (var entry in this.IntersectionsByRoadName)
        {
            foreach (ZeroRoadIntersection intersection in entry.Value)
            {
                if (new List<string> {
                    ZeroRoadTest.Test2,
                    ZeroRoadTest.Test3,
                    ZeroRoadTest.Test4}
                .Contains(testName))
                {
                    intersection.LaneIntersectionsLogPairs()
                    .ToList()
                    .ForEach(e => testData[e.Key] = e.Value);
                    intersection.SidewalksLogPairs()
                    .ToList()
                    .ForEach(e => testData[e.Key] = e.Value);
                    intersection.CrosswalksLogPairs()
                    .ToList()
                    .ForEach(e => testData[e.Key] = e.Value);
                    intersection.RoadEdgesLogPairs()
                    .ToList()
                    .ForEach(e => testData[e.Key] = e.Value);
                    intersection.MainSquareLogPairs()
                    .ToList()
                    .ForEach(e => testData[e.Key] = e.Value);
                }
            }
        }
        return testData;
    }

    private ZeroRoadLane[] GetLanes(Vector3[] centerVertices)
    {
        Vector3[][] allLaneVertices = new Vector3[this.NumberOfLanesInclSidewalks * 2 + 1][];

        allLaneVertices[this.NumberOfLanesInclSidewalks] = centerVertices;
        for (
            int leftIndex = this.NumberOfLanesInclSidewalks - 1, rightIndex = this.NumberOfLanesInclSidewalks + 1, distMultiplier = 1;
            leftIndex >= 0;
            leftIndex--, rightIndex++, distMultiplier++)
        {
            Vector3[][] parallelLines = GetParallelLines(
                 vertices: centerVertices,
                 distance: distMultiplier * 0.5f * ZeroRoadBuilder.RoadLaneWidth);

            allLaneVertices[leftIndex] = parallelLines[0];
            allLaneVertices[rightIndex] = parallelLines[1];
        }

        ZeroRoadLane[] lanes = new ZeroRoadLane[this.NumberOfLanesInclSidewalks];

        int laneIndex = 0;
        for (; laneIndex < this.NumberOfLanesInclSidewalks; laneIndex++)
        {
            float height = this.Height;
            if (laneIndex == this.LeftSidewalkIndex || laneIndex == this.RightSidewalkIndex)
                height = this.SidewalkHeight;

            ZeroRoadLane newLane =
                new(
                    laneIndex: laneIndex,
                    width: ZeroRoadBuilder.RoadLaneWidth,
                    height: height,
                    leftVertices: allLaneVertices[laneIndex * 2],
                    centerVertices: allLaneVertices[laneIndex * 2 + 1],
                    rightVertices: allLaneVertices[laneIndex * 2 + 2],
                    parentRoad: this);
            lanes[laneIndex] = newLane;
        }
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
    public Dictionary<string, List<ZeroRoadIntersection>> GetRoadIntersectionsByRoad()
    {
        Dictionary<string, List<ZeroRoadIntersection>> intersectionsByRoadName = new();
        Dictionary<string, ZeroLaneIntersection[]> leftIntersectionsByRoadName =
            new ZeroCollisionMap(
            roadName: this.Name,
            primaryLane: this.Lanes[this.LeftSidewalkIndex],
            layerMaskName: ZeroRoadBuilder.RoadSidewalkMaskName)
            .GetLaneIntersectionsByRoadName();
        Dictionary<string, ZeroLaneIntersection[]> rightIntersectionsByRoadName =
            new ZeroCollisionMap(
            roadName: this.Name,
            primaryLane: this.Lanes[this.RightSidewalkIndex],
            layerMaskName: ZeroRoadBuilder.RoadSidewalkMaskName)
            .GetLaneIntersectionsByRoadName();

        List<string> allCollidingRoads =
            leftIntersectionsByRoadName.Keys
            .Union(rightIntersectionsByRoadName.Keys)
            .ToList();
        foreach (var collidingRoadName in allCollidingRoads)
        {
            ZeroRoad collidingRoad = ZeroRoadBuilder.BuiltRoadsByName[collidingRoadName];

            ZeroLaneIntersection[] leftIntersections, rightIntersections;
            if (leftIntersectionsByRoadName
                .ContainsKey(collidingRoadName))
                leftIntersections =
                  leftIntersectionsByRoadName[collidingRoadName];
            else
                leftIntersections = new ZeroLaneIntersection[0];

            if (rightIntersectionsByRoadName
                .ContainsKey(collidingRoadName))
                rightIntersections =
                   rightIntersectionsByRoadName[collidingRoadName];
            else
                rightIntersections = new ZeroLaneIntersection[0];


            if (!intersectionsByRoadName.ContainsKey(collidingRoadName))
                intersectionsByRoadName[collidingRoadName] = new();

            intersectionsByRoadName[collidingRoadName].AddRange(
                ZeroRoadIntersection.GetRoadIntersections(
                    this.Height,
                    collidingRoad.Height,
                    this.ControlPoints,
                    leftIntersections,
                    rightIntersections));
        }
        return intersectionsByRoadName;
    }
}

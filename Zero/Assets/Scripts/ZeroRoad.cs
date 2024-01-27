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
    public Vector3[] CenterVertices;
    public int NumberOfLanesExclSidewalks;
    public int NumberOfLanesInclSidewalks;
    public int NumberOfSegmentsPerLane;
    public int LeftSidewalkIndex;
    public int RightSidewalkIndex;
    public bool IsCurved;
    public bool HasBusLane;
    public bool IsRoadAngleChangeValid;
    private bool _forceSyncTransform;
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
        InitialiseRoad(
            isPrimaryRoad: true,
            isCurved: isCurved,
            hasBusLane: hasBusLane,
            numberOfLanesExclSidewalks: numberOfLanesExclSidewalks,
            height: height,
            sidewalkHeight: sidewalkHeight,
            forceSyncTransform: forceSyncTransform);
        Build(controlPoints);
    }

    public ZeroRoad(
        ZeroRoad sourceRoad,
        Vector3[] centerVertices,
        Vector3[] controlPoints)
    {
        InitialiseRoad(
            isPrimaryRoad: false,
            isCurved: sourceRoad.IsCurved,
            hasBusLane: sourceRoad.HasBusLane,
            numberOfLanesExclSidewalks: sourceRoad.NumberOfLanesExclSidewalks,
            height: sourceRoad.Height,
            sidewalkHeight: sourceRoad.SidewalkHeight,
            forceSyncTransform: sourceRoad._forceSyncTransform);

        Build(
            controlPoints: controlPoints,
            centerVertices: centerVertices);
    }

    private void InitialiseRoad(
        bool isPrimaryRoad,
        bool isCurved,
        bool hasBusLane,
        int numberOfLanesExclSidewalks,
        float height,
        float sidewalkHeight,
        bool forceSyncTransform)
    {
        if (isPrimaryRoad)
            Name = "R" + ZeroRoadBuilder.BuiltRoadsByName.Count();
        else
            Name = "R"
            + (
                1/*active road*/
                + ZeroRoadBuilder.BuiltRoadsByName.Count()
                + ZeroRoadBuilder.ActiveSecondaryRoads.Count()).ToString();
        IsCurved = isCurved;
        NumberOfLanesExclSidewalks = numberOfLanesExclSidewalks;
        NumberOfLanesInclSidewalks = numberOfLanesExclSidewalks + 2;
        LeftSidewalkIndex = 0;
        RightSidewalkIndex = NumberOfLanesInclSidewalks - 1;
        WidthExclSidewalks = numberOfLanesExclSidewalks * ZeroRoadBuilder.RoadLaneWidth;
        WidthInclSidewalks = (numberOfLanesExclSidewalks + 2) * ZeroRoadBuilder.RoadLaneWidth;
        Height = height;
        SidewalkHeight = sidewalkHeight;
        HasBusLane = hasBusLane && numberOfLanesExclSidewalks > 1;
        _forceSyncTransform = forceSyncTransform;
        InitRoadObject();
    }

    private void InitRoadObject()
    {
    }

    public void Hide()
    {
        foreach (ZeroRoadLane lane in Lanes)
            lane.HideAllSegments();
    }

    public void Build(Vector3[] controlPoints)
    {
        IntersectionsByRoadName = new();
        ControlPoints = controlPoints;
        (Vector3[], float) bazierResult =
               ZeroCurvedLine.FindBazierLinePoints(
                   controlPoints);

        CenterVertices = bazierResult.Item1;
        NumberOfSegmentsPerLane = CenterVertices.Length;

        Lanes = GetLanes();
        IsRoadAngleChangeValid = true;

        foreach (var lane in Lanes)
            IsRoadAngleChangeValid &= lane.IsLaneAngleChangeValid;

        Debug.LogFormat("Road={0} roadAngleChangeValid={1}", Name, IsRoadAngleChangeValid);
        if (IsRoadAngleChangeValid)
        {
            IntersectionsByRoadName = GetRoadIntersectionsByRoad();
            if (IntersectionsByRoadName.Count() > 0)
            {
                foreach (var entry in IntersectionsByRoadName)
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
        if (_forceSyncTransform)
            Physics.SyncTransforms();
    }

    public void Build(
        Vector3[] controlPoints,
        Vector3[] centerVertices)
    {
        IntersectionsByRoadName = new();
        ControlPoints = controlPoints;
        CenterVertices = centerVertices;
        NumberOfSegmentsPerLane = CenterVertices.Length;
        Lanes = GetLanes();
        IsRoadAngleChangeValid = true;

        if (_forceSyncTransform)
            Physics.SyncTransforms();
    }

    public Dictionary<string, Vector3> GenerateTestData(string testName)
    {
        Dictionary<string, Vector3> testData = new();
        if (testName.Length > 0)
        {
            for (int i = 0; i < ControlPoints.Length; i++)
                testData[Name + "Control" + i.ToString()] = ControlPoints[i];
        }
        foreach (var lane in Lanes)
        {
            if (testName == ZeroRoadTest.Test1)
            {
                Lanes
                    .Select(
                        e => e.GetSegmentVertexLogs())
                    .SelectMany(e => e)
                    .ToList()
                    .ForEach(e => testData[e.Key] = e.Value);
            }
        }
        foreach (var entry in IntersectionsByRoadName)
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

    private ZeroRoadLane[] GetLanes()
    {
        Vector3[][] allLaneVertices = new Vector3[NumberOfLanesInclSidewalks * 2 + 1][];

        allLaneVertices[NumberOfLanesInclSidewalks] = CenterVertices;
        for (
            int leftIndex = NumberOfLanesInclSidewalks - 1, rightIndex = NumberOfLanesInclSidewalks + 1, distMultiplier = 1;
            leftIndex >= 0;
            leftIndex--, rightIndex++, distMultiplier++)
        {
            Vector3[][] parallelLines =
                GetParallelLines(
                    vertices: CenterVertices,
                    distance: distMultiplier * 0.5f * ZeroRoadBuilder.RoadLaneWidth);

            allLaneVertices[leftIndex] = parallelLines[0];
            allLaneVertices[rightIndex] = parallelLines[1];
        }

        ZeroRoadLane[] lanes = new ZeroRoadLane[NumberOfLanesInclSidewalks];

        int laneIndex = 0;
        for (; laneIndex < NumberOfLanesInclSidewalks; laneIndex++)
        {
            float height = Height;
            if (laneIndex == LeftSidewalkIndex || laneIndex == RightSidewalkIndex)
                height = SidewalkHeight;

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
            roadName: Name,
            primaryLane: Lanes[LeftSidewalkIndex],
            layerMaskName: ZeroRoadBuilder.RoadSidewalkMaskName)
            .GetLaneIntersectionsByRoadName();
        Dictionary<string, ZeroLaneIntersection[]> rightIntersectionsByRoadName =
            new ZeroCollisionMap(
            roadName: Name,
            primaryLane: Lanes[RightSidewalkIndex],
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
                    Height,
                    collidingRoad.Height,
                    ControlPoints,
                    leftIntersections,
                    rightIntersections));
        }
        return intersectionsByRoadName;
    }
}

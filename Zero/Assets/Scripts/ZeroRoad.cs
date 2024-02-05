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
    public float Length;
    public bool IsValid;
    public bool IsPrimaryRoad;
    public Vector3[] CenterVertices;
    public int NumberOfLanesExclSidewalks;
    public int NumberOfLanesInclSidewalks;
    public int LeftSidewalkIndex;
    public int RightSidewalkIndex;
    public bool IsCurved;
    public bool HasBusLane;
    public bool IsRoadAngleChangeValid;
    private bool _forceSyncTransform;
    public Vector3[] ControlPoints;
    public ZeroRoadLane[] Lanes;
    public ZeroRoadLane[] Sidewalks;
    public ZeroRoadIntersection[] Intersections;
    public ZeroRoad[] IntersectionBranchRoads;

    public ZeroRoad(
        bool isPrimaryRoad,
        bool isCurved = false,
        bool hasBusLane = false,
        int numberOfLanesExclSidewalks = -1,
        float height = -1,
        float sidewalkHeight = -1,
        bool forceSyncTransform = false,
        Vector3[] controlPoints = null,
        Vector3[] centerVertices = null,
        ZeroRoad sourceRoad = null)
    {
        if (isPrimaryRoad)
        {
            IsPrimaryRoad = isPrimaryRoad;
            InitialiseRoad(
                controlPoints: controlPoints,
                isCurved: isCurved,
                hasBusLane: hasBusLane,
                numberOfLanesExclSidewalks: numberOfLanesExclSidewalks,
                height: height,
                sidewalkHeight: sidewalkHeight,
                forceSyncTransform: forceSyncTransform);
        }
        else
        {
            IsPrimaryRoad = isPrimaryRoad;
            InitialiseRoad(sourceRoad, centerVertices);
        }
        Build();
    }

    private void InitialiseRoad(
        Vector3[] controlPoints,
        bool isCurved,
        bool hasBusLane,
        int numberOfLanesExclSidewalks,
        float height,
        float sidewalkHeight,
        bool forceSyncTransform)
    {
        Name = "R" + ZeroRoadBuilder.BuiltRoadsByName.Count();

        ControlPoints = controlPoints;
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
    }

    private void InitialiseRoad(
        ZeroRoad sourceRoad,
        Vector3[] centerVertices)
    {
        Name =
            "R"
            + (
                1/*active road*/
                + ZeroRoadBuilder.BuiltRoadsByName.Count()
                + ZeroRoadBuilder.ActiveSecondaryRoads.Count()).ToString();
        CenterVertices = centerVertices;
        IsCurved = sourceRoad.IsCurved;
        NumberOfLanesExclSidewalks = sourceRoad.NumberOfLanesExclSidewalks;
        NumberOfLanesInclSidewalks = sourceRoad.NumberOfLanesInclSidewalks;
        LeftSidewalkIndex = sourceRoad.LeftSidewalkIndex;
        RightSidewalkIndex = sourceRoad.RightSidewalkIndex;
        WidthExclSidewalks = sourceRoad.WidthExclSidewalks;
        WidthInclSidewalks = sourceRoad.WidthInclSidewalks;
        Height = sourceRoad.Height;
        SidewalkHeight = sourceRoad.SidewalkHeight;
        HasBusLane = sourceRoad.HasBusLane;
        _forceSyncTransform = sourceRoad._forceSyncTransform;
    }

    public void Hide()
    {
        foreach (ZeroRoadLane lane in Lanes)
            lane.HideAllSegments();
    }

    public void Build()
    {
        if (IsPrimaryRoad)
        {
            (Vector3[], float) bazierResult =
                   ZeroCurvedLine.FindBazierLinePoints(ControlPoints);
            CenterVertices = bazierResult.Item1;
        }
        if (GetLength(CenterVertices) > ZeroRoadBuilder.RoadMinimumLength)
        {
            Lanes = GetLanes();
            Length = Lanes.First().Segments.Last().RoadLengthSofar;
            IsRoadAngleChangeValid = true;

            foreach (var lane in Lanes)
                IsRoadAngleChangeValid &= lane.IsLaneAngleChangeValid;

            if (IsPrimaryRoad && IsRoadAngleChangeValid)
                BuildIntersections();

            if (!IsPrimaryRoad)
                ZeroRoadBuilder.ActiveSecondaryRoads[Name] = this;
            if (_forceSyncTransform)
                Physics.SyncTransforms();
        }
    }

    public void BuildIntersections()
    {
        ZeroCollisionMap leftCollisionMap =
            new(
                roadName: Name,
                primaryLane: Lanes[LeftSidewalkIndex],
                layerMaskName: ZeroRoadBuilder.RoadSidewalkMaskName);
        ZeroCollisionMap rightCollisionMap =
            new(
                roadName: Name,
                primaryLane: Lanes[RightSidewalkIndex],
                layerMaskName: ZeroRoadBuilder.RoadSidewalkMaskName);

        IsValid = ZeroRoadIntersection.GetRoadIntersectionsForPrimary(
           primaryRoad: this,
           leftIntersectionsByRoadName: leftCollisionMap.GetLaneIntersectionsByRoadName(),
           rightIntersectionsByRoadName: rightCollisionMap.GetLaneIntersectionsByRoadName());

        if (IsValid && Intersections != null)
        {
            foreach (ZeroRoadIntersection intersection in Intersections)
            {
                intersection.RenderLaneIntersections();
                // intersection.RenderSidewalks();
                // intersection.RenderCrosswalks();
                // intersection.RenderRoadEdges();
                // intersection.RenderMainSquare();
            }
        }
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
        foreach (ZeroRoadIntersection intersection in Intersections)
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
            // if (distMultiplier == 1)
            // {
            //     for (int j = 0; j < parallelLines[0].Count(); j++)
            //     {
            //         ZeroRenderer.RenderSphere(CenterVertices[j], Name + "_CV_" + j);
            //         ZeroRenderer.RenderSphere(parallelLines[0][j], Name + "_P0V_" + j);
            //         ZeroRenderer.RenderSphere(parallelLines[1][j], Name + "_P1V_" + j);
            //     }
            // }
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
        Vector3 leftVector = Vector3.Cross(forward, Vector3.up).normalized;

        leftPoint = originPoint + (leftVector * distance);
        rightPoint = originPoint - (leftVector * distance);
    }
    public static float GetLength(Vector3[] vertices)
    {
        float totalDistance = 0;
        for (int i = 1; i < vertices.Length; i++)
            totalDistance += (vertices[i] - vertices[i - 1]).magnitude;
        return totalDistance;
    }

}

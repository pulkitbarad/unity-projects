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
        int numberOfLanesExclSidewalks,
        float height,
        float sidewalkHeight,
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
        this.IntersectionsByRoadName = GetRoadIntersectionsByRoad();

        if (IntersectionsByRoadName.Count() > 0)
        {
            foreach (var entry in this.IntersectionsByRoadName)
            {
                foreach (ZeroRoadIntersection intersection in entry.Value)
                {
                    intersection.RenderLaneIntersections();
                    intersection.RenderSidewalks();
                    intersection.RenderCrosswalks();
                    intersection.RenderMainSquare();
                    intersection.RenderRoadEdges();
                    ZeroController.AppendToDebugLog(
                        intersection.SidewalksLogPairs());
                    ZeroController.AppendToDebugLog(
                        intersection.CrosswalksLogPairs());
                    ZeroController.AppendToDebugLog(
                        intersection.LaneIntersectionsLogPairs());
                    ZeroController.AppendToDebugLog(
                        intersection.MainSquareLogPairs());
                    ZeroController.AppendToDebugLog(
                        intersection.RoadEdgesLogPairs());
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

        List<string> allIntersectingRoads =
            leftIntersectionsByRoadName.Keys
            .Union(rightIntersectionsByRoadName.Keys)
            .ToList();
        foreach (var intersectingRoadName in allIntersectingRoads)
        {
            ZeroRoad intersectingRoad = ZeroRoadBuilder.BuiltRoadsByName[intersectingRoadName];

            ZeroLaneIntersection[] leftIntersections =
               leftIntersectionsByRoadName[intersectingRoadName];

            ZeroLaneIntersection[] rightIntersections =
                rightIntersectionsByRoadName[intersectingRoadName];

            List<ZeroRoadIntersection> roadIntersections = new();

            if (leftIntersections != null
                && rightIntersections != null)
            {
                if (!intersectionsByRoadName.ContainsKey(intersectingRoadName))
                    intersectionsByRoadName[intersectingRoadName] = new();

                intersectionsByRoadName[intersectingRoadName].AddRange(
                    GetRoadIntersections(
                        intersectingRoad.Height,
                        leftIntersections,
                        rightIntersections));
            }
        }
        return intersectionsByRoadName;
    }

    private ZeroRoadIntersection[] GetRoadIntersections(
        float intersectingRoadHeight,
        ZeroLaneIntersection[] leftIntersections,
        ZeroLaneIntersection[] rightIntersections)
    {

        List<ZeroRoadIntersection> roadIntersections = new();
        int leftIntersectionCount = leftIntersections.Length;
        int rightIntersectionCount = rightIntersections.Length;
        float intersectionHeight = Mathf.Max(this.Height, intersectingRoadHeight);

        if (leftIntersectionCount == 4 && rightIntersectionCount == 4)
        {
            roadIntersections.Add(new ZeroRoadIntersection(
                    height: intersectionHeight,
                    roadIntersectionType: ZeroRoadIntersection.ROAD_INTERSESCTION_TYPE_X,
                    laneIntersections: new ZeroLaneIntersection[]{
                        leftIntersections[0],
                        rightIntersections[0],
                        leftIntersections[1],
                        rightIntersections[1]
                    }
                ));
            roadIntersections.Add(new ZeroRoadIntersection(
                    height: intersectionHeight,
                    roadIntersectionType: ZeroRoadIntersection.ROAD_INTERSESCTION_TYPE_X,
                    laneIntersections: new ZeroLaneIntersection[]{
                        leftIntersections[2],
                        rightIntersections[2],
                        leftIntersections[3],
                        rightIntersections[3]
                    }
                ));
        }
        else if (leftIntersectionCount == 2 && rightIntersectionCount == 2)
        {
            roadIntersections.Add(new ZeroRoadIntersection(
                    height: intersectionHeight,
                    roadIntersectionType: ZeroRoadIntersection.ROAD_INTERSESCTION_TYPE_X,
                    laneIntersections: new ZeroLaneIntersection[]{
                        leftIntersections[0],
                        rightIntersections[0],
                        leftIntersections[1],
                        rightIntersections[1]
                    }
                ));
        }
        else if (leftIntersectionCount == 2 && rightIntersectionCount == 0)
        {
            roadIntersections.Add(new ZeroRoadIntersection(
                    height: intersectionHeight,
                    roadIntersectionType: ZeroRoadIntersection.ROAD_INTERSESCTION_TYPE_UP,
                    laneIntersections: new ZeroLaneIntersection[]{
                        leftIntersections[0],
                        leftIntersections[1]
                    }
                ));
        }
        else if (leftIntersectionCount == 0 && rightIntersectionCount == 2)
        {
            roadIntersections.Add(new ZeroRoadIntersection(
                    height: intersectionHeight,
                    roadIntersectionType: ZeroRoadIntersection.ROAD_INTERSESCTION_TYPE_UP,
                    laneIntersections: new ZeroLaneIntersection[]{
                        rightIntersections[0],
                        rightIntersections[1]
                    }
                ));
        }
        else if (leftIntersectionCount == 1 && rightIntersectionCount == 1)
        {
            int intersectionType;
            if (IsRoadStartInsideCollidingRoad(leftIntersections, rightIntersections))
                intersectionType = ZeroRoadIntersection.ROAD_INTERSESCTION_TYPE_RIGHT;
            else
                intersectionType = ZeroRoadIntersection.ROAD_INTERSESCTION_TYPE_LEFT;
            //
            roadIntersections.Add(new ZeroRoadIntersection(
                height: intersectionHeight,
                roadIntersectionType: intersectionType,
                laneIntersections: new ZeroLaneIntersection[]{
                leftIntersections[0],
                rightIntersections[0]
                }
            ));
        }
        return roadIntersections.ToArray();
    }

    private bool IsRoadStartInsideCollidingRoad(
        ZeroLaneIntersection[] leftIntersections,
        ZeroLaneIntersection[] rightIntersections
    )
    {
        float collidingRoadWidth =
                leftIntersections[0].IntersectionPoints[0]
                .CollidingSegment.ParentLane
                .ParentRoad.WidthInclSidewalks;
        ZeroRoadSegment leftSegment = leftIntersections[0].IntersectionPoints[0].PrimarySegment;
        ZeroRoadSegment rightSegment = rightIntersections[0].IntersectionPoints[0].PrimarySegment;
        Vector3 leftStartPoint = leftIntersections[0].IntersectionPoints[0].CollisionPoint;
        Vector3 rightStartPoint = rightIntersections[0].IntersectionPoints[0].CollisionPoint;
        Vector3 leftEndPoint = leftStartPoint + leftSegment.Forward * collidingRoadWidth;
        Vector3 rightEndPoint = rightStartPoint + rightSegment.Forward * collidingRoadWidth;

        return ZeroCollisionMap.IsPointInsideBounds(
            this.ControlPoints[0],
            new Vector3[]{
                    leftStartPoint,
                    leftEndPoint,
                    rightEndPoint,
                    rightStartPoint});
    }
}

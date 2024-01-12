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
    public float Width;
    public float Height;
    public float SidewalkHeight;
    public int VertexCount;
    public int NumberOfLanes;
    public int NumberOfSegmentsPerLane;
    public int LeftSidewalkIndex;
    public int RightSidewalkIndex;
    public bool IsCurved = true;
    public bool HasBusLane = false;
    public ZeroRoadLane[] Lanes;
    public ZeroRoadLane[] Sidewalks;
    public Dictionary<string, List<ZeroRoadIntersection>> IntersectionsByRoadName = new();

    public ZeroRoad(
        bool isCurved,
        bool hasBusLane,
        int numberOfLanes,
        float height,
        float sidewalkHeight)
    {
        this.Name = "R" + ZeroRoadBuilder.BuiltRoadsByName.Count();
        this.IsCurved = isCurved;
        this.NumberOfLanes = numberOfLanes;
        this.LeftSidewalkIndex = numberOfLanes;
        this.RightSidewalkIndex = numberOfLanes + 1;
        this.Width = numberOfLanes * ZeroRoadBuilder.RoadLaneWidth;
        this.Height = height;
        this.SidewalkHeight = sidewalkHeight;
        this.HasBusLane = hasBusLane && numberOfLanes > 1;
        InitRoadObject();

        if (!this.IsCurved)
            VertexCount = 4;

        ZeroRoadBuilder.RepositionControlObjects(isCurved);
        this.Build(true, Vector3.zero);
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

    public void Build(bool forceBuild, Vector2 touchPosition)
    {
        if (forceBuild || this.HandleControlsDrag(touchPosition))
        {
            Vector3[] controlPoints;
            if (this.IsCurved)
            {
                controlPoints = new Vector3[3];
                controlPoints[0] = ZeroRoadBuilder.StartObject.transform.position;
                controlPoints[1] = ZeroRoadBuilder.ControlObject.transform.position;
                controlPoints[2] = ZeroRoadBuilder.EndObject.transform.position;
            }
            else
            {
                controlPoints = new Vector3[2];
                controlPoints[0] = ZeroRoadBuilder.StartObject.transform.position;
                controlPoints[1] = ZeroRoadBuilder.EndObject.transform.position;
            }

            Vector3[] centerVertices =
                   ZeroCurvedLine.FindBazierLinePoints(
                       controlPoints);
            this.NumberOfSegmentsPerLane = centerVertices.Length;

            this.Lanes =
               GetLanes(
                   centerVertices: centerVertices);
            this.IntersectionsByRoadName = GetRoadIntersections();

            int i = 0;
            if (IntersectionsByRoadName.Count() > 0)

                foreach (var entry in this.IntersectionsByRoadName)
                {
                    foreach (ZeroRoadIntersection intersection in entry.Value)
                    {
                        // intersection.RenderCornerVertices();
                        intersection.RenderCrosswalkVertices();
                        i++;
                    }
                }
        }
    }

    private ZeroRoadLane[] GetLanes(Vector3[] centerVertices)
    {
        Vector3[] leftMostVertices =
            GetLeftParallelLine(
                vertices: centerVertices,
                distance: 0.5f * this.Width);
        ZeroRoadLane[] lanes = new ZeroRoadLane[this.NumberOfLanes + 2];
        int laneIndex = 0;
        for (; laneIndex < this.NumberOfLanes; laneIndex++)
        {
            float distanceFromLeftMostLane =
                (2 * laneIndex + 1) / 2f * ZeroRoadBuilder.RoadLaneWidth;

            ZeroRoadLane newLane =
                new(
                    laneIndex: laneIndex,
                    parentRoad: this,
                    width: ZeroRoadBuilder.RoadLaneWidth,
                    height: ZeroRoadBuilder.RoadLaneHeight,
                    centerVertices:
                        GetRightParallelLine(
                            vertices: leftMostVertices,
                            distance: distanceFromLeftMostLane));
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

        lanes[0] = new(
            laneIndex: this.LeftSidewalkIndex,
            parentRoad: this,
            width: ZeroRoadBuilder.RoadLaneWidth,
            height: this.SidewalkHeight,
            centerVertices: sideWalkCenterVertices[0]);
        lanes[1] = new(
            laneIndex: this.RightSidewalkIndex,
            parentRoad: this,
            width: ZeroRoadBuilder.RoadLaneWidth,
            height: this.SidewalkHeight,
            centerVertices: sideWalkCenterVertices[1]);
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
            ZeroRoadSegment.GetParallelPoints(
               originPoint: vertices[i - 1],
               targetPoint: vertices[i],
               distance: distance,
               out leftLine[i - 1],
               out rightLine[i - 1]);

            if (i == vertices.Length - 1)
            {
                ZeroRoadSegment.GetParallelPoints(
                    originPoint: vertices[i],
                    targetPoint: vertices[i - 1],
                    distance: distance,
                    out rightLine[i],
                    out leftLine[i]);
            }
        }
        return new Vector3[][] { leftLine, rightLine };
    }

    private bool HandleControlsDrag(Vector2 touchPosition)
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            var roadStartChanged =
                !ZeroRoadBuilder.StartObject.transform.position.Equals(Vector3.zero)
                && ZeroUIHandler.HandleGameObjectDrag(ZeroRoadBuilder.StartObject, touchPosition);

            var roadControlChanged =
                this.IsCurved
                && !ZeroRoadBuilder.ControlObject.transform.position.Equals(Vector3.zero)
                && ZeroUIHandler.HandleGameObjectDrag(ZeroRoadBuilder.ControlObject, touchPosition);

            var roadEndChanged =
                !ZeroRoadBuilder.EndObject.transform.position.Equals(Vector3.zero)
                && ZeroUIHandler.HandleGameObjectDrag(ZeroRoadBuilder.EndObject, touchPosition);

            return roadStartChanged || roadControlChanged || roadEndChanged;
        }
        else return false;
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
                   .ThenBy(e => (e.IntersectionPoints.LeftStart - e.PrimaryLane.Segments[0].SegmentBounds.TopPlane.LeftStart).magnitude)
                   .ToArray();

                ZeroLaneIntersection[] rightIntersections =
                    rightIntersectionsByRoadName[intersectingRoadName]
                    .OrderBy(e => e.PrimaryDistance)
                    .ThenBy(e => (e.IntersectionPoints.LeftStart - e.PrimaryLane.Segments[0].SegmentBounds.TopPlane.LeftStart).magnitude)
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
                        primaryRoadWidth: this.Width,
                        intersectingRoadWidth: intersectingRoad.Width,
                        leftStartIntersection: leftIntersections[0],
                        rightStartIntersection: rightIntersections[0],
                        leftEndIntersection: leftIntersections[1],
                        rightEndIntersection: rightIntersections[1]
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
                            primaryRoadWidth: this.Width,
                            intersectingRoadWidth: intersectingRoad.Width,
                            leftStartIntersection: leftIntersections[2],
                            rightStartIntersection: rightIntersections[2],
                            leftEndIntersection: leftIntersections[3],
                            rightEndIntersection: rightIntersections[3]
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

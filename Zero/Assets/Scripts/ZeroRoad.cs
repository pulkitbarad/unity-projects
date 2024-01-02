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
    public int LeftSidewalkIndex;
    public int RightSidewalkIndex;
    public bool IsCurved = true;
    public bool HasBusLane = false;
    public ZeroRoadLane[] Lanes;
    public ZeroRoadLane[] Sidewalks;
    public GameObject RoadObject;
    public Dictionary<string, List<ZeroRoadIntersection>> RoadIntersectionsByName = new();

    public ZeroRoad(
        bool isCurved,
        bool hasBusLane,
        int numberOfLanes,
        float height,
        float sidewalkHeight)
    {
        this.Name = "R" + ZeroRoadBuilder.BuiltRoads.Count();
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
        this.RoadObject =
            ZeroController.FindGameObject(this.Name, true)
                ?? new GameObject(this.Name);
        this.RoadObject.transform.localScale = Vector3.zero;
        this.RoadObject.transform.SetParent(ZeroRoadBuilder.BuiltRoadsParent.transform);
        this.RoadObject.SetActive(true);
        ZeroRoadBuilder.BuiltRoads[this.Name] = this;
    }

    public void Hide()
    {
        foreach (ZeroRoadLane lane in this.Lanes)
        {
            lane.HideAllSegments();
            lane.LaneObject.SetActive(false);
        }
        this.RoadObject.SetActive(false);
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
            this.Lanes =
               GetLanes(
                   centerVertices:
                   ZeroCurvedLine.FindBazierLinePoints(
                       controlPoints));
            GetRoadIntersections();
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
        Vector3[] parallelPoints = new Vector3[2];
        for (int i = 1; i < vertices.Length; i++)
        {
            parallelPoints = ZeroRoadSegment.GetParallelPoints(
                originPoint: vertices[i - 1],
                targetPoint: vertices[i],
                distance: distance);

            leftLine[i - 1] = parallelPoints[0];
            rightLine[i - 1] = parallelPoints[1];
            if (i == vertices.Length - 1)
            {
                parallelPoints = ZeroRoadSegment.GetParallelPoints(
                    originPoint: vertices[i],
                    targetPoint: vertices[i - 1],
                    distance: distance);
                leftLine[i] = parallelPoints[1];
                rightLine[i] = parallelPoints[0];
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

    public bool GetRoadIntersections()
    {
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
            List<string> listOfRoads = leftIntersectionsByRoadName.Keys.ToList();
            foreach (var intersectingRoadName in listOfRoads)
            {

                ZeroLaneIntersection[] leftIntersections =
                   leftIntersectionsByRoadName[intersectingRoadName]
                   .OrderBy(e => e.PrimaryLengthSoFar)
                   .ToArray();

                ZeroLaneIntersection[] rightIntersections =
                    rightIntersectionsByRoadName[intersectingRoadName]
                    .OrderBy(e => e.PrimaryLengthSoFar)
                    .ToArray();

                List<ZeroRoadIntersection> roadIntersections = new();

                if (leftIntersections != null
                    && leftIntersections.Length == 2
                    && leftIntersections.Length == rightIntersections.Length)
                {
                    for (int i = 0; i < leftIntersections.Length; i++)
                    {
                        ZeroLaneIntersection leftStartIntersection = leftIntersections[0];
                        ZeroLaneIntersection rightStartIntersection = rightIntersections[0];
                        ZeroLaneIntersection leftEndIntersection = leftIntersections[1];
                        ZeroLaneIntersection rightEndIntersection = rightIntersections[1];
                        if (
                            leftStartIntersection.IntersectingLane.ParentRoad.Name
                                .Equals(leftEndIntersection.IntersectingLane.ParentRoad.Name)
                            && leftStartIntersection.IntersectingLane.ParentRoad.Name
                                .Equals(rightStartIntersection.IntersectingLane.ParentRoad.Name)
                            && rightStartIntersection.IntersectingLane.ParentRoad.Name
                                .Equals(rightEndIntersection.IntersectingLane.ParentRoad.Name))
                        {
                            if (!this.RoadIntersectionsByName.ContainsKey(intersectingRoadName))
                            {
                                this.RoadIntersectionsByName[intersectingRoadName] = new();
                            }
                            this.RoadIntersectionsByName[intersectingRoadName].Add(new ZeroRoadIntersection(
                                primaryLengthSoFar: leftIntersections[0].PrimaryLengthSoFar,
                                leftStartIntersection: leftIntersections[0],
                                rightStartIntersection: rightIntersections[0],
                                leftEndIntersection: leftIntersections[1],
                                rightEndIntersection: rightIntersections[1]
                            ));
                        }
                    }
                }
            }
        }
        return true;
    }
}

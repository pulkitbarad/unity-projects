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
    public bool IsCurved = true;
    public bool HasBusLane = false;
    public ZeroRoadLane[] Lanes;
    public ZeroRoadLane[] Sidewalks;
    public GameObject RoadObject;

    public ZeroRoad(
        string name,
        bool isCurved,
        bool hasBusLane,
        int numberOfLanes,
        float height,
        float sidewalkHeight)
    {
        this.Name = name;
        this.IsCurved = isCurved;
        this.NumberOfLanes = numberOfLanes;
        this.Width = numberOfLanes * ZeroRoadBuilder.RoadLaneWidth;
        this.Height = height;
        this.SidewalkHeight = sidewalkHeight;
        this.HasBusLane = hasBusLane && numberOfLanes > 1;

        this.RoadObject =
            ZeroController.FindGameObject(name, true)
                ?? new GameObject("Road" + ZeroRoadBuilder.BuiltRoads.Count);
        this.RoadObject.transform.localScale = Vector3.zero;
        this.RoadObject.transform.SetParent(ZeroRoadBuilder.BuiltRoadsParent.transform);

        if (!this.IsCurved)
            VertexCount = 4;

        ZeroRoadBuilder.RepositionControlObjects(isCurved);
        this.Build(forceBuild: true, touchPosition: Vector2.zero);
    }

    public void Build(
        bool forceBuild,
        Vector2 touchPosition)
    {
        if (forceBuild || this.IsBuildRequired(touchPosition))
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

            Vector3[] centerVertices = ZeroCurvedLine.FindBazierLinePoints(controlPoints);

            this.Lanes =
                GetLanes(
                    leftMostVertices:
                        GetLeftParallelLine(
                            vertices: centerVertices,
                            distance: 0.5f * this.Width)
                        );
            this.Sidewalks = this.GetSideWalks(centerVertices, this.NumberOfLanes);

            this.RenderRoadLines(
                leftSegments: this.Lanes[0].Segments,
                rightSegments: this.Lanes[^1].Segments);
        }
    }

    private ZeroRoadLane[] GetLanes(Vector3[] leftMostVertices)
    {
        ZeroRoadLane[] lanes = new ZeroRoadLane[this.NumberOfLanes];
        for (int laneIndex = 0; laneIndex < this.NumberOfLanes; laneIndex++)
        {
            float distanceFromLeftMostLane =
                (2 * laneIndex + 1) / 2f * ZeroRoadBuilder.RoadLaneWidth;
            string laneName = this.Name + "Lane" + laneIndex;
            ZeroRoadSegment[] roadSegments =
                GetRoadSegments(
                    laneName: laneName,
                    width: ZeroRoadBuilder.RoadLaneWidth,
                    height: ZeroRoadBuilder.RoadLaneHeight,
                    centerVertices:
                        GetRightParallelLine(
                            vertices: leftMostVertices,
                            distance: distanceFromLeftMostLane));
            ZeroRoadLane newLane =
            new(
                name: laneName,
                laneIndex: laneIndex,
                parentRoad: this,
                segments: roadSegments);
            lanes[laneIndex] = newLane;
        }
        return lanes;
    }

    private ZeroRoadLane[] GetSideWalks(Vector3[] centerVertices, int lanesBuilt)
    {
        ZeroRoadLane[] lanes = new ZeroRoadLane[2];

        float distanceToSideWalkCenter =
            (this.NumberOfLanes + 1) * (0.5f * ZeroRoadBuilder.RoadLaneWidth);

        string leftSidewalkName = this.Name + "LeftSideWalk";
        string rightSidewalkName = this.Name + "RightSideWalk";

        Vector3[][] sideWalkCenterVertices =
            GetParallelLines(
                vertices: centerVertices,
                distance: distanceToSideWalkCenter);

        ZeroRoadSegment[] leftSideWalkSegments =
            GetRoadSegments(
                laneName: leftSidewalkName,
                width: ZeroRoadBuilder.RoadLaneWidth,
                height: this.SidewalkHeight,
                centerVertices: sideWalkCenterVertices[0]);

        ZeroRoadSegment[] rightSideWalkSegments =
            GetRoadSegments(
                laneName: rightSidewalkName,
                ZeroRoadBuilder.RoadLaneWidth,
                this.SidewalkHeight,
                centerVertices: sideWalkCenterVertices[1]);

        lanes[0] = new(
            name: leftSidewalkName,
            laneIndex: -1,
            parentRoad: this,
            segments: leftSideWalkSegments);
        lanes[1] = new(
            name: rightSidewalkName,
            laneIndex: this.NumberOfLanes,
            parentRoad: this,
            segments: rightSideWalkSegments);
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

    private ZeroRoadSegment[] GetRoadSegments(
        string laneName,
        float width,
        float height,
        Vector3[] centerVertices)
    {
        ZeroRoadSegment[] segments = new ZeroRoadSegment[centerVertices.Length - 1];
        for (int vertexIndex = 0; vertexIndex < centerVertices.Length - 1; vertexIndex++)
        {
            string roadSegmentName =
            String.Concat(
                laneName,
                "Segment",
                vertexIndex);
            Vector3 currVertex = centerVertices[vertexIndex];
            Vector3 nextVertex = centerVertices[vertexIndex + 1];
            Vector3 secondNextVertex =
                centerVertices[
                    vertexIndex == centerVertices.Length - 2
                    ? vertexIndex + 1
                    : vertexIndex + 2];
            ZeroRoadSegment previousSegment =
                vertexIndex == 0
                ? null
                : segments[vertexIndex - 1];

            float lengthSoFar = 0f;
            if (vertexIndex > 0)
                lengthSoFar = segments[vertexIndex - 1].DistanceToLaneStart;

            ZeroRoadSegment newSegment = new(
                        name: roadSegmentName,
                        index: vertexIndex,
                        width: width,
                        height: height,
                        distanceToLaneStart: lengthSoFar,
                        centerStart: currVertex,
                        centerEnd: nextVertex,
                        nextCenterEnd: secondNextVertex,
                        previousSibling: previousSegment,
                        renderSegment: false);
            segments[vertexIndex] = newSegment;
        }
        return segments;
    }

    private bool IsBuildRequired(Vector2 touchPosition)
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

    public void RenderRoad()
    {
        this.RenderLanes();
        this.RenderSidewalks();
        this.RenderIntersections();
        ZeroRoadBuilder.BuiltRoads[this.Name] = this;
    }

    private void RenderLanes()
    {
        for (int laneIndex = 0; laneIndex < this.Lanes.Length; laneIndex++)
        {
            RenderLane(this.Lanes[laneIndex].Segments);
        }
    }

    private void RenderSidewalks()
    {
        for (int sidewalkIndex = 0; sidewalkIndex < this.Sidewalks.Length; sidewalkIndex++)
        {
            ZeroRoadSegment[] segments = this.Sidewalks[sidewalkIndex].Segments;
            RenderLane(segments);
            for (int segmentIndex = 0; segmentIndex < segments.Length; segmentIndex++)
            {
                // GetIntersectionPoints(
                //     roadName: this.Name,
                //     segment: segments[segmentIndex],
                //     layerMaskName: RoadSidewalkMaskName,
                //     sideOfTheRoad: 2
                // );
            }
        }
    }

    private void RenderLane(ZeroRoadSegment[] segments)
    {
        for (int segmentIndex = 0; segmentIndex < segments.Length; segmentIndex++)
        {
            ZeroRoadSegment segment = segments[segmentIndex];
            ZeroRoadBuilder.BuiltRoadSegments[segment.Name] = segment;
            segment.InitSegmentObject();
        }
    }

    public void RenderRoadLines(ZeroRoadSegment[] leftSegments, ZeroRoadSegment[] rightSegments)
    {
        Vector3[] leftVertices = new Vector3[leftSegments.Length];
        Vector3[] rightVertices = new Vector3[leftSegments.Length];
        for (int i = 0; i < leftSegments.Length; i++)
        {
            leftVertices[i] = leftSegments[i].TopPlane.LeftStart;
            leftVertices[i] = leftSegments[i].TopPlane.LeftEnd;
            rightVertices[i] = rightSegments[i].TopPlane.RightStart;
            rightVertices[i] = rightSegments[i].TopPlane.RightEnd;
        }

        ZeroRoadBuilder.LeftLineObject =
        ZeroRenderer.RenderLine(
            name: ZeroRoadBuilder.RoadLeftEdgeObjectName,
            color: UnityEngine.Color.yellow,
            width: this.Width,
            pointSize: 0.5f,
            parentTransform: ZeroRoadBuilder.RoadControlsParent.transform,
            renderPoints: false,
            linePoints: leftVertices);

        ZeroRoadBuilder.RightLineObject =
        ZeroRenderer.RenderLine(
            name: ZeroRoadBuilder.RoadRightEdgeObjectName,
            color: UnityEngine.Color.red,
            width: this.Width,
            pointSize: 0.5f,
            parentTransform: ZeroRoadBuilder.RoadControlsParent.transform,
            renderPoints: false,
            linePoints: rightVertices);
    }

    public bool RenderIntersections()
    {
        ZeroLaneIntersection[] leftIntersections =
             new ZeroCollisionMap(
                 roadName: this.Name,
                 primaryLane: this.Sidewalks[0],
                 layerMaskName: ZeroRoadBuilder.RoadSidewalkMaskName).GetIntersections();

        ZeroLaneIntersection[] rightIntersections =
            new ZeroCollisionMap(
                roadName: this.Name,
                primaryLane: this.Sidewalks[0],
                layerMaskName: ZeroRoadBuilder.RoadSidewalkMaskName).GetIntersections();


        if (leftIntersections != null)
        {
            for (int i = 0; i < leftIntersections.Length; i++)
            {
                ZeroLaneIntersection intersection = leftIntersections[i];
                Vector3[] intersectionPoints = intersection.IntersectionPoints.GetVertices();
                for (int j = 0; j < intersectionPoints.Length; j++)
                {
                    // ZeroRenderer.RenderSphere(
                    //     intersectionPoints[j],
                    //     intersection.PrimaryLane.Name + "_" + intersection.IntersectingLane.Name + j,
                    //     color: Color.blue);
                }
            }
            for (int i = 0; i < rightIntersections.Length; i++)
            {
                ZeroLaneIntersection intersection = rightIntersections[i];
                Vector3[] intersectionPoints = intersection.IntersectionPoints.GetVertices();
                for (int j = 0; j < intersectionPoints.Length; j++)
                {
                    // ZeroRenderer.RenderSphere(
                    //     intersectionPoints[j],
                    //     intersection.PrimaryLane.Name + "_" + intersection.IntersectingLane.Name + j,
                    //     color: Color.red);
                }
            }
        }
        return true;
    }
}

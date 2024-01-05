using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ZeroRoadSegment
{
    public string Name;

    public ZeroPolygon3D SegmentBounds;
    public Vector3 Center;
    public Vector3 Forward;
    public Vector3 Up;
    public int Index;
    public float Width;
    public float Height;
    public float Length;
    public float RoadLengthSofar;
    public float OldLength;
    public bool IsPrimitiveShape = true;
    public Vector3 CenterStart;
    public Vector3 CenterEnd;
    public Vector3 NextCenterEnd;
    public GameObject SegmentObject;
    public ZeroRoadSegment PreviousSibling;
    public ZeroRoadSegment NextSibling;
    public ZeroRoadLane ParentLane;

    public ZeroRoadSegment(
         int index,
         float width,
         float height,
         float roadLengthSoFar,
         Vector3 centerStart,
         Vector3 centerEnd,
         Vector3 nextCenterEnd,
         ZeroRoadSegment previousSibling,
         ZeroRoadLane parentLane)
    {
        this.Index = index;
        this.ParentLane = parentLane;
        this.Name = parentLane.Name + "S" + index;
        this.PreviousSibling = previousSibling;
        this.Width = width;
        this.Height = height;
        this.CenterStart = centerStart;
        this.CenterEnd = centerEnd;

        this.NextCenterEnd = nextCenterEnd;
        this.RoadLengthSofar = roadLengthSoFar;

        this.Up = GetUpVector(
            start: centerStart,
            end: centerEnd);

        ComputeSegmentDimensions();

        this.SegmentBounds =
            new ZeroPolygon3D(
                name: this.Name,
                height: this.Height,
                up: this.Up,
                centerPlane: this.GetCenterPlane());
        // this.IsPrimitiveShape = ValidatePrimitiveShape();

        this.InitSegmentObject();
        if (this.Index > 0)
            this.PreviousSibling.NextSibling = this;
    }

    private bool ValidatePrimitiveShape()
    {
        ZeroParallelogram topPlane = this.SegmentBounds.TopPlane;
        var topPlaneAngle =
            Math.Round(
                Vector3.Angle(
                    topPlane.RightStart - topPlane.LeftStart,
                    topPlane.LeftEnd - topPlane.LeftStart));
        return topPlaneAngle == 90 || topPlaneAngle == 270;
    }

    private void ComputeSegmentDimensions()
    {
        float extension = GetSegmentExtension();

        this.Forward = this.CenterEnd - this.CenterStart;
        this.OldLength = this.Forward.magnitude;
        if (extension > 0)
        {
            this.CenterEnd += this.Forward.normalized * extension;
        }

        this.Center = GetSegmentCenter();

        this.Forward = this.CenterEnd - this.CenterStart;
        this.Length = this.Forward.magnitude;
        this.RoadLengthSofar += this.Length;

    }

    private Vector3 GetSegmentCenter()
    {
        return this.CenterStart + (this.CenterEnd - this.CenterStart) / 2 + 0.5f * this.Height * this.Up;
    }

    private float GetSegmentExtension()
    {

        if (this.NextCenterEnd == null || this.NextCenterEnd.Equals(this.CenterEnd))
            return 0;
        else
        {
            float angleBetweenSegments =
             Math.Abs(
                Vector3.Angle(
                    this.NextCenterEnd - this.CenterEnd,
                    this.CenterEnd - this.CenterStart));
            if (angleBetweenSegments > 180)
                angleBetweenSegments -= 180;

            return
                this.Width / 2
                * Math.Abs(
                    MathF.Sin(
                        angleBetweenSegments * MathF.PI / 180f));
        }
    }

    private ZeroParallelogram GetCenterPlane()
    {
        Vector3[] startPoints = GetParallelPoints(
                originPoint: this.CenterStart,
                targetPoint: this.CenterEnd,
                distance: this.Width * 0.5f);
        Vector3[] endPoints = GetParallelPoints(
                originPoint: this.CenterEnd,
                targetPoint: this.CenterStart,
                distance: this.Width * 0.5f);
        return new(
            name: this.Name + "Center",
            leftStart: startPoints[0],
            rightStart: startPoints[1],
            leftEnd: endPoints[1],
            rightEnd: endPoints[0]);

    }

    private void InitSegmentObject()
    {
        GameObject segmentObject =
             this.IsPrimitiveShape
                ? ZeroObjectManager.GetNewObject(this.Name, ZeroObjectManager.PoolType.ROAD_CUBE)
                : ZeroObjectManager.GetNewObject(this.Name, ZeroObjectManager.PoolType.ROAD_POLYGON_3D);

        segmentObject.transform.position = this.Center;
        segmentObject.transform.rotation = Quaternion.LookRotation(this.Forward);
        segmentObject.transform.localScale = new Vector3(this.Width, this.Height, this.Length);
        segmentObject.transform.SetParent(ZeroRoadBuilder.BuiltRoadSegmentsParent.transform);

        ZeroRoadLane parentLane = this.ParentLane;
        ZeroRoad parentRoad = parentLane.ParentRoad;
        int numOfLanes = parentRoad.NumberOfLanes;
        if (parentLane.LaneIndex == parentRoad.LeftSidewalkIndex || parentLane.LaneIndex == parentRoad.RightSidewalkIndex)
            segmentObject.layer = LayerMask.NameToLayer(ZeroRoadBuilder.RoadSidewalkMaskName);
        else if (parentLane.LaneIndex == 0 || parentLane.LaneIndex == numOfLanes - 1)
            segmentObject.layer = LayerMask.NameToLayer(ZeroRoadBuilder.RoadEdgeLaneMaskName);

        segmentObject.SetActive(true);
        this.SegmentObject = segmentObject;
        ZeroRoadBuilder.BuiltRoadSegments[this.Name] = this;

        if (!this.IsPrimitiveShape)
            RenderMesh();
    }

    private void RenderMesh()
    {
        List<Vector3> allVertices = new();
        List<int> allTriangles = new();

        allVertices.AddRange(this.SegmentBounds.TopPlane.GetMeshVertices(this.SegmentObject));
        allVertices.AddRange(this.SegmentBounds.BottomPlane.GetMeshVertices(this.SegmentObject));
        allTriangles.AddRange(this.SegmentBounds.TopPlane.GetMeshTriangles(this.SegmentObject));
        allTriangles.AddRange(this.SegmentBounds.BottomPlane.GetMeshTriangles(this.SegmentObject));
        allTriangles.AddRange(this.SegmentBounds.LeftPlane.GetMeshTriangles(this.SegmentObject));
        allTriangles.AddRange(this.SegmentBounds.RightPlane.GetMeshTriangles(this.SegmentObject));

        if (this.Index == 0)
            allTriangles.AddRange(this.SegmentBounds.BackPlane.GetMeshTriangles(this.SegmentObject));
        else if (this.Index == this.ParentLane.ParentRoad.NumberOfSegmentsPerLane)
            allTriangles.AddRange(this.SegmentBounds.FrontPlane.GetMeshTriangles(this.SegmentObject));

        Mesh mesh = new() { name = this.Name + "Mesh" };
        this.SegmentObject.AddComponent<MeshRenderer>();
        mesh.vertices = allVertices.ToArray();
        this.SegmentObject.AddComponent<MeshFilter>().mesh = mesh;
        mesh.RecalculateBounds();
    }
    private static Vector3 GetUpVector(
        Vector3 start,
        Vector3 end)
    {
        Vector3 forward = start - end;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        return Vector3.Cross(forward, right).normalized;
    }

    public static Vector3[] GetParallelPoints(
        Vector3 originPoint,
        Vector3 targetPoint,
        float distance)
    {
        Vector3 forwardVector = targetPoint - originPoint;
        Vector3 upVector = GetUpVector(originPoint, targetPoint);
        Vector3 leftVector = Vector3.Cross(forwardVector, upVector).normalized;
        var leftPoint = originPoint + (leftVector * distance);
        var rightPoint = originPoint - (leftVector * distance);
        return new Vector3[] { leftPoint, rightPoint };
    }


}

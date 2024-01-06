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
    public int SegmentObjectType = ZeroObjectManager.OBJECT_TYPE_ROAD_CUBE;
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
        (Vector3[] topVertices, Vector3[] bottomVertices) = GetTopAndBottomPlanes();
        this.SegmentBounds =
            new ZeroPolygon3D(
                name: this.Name,
                topVertices,
                bottomVertices);
        // this.IsPrimitiveShape = ValidatePrimitiveShape();

        this.InitSegmentObject();
        if (this.Index > 0)
            this.PreviousSibling.NextSibling = this;
    }

    private (Vector3[], Vector3[]) GetTopAndBottomPlanes()
    {
        Vector3[] centerPlane = this.GetCenterPlane();
        Vector3 halfUp = 0.5f * this.Height * this.Up;
        Vector3 halfDown = -0.5f * this.Height * this.Up;
        Vector3[] topPlane =
            new Vector3[] {
                centerPlane[0] + halfUp,
                centerPlane[1] + halfUp,
                centerPlane[2] + halfUp,
                centerPlane[3] + halfUp};

        Vector3[] bottomPlane =
            new Vector3[] {
                centerPlane[0] + halfDown,
                centerPlane[1] + halfDown,
                centerPlane[2] + halfDown,
                centerPlane[3] + halfDown};
        return (topPlane, bottomPlane);
    }

    private bool ValidatePrimitiveShape()
    {
        ZeroPolygon topPlane = this.SegmentBounds.TopPlane;
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

    private Vector3[] GetCenterPlane()
    {
        Vector3[] startPoints = GetParallelPoints(
                originPoint: this.CenterStart,
                targetPoint: this.CenterEnd,
                distance: this.Width * 0.5f);
        Vector3[] endPoints = GetParallelPoints(
                originPoint: this.CenterEnd,
                targetPoint: this.CenterStart,
                distance: this.Width * 0.5f);
        return
            new Vector3[] {
                startPoints[0],
                startPoints[1],
                endPoints[1],
                endPoints[0]};

    }

    private void InitSegmentObject()
    {
        GameObject segmentObject =
            ZeroObjectManager.GetObjectFromPool(this.Name, this.SegmentObjectType);

        segmentObject.name = this.Name;

        segmentObject.transform.position = this.Center;
        segmentObject.transform.localScale = new Vector3(this.Width, this.Height, this.Length);
        segmentObject.transform.rotation = Quaternion.LookRotation(this.Forward);
        segmentObject.transform.SetParent(ZeroRoadBuilder.BuiltRoadSegmentsParent.transform);

        ZeroRoadLane parentLane = this.ParentLane;
        ZeroRoad parentRoad = parentLane.ParentRoad;
        int numOfLanes = parentRoad.NumberOfLanes;
        if (parentLane.LaneIndex == parentRoad.LeftSidewalkIndex || parentLane.LaneIndex == parentRoad.RightSidewalkIndex)
            segmentObject.layer = LayerMask.NameToLayer(ZeroRoadBuilder.RoadSidewalkMaskName);
        else if (parentLane.LaneIndex == 0 || parentLane.LaneIndex == numOfLanes - 1)
            segmentObject.layer = LayerMask.NameToLayer(ZeroRoadBuilder.RoadEdgeLaneMaskName);

        this.SegmentObject = segmentObject;
        RegisterToLookups();

        if (this.SegmentObjectType == ZeroObjectManager.OBJECT_TYPE_ROAD_POLYGON_3D)
            RenderMesh();
    }

    private void RegisterToLookups()
    {
        if (!ZeroRoadBuilder.BuiltRoadSegmentsByLane.ContainsKey(this.ParentLane.Name))
            ZeroRoadBuilder.BuiltRoadSegmentsByLane[this.ParentLane.Name] = new();

        ZeroRoadBuilder.BuiltRoadSegmentsByLane[this.ParentLane.Name].Add(this);
        ZeroRoadBuilder.BuiltRoadSegmentsByName[this.Name] = this;
    }

    private void RenderMesh()
    {
        List<int> sidesToRender = new();
        if (this.Index == 0)
            sidesToRender.Add(ZeroPolygon3D.BackSideIndex);
        else if (this.Index == this.ParentLane.ParentRoad.NumberOfSegmentsPerLane - 1)
            sidesToRender.Add(ZeroPolygon3D.FrontSideIndex);
        else if (this.Index == this.ParentLane.ParentRoad.LeftSidewalkIndex)
            sidesToRender.Add(ZeroPolygon3D.LeftSideIndex);
        else if (this.Index == this.ParentLane.ParentRoad.RightSidewalkIndex)
            sidesToRender.Add(ZeroPolygon3D.RightSideIndex);

        Mesh mesh = new() { name = this.Name + "Mesh" };
        this.SegmentObject.AddComponent<MeshRenderer>();
        mesh.vertices = this.SegmentBounds.GetMeshVertices(this.SegmentObject);
        mesh.triangles = this.SegmentBounds.GetMeshTriangles(true, false, sidesToRender.ToArray());
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

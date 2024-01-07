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
    public int SegmentObjectType;
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
        Index = index;
        ParentLane = parentLane;
        Name = parentLane.Name + "S" + index;
        PreviousSibling = previousSibling;
        Width = width;
        Height = height;
        CenterStart = centerStart;
        CenterEnd = centerEnd;

        NextCenterEnd = nextCenterEnd;
        RoadLengthSofar = roadLengthSoFar;

        Up = GetUpVector(
            start: centerStart,
            end: centerEnd);

        ComputeSegmentDimensions();
        (Vector3[] topVertices, Vector3[] bottomVertices) = GetTopAndBottomPlanes();
        SegmentBounds =
            new ZeroPolygon3D(
                name: Name,
                topVertices,
                bottomVertices);
        //Mesh collision is not as responsive. Fix object type to cube.
        SegmentObjectType = ZeroObjectManager.OBJECT_TYPE_ROAD_CUBE;
        // SegmentObjectType = GetSegmentObjectType();

        InitSegmentObject();
        if (Index > 0)
            PreviousSibling.NextSibling = this;
    }

    private (Vector3[], Vector3[]) GetTopAndBottomPlanes()
    {
        Vector3[] centerPlane = GetCenterPlane();
        Vector3 halfUp = 0.5f * Height * Up;
        Vector3 halfDown = -0.5f * Height * Up;
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

    private int GetSegmentObjectType()
    {
        ZeroPolygon topPlane = SegmentBounds.TopPlane;
        var topPlaneAngle =
            Math.Round(
                Vector3.Angle(
                    topPlane.RightStart - topPlane.LeftStart,
                    topPlane.LeftEnd - topPlane.LeftStart));
        if (topPlaneAngle == 90 || topPlaneAngle == 270)
            return SegmentObjectType = ZeroObjectManager.OBJECT_TYPE_ROAD_CUBE;
        else
            return SegmentObjectType = ZeroObjectManager.OBJECT_TYPE_ROAD_POLYGON_3D;
    }

    private void ComputeSegmentDimensions()
    {
        float extension = GetSegmentExtension();

        Forward = CenterEnd - CenterStart;
        OldLength = Forward.magnitude;
        if (extension > 0)
        {
            CenterEnd += Forward.normalized * extension;
        }

        Center = GetSegmentCenter();

        Forward = CenterEnd - CenterStart;
        Length = Forward.magnitude;
        RoadLengthSofar += Length;

    }

    private Vector3 GetSegmentCenter()
    {
        return CenterStart + (CenterEnd - CenterStart) / 2 + 0.5f * Height * Up;
    }

    private float GetSegmentExtension()
    {

        if (NextCenterEnd == null || NextCenterEnd.Equals(CenterEnd))
            return 0;
        else
        {
            float angleBetweenSegments =
             Math.Abs(
                Vector3.Angle(
                    NextCenterEnd - CenterEnd,
                    CenterEnd - CenterStart));
            if (angleBetweenSegments > 180)
                angleBetweenSegments -= 180;

            return
                Width / 2
                * Math.Abs(
                    MathF.Sin(
                        angleBetweenSegments * MathF.PI / 180f));
        }
    }

    private Vector3[] GetCenterPlane()
    {
        GetParallelPoints(
                originPoint: CenterStart,
                targetPoint: CenterEnd,
                distance: Width * 0.5f,
                out Vector3 leftStart,
                out Vector3 rightStart);
        GetParallelPoints(
              originPoint: CenterEnd,
              targetPoint: CenterStart,
              distance: Width * 0.5f,
              out Vector3 rightEnd,
              out Vector3 leftEnd);
        return
            new Vector3[] {
                leftStart,
                leftEnd,
                rightEnd,
                rightStart};

    }

    private void InitSegmentObject()
    {
        GameObject segmentObject =
            ZeroObjectManager.GetObjectFromPool(Name, SegmentObjectType);

        segmentObject.name = Name;

        segmentObject.transform.position = Center;
        segmentObject.transform.localScale = new Vector3(Width, Height, Length);
        segmentObject.transform.rotation = Quaternion.LookRotation(Forward);
        segmentObject.transform.SetParent(ZeroRoadBuilder.BuiltRoadSegmentsParent.transform);

        ZeroRoadLane parentLane = ParentLane;
        ZeroRoad parentRoad = parentLane.ParentRoad;
        int numOfLanes = parentRoad.NumberOfLanes;
        if (parentLane.LaneIndex == parentRoad.LeftSidewalkIndex || parentLane.LaneIndex == parentRoad.RightSidewalkIndex)
            segmentObject.layer = LayerMask.NameToLayer(ZeroRoadBuilder.RoadSidewalkMaskName);
        else if (parentLane.LaneIndex == 0 || parentLane.LaneIndex == numOfLanes - 1)
            segmentObject.layer = LayerMask.NameToLayer(ZeroRoadBuilder.RoadEdgeLaneMaskName);

        SegmentObject = segmentObject;
        RegisterToLookups();

        if (SegmentObjectType == ZeroObjectManager.OBJECT_TYPE_ROAD_POLYGON_3D)
            RenderMesh();
    }

    private void RegisterToLookups()
    {
        if (!ZeroRoadBuilder.BuiltRoadSegmentsByLane.ContainsKey(ParentLane.Name))
            ZeroRoadBuilder.BuiltRoadSegmentsByLane[ParentLane.Name] = new();

        ZeroRoadBuilder.BuiltRoadSegmentsByLane[ParentLane.Name].Add(this);
        ZeroRoadBuilder.BuiltRoadSegmentsByName[Name] = this;
    }

    private void RenderMesh()
    {
        List<int> sidesToRender = new()
        {
            ZeroPolygon3D.BackSideIndex,
            ZeroPolygon3D.FrontSideIndex,
            ZeroPolygon3D.LeftSideIndex,
            ZeroPolygon3D.RightSideIndex,

        };
        // if (this.Index == 0)
        //     sidesToRender.Add(ZeroPolygon3D.BackSideIndex);
        // else if (this.Index == this.ParentLane.ParentRoad.NumberOfSegmentsPerLane - 1)
        //     sidesToRender.Add(ZeroPolygon3D.FrontSideIndex);
        // else if (this.Index == this.ParentLane.ParentRoad.LeftSidewalkIndex)
        //     sidesToRender.Add(ZeroPolygon3D.LeftSideIndex);
        // else if (this.Index == this.ParentLane.ParentRoad.RightSidewalkIndex)
        //     sidesToRender.Add(ZeroPolygon3D.RightSideIndex);

        if (!SegmentObject.TryGetComponent<MeshRenderer>(out _))
            SegmentObject.AddComponent<MeshRenderer>();

        if (!SegmentObject.TryGetComponent<MeshFilter>(out MeshFilter meshFilter))
        {
            meshFilter = SegmentObject.AddComponent<MeshFilter>();
            meshFilter.mesh = new()
            {
                name = Name + "Mesh"
            };
        }
        else
            meshFilter.mesh.Clear();
        meshFilter.mesh.vertices = SegmentBounds.GetMeshVertices(SegmentObject);
        // mesh.triangles = this.SegmentBounds.GetMeshTriangles(true, false, sidesToRender.ToArray());
        meshFilter.mesh.triangles = SegmentBounds.GetMeshTriangles(true, true, sidesToRender.ToArray());
        meshFilter.mesh.RecalculateBounds();

        if (!SegmentObject.TryGetComponent<MeshCollider>(out MeshCollider meshCollider))
            meshCollider = SegmentObject.AddComponent<MeshCollider>();
        meshCollider.convex = true;
    }

    private static Vector3 GetUpVector(
        Vector3 start,
        Vector3 end)
    {
        Vector3 forward = start - end;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        return Vector3.Cross(forward, right).normalized;
    }

    public static void GetParallelPoints(
        Vector3 originPoint,
        Vector3 targetPoint,
        float distance,
        out Vector3 leftPoint,
        out Vector3 rightPoint)
    {
        Vector3 forwardVector = targetPoint - originPoint;
        Vector3 upVector = GetUpVector(originPoint, targetPoint);
        Vector3 leftVector = Vector3.Cross(forwardVector, upVector).normalized;
        leftPoint = originPoint + (leftVector * distance);
        rightPoint = originPoint - (leftVector * distance);
    }
}

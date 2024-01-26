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
    public int SegmentObjectType;
    public Vector3 CenterStart;
    public Vector3 CenterEnd;
    public Vector3 NextCenterEnd;
    public Vector3 DirectionL2R;
    public bool IsSegmentAngleChangeValid;
    public GameObject SegmentObject;
    public ZeroRoadSegment PreviousSibling;
    public ZeroRoadSegment NextSibling;
    public ZeroRoadLane ParentLane;


    public ZeroRoadSegment(
         int index,
         float width,
         float height,
         Vector3[] centerVertices,
         Vector3 centerStart,
         Vector3 centerEnd,
         Vector3 nextCenterEnd,
         ZeroRoadSegment previousSibling,
         ZeroRoadLane parentLane)
    {
        Index = index;
        ParentLane = parentLane;
        Name = parentLane.Name + "_S" + index;
        PreviousSibling = previousSibling;
        Width = width;
        Height = height;
        CenterStart = centerStart;
        CenterEnd = centerEnd;

        NextCenterEnd = nextCenterEnd;

        SegmentObjectType = ZeroObjectManager.OBJECT_TYPE_ROAD_CUBE;
        Up = GetUpVector(centerVertices);

        ComputeSegmentDimensions();
        SegmentBounds =
            new ZeroPolygon3D(
                name: Name,
                Height,
                Up,
                centerVertices);
        DirectionL2R = (SegmentBounds.TopPlane[3] - SegmentBounds.TopPlane[0]).normalized;
        InitSegmentObject();
        if (Index > 0)
            PreviousSibling.NextSibling = this;
        this.IsSegmentAngleChangeValid = ValidateSegmentAngleChange();
    }

    private void ComputeSegmentDimensions()
    {
        float roadLengthSoFar =
            Index == 0
            ? 0
            : PreviousSibling.RoadLengthSofar;
        float extension = GetSegmentExtension();
        if (extension > 0)
            CenterEnd += (CenterEnd - CenterStart).normalized * extension;

        Center = GetSegmentCenter();
        Forward = CenterEnd - CenterStart;
        Length = Forward.magnitude;
        RoadLengthSofar = roadLengthSoFar + Length;
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

    private bool ValidateSegmentAngleChange()
    {
        if (this.Index > 1)
            return
                ValidateSegmentCollision(this.PreviousSibling.PreviousSibling, this)
                && ValidateSegmentCollision(this, this.PreviousSibling.PreviousSibling);
        return true;
    }

    private bool ValidateSegmentCollision(
        ZeroRoadSegment segment,
        ZeroRoadSegment refSegment)
    {
        Vector3 topPlaneCenter = segment.Center + 0.5f * segment.Height * segment.Up;
        float diagonalAngle = Vector3.Angle(segment.SegmentBounds.TopPlane[3] - topPlaneCenter, segment.Forward);
        float collisionRadius = segment.Width / 2 / Mathf.Sin(diagonalAngle / 180f * Mathf.PI);
        Vector3[] refTopPlane = refSegment.SegmentBounds.TopPlane;


        if (refSegment.Index > segment.Index)
            return (refTopPlane[0] - topPlaneCenter).magnitude > collisionRadius
                 && (refTopPlane[3] - topPlaneCenter).magnitude > collisionRadius;
        else
            return (refTopPlane[1] - topPlaneCenter).magnitude > collisionRadius
                 && (refTopPlane[2] - topPlaneCenter).magnitude > collisionRadius;
    }

    private void InitSegmentObject()
    {
        ZeroObjectManager.ReleaseObjectToPool(Name, SegmentObjectType);
        GameObject segmentObject =
            ZeroObjectManager.GetObjectFromPool(Name, SegmentObjectType);

        segmentObject.name = Name;

        segmentObject.transform.position = Center;
        segmentObject.transform.localScale = new Vector3(Width, Height, Length);
        segmentObject.transform.rotation = Quaternion.LookRotation(Forward);

        segmentObject.transform.SetParent(ZeroRoadBuilder.BuiltRoadSegmentsParent.transform);
        segmentObject.GetComponent<MeshRenderer>().material = ZeroRoadBuilder.RoadSegmentMaterial;

        ZeroRoadLane parentLane = ParentLane;
        ZeroRoad parentRoad = parentLane.ParentRoad;
        if (parentLane.IsLeftSidewalk || parentLane.IsRightSidewalk)
            segmentObject.layer = LayerMask.NameToLayer(ZeroRoadBuilder.RoadSidewalkMaskName);
        else
            segmentObject.layer = LayerMask.NameToLayer(ZeroRoadBuilder.RoadLaneMaskName);

        SegmentObject = segmentObject;
        RegisterToLookups();
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
        Vector3[] centerVertices)
    {
        return Vector3.Cross(
                centerVertices[1] - centerVertices[0],
                centerVertices[3] - centerVertices[0]).normalized;
    }
}

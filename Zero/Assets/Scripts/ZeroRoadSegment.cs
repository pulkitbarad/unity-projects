using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ZeroRoadSegment
{
    public string Name;
    public ZeroPolygon3D SegmentPolygon;
    public Vector3 Center;
    public Vector3 Forward;
    public Vector3 Up;
    public int Index;
    public float Width;
    public float Height;
    public float RoadLengthSofar;
    public GameObject SegmentObject;
    public ZeroRoadLane ParentLane;
    private Vector3[] _centerVertices;
    private Vector3 _centerStart;
    private Vector3 _centerEnd;

    public ZeroRoadSegment()
    {
    }

    public ZeroRoadSegment(
         int index,
         float width,
         float height,
         float roadLengthSoFar,
         Vector3[] centerVertices,
         Vector3 centerStart,
         Vector3 centerEnd,
         ZeroRoadLane parentLane)
    {
        _centerStart = centerStart;
        _centerEnd = centerEnd;
        _centerVertices = centerVertices;
        Index = index;
        ParentLane = parentLane;
        Name = parentLane.Name + "S" + index;
        Width = width;
        Height = height;
        RoadLengthSofar = roadLengthSoFar;
        Forward = centerEnd - _centerStart;

        Up = GetUpVector(centerVertices);
        Center = _centerStart + 0.5f * Forward;

        SegmentPolygon =
            new ZeroPolygon3D(
                name: Name,
                height: height,
                up: Up,
                centerVertices: centerVertices);

        InitSegmentObject();
    }

    public (float, Vector3) GetColliderLengthAndCenter()
    {

        float length;
        Vector3 center;
        Vector3 leftForward = _centerVertices[1] - _centerVertices[0];
        Vector3 rightForwrad = _centerVertices[3] - _centerVertices[2];
        if (leftForward.magnitude != rightForwrad.magnitude)
        {
            Vector3 edgeMidPoint, edgeForward, edgeSideways;
            if (leftForward.magnitude > rightForwrad.magnitude)
            {
                edgeForward = leftForward;
                edgeSideways = Vector3.Cross(Forward, -Up);
            }
            else
            {
                edgeForward = rightForwrad;
                edgeSideways = Vector3.Cross(Forward, Up);
            }
            edgeMidPoint = _centerVertices[0] + (0.5f * edgeForward);
            length = edgeForward.magnitude;
            center = edgeMidPoint + edgeSideways * Width;
        }
        else
        {
            length = rightForwrad.magnitude;
            center = _centerStart + 0.5f * Forward;
        }
        return (length, center);
    }

    private static Vector3 GetUpVector(
        Vector3[] centerVertices)
    {
        return Vector3.Cross(
                centerVertices[1] - centerVertices[0],
                centerVertices[3] - centerVertices[0]).normalized;
    }

    private void InitSegmentObject()
    {
        ZeroObjectManager.ReleaseObjectToPool(Name, ZeroObjectManager.OBJECT_TYPE_ROAD_POLYGON_3D);
        GameObject segmentObject =
            ZeroObjectManager.GetObjectFromPool(Name, ZeroObjectManager.OBJECT_TYPE_ROAD_POLYGON_3D);

        segmentObject.name = Name;

        segmentObject.transform.position = Center;
        // segmentObject.transform.localScale = new Vector3(Width, Height, Length);
        segmentObject.transform.localScale = new Vector3(1, 1, 1);
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

        SegmentObject.GetComponent<MeshRenderer>().material = ZeroRoadBuilder.RoadSegmentMaterial;

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
        meshFilter.mesh.vertices = SegmentPolygon.GetMeshVertices(SegmentObject);
        meshFilter.mesh.triangles = SegmentPolygon.GetMeshTriangles(true, true, sidesToRender.ToArray());
        meshFilter.mesh.RecalculateBounds();

        if (!SegmentObject.TryGetComponent<MeshCollider>(out MeshCollider meshCollider))
            meshCollider = SegmentObject.AddComponent<MeshCollider>();
        meshCollider.convex = true;
    }
}

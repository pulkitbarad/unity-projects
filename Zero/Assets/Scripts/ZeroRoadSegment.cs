using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public float Length;
    public float RoadLengthSofar;
    public GameObject SegmentObject;
    public ZeroRoadLane ParentLane;

    public ZeroRoadSegment()
    {
    }

    public ZeroRoadSegment(
         int index,
         float width,
         float height,
         float length,
         float roadLengthSoFar,
         Vector3[] centerVertices,
         Vector3 center,
         ZeroRoadLane parentLane)
    {
        Index = index;
        ParentLane = parentLane;
        Name = parentLane.Name + "S" + index;
        Width = width;
        Height = height;
        Length = length;
        RoadLengthSofar = roadLengthSoFar;
        Center = center;
        Up = GetUpVector(centerVertices);

        SegmentPolygon =
            new ZeroPolygon3D(
                name: Name,
                height: height,
                up: Up,
                centerVertices: centerVertices);

        InitSegmentObject();
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
        meshFilter.mesh.vertices = SegmentPolygon.GetMeshVertices(SegmentObject);
        // mesh.triangles = this.SegmentBounds.GetMeshTriangles(true, false, sidesToRender.ToArray());
        meshFilter.mesh.triangles = SegmentPolygon.GetMeshTriangles(true, true, sidesToRender.ToArray());
        meshFilter.mesh.RecalculateBounds();

        if (!SegmentObject.TryGetComponent<MeshCollider>(out MeshCollider meshCollider))
            meshCollider = SegmentObject.AddComponent<MeshCollider>();
        meshCollider.convex = true;
    }
}

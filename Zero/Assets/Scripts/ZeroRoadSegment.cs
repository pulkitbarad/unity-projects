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
    public ZeroParallelogram TopPlane;
    public ZeroParallelogram BottomPlane;
    public Vector3 Center;
    public Vector3 Forward;
    public Vector3 Up;
    public int Index;
    public float Width;
    public float Height;
    public float Length;
    public float DistanceToLaneStart;
    public float OldLength;
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
         float distanceToLaneStart,
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

        this.Up = GetUpVector(
            start: centerStart,
            end: centerEnd);

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
        this.DistanceToLaneStart = distanceToLaneStart + this.Length;

        ZeroParallelogram[] planes = this.GetPlanes();

        this.TopPlane = planes[0];
        this.BottomPlane = planes[1];
        this.InitSegmentObject();
        if (this.PreviousSibling != null)
            this.PreviousSibling.NextSibling = this;
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

    private ZeroParallelogram[] GetPlanes()
    {
        Vector3[] startPoints = GetParallelPoints(
                originPoint: this.CenterStart,
                targetPoint: this.CenterEnd,
                distance: this.Width * 0.5f);
        Vector3[] endPoints = GetParallelPoints(
                originPoint: this.CenterEnd,
                targetPoint: this.CenterStart,
                distance: this.Width * 0.5f);
        Vector3 halfUp = 0.5f * this.Height * this.Up;
        Vector3 halfDown = -0.5f * this.Height * this.Up;
        ZeroParallelogram topPlane =
            new(
                leftStart: startPoints[0] + halfUp,
                rightStart: startPoints[1] + halfUp,
                leftEnd: endPoints[1] + halfUp,
                rightEnd: endPoints[0] + halfUp);

        ZeroParallelogram bottomPlane =
            new(
                 leftStart: startPoints[0] + halfDown,
                 rightStart: startPoints[1] + halfDown,
                 leftEnd: endPoints[1] + halfDown,
                 rightEnd: endPoints[0] + halfDown);

        return new ZeroParallelogram[] { topPlane, bottomPlane };
    }

    private void InitSegmentObject()
    {
        GameObject segmentObject =
            ZeroController.FindGameObject(this.Name, true)
            ?? GameObject.CreatePrimitive(PrimitiveType.Cube);

        segmentObject.name = this.Name;
        segmentObject.transform.position = this.Center;
        segmentObject.transform.rotation = Quaternion.LookRotation(this.Forward);
        segmentObject.transform.localScale = new Vector3(this.Width, this.Height, this.Length);
        segmentObject.transform.SetParent(ZeroRoadBuilder.BuiltRoadSegmentsParent.transform);

        int numOfLanes = this.ParentLane.ParentRoad.NumberOfLanes;
        if (this.ParentLane.LaneIndex == numOfLanes || this.ParentLane.LaneIndex == numOfLanes + 1)
            segmentObject.layer = LayerMask.NameToLayer(ZeroRoadBuilder.RoadSidewalkMaskName);
        else if (this.ParentLane.LaneIndex == 0 || this.ParentLane.LaneIndex == numOfLanes - 1)
            segmentObject.layer = LayerMask.NameToLayer(ZeroRoadBuilder.RoadEdgeLaneMaskName);

        segmentObject.SetActive(true);
        this.SegmentObject = segmentObject;
        ZeroRoadBuilder.BuiltRoadSegments[this.Name] = this;
        // RenderMesh(
        //     topPlane: this.TopPlane,
        //     bottomPlane: this.BottomPlane,
        //     segment: segmentObject);

    }

    // private void AddBoxCollider()
    // {
    //     if (!this.SegmentObject.TryGetComponent<BoxCollider>(out var boxCollider))
    //     {
    //         boxCollider = this.SegmentObject.AddComponent<BoxCollider>();
    //     }
    //     Debug.Log("segment=" + this.Name);
    //     Debug.Log("old length=" + this.OldLength);
    //     Debug.Log("new length=" + this.Length);
    //     boxCollider.center = new(0, 0, 0 - (0.5f * (this.OldLength - this.Length) / this.Length));
    //     boxCollider.size = new(1, 1, this.OldLength / this.Length);
    //     Debug.Log("boxCollider.center=" + boxCollider.center);
    //     Debug.Log("boxCollider.size=" + boxCollider.size);
    // }

    private void RenderMesh()
    {
        Vector3[] allVertices = new Vector3[2 * this.BottomPlane.GetVertices().Length];

        for (int i = 0; i < this.TopPlane.GetVertices().Length; i++)
        {
            allVertices[i] =
                this.SegmentObject.transform.InverseTransformPoint(this.TopPlane.GetVertices()[i]);
            allVertices[i + this.TopPlane.GetVertices().Length] =
                this.SegmentObject.transform.InverseTransformPoint(this.BottomPlane.GetVertices()[i]);
        }

        Mesh mesh = new() { name = "Generated" };
        this.SegmentObject.AddComponent<MeshRenderer>();
        mesh.vertices =
            new Vector3[]{
                    //Top
                    new (-0.5f,0.5f,1),
                    new (0.5f,0.5f,1),
                    new (0.5f,0.5f,-1),
                    new (-0.5f,0.5f,-1),
                    new (-0.5f,0.5f,-1),
                    new (-0.5f,-0.5f,1),
                    //Bottom
                    new (0.5f,-0.5f,1),
                    new (0.5f,-0.5f,-1),
                    new (-0.5f,-0.5f,-1),
                    new (-0.5f,-0.5f,-1),
                };
        mesh.triangles = new int[] { 
                //Top
                0,1, 2,
                0, 2, 3, 
                //Bottom
                4,5,6,
                4,6,7
                };
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

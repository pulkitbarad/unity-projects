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
         string name,
         int index,
         float width,
         float height,
         float distanceToLaneStart,
         Vector3 centerStart,
         Vector3 centerEnd,
         Vector3 nextCenterEnd,
         ZeroRoadSegment previousSibling,
         bool renderSegment)
    {
        this.Name = name;
        this.PreviousSibling = previousSibling;
        this.Index = index;
        this.Width = width;
        this.Height = height;
        this.CenterStart = centerStart;
        this.CenterEnd = centerEnd;

        this.NextCenterEnd = nextCenterEnd;

        this.Up = GetUpVector(
            start: centerStart,
            end: centerEnd);

        float extension = GetSegmentExtension(
            width: width,
            centerStart: centerStart,
            centerEnd: centerEnd,
            nextCenterEnd: nextCenterEnd);

        this.Forward = this.CenterEnd - this.CenterStart;
        this.OldLength = this.Forward.magnitude;
        if (extension > 0)
        {
            this.CenterEnd += this.Forward.normalized * extension;
        }

        this.Center = GetSegmentCenter(
            height: height,
            centerStart: this.CenterStart,
            centerEnd: this.CenterEnd,
            up: this.Up);

        this.Forward = this.CenterEnd - this.CenterStart;
        this.Length = this.Forward.magnitude;
        this.DistanceToLaneStart = distanceToLaneStart + this.Length;

        ZeroParallelogram[] planes =
            this.GetPlanes(
                width: this.Width,
                height: this.Height,
                centerStart: this.CenterStart,
                centerEnd: this.CenterEnd,
                up: this.Up);

        this.TopPlane = planes[0];
        this.BottomPlane = planes[1];
        if (renderSegment)
            this.InitSegmentObject();
        if (this.PreviousSibling != null)
            this.PreviousSibling.NextSibling = this;
    }

    private Vector3 GetSegmentCenter(
        float height,
        Vector3 centerStart,
        Vector3 centerEnd,
        Vector3 up)
    {
        return centerStart + (centerEnd - centerStart) / 2 + 0.5f * height * up;
    }

    private float GetSegmentExtension(
        float width,
        Vector3 centerStart,
        Vector3 centerEnd,
        Vector3 nextCenterEnd)
    {

        if (nextCenterEnd == null || nextCenterEnd.Equals(centerEnd))
            return 0;
        else
        {
            float angleBetweenSegments =
             Math.Abs(
                Vector3.Angle(
                    nextCenterEnd - centerEnd,
                    centerEnd - centerStart));
            if (angleBetweenSegments > 180)
                angleBetweenSegments -= 180;

            return
                width / 2
                * Math.Abs(
                    MathF.Sin(
                        angleBetweenSegments * MathF.PI / 180f));
        }
    }

    private ZeroParallelogram[] GetPlanes(
        float width,
        float height,
        Vector3 centerStart,
        Vector3 centerEnd,
        Vector3 up)
    {
        Vector3[] startPoints = GetParallelPoints(
                originPoint: centerStart,
                targetPoint: centerEnd,
                distance: width * 0.5f);
        Vector3[] endPoints = GetParallelPoints(
                originPoint: centerEnd,
                targetPoint: centerStart,
                distance: width * 0.5f);
        Vector3 halfUp = 0.5f * height * up;
        Vector3 halfDown = -0.5f * height * up;
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

    public void InitSegmentObject()
    {
        GameObject segmentObject =
            ZeroController.FindGameObject(this.Name, true)
            ?? GameObject.CreatePrimitive(PrimitiveType.Cube);
        var headingChange =
            Quaternion.FromToRotation(
                segmentObject.transform.forward,
                this.Forward);

        segmentObject.name = this.Name;
        segmentObject.transform.position = this.Center;
        segmentObject.transform.localScale = new Vector3(this.Width, this.Height, this.Length);
        segmentObject.transform.localRotation *= headingChange;
        segmentObject.transform.SetParent(ZeroRoadBuilder.BuiltRoadSegmentsParent.transform);

        if (this.ParentLane.LaneIndex == -1 || this.ParentLane.LaneIndex == this.ParentLane.ParentRoad.NumberOfLanes)
            segmentObject.layer = LayerMask.NameToLayer(ZeroRoadBuilder.RoadSidewalkMaskName);
        else if (this.ParentLane.LaneIndex == 0 || this.ParentLane.LaneIndex == this.ParentLane.ParentRoad.NumberOfLanes - 1)
            segmentObject.layer = LayerMask.NameToLayer(ZeroRoadBuilder.RoadEdgeLaneMaskName);

        this.SegmentObject = segmentObject;
        // AddBoxCollider();
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

    private void RenderMesh(ZeroParallelogram topPlane, ZeroParallelogram bottomPlane, GameObject segment)
    {
        Vector3[] allVertices = new Vector3[2 * bottomPlane.GetVertices().Length];

        for (int i = 0; i < topPlane.GetVertices().Length; i++)
        {
            allVertices[i] =
                segment.transform.InverseTransformPoint(topPlane.GetVertices()[i]);
            allVertices[i + topPlane.GetVertices().Length] =
                segment.transform.InverseTransformPoint(bottomPlane.GetVertices()[i]);
        }

        Mesh mesh = new() { name = "Generated" };
        segment.AddComponent<MeshRenderer>();
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
        // mesh.vertices = allVertices;

        // mesh.triangles = new int[] { 
        //     //Top
        //     1,0,2,
        //     1,2,3, 
        //     //Bottom
        //     5,4,6,
        //     5,6,7,
        //     //Left Side
        //     6,2,0,
        //     6,0,4,
        //     //Right Side
        //     5,1,3,
        //     5,3,7,
        //     //Front Side
        //     4,0,1,
        //     4,1,5,
        //     //Back Side
        //     7,3,2,
        //     7,2,6,
        // };
        segment.AddComponent<MeshFilter>().mesh = mesh;
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

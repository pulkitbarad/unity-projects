using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ZeroRoadLane
{
    public string Name;
    public int LaneIndex;
    public float Width;
    public float Height;
    public ZeroRoadSegment[] Segments;
    public ZeroRoad ParentRoad;
    public bool IsLeftSidewalk;
    public bool IsRightSidewalk;
    public bool IsLaneAngleChangeValid;

    public ZeroRoadLane()
    {
    }

    public ZeroRoadLane(
        int laneIndex,
        float width,
        float height,
        Vector3[] leftVertices,
        Vector3[] centerVertices,
        Vector3[] rightVertices,
        ZeroRoad parentRoad)
    {
        this.LaneIndex = laneIndex;
        this.ParentRoad = parentRoad;
        this.Width = width;
        this.Height = height;
        this.Name = this.ParentRoad.Name + "_L" + laneIndex;
        this.IsLeftSidewalk = this.LaneIndex == this.ParentRoad.LeftSidewalkIndex;
        this.IsRightSidewalk = this.LaneIndex == this.ParentRoad.RightSidewalkIndex;
        this.Segments = GetRoadSegments(
            leftVertices: leftVertices,
            centerVertices: centerVertices,
            rightVertices: rightVertices);

        this.IsLaneAngleChangeValid = true;
        foreach (var segment in this.Segments)
        {
            this.IsLaneAngleChangeValid &= segment.IsSegmentAngleChangeValid;
        }
    }

    public void HideAllSegments()
    {
        if (ZeroRoadBuilder.BuiltRoadSegmentsByLane.ContainsKey(this.Name))
        {
            foreach (ZeroRoadSegment segment in
                ZeroRoadBuilder.BuiltRoadSegmentsByLane[this.Name])
            {
                ZeroObjectManager.ReleaseObjectToPool(
                    objectName: segment.Name,
                    objectType: segment.SegmentObjectType);
                if (ZeroRoadBuilder.BuiltRoadSegmentsByName.ContainsKey(segment.Name))
                    ZeroRoadBuilder.BuiltRoadSegmentsByName.Remove(segment.Name);
            }
            ZeroRoadBuilder.BuiltRoadSegmentsByLane[this.Name] = new();
        }
    }


    private ZeroRoadSegment[] GetRoadSegments(
        Vector3[] leftVertices,
        Vector3[] centerVertices,
        Vector3[] rightVertices)
    {
        HideAllSegments();
        ZeroRoadSegment[] segments = new ZeroRoadSegment[leftVertices.Length - 1];
        Vector3 startCenter = centerVertices[0];

        for (int vertexIndex = 0; vertexIndex < leftVertices.Length - 1; vertexIndex++)
        {
            Vector3 currLeft = leftVertices[vertexIndex];
            Vector3 nextLeft = leftVertices[vertexIndex + 1];
            Vector3 currCenter = centerVertices[vertexIndex];
            Vector3 nextCenter = centerVertices[vertexIndex + 1];
            Vector3 currRight = rightVertices[vertexIndex];
            Vector3 nextRight = rightVertices[vertexIndex + 1];
            Vector3 secondNextVertex =
                            centerVertices[
                                vertexIndex == centerVertices.Length - 2
                                ? vertexIndex + 1
                                : vertexIndex + 2];
            ZeroRoadSegment previousSegment =
                vertexIndex == 0
                ? null
                : segments[vertexIndex - 1];

            ZeroRoadSegment newSegment = new(
                index: vertexIndex,
                width: this.Width,
                height: this.Height,
                centerVertices: new Vector3[] { currLeft, nextLeft, nextRight, currRight },
                centerStart: currCenter,
                centerEnd: nextCenter,
                nextCenterEnd: secondNextVertex,
                previousSibling: previousSegment,
                parentLane: this);
            segments[vertexIndex] = newSegment;
        }
        return segments;
    }

    public override string ToString()
    {
        return "ZeroLaneIntersection("
        + "\n Name:" + this.Name
        + "\n LaneIndex:" + this.LaneIndex.ToString()
        + "\n Width:" + this.Width.ToString()
        + "\n Height:" + this.Height.ToString()
        + "\n Segments:" + this.Segments.Select(e => e.Name).ToCommaSeparatedString()
        + "\n ParentRoad:" + this.ParentRoad.Name
        + ")";
    }

    public Dictionary<string, Vector3> GetSegmentVertexLogs()
    {
        Dictionary<string, Vector3> vertexStrings = new();
        for (int i = 0; i < this.Segments.Length; i++)
            vertexStrings.AddRange(this.Segments[i].SegmentBounds.GetVertexLogPairs());
        return vertexStrings;
    }
}

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

    public ZeroRoadLane()
    {
    }

    public ZeroRoadLane(
        int laneIndex,
        float width,
        float height,
        Vector3[] centerVertices,
        ZeroRoad parentRoad)
    {
        this.LaneIndex = laneIndex;
        this.ParentRoad = parentRoad;
        this.Width = width;
        this.Height = height;
        this.Name = this.ParentRoad.Name + "L" + laneIndex;
        this.Segments = GetRoadSegments(
            centerVertices: centerVertices); ;
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
        Vector3[] centerVertices)
    {
        HideAllSegments();
        ZeroRoadSegment[] segments = new ZeroRoadSegment[centerVertices.Length - 1];
        for (int vertexIndex = 0; vertexIndex < centerVertices.Length - 1; vertexIndex++)
        {
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
                lengthSoFar = segments[vertexIndex - 1].RoadLengthSofar;

            ZeroRoadSegment newSegment = new(
                index: vertexIndex,
                width: this.Width,
                height: this.Height,
                roadLengthSoFar: lengthSoFar,
                centerStart: currVertex,
                centerEnd: nextVertex,
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

}

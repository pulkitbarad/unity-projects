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
    public GameObject LaneObject;
    public ZeroRoad ParentRoad;

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
        InitLaneObject(position: centerVertices[0]);
        this.Segments = GetRoadSegments(
            centerVertices: centerVertices); ;
    }

    public void HideAllSegments()
    {

        foreach (ZeroRoadSegment segment in
            ZeroRoadBuilder.BuiltRoadSegments
            .Values
            .Where(e => e.ParentLane.Name.Equals(this.Name)))
        {
            segment.SegmentObject.SetActive(false);
        }
    }

    public void InitLaneObject(Vector3 position)
    {
        GameObject laneObject =
            ZeroController.FindGameObject(this.Name, true)
            ?? new GameObject();

        laneObject.name = this.Name;
        laneObject.transform.position = position;
        laneObject.transform.SetParent(this.ParentRoad.RoadObject.transform);
        laneObject.SetActive(true);
        this.LaneObject = laneObject;
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

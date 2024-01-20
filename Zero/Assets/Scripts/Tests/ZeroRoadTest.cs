using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

public class ZeroRoadTest
{

    private Dictionary<string, Vector3> _pointPositions;

    public ZeroRoadTest(string testDataTimestamp)
    {
        _pointPositions = new();
        ZeroController.LoadLogEntries(testDataTimestamp)
        .ToList().ForEach(
            (e) =>
            {
                string vectorString = e.Value;
                float[] cordinates = vectorString.Substring(1, vectorString.Length - 2).Split(",").Select(e => float.Parse(e)).ToArray();
                _pointPositions[e.Key] = new Vector3(cordinates[0], cordinates[1], cordinates[2]);
            }
        );
    }

    public void ZeroRoadTestIntersectionStraight()
    {
        Vector3[] controlPoints1 = new Vector3[]{
        _pointPositions["R0Control0"],
        _pointPositions["R0Control1"]
        };


        Vector3[] controlPoints2 = new Vector3[]{
        _pointPositions["R1Control0"],
        _pointPositions["R1Control1"]
        };

        Debug.LogFormat("controlpoints1 = {0}" + controlPoints1);
        Debug.LogFormat("controlpoints2 = {0}" + controlPoints2);
        ZeroRoad actualRoad1 =
            new(
                isCurved: false,
                hasBusLane: true,
                numberOfLanesExclSidewalks: 2,
                height: ZeroRoadBuilder.RoadLaneHeight,
                sidewalkHeight: ZeroRoadBuilder.RoadSideWalkHeight,
                controlPoints: controlPoints1);

        ZeroRoad actualRoad2 =
            new(
                isCurved: false,
                hasBusLane: true,
                numberOfLanesExclSidewalks: 2,
                height: ZeroRoadBuilder.RoadLaneHeight,
                sidewalkHeight: ZeroRoadBuilder.RoadSideWalkHeight,
                controlPoints: controlPoints2);

        Debug.LogFormat("actualRoad1 segment counts= {0}",
            actualRoad1.Lanes.Select(e => e.Segments.Length).ToCommaSeparatedString());
        Debug.LogFormat("actualRoad2 segment counts= {0}",
            actualRoad2.Lanes.Select(e => e.Segments.Length).ToCommaSeparatedString());

        // Debug.LogFormat("intersections={0}", actualRoad2.IntersectionsByRoadName.Values.Count());
        // ZeroRoadIntersection intersection =
        //     actualRoad2.IntersectionsByRoadName.Values.SelectMany(e => e).ToArray()[0];
        // Assert.AreEqual(
        //     AssertPolygonAgainstTestData(
        //         "R1R0I0SW",
        //         intersection.Sidewalks),
        //     true);
        // Assert.AreEqual(
        //     AssertPolygonAgainstTestData(
        //         "R1R0I0SWCR",
        //         intersection.CrossWalks),
        //     true);
        // Assert.AreEqual(
        //     AssertPolygonAgainstTestData(
        //         "R1R0I0SWCR",
        //         intersection.SidewalkCorners),
        //     true);
        // Assert.AreEqual(
        //      AssertPolygonAgainstTestData(
        //          "R1R0I0LI",
        //          intersection.LaneIntersections),
        //      true);
    }

    public void ZeroRoadTestCurvedTwoLane()
    {
        Vector3[] controlPoints = new Vector3[]{
            new(-123.298584f,0,-39.1640625f),
            new(-68.0500488f,0,-58.3859863f),
            new(-57.7009277f,0,2.87182617f)
        };
        Vector3 R0L3S5Postion = new(-73.8933716f, 0.150000006f, -39.2758675f);
        Vector3 R0L3S5Scale = new(3, 0.300000012f, 9.17414474f);

        ZeroRoad actualRoad =
            new(
                isCurved: false,
                hasBusLane: true,
                numberOfLanesExclSidewalks: 2,
                height: ZeroRoadBuilder.RoadLaneHeight,
                sidewalkHeight: ZeroRoadBuilder.RoadSideWalkHeight,
                controlPoints: controlPoints);
        Transform testSegmentTransform = actualRoad.Lanes[3].Segments[5].SegmentObject.transform;
        AssertVectors(testSegmentTransform.position, R0L3S5Postion);
        AssertVectors(testSegmentTransform.localScale, R0L3S5Scale);
    }

    public void ZeroRoadTestStraightTwoLane()
    {
        Vector3[] controlPoints = new Vector3[]{
            new(-136.1067f, 0, -28.56543f),
            new(-43.16528f, 0, -32.26611f)
        };
        Vector3[] segmentPositions = new Vector3[]{
            new (-89.5763092f,0.125f,-28.9169579f),
            new(-89.6956635f,0.125f,-31.9145813f),
            new(-89.456955f,0.150000006f,-25.9193344f),
            new(-89.8150177f,0.150000006f,-34.9122086f)
        };
        Vector3[] segmentScales = new Vector3[]{
            new(3f,0.25f,93.0150452f),
            new(3f,0.25f,93.0150452f),
            new(3f,0.300000012f,93.0150452f),
            new(3f,0.300000012f,93.0150452f)
        };

        ZeroRoad expectedRoad = GetStraightTestRoad(controlPoints, segmentPositions, segmentScales);
        ZeroRoad actualRoad =
            new(
                isCurved: false,
                hasBusLane: true,
                numberOfLanesExclSidewalks: 2,
                height: ZeroRoadBuilder.RoadLaneHeight,
                sidewalkHeight: ZeroRoadBuilder.RoadSideWalkHeight,
                controlPoints: controlPoints);
        AssertRoads(expectedRoad, actualRoad);
    }

    private void AssertRoads(ZeroRoad expectedRoad, ZeroRoad actualRoad)
    {
        Assert.AreEqual(expectedRoad.Lanes.Length, actualRoad.Lanes.Length);
        for (int laneIndex = 0; laneIndex < actualRoad.Lanes.Length; laneIndex++)
        {
            ZeroRoadLane expectedLane = expectedRoad.Lanes[laneIndex];
            ZeroRoadLane actualLane = actualRoad.Lanes[laneIndex];

            Assert.AreEqual(expectedLane.Segments.Length, actualLane.Segments.Length);
            for (int segmentIndex = 0; segmentIndex < expectedLane.Segments.Length; segmentIndex++)
            {
                Transform expectedSegment = expectedLane.Segments[segmentIndex].SegmentObject.transform;
                Transform actualSegment = actualLane.Segments[segmentIndex].SegmentObject.transform;
                Assert.AreEqual(
                    AssertVectors(expectedSegment.position, actualSegment.position),
                    true);
                Assert.AreEqual(
                    AssertVectors(expectedSegment.localScale, actualSegment.localScale),
                    true);
            }
        }
    }

    private ZeroRoad GetStraightTestRoad(
        Vector3[] controlPoints,
        Vector3[] segmentPositions,
        Vector3[] segmentScales
    )
    {
        return
           new()
           {
               ControlPoints = controlPoints,
               Lanes =
                    Enumerable.Range(0, 4)
                    .Select((i) =>
                        {
                            ZeroRoadLane lane0 = new();
                            GameObject gameObject = new();
                            gameObject.transform.position = segmentPositions[i];
                            gameObject.transform.localScale = segmentScales[i];
                            return
                                new ZeroRoadLane()
                                {
                                    Segments = new ZeroRoadSegment[]{
                                        new ()
                                        {
                                            SegmentObject = gameObject
                                        }
                                    }
                                };
                        }).ToArray()
           };
    }

    private bool AssertPolygonAgainstTestData(string prefix, Vector3[][] polygons)
    {
        bool areEqual = true;
        for (int i = 0; i < polygons.Length; i++)
            for (int j = 0; j < polygons.Length; j++)
                areEqual &= AssertVectors(polygons[i][j], _pointPositions[prefix + i + j]);
        return areEqual;
    }

    private bool AssertVectors(Vector3 vectorExpected, Vector3 vectorActual)
    {
        return
            RoundCordinates(vectorExpected)
            .Equals(RoundCordinates(vectorActual));
    }

    private Vector3 RoundCordinates(Vector3 vector)
    {
        return new Vector3(
            (float)Math.Round(vector.x, 3),
            (float)Math.Round(vector.y, 3),
            (float)Math.Round(vector.z, 3)
        );
    }

}

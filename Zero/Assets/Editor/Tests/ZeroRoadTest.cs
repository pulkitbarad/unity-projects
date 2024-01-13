using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TestTools;

public class ZeroRoadTest
{

    private static void ZeroControllerSetup()
    {
        ZeroController.IsPlayMode = false;
        ZeroObjectManager.Initialise();
        ZeroRoadBuilder.Initialise();
    }

    [Test]
    public void ZeroRoadTestIntersection()
    {
        ZeroControllerSetup();
        Vector3[] controlPoints1 = new Vector3[]{
            new(-123.298584f,0,-39.1640625f),
            new(-68.0500488f,0,-58.3859863f),
            new(-57.7009277f,0,2.87182617f)
        };


        Vector3[] controlPoints2 = new Vector3[]{
            new(-123.298584f,0,-39.1640625f),
            new(-68.0500488f,0,-58.3859863f),
            new(-57.7009277f,0,2.87182617f)
        };

        ZeroRoad actualRoad1 =
            new(
                isCurved: false,
                hasBusLane: true,
                numberOfLanes: 2,
                height: ZeroRoadBuilder.RoadLaneHeight,
                sidewalkHeight: ZeroRoadBuilder.RoadSideWalkHeight,
                controlPoints: controlPoints1);

        ZeroRoad actualRoad2 =
            new(
                isCurved: false,
                hasBusLane: true,
                numberOfLanes: 2,
                height: ZeroRoadBuilder.RoadLaneHeight,
                sidewalkHeight: ZeroRoadBuilder.RoadSideWalkHeight,
                controlPoints: controlPoints2);
    }

    [Test]
    public void ZeroRoadTestCurvedTwoLane()
    {
        ZeroControllerSetup();
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
                numberOfLanes: 2,
                height: ZeroRoadBuilder.RoadLaneHeight,
                sidewalkHeight: ZeroRoadBuilder.RoadSideWalkHeight,
                controlPoints: controlPoints);
        Transform testSegmentTransform = actualRoad.Lanes[3].Segments[5].SegmentObject.transform;
        AssertVectors(testSegmentTransform.position, R0L3S5Postion);
        AssertVectors(testSegmentTransform.localScale, R0L3S5Scale);
    }

    [Test]
    public void ZeroRoadTestStraightTwoLane()
    {
        ZeroControllerSetup();
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
                numberOfLanes: 2,
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

    private bool AssertVectors(Vector3 vector1, Vector3 vector2)
    {
        return
            RoundCordinates(vector1)
            .Equals(RoundCordinates(vector2));
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

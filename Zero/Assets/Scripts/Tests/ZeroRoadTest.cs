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
    public static string Test1 = "RoadStraightAndCurved";
    public static string Test2 = "IntersectionStraight";
    public static string Test3 = "IntersectionCurved";
    public static string Test4 = "IntersectionCurvedOnStraight";
    public static string Test5 = "IntersectionStraightOnCurved";

    public ZeroRoadTest(string testDataTimestamp)
    {
    }

    public static void RunTest1()
    {
        Dictionary<string, Vector3> testData = ZeroController.LoadLogEntries(Test1);
        Vector3[] controlPoints0 = new Vector3[]{
            testData["R0Control0"],
            testData["R0Control1"]
        };


        Vector3[] controlPoints1 = new Vector3[]{
            testData["R1Control0"],
            testData["R1Control1"]
        };

        Debug.LogFormat("controlpoints0 = {0}", controlPoints0.Select(e => e.ToString()).ToCommaSeparatedString());
        Debug.LogFormat("controlpoints1 = {0}", controlPoints1.Select(e => e.ToString()).ToCommaSeparatedString());
        ZeroRoad actualRoad0 =
            new(
                isCurved: false,
                hasBusLane: true,
                numberOfLanesExclSidewalks: 2,
                height: ZeroRoadBuilder.RoadLaneHeight,
                sidewalkHeight: ZeroRoadBuilder.RoadSideWalkHeight,
                controlPoints: controlPoints0);

        ZeroRoad actualRoad1 =
            new(
                isCurved: false,
                hasBusLane: true,
                numberOfLanesExclSidewalks: 2,
                height: ZeroRoadBuilder.RoadLaneHeight,
                sidewalkHeight: ZeroRoadBuilder.RoadSideWalkHeight,
                controlPoints: controlPoints1);

        AssertRoads(testData, actualRoad0);
        AssertRoads(testData, actualRoad1);

    }

    private static void AssertRoads(Dictionary<string, Vector3> expectedTestData, ZeroRoad actualRoad)
    {
        for (int laneIndex = 0; laneIndex < actualRoad.Lanes.Length; laneIndex++)
        {
            ZeroRoadLane actualLane = actualRoad.Lanes[laneIndex];

            for (int segmentIndex = 0; segmentIndex < actualLane.Segments.Length; segmentIndex++)
            {
                ZeroRoadSegment actualSegment = actualLane.Segments[segmentIndex];
                Dictionary<string, Vector3> actualVertexDict = actualSegment.SegmentBounds.GetVertexLogPairs();
                foreach (var pair in actualVertexDict)
                    AssertVectors(expectedTestData[pair.Key], pair.Value);
            }
        }
    }

    // private ZeroRoad GetStraightTestRoad(
    //     Vector3[] controlPoints,
    //     Vector3[] segmentPositions,
    //     Vector3[] segmentScales
    // )
    // {
    //     return
    //        new()
    //        {
    //            ControlPoints = controlPoints,
    //            Lanes =
    //                 Enumerable.Range(0, 4)
    //                 .Select((i) =>
    //                     {
    //                         ZeroRoadLane lane0 = new();
    //                         GameObject gameObject = new();
    //                         gameObject.transform.position = segmentPositions[i];
    //                         gameObject.transform.localScale = segmentScales[i];
    //                         return
    //                             new ZeroRoadLane()
    //                             {
    //                                 Segments = new ZeroRoadSegment[]{
    //                                     new ()
    //                                     {
    //                                         SegmentObject = gameObject
    //                                     }
    //                                 }
    //                             };
    //                     }).ToArray()
    //        };
    // }

    // private bool AssertPolygonAgainstTestData(string prefix, Vector3[][] polygons)
    // {
    //     bool areEqual = true;
    //     for (int i = 0; i < polygons.Length; i++)
    //         for (int j = 0; j < polygons.Length; j++)
    //             areEqual &= AssertVectors(polygons[i][j], _registeredPoints[prefix + i + j]);
    //     return areEqual;
    // }

    private static bool AssertVectors(Vector3 vectorExpected, Vector3 vectorActual)
    {
        return
            RoundCordinates(vectorExpected)
            .Equals(RoundCordinates(vectorActual));
    }

    private static Vector3 RoundCordinates(Vector3 vector)
    {
        return new Vector3(
            (float)Math.Round(vector.x, 3),
            (float)Math.Round(vector.y, 3),
            (float)Math.Round(vector.z, 3)
        );
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public class ZeroRoadTest
{
    public static string Test1 = "RoadStraightAndCurved";
    public static string Test2 = "IntersectionStraight";
    public static string Test3 = "IntersectionCurved";
    public static string Test4 = "IntersectionCurvedOnStraight";
    public static string Test5 = "IntersectionStraightOnCurved";

    public ZeroRoadTest()
    {
        if (!ZeroController.IsPlayMode)
        {
            // RunTest1();
            RunTest2();
        }
    }

    private void RunTest1()
    {
        Dictionary<string, Vector3> testData = ZeroController.LoadTestData(Test1);
        ZeroRoad straightRoad = GetTestRoad(testData, "R0", false);

        ZeroRoad curvedRoad = GetTestRoad(testData, "R1", true);

        AssertRoad(testData, straightRoad);
        AssertRoad(testData, curvedRoad);

    }
    private void RunTest2()
    {
        //R1 -> R0
        //R2, R3
        //R4 -> R2,R3, R0
        Dictionary<string, Vector3> testData = ZeroController.LoadTestData(Test2);
        ZeroRoad road0 = GetTestRoad(testData, "R0", false);
        ZeroRoad road1 = GetTestRoad(testData, "R1", false);
        ZeroRoad road2 = GetTestRoad(testData, "R2", false);
        ZeroRoad road3 = GetTestRoad(testData, "R3", false);
        ZeroRoad road4 = GetTestRoad(testData, "R4", false);


    }

    private ZeroRoad GetTestRoad(Dictionary<string, Vector3> testData, string roadName, bool isCurved)
    {
        List<Vector3> controlPoints = new()
        {
            testData[roadName + "Control0"],
            testData[roadName + "Control1"]
        };
        if (isCurved)
            controlPoints.Add(testData[roadName + "Control2"]);

        return
            new(
                isCurved: isCurved,
                hasBusLane: true,
                numberOfLanesExclSidewalks: 2,
                height: ZeroRoadBuilder.RoadLaneHeight,
                sidewalkHeight: ZeroRoadBuilder.RoadSideWalkHeight,
                controlPoints: controlPoints.ToArray());
    }

    private static void AssertRoad(Dictionary<string, Vector3> expectedTestData, ZeroRoad actualRoad)
    {
        Dictionary<string, Vector3> actualTestData = actualRoad.GenerateTestData();

        foreach (var actualKey in actualTestData.Keys)
        {
            Assert.AreEqual(true, expectedTestData.ContainsKey(actualKey));
            Assert.AreEqual(
                RoundCordinates(expectedTestData[actualKey]),
                RoundCordinates(actualTestData[actualKey]));
        }
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

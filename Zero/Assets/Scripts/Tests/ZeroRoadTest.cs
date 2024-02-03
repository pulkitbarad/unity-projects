using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class ZeroRoadTest
{
    public static string Test1 = "RoadStraightAndCurved";
    public static string Test2 = "IntersectionStraight";
    public static string Test3 = "IntersectionCurved";
    public static string Test4 = "IntersectionMixed";

    public ZeroRoadTest()
    {
        if (!ZeroController.IsPlayMode)
        {
            // RunTest1();
            // RunTest2();
            // RunTest3();
            RunTest4();
        }
    }

    private void RunTest1()
    {
        Dictionary<string, Vector3> testData = ZeroController.LoadTestData(Test1);

        AssertRoad(testData, Test1, "R0", false);
        AssertRoad(testData, Test1, "R1", true);

        Debug.Log("Test 1 was successful.");
    }
    private void RunTest2()
    {
        //R1 -> R0
        //R2, R3
        //R4 -> R2,R3, R0
        Dictionary<string, Vector3> testData = ZeroController.LoadTestData(Test2);

        AssertRoad(testData, Test2, "R0", false);
        AssertRoad(testData, Test2, "R1", false);
        AssertRoad(testData, Test2, "R2", false);
        AssertRoad(testData, Test2, "R3", false);
        AssertRoad(testData, Test2, "R4", false);

        Debug.Log("Test 2 was successful.");
    }

    private void RunTest3()
    {

        Dictionary<string, Vector3> testData = ZeroController.LoadTestData(Test3);

        AssertRoad(testData, Test3, "R0", true);
        AssertRoad(testData, Test3, "R1", true);
        AssertRoad(testData, Test3, "R2", true);
        AssertRoad(testData, Test3, "R3", true);
        AssertRoad(testData, Test3, "R4", true);

        Debug.Log("Test 3 was successful.");
    }

    private void RunTest4()
    {

        Dictionary<string, Vector3> testData = ZeroController.LoadTestData(Test4);

        AssertRoad(testData, Test4, "R0", false);
        AssertRoad(testData, Test4, "R1", false);
        AssertRoad(testData, Test4, "R2", true);
        AssertRoad(testData, Test4, "R3", true);
        AssertRoad(testData, Test4, "R4", false);

        Debug.Log("Test 4 was successful.");
    }

    private void AssertRoad(Dictionary<string, Vector3> expectedTestData, string testName, string roadName, bool isCurved)
    {
        ZeroRoad actualRoad = GetTestRoad(expectedTestData, roadName: roadName, isCurved: isCurved);
        Dictionary<string, Vector3> actualTestData = actualRoad.GenerateTestData(testName);
        List<string> filteredExpectedKeys = expectedTestData.Keys.Where(e => e.StartsWith(actualRoad.Name)).ToList();

        if (filteredExpectedKeys.Count != actualTestData.Keys.Count)
        {
            Debug.LogFormat("testName={0} actualRoadName={1} expected_keys={2} actual_keys={3}",
                testName,
                actualRoad.Name,
                filteredExpectedKeys.Count,
                actualTestData.Count);
        }
        Assert.AreEqual(filteredExpectedKeys.Count, actualTestData.Keys.Count);

        foreach (var expectedKey in filteredExpectedKeys)
        {
            Assert.AreEqual(true, actualTestData.ContainsKey(expectedKey));
            if (!AreVectorsEqual(expectedTestData[expectedKey], actualTestData[expectedKey]))
            {
                Debug.LogFormat("key={0} expected={1} actual{2}",
                expectedKey,
                VectorToString(expectedTestData[expectedKey]),
                VectorToString(actualTestData[expectedKey]));
            }
            Assert.AreEqual(true, AreVectorsEqual(expectedTestData[expectedKey], actualTestData[expectedKey]));
        }
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
                isPrimaryRoad: true,
                isCurved: isCurved,
                hasBusLane: true,
                numberOfLanesExclSidewalks: 2,
                height: ZeroRoadBuilder.RoadLaneHeight,
                sidewalkHeight: ZeroRoadBuilder.RoadSideWalkHeight,
                forceSyncTransform: true,
                controlPoints: controlPoints.ToArray());
    }

    private static bool AreVectorsEqual(Vector3 vector1, Vector3 vector2)
    {
        return
            Math.Abs(vector1.x - vector2.x) <= 0.1
            && Math.Abs(vector1.y - vector2.y) <= 0.1
            && Math.Abs(vector1.z - vector2.z) <= 0.1;
    }

    private static string VectorToString(Vector3 vector)
    {
        return String.Format("{0},{1},{2}", vector.x, vector.y, vector.z);

    }
}

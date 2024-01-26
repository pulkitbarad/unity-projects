using System.Linq;
using UnityEngine;
using System;
using UnityEditor;
using System.Text;
using System.Collections.Generic;
using System.IO;
public class ZeroController : MonoBehaviour
{

    private static string _testDataFilePath;
    [SerializeField] public GameObject MainCameraRoot;
    [SerializeField] public GameObject MainCameraAnchor;
    [SerializeField] public GameObject MainCameraHolder;
    [SerializeField] public Camera MainCamera;
    [SerializeField] public Material RoadSegmentMaterial;
    public static bool IsPlayMode;
    public static bool IsDebuggingEnabled;
    public static string TestToGenerateData;

    void Awake()
    {
        ZeroCameraMovement.MainCamera = MainCamera;
        ZeroCameraMovement.MainCameraHolder = MainCameraHolder;
        ZeroCameraMovement.MainCameraRoot = MainCameraRoot;
        ZeroCameraMovement.MainCameraAnchor = MainCameraAnchor;
        ZeroRoadBuilder.RoadSegmentMaterial = RoadSegmentMaterial;

        IsPlayMode = false;
        IsDebuggingEnabled = true;
        TestToGenerateData = ZeroRoadTest.Test4;
        ZeroObjectManager.Initialise();
        ZeroRoadBuilder.Initialise();
        ZeroCameraMovement.Initialise();
        ZeroUIHandler.Initialise();
        ZeroRenderer.Initialise();
    }

    void Start()
    {
        new ZeroRoadTest();
    }

    void Update()
    {
        ZeroUIHandler.HandleInputChanges();
    }


    public static void OverWriteTestDataFile(Dictionary<string, Vector3> testData)
    {
        if (testData.Count() > 0)
        {
            _testDataFilePath = @"C:\Users\pulki\Desktop\UnityLogs\" + TestToGenerateData + ".txt";
            File.WriteAllLines(_testDataFilePath,
                testData
                .Select(e =>
                    String.Format("{0}={1}",
                        e.Key,
                        String.Format("{0},{1},{2}",
                            e.Value.x,
                            e.Value.y,
                            e.Value.z))));
        }
    }

    public static void AppendToTestDataFile(Dictionary<string, Vector3> testData)
    {
        if (testData.Count() > 0)
        {
            _testDataFilePath = @"C:\Users\pulki\Desktop\UnityLogs\" + TestToGenerateData + ".txt";
            File.AppendAllLines(_testDataFilePath,
                testData
                .Select(e =>
                    String.Format("{0}={1}",
                        e.Key,
                        String.Format("{0},{1},{2}",
                            e.Value.x,
                            e.Value.y,
                            e.Value.z))));
        }
    }

    public static Dictionary<string, Vector3> LoadTestData(string testDataFileName)
    {
        Dictionary<string, Vector3> tempLogEntries = new();
        _testDataFilePath = @"C:\Users\pulki\Desktop\UnityLogs\" + testDataFileName + ".txt";

        File.ReadAllLines(_testDataFilePath)
        .ToList()
        .ForEach(
            (e) =>
            {
                var pair = e.Split("=");
                string vectorString = pair[1];
                float[] cordinates =
                    vectorString.Split(",").Select(e => float.Parse(e)).ToArray();
                tempLogEntries[pair[0]] = new Vector3(cordinates[0], cordinates[1], cordinates[2]);
            }
        );
        return tempLogEntries;
    }

    // public static string GetPositionHexCode(params Vector3[] positions)
    // {
    //     Vector3 position = Vector3.zero;
    //     for (int i = 0; i < positions.Length; i++)
    //     {
    //         position += positions[i];
    //     }
    //     float coordinates = position.x + position.y + position.z;
    //     return BitConverter.ToString(Encoding.Default.GetBytes(coordinates.ToString())).Replace("-", "");
    // }
}
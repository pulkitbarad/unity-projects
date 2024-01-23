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
    private static Dictionary<string, Vector3> _loggedVertices = new();
    [SerializeField] public GameObject MainCameraRoot;
    [SerializeField] public GameObject MainCameraAnchor;
    [SerializeField] public GameObject MainCameraHolder;
    [SerializeField] public Camera MainCamera;
    [SerializeField] public Material RoadSegmentMaterial;
    public static bool IsPlayMode = true;
    public static bool IsDebuggingEnabled = false;
    public static string TestToGenerateData = "";

    void Awake()
    {
        ZeroCameraMovement.MainCamera = MainCamera;
        ZeroCameraMovement.MainCameraHolder = MainCameraHolder;
        ZeroCameraMovement.MainCameraRoot = MainCameraRoot;
        ZeroCameraMovement.MainCameraAnchor = MainCameraAnchor;
        ZeroRoadBuilder.RoadSegmentMaterial = RoadSegmentMaterial;
    }

    void Start()
    {
        IsPlayMode = false;
        IsDebuggingEnabled = true;
        ZeroObjectManager.Initialise();
        ZeroRoadBuilder.Initialise();
        ZeroCameraMovement.Initialise();
        ZeroUIHandler.Initialise();
        ZeroRenderer.Initialise();
        TestToGenerateData = ZeroRoadTest.Test2;
        new ZeroRoadTest();
    }

    void Update()
    {
        ZeroUIHandler.HandleInputChanges();
    }

    public static void AppendToTestData(Dictionary<string, Vector3> logs)
    {
        if (IsDebuggingEnabled)
            foreach (string key in logs.Keys)
                _loggedVertices[key] = logs[key];
    }

    public static void WriteTestDataFile()
    {
        if (IsDebuggingEnabled)
            if (_loggedVertices.Count() > 0)
            {
                _testDataFilePath = @"C:\Users\pulki\Desktop\UnityLogs\" + TestToGenerateData + ".txt";
                File.WriteAllLines(_testDataFilePath,
                    _loggedVertices
                    .Select(e =>
                        String.Format("{0}={1}",
                            e.Key,
                            String.Format("{0},{1},{2}",
                                e.Value.x,
                                e.Value.y,
                                e.Value.z))));

                _loggedVertices.Clear();
            }
    }

    public static void AppendToTestDataFile()
    {
        if (IsDebuggingEnabled)
            if (_loggedVertices.Count() > 0)
            {
                _testDataFilePath = @"C:\Users\pulki\Desktop\UnityLogs\" + TestToGenerateData + ".txt";
                File.AppendAllLines(_testDataFilePath,
                    _loggedVertices
                    .Select(e =>
                        String.Format("{0}={1}",
                            e.Key,
                            String.Format("{0},{1},{2}",
                                e.Value.x,
                                e.Value.y,
                                e.Value.z))));
                _loggedVertices.Clear();
            }
    }

    public static Dictionary<string, Vector3> LoadTestData(string testDataFileName)
    {
        Dictionary<string, Vector3> tempLogEntries = new();
        _testDataFilePath = @"C:\Users\pulki\Desktop\UnityLogs\" + testDataFileName + ".txt";

        if (IsDebuggingEnabled)
        {
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
        }
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
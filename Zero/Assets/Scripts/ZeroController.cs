using System.Linq;
using UnityEngine;
using System;
using UnityEditor;
using System.Text;
using System.Collections.Generic;
using System.IO;
public class ZeroController : MonoBehaviour
{

    private static string _logFilePath;
    private static string _logFileName;
    private static Dictionary<string, Vector3> _loggedVertices = new();
    [SerializeField] public GameObject MainCameraRoot;
    [SerializeField] public GameObject MainCameraAnchor;
    [SerializeField] public GameObject MainCameraHolder;
    [SerializeField] public Camera MainCamera;
    [SerializeField] public Material RoadSegmentMaterial;
    public static bool IsPlayMode = false;
    public static bool IsDebuggingEnabled = false;
    public static string TestType;

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
        IsPlayMode = true;
        IsDebuggingEnabled = true;
        ZeroObjectManager.Initialise();
        ZeroRoadBuilder.Initialise();
        ZeroCameraMovement.Initialise();
        ZeroUIHandler.Initialise();
        ZeroRenderer.Initialise();
        TestType = ZeroRoadTest.Test1;
        // _logFileName = DateTime.Now.ToString("yyyyMMddTHHmm");
        _logFileName = ZeroRoadTest.Test1;
        ZeroRoadTest.RunTest1();
    }

    void Update()
    {
        ZeroUIHandler.HandleInputChanges();
    }

    public static void AppendToDebugLog(Dictionary<string, Vector3> logs)
    {
        if (IsDebuggingEnabled)
            foreach (string key in logs.Keys)
                _loggedVertices[key] = logs[key];
    }

    public static void WriteDebugFile()
    {
        if (IsDebuggingEnabled)
            if (_loggedVertices.Count() > 0)
            {
                _logFilePath = @"C:\Users\pulki\Desktop\UnityLogs\log" + _logFileName + ".txt";
                File.WriteAllLines(_logFilePath, _loggedVertices.Select(e => String.Format("{0}={1}", e.Key, e.Value.ToString())));
                _loggedVertices.Clear();
            }
    }

    public static void AppendToDebugFile()
    {
        if (IsDebuggingEnabled)
            if (_loggedVertices.Count() > 0)
            {
                _logFilePath = @"C:\Users\pulki\Desktop\UnityLogs\log" + _logFileName + ".txt";
                File.AppendAllLines(_logFilePath, _loggedVertices.Select(e => String.Format("{0}={1}", e.Key, e.Value.ToString())));
                _loggedVertices.Clear();
            }
    }

    public static Dictionary<string, Vector3> LoadLogEntries(string logFileName)
    {
        Dictionary<string, Vector3> tempLogEntries = new();
        _logFilePath = @"C:\Users\pulki\Desktop\UnityLogs\log" + logFileName + ".txt";

        if (IsDebuggingEnabled)
        {
            File.ReadAllLines(_logFilePath)
            .ToList()
            .ForEach(
                (e) =>
                {
                    var pair = e.Split("=");
                    string vectorString = pair[1];
                    float[] cordinates = vectorString.Substring(1, vectorString.Length - 2).Split(",").Select(e => float.Parse(e)).ToArray();
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
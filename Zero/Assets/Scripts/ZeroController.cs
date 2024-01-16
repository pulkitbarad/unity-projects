using System.Linq;
using UnityEngine;
using System;
using UnityEditor;
using System.Text;
using System.Collections.Generic;
using System.IO;
public class ZeroController : MonoBehaviour
{

    private static string _logFile;
    private static Dictionary<string, string> _logEntries = new();
    [SerializeField] public GameObject MainCameraRoot;
    [SerializeField] public GameObject MainCameraAnchor;
    [SerializeField] public GameObject MainCameraHolder;
    [SerializeField] public Camera MainCamera;
    [SerializeField] public Material RoadSegmentMaterial;
    public static bool IsPlayMode = false;

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
        ZeroObjectManager.Initialise();
        ZeroRoadBuilder.Initialise();
        ZeroCameraMovement.Initialise();
        ZeroUIHandler.Initialise();
        ZeroRenderer.Initialise();
        // ZeroRoadTest roadTest = new ZeroRoadTest("20240114T0135");
        // // roadTest.ZeroRoadTestIntersectionStraight();
    }

    void Update()
    {
        ZeroUIHandler.HandleInputChanges();
    }

    public static void AppendToDebugLog((string, string)[] pairs)
    {
        for (int i = 0; i < pairs.Length; i++)
        {
            _logEntries[pairs[i].Item1] = pairs[i].Item2;
        }
    }

    public static void WriteDebugFile()
    {
        if (_logEntries.Count() > 0)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddTHHmm");
            _logFile = @"C:\Users\pulki\Desktop\logFile" + timestamp;
            File.WriteAllLines(_logFile, _logEntries.Select(e => String.Format("{0}={1}", e.Key, e.Value)));
            _logEntries.Clear();
        }
    }

    public static Dictionary<string, string> LoadLogEntries(string timeStamp)
    {
        _logEntries = new();
        _logFile = @"C:\Users\pulki\Desktop\logFile" + timeStamp;

        File.ReadAllLines(_logFile)
        .Select(
            (e) =>
            {
                var pair = e.Split("=");
                return (pair[0], pair[1].Substring(1, pair[1].Length - 2));
            }
        ).ToList()
        .ForEach(
            (e) =>
            {
                _logEntries[e.Item1] = e.Item2;
            }
        );
        return _logEntries;
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
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
    public static bool IsPlayMode = false;

    void Awake()
    {
        ZeroCameraMovement.MainCamera = MainCamera;
        ZeroCameraMovement.MainCameraHolder = MainCameraHolder;
        ZeroCameraMovement.MainCameraRoot = MainCameraRoot;
        ZeroCameraMovement.MainCameraAnchor = MainCameraAnchor;
    }

    void Start()
    {
        IsPlayMode = true;
        ZeroObjectManager.Initialise();
        ZeroRoadBuilder.Initialise();
        ZeroCameraMovement.Initialise();
        ZeroUIHandler.Initialise();
        ZeroRenderer.Initialise();
    }

    void Update()
    {
        ZeroUIHandler.HandleInputChanges();
    }

    public static void AppendToLog((string, string)[] pairs)
    {
        for (int i = 0; i < pairs.Length; i++)
        {
            _logEntries[pairs[i].Item1] = pairs[i].Item2;
        }
        Debug.LogFormat("Log line count={0}", _logEntries.Count());
    }

    public static void WriteLogFile()
    {
        Debug.LogFormat("Before writing Log line count={0}", _logEntries.Count());
        if (_logEntries.Count() > 0)
        {
            _logFile = @"C:\Users\pulki\Desktop\logFile.txt";
            File.WriteAllLines(_logFile, _logEntries.Select(e => String.Format("{0}={1}", e.Key, e.Value)));
            _logEntries.Clear();
            Debug.LogFormat("After writing Log line count={0}", _logEntries.Count());
        }
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

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
    private static List<string> _logLines = new();
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

    public static void AppendToLog(string[] lines)
    {
        _logLines.AddRange(lines);
    }

    public static void WriteLogFile()
    {
        if (_logLines.Count() > 0)
        {
            _logFile = @"C:\Users\pulki\Desktop\logFile.txt";
            File.WriteAllLines(_logFile, _logLines);
            _logLines.Clear();
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

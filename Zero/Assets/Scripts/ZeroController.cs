using System.Linq;
using UnityEngine;
using System;
using UnityEditor;
using System.Text;
using System.Collections.Generic;

public class ZeroController : MonoBehaviour
{
    [SerializeField] public float MainCameraMoveSpeed;
    [SerializeField] public float MainCameraSmoothing;
    [SerializeField] public float MainCameraZoomSpeed;
    [SerializeField] public float MainCameraRotationSpeed;
    [SerializeField] public float MainCameraTiltSpeed;
    [SerializeField] public float MainCameraTiltAngleThreshold;
    [SerializeField] public float RoadMaxChangeInAngle;
    [SerializeField] public int RoadMaxVertexCount;
    [SerializeField] public int RoadMinVertexCount;
    [SerializeField] public float RoadSegmentMinLength;
    [SerializeField] public float RoadLaneHeight;
    [SerializeField] public float RoadLaneWidth;
    [SerializeField] public float RoadSideWalkHeight;

    [SerializeField] public GameObject MainCameraRoot;
    [SerializeField] public GameObject MainCameraAnchor;
    [SerializeField] public GameObject MainCameraHolder;
    [SerializeField] public Camera MainCamera;

    void Awake()
    {
        ZeroCameraMovement.MainCamera = MainCamera;
        ZeroCameraMovement.MainCameraHolder = MainCameraHolder;
        ZeroCameraMovement.MainCameraRoot = MainCameraRoot;
        ZeroCameraMovement.MainCameraAnchor = MainCameraAnchor;
    }

    void Start()
    {
        ZeroObjectManager.Initialise();
        ZeroRoadBuilder.Initialise();
        ZeroCameraMovement.Initialise();
        ZeroUIHandler.Initialise();
    }

    void Update()
    {
        ZeroUIHandler.HandleInputChanges();
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

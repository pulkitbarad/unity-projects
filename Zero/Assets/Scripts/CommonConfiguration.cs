using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonConfiguration : MonoBehaviour
{
    [SerializeField] private float MainCameraMoveSpeed = 4f;
    [SerializeField] private float MainCameraSmoothing = 2f;
    [SerializeField] private float MainCameraZoomSpeed = 100f;
    [SerializeField] private float MainCameraZoomSpeedTouch = 100f;
    [SerializeField] private float MainCameraRotationSpeed = 1f;
    [SerializeField] private float MainCameraTiltSpeed = 1f;
    [SerializeField] private float MainCameraPinchDistanceThreshold = 50f;
    [SerializeField] private float MainCameraRotateAngleThreshold = 10f;
    [SerializeField] private GameObject StartControlObject;
    [SerializeField] private GameObject StartObject;
    [SerializeField] private GameObject EndControlObject;
    [SerializeField] private GameObject EndObject;
    [SerializeField] private Camera MainCamera;
    [SerializeField] private GameObject MainCameraHolder;
    [SerializeField] private GameObject MainCameraRoot;
    [SerializeField] private GameObject CurvedRoadButton;
    [SerializeField] private GameObject ZoomInButton;
    [SerializeField] private GameObject ZoomOutButton;
    [SerializeField] private GameObject RotateClockwiseButton;
    [SerializeField] private GameObject RotateAntiClockwiseButton;
    [SerializeField] private GameObject TiltUpButton;
    [SerializeField] private GameObject TiltDownButton;

    // Start is called before the first frame update
    void Awake()
    {
        CommonController.MainCameraMoveSpeed = MainCameraMoveSpeed;
        CommonController.MainCameraSmoothing = MainCameraSmoothing;
        CommonController.MainCameraZoomSpeed = MainCameraZoomSpeed;
        CommonController.MainCameraZoomSpeedTouch = MainCameraZoomSpeedTouch;
        CommonController.MainCameraRotationSpeed = MainCameraRotationSpeed;
        CommonController.MainCameraTiltSpeed = MainCameraTiltSpeed;
        CommonController.MainCameraPinchDistanceThreshold = MainCameraPinchDistanceThreshold;
        CommonController.MainCameraRotateAngleThreshold = MainCameraRotateAngleThreshold;
        CommonController.StartControlObject = StartControlObject;
        CommonController.StartObject = StartObject;
        CommonController.EndControlObject = EndControlObject;
        CommonController.EndObject = EndObject;
        CommonController.MainCamera = MainCamera;
        CommonController.MainCameraHolder = MainCameraHolder;
        CommonController.MainCameraRoot = MainCameraRoot;
        CommonController.CurvedRoadButton = CurvedRoadButton;
        CommonController.ZoomInButton = ZoomInButton;
        CommonController.ZoomOutButton = ZoomOutButton;
        CommonController.RotateClockwiseButton = RotateClockwiseButton;
        CommonController.RotateAntiClockwiseButton = RotateAntiClockwiseButton;
        CommonController.TiltUpButton = TiltUpButton;
        CommonController.TiltDownButton = TiltDownButton;
    }

}

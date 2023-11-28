using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonConfiguration : MonoBehaviour
{
    [SerializeField] private float MainCameraMoveSpeed = 1f;
    [SerializeField] private float MainCameraSmoothing = 2f;
    [SerializeField] private float MainCameraZoomSpeed = 100f;
    [SerializeField] private float MainCameraRotationSpeed = 100f;
    [SerializeField] private float MainCameraTiltSpeed = 100f;
    [SerializeField] private float MainCameraTiltAngleThreshold = 10f;
    [SerializeField] private GameObject StartControlObject;
    [SerializeField] private GameObject StartObject;
    [SerializeField] private GameObject EndControlObject;
    [SerializeField] private GameObject EndObject;
    [SerializeField] private Camera MainCamera;
    [SerializeField] private GameObject MainCameraHolder;
    [SerializeField] private GameObject MainCameraRoot;
    [SerializeField] private GameObject MainCameraAnchor;

    // Start is called before the first frame update
    void Awake()
    {
        CommonController.MainCameraMoveSpeed = MainCameraMoveSpeed;
        CommonController.MainCameraSmoothing = MainCameraSmoothing;
        CommonController.MainCameraZoomSpeed = MainCameraZoomSpeed;
        CommonController.MainCameraRotationSpeed = MainCameraRotationSpeed;
        CommonController.MainCameraTiltSpeed = MainCameraTiltSpeed;
        CommonController.MainCameraTiltAngleThreshold = MainCameraTiltAngleThreshold;
        CommonController.StartControlObject = StartControlObject;
        CommonController.StartObject = StartObject;
        CommonController.EndControlObject = EndControlObject;
        CommonController.EndObject = EndObject;
        CommonController.MainCamera = MainCamera;
        CommonController.MainCameraHolder = MainCameraHolder;
        CommonController.MainCameraRoot = MainCameraRoot;
        CommonController.MainCameraAnchor = MainCameraAnchor;
    }

}

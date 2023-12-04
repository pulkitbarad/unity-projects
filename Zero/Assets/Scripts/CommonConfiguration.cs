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
   
    [SerializeField] private GameObject MainCameraRoot;
    [SerializeField] private GameObject MainCameraAnchor;
    [SerializeField] private GameObject MainCameraHolder;
    [SerializeField] private Camera MainCamera;

    // Start is called before the first frame update
    void Awake()
    {
        CameraMovement.MainCameraMoveSpeed = MainCameraMoveSpeed;
        CameraMovement.MainCameraSmoothing = MainCameraSmoothing;
        CameraMovement.MainCameraZoomSpeed = MainCameraZoomSpeed;
        CameraMovement.MainCameraRotationSpeed = MainCameraRotationSpeed;
        CameraMovement.MainCameraTiltSpeed = MainCameraTiltSpeed;
        CameraMovement.MainCameraTiltAngleThreshold = MainCameraTiltAngleThreshold;
      
        CameraMovement.MainCamera = MainCamera;
        CameraMovement.MainCameraHolder = MainCameraHolder;
        CameraMovement.MainCameraRoot = MainCameraRoot;
        CameraMovement.MainCameraAnchor = MainCameraAnchor;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonConfiguration : MonoBehaviour
{
    [SerializeField] private float MainCameraMoveSpeed;// = 1f;
    [SerializeField] private float MainCameraSmoothing;// = 2f;
    [SerializeField] private float MainCameraZoomSpeed;// = 100f;
    [SerializeField] private float MainCameraRotationSpeed;// = 100f;
    [SerializeField] private float MainCameraTiltSpeed;// = 100f;
    [SerializeField] private float MainCameraTiltAngleThreshold;// = 10f;
    [SerializeField] private float RoadMaxChangeInAngle;// = 15f;
    [SerializeField] private int RoadMaxVertexCount;// = 30;
    [SerializeField] private int RoadMinVertexCount;// = 6;
    [SerializeField] private int RoadSegmentMinLength;// = 3;
    [SerializeField] private int RoadLaneHeight;// = 1;
    [SerializeField] private int RoadLaneWidth;// = 3;
    [SerializeField] private int RoadSideWalkWidth;// = 3;

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
        CustomRoadBuilder.RoadMaxChangeInAngle = RoadMaxChangeInAngle;
        CustomRoadBuilder.RoadMaxVertexCount = RoadMaxVertexCount;
        CustomRoadBuilder.RoadMinVertexCount = RoadMinVertexCount;
        CustomRoadBuilder.RoadSegmentMinLength = RoadSegmentMinLength;
        CustomRoadBuilder.RoadLaneHeight = RoadLaneHeight;
        CustomRoadBuilder.RoadLaneWidth = RoadLaneWidth;
        CustomRoadBuilder.RoadSideWalkWidth = RoadSideWalkWidth;

        CameraMovement.MainCamera = MainCamera;
        CameraMovement.MainCameraHolder = MainCameraHolder;
        CameraMovement.MainCameraRoot = MainCameraRoot;
        CameraMovement.MainCameraAnchor = MainCameraAnchor;
    }

}

using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.VisualScripting;
//using UnityEngine.UIElements;

public class UIHandling : MonoBehaviour
{


    [SerializeField] private Button _buttonRoad;

    [SerializeField] private Camera _mainCamera;
    [SerializeField] private GameObject _mainCameraHolder;
    [SerializeField] private GameObject _mainCameraRoot;

    void Awake()
    {
        _mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        _mainCameraHolder = GameObject.Find("CameraHolder");

    }
    void Start()
    {
        Button btn = _buttonRoad.GetComponent<Button>();
        btn.onClick.AddListener(OnClickButtonRoad);
    }

    void Update()
    {
        if (CommonController.IsRoadBuildEnabled && CommonController.IsTouchOverNonUI())
        {
            HandleRoadBuilding();
        }
    }

    void OnClickButtonRoad()
    {
        CommonController.IsRoadBuildEnabled = true;

        Debug.Log("Tap on screen to build road");
    }

    private void HandleRoadBuilding()
    {
        var touch0 = Input.GetTouch(0);
        var touch0Position = touch0.position;
        Vector2 touch1Position = new Vector2(0, 0);
        if (Input.touchCount == 2)
            touch1Position = Input.GetTouch(1).position;
        else
        {
            if (touch0Position.x > Screen.width * 1.1f)
                touch1Position.x = touch0Position.x - Screen.width * 0.1f;
            else
                touch1Position.x = touch0Position.x + Screen.width * 0.1f;
            if (touch0Position.y > Screen.width * 1.1f)
                touch1Position.y = touch0Position.y - Screen.width * 0.1f;
            else
                touch1Position.y = touch0Position.y + Screen.width * 0.1f;
        }

        GetStartAndEndPositions(touch0Position, touch1Position, out Vector2 startPosition, out Vector2 endPosition);
        GetControlPoints(startPosition, endPosition, out Vector3 controlPoint0, out Vector3 controlPoint1);

        GetTerrainHitPoint(startPosition, out Vector3 startGoundPosition);
        CommonController.DrawPointSphere(point: startGoundPosition, color: Color.green, size: 20f);
        GetTerrainHitPoint(controlPoint0, out Vector3 control0GoundPosition);
        CommonController.DrawPointSphere(point: control0GoundPosition, color: Color.black, size: 20f);
        GetTerrainHitPoint(endPosition, out Vector3 endGroundPosition);
        CommonController.DrawPointSphere(point: endGroundPosition, color: Color.blue, size: 20f);
        GetTerrainHitPoint(controlPoint1, out Vector3 control1GoundPosition);
        CommonController.DrawPointSphere(point: control1GoundPosition, color: Color.grey, size: 20f);
        Debug.Log("startGoundPosition=" + startGoundPosition);
        Debug.Log("control0GoundPosition=" + control0GoundPosition);
        Debug.Log("endGroundPosition=" + endGroundPosition);
        Debug.Log("control1GoundPosition=" + control1GoundPosition);

        CommonController.IsRoadBuildEnabled = false;

    }

    private void GetStartAndEndPositions(Vector2 touch0Position, Vector2 touch1Position, out Vector2 startPosition, out Vector2 endPosition)
    {
        var screenCenter = GetScreenCenter();
        var distanceFromCenterTo0 = Vector2.Distance(touch0Position, screenCenter);
        var distanceFromCenterTo1 = Vector2.Distance(touch0Position, screenCenter);

        startPosition = touch0Position;
        endPosition = touch1Position;
        if (distanceFromCenterTo0 > distanceFromCenterTo1)
        {
            startPosition = touch1Position;
            endPosition = touch0Position;
        }

    }
    private void GetControlPoints(Vector3 startGoundPosition, Vector3 endGroundPosition, out Vector3 controlPoint0, out Vector3 controlPoint1)
    {

        var directionFrom0To1 = endGroundPosition - startGoundPosition;

        var parallelWidth = 100f;

        Vector3 rightVector = Vector3.Cross(directionFrom0To1, Vector3.up);
        Vector3 leftVector = Vector3.Cross(-directionFrom0To1, Vector3.up).normalized;
        Debug.DrawRay(startGoundPosition, rightVector);
        controlPoint0 = startGoundPosition + (rightVector * parallelWidth);
        controlPoint1 = endGroundPosition + (leftVector * parallelWidth);
    }

    private Vector2 GetScreenCenter()
    {
        return new Vector2(Screen.width / 2, Screen.height / 2);
    }

    private void GetTerrainHitPoint(Vector2 origin, out Vector3 groundPosition)
    {
        var didRayHit = Physics.Raycast(ray: _mainCamera.ScreenPointToRay(origin),
                                        hitInfo: out RaycastHit _rayHit,
                                        maxDistance: _mainCamera.farClipPlane,
                                        layerMask: LayerMask.GetMask("Ground"));
        var size = 0;

        if (didRayHit)
        {
            groundPosition = _rayHit.point;
            groundPosition.x += size / 2;
            groundPosition.y += size / 2;
            groundPosition.z += size / 2;

        }
        else
        {
            groundPosition = new Vector3(0, 0, 0);
        }

    }
}

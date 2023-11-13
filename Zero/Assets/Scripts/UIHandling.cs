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

        GetStartAndEndPositions(touch0Position, touch1Position, out Vector2 startScreenPosition, out Vector2 endScreenPosition);

        GetTerrainHitPoint(startScreenPosition, out Vector3 startGoundPosition);
        GetTerrainHitPoint(endScreenPosition, out Vector3 endGroundPosition);
        GetControlPoints(startGoundPosition, endGroundPosition, out Vector3 control0GoundPosition, out Vector3 control1GoundPosition);
        CommonController.DrawPointSphere(point: startGoundPosition, color: Color.green, size: 20f);
        CommonController.DrawPointSphere(point: endGroundPosition, color: Color.blue, size: 20f);
        CommonController.DrawPointSphere(point: control0GoundPosition, color: Color.black, size: 20f);
        CommonController.DrawPointSphere(point: control1GoundPosition, color: Color.grey, size: 20f);


        CommonController.IsRoadBuildEnabled = false;

    }

    private void GetStartAndEndPositions(Vector2 touch0Position, Vector2 touch1Position, out Vector2 startScreenPosition, out Vector2 endScreenPosition)
    {
        var screenCenter = GetScreenCenter();
        var distanceFromCenterTo0 = Vector2.Distance(touch0Position, screenCenter);
        var distanceFromCenterTo1 = Vector2.Distance(touch0Position, screenCenter);

        startScreenPosition = touch0Position;
        endScreenPosition = touch1Position;
        if (distanceFromCenterTo0 > distanceFromCenterTo1)
        {
            startScreenPosition = touch1Position;
            endScreenPosition = touch0Position;
        }

    }
    private void GetControlPoints(Vector3 startGroundPosition, Vector3 endGroundPosition, out Vector3 controlPoint0, out Vector3 controlPoint1)
    {

        var direction0To1 = endGroundPosition - startGroundPosition;
        var direction1To0 = -direction0To1;
        var distance0To1 = direction0To1.magnitude;

        controlPoint0 = startGroundPosition;
        controlPoint1 = endGroundPosition;
        var rotationVector = Quaternion.AngleAxis(30f, Vector3.up);
        controlPoint0 += rotationVector * direction0To1.normalized * distance0To1 * 0.25f;
        controlPoint1 += rotationVector * direction1To0.normalized * distance0To1 * 0.25f;

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

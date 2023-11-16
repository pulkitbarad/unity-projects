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
    private Vector3 _mainCameraTargetPosition;

    void Awake()
    {

        //_mainCameraRoot = GameObject.Find("MainCameraRoot");
        //_mainCameraHolder = GameObject.Find("MainCameraHolder");
        //_mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();

        _mainCameraTargetPosition = CommonController.MainCameraRoot.transform.position;

    }
    void Start()
    {
        Button btn = CommonController.ButtonRoad.GetComponent<Button>();
        btn.onClick.AddListener(OnClickButtonRoad);
    }

    void Update()
    {
        if (CommonController.IsTouchOverNonUI())
        {
            if (CommonController.IsRoadMenuActive)
            {
                HandleBuildRoad();
            }
            else
            {
                HandleCameraMovement();
            }
        }
    }

    void OnClickButtonRoad()
    {
        CommonController.IsRoadMenuActive = true;

        Debug.Log("Tap on screen to build road");
    }

    private void HandleCameraMovement()
    {

        var touch = Input.GetTouch(0);

        Vector3 right = -CommonController.MainCameraRoot.transform.right * touch.deltaPosition.x;
        Vector3 forward = -CommonController.MainCameraRoot.transform.forward * touch.deltaPosition.y;
        var input = (forward + right);
        Vector3 nextTargetPosition = _mainCameraTargetPosition + input / 10 * CommonController.MainCameraMoveSpeed;
        _mainCameraTargetPosition = nextTargetPosition;
        CommonController.MainCameraRoot.transform.position = Vector3.Lerp(CommonController.MainCameraRoot.transform.position, nextTargetPosition, Time.deltaTime * 100 * CommonController.MainCameraSmoothing);

    }

    private void HandleBuildRoad()
    {


        if (CommonController.StartObject.transform.position.Equals(Vector3.zero)
            && CommonController.EndObject.transform.position.Equals(Vector3.zero))
        {
            InitializeStartAndEndPositions();
        }
        else
        {
            CommonController.HandleRoadObjectsDrag();
        }

        CommonController.CurvedLine.FindBazierLinePoints(
            startObject: CommonController.StartObject,
            endObject: CommonController.EndObject,
            vertexCount: 10,
            startControlObject: CommonController.StartControlObject,
            endControlObject: CommonController.EndControlObject,
            bazierLinePoints: out List<Vector3> bazierLinePoints);

        CommonController.IsDebugEnabled = true;
        CommonController.TestRendrer.RenderLine(
            name: "Road_center",
            color: Color.blue,
            width: 10,
            pointSize: 20,
            linePoints: bazierLinePoints.ToArray());

        CommonController.IsDebugEnabled = false;
    }


    private void InitializeStartAndEndPositions()
    {
        var touch0 = Input.GetTouch(0);
        var touch0Position = touch0.position;
        Vector2 touch1Position;
        if (touch0Position.x > Screen.width * 1.1f)
            touch1Position.x = touch0Position.x - Screen.width * 0.1f;
        else
            touch1Position.x = touch0Position.x + Screen.width * 0.1f;
        if (touch0Position.y > Screen.width * 1.1f)
            touch1Position.y = touch0Position.y - Screen.width * 0.1f;
        else
            touch1Position.y = touch0Position.y + Screen.width * 0.1f;

        var screenCenter = GetScreenCenter();
        var distanceFromCenterTo0 = Vector2.Distance(touch0Position, screenCenter);
        var distanceFromCenterTo1 = Vector2.Distance(touch0Position, screenCenter);

        Vector2 startScreenPosition = touch0Position;
        Vector2 endScreenPosition = touch1Position;
        if (distanceFromCenterTo0 > distanceFromCenterTo1)
        {
            startScreenPosition = touch1Position;
            endScreenPosition = touch0Position;
        }
        CommonController.StartObject.transform.position = GetTerrainHitPoint(startScreenPosition);
        CommonController.EndObject.transform.position = GetTerrainHitPoint(endScreenPosition);
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

    private Vector3 GetTerrainHitPoint(Vector2 origin)
    {
        Vector3 groundPosition = Vector3.zero;

        if (
            Physics.Raycast(ray: CommonController.MainCamera.ScreenPointToRay(origin),
                hitInfo: out RaycastHit _rayHit,
                maxDistance: CommonController.MainCamera.farClipPlane,
                layerMask: LayerMask.GetMask("Ground")))
        {
            groundPosition = _rayHit.point;

        }
        return groundPosition;

    }
}

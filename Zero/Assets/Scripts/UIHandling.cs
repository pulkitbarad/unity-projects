using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.VisualScripting;

public class UIHandling : MonoBehaviour
{

    void Start()
    {

    }

    void Update()
    {
        HandleButtonEvents();

        if (CommonController.IsTouchOverNonUI())
        {
            if (CommonController.IsRoadMenuActive)
            {
                HandleBuildRoad();
            }
            else
            {
                CommonController.CameraMovement.MoveCamera();
            }
        }
    }

    void HandleButtonEvents()
    {
        CommonController.RunWhileTouchHold(
            button: CommonController.ZoomInButton,
            directionFlag: true,
            magnitude: CommonController.MainCameraZoomSpeed,
            onButtonDown: CommonController.CameraMovement.ZoomCamera);
        CommonController.RunWhileTouchHold(
            button: CommonController.ZoomOutButton,
            directionFlag: false,
            magnitude: CommonController.MainCameraZoomSpeed,
            onButtonDown: CommonController.CameraMovement.ZoomCamera);

        CommonController.RunWhileTouchHold(
            button: CommonController.RotateClockwiseButton,
            directionFlag: true,
            magnitude: CommonController.MainCameraRotationSpeed,
            onButtonDown: CommonController.CameraMovement.RotateCamera);
        CommonController.RunWhileTouchHold(
            button: CommonController.RotateAntiClockwiseButton,
            directionFlag: false,
            magnitude: CommonController.MainCameraRotationSpeed,
            onButtonDown: CommonController.CameraMovement.RotateCamera);

        CommonController.RunWhileTouchHold(
            button: CommonController.TiltUpButton,
            directionFlag: true,
            magnitude: CommonController.MainCameraTiltSpeed,
            onButtonDown: CommonController.CameraMovement.TiltCamera);
        CommonController.RunWhileTouchHold(
            button: CommonController.TiltDownButton,
            directionFlag: false,
            magnitude: CommonController.MainCameraTiltSpeed,
            onButtonDown: CommonController.CameraMovement.TiltCamera);

    }

    void OnClickButtonRoad()
    {
        CommonController.IsRoadMenuActive = true;
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
        CommonController.StartObject.transform.position = CommonController.CameraMovement.GetTerrainHitPoint(startScreenPosition);
        CommonController.EndObject.transform.position = CommonController.CameraMovement.GetTerrainHitPoint(endScreenPosition);
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
}

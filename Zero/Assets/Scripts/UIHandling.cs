using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

public class UIHandling : MonoBehaviour
{

    private PlayerInput playerInput;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    public void OnMove(InputValue value){

        Debug.Log("value="+value.Get<Vector2>());

    }
    public void OnLook(InputValue value){

        Debug.Log("value="+value.Get<Vector2>());

    }

    void Update()
    {
        // HandleButtonEvents();

        if (CommonController.IsTouchOverNonUI())
        {
            if (CommonController.HandleRoadObjectsDrag())
            {
                CommonController.RebuildRoad();
            }
            else
            {
                // CommonController.CameraMovement.MoveCamera();
                // CommonController.CameraMovement.HandleTouchZoomAndTilt();
            }
        }
    }

    void HandleButtonEvents()
    {
        CommonController.InvokeOnTapHold(
            button: CommonController.ZoomInButton,
            directionFlag: true,
            magnitude: CommonController.MainCameraZoomSpeed,
            onButtonDown: CommonController.CameraMovement.ZoomCamera);
        CommonController.InvokeOnTapHold(
            button: CommonController.ZoomOutButton,
            directionFlag: false,
            magnitude: CommonController.MainCameraZoomSpeed,
            onButtonDown: CommonController.CameraMovement.ZoomCamera);

        CommonController.InvokeOnTapHold(
            button: CommonController.RotateClockwiseButton,
            directionFlag: true,
            magnitude: CommonController.MainCameraRotationSpeed,
            onButtonDown: CommonController.CameraMovement.RotateCamera);
        CommonController.InvokeOnTapHold(
            button: CommonController.RotateAntiClockwiseButton,
            directionFlag: false,
            magnitude: CommonController.MainCameraRotationSpeed,
            onButtonDown: CommonController.CameraMovement.RotateCamera);

        CommonController.InvokeOnTapHold(
            button: CommonController.TiltUpButton,
            directionFlag: true,
            magnitude: CommonController.MainCameraTiltSpeed,
            onButtonDown: CommonController.CameraMovement.TiltCamera);
        CommonController.InvokeOnTapHold(
            button: CommonController.TiltDownButton,
            directionFlag: false,
            magnitude: CommonController.MainCameraTiltSpeed,
            onButtonDown: CommonController.CameraMovement.TiltCamera);
        CommonController.InvokeOnTap(
              button: CommonController.CurvedRoadButton,
              onButtonDown: () =>
                {
                    CommonController.IsRoadMenuActive = true;
                });

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

}

using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIHandling : MonoBehaviour
{

    private bool _isZoomInButtonDown = false;
    private bool _isZoomOutButtonDown = false;
    private MainActions _mainActions;
    private InputAction _zoomInAction;
    private InputAction _zoomOutAction;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _touchAction;

    void Awake()
    {
    }

    void Start()
    {
        _mainActions = new MainActions();
        _moveAction = _mainActions.Player.Move;
        _lookAction = _mainActions.Player.Look;
        _zoomOutAction = _mainActions.Player.ZoomOut;
        _zoomInAction = _mainActions.Player.ZoomIn;
        _touchAction = _mainActions.Player.Touch;

        _zoomOutAction.Enable();
        _zoomInAction.Enable();
        _moveAction.Enable();
        _lookAction.Enable();
        _touchAction.Enable();
    }

    void Update()
    {

        if (_touchAction.phase != InputActionPhase.Waiting)
        {
            Debug.Log("touch phase=" + _touchAction.phase);

        }

        if (_zoomOutAction.phase == InputActionPhase.Performed)
        {
            CommonController.CameraMovement.TiltCamera(-1f);
        }
        if (_zoomInAction.phase == InputActionPhase.Performed)
        {
            CommonController.CameraMovement.TiltCamera(1f);
        }
        if (_moveAction.phase == InputActionPhase.Started)
        {
            CommonController.CameraMovement.MoveCamera(_moveAction.ReadValue<Vector2>());
        }
        if (_lookAction.phase == InputActionPhase.Started)
        {
            // CommonController.CameraMovement.TiltCamera(_lookAction.ReadValue<Vector2>().y);
            CommonController.CameraMovement.ZoomCamera(_lookAction.ReadValue<Vector2>().y * 100f);
            CommonController.CameraMovement.RotateCamera(-1f * _lookAction.ReadValue<Vector2>().x);
        }
        // HandleButtonEvents();
        // if (CommonController.IsTouchOverNonUI())
        // {
        //     if (CommonController.HandleRoadObjectsDrag())
        //     {
        //         CommonController.RebuildRoad();
        //     }
        //     else
        //     {
        //         // CommonController.CameraMovement.MoveCamera();
        //         // CommonController.CameraMovement.HandleTouchZoomAndTilt();
        //     }
        // }
    }


    void HandleButtonEvents()
    {
        // CommonController.InvokeOnTapHold(
        //     button: CommonController.ZoomInButton,
        //     directionFlag: true,
        //     magnitude: CommonController.MainCameraZoomSpeed,
        //     onButtonDown: CommonController.CameraMovement.ZoomCamera);
        // CommonController.InvokeOnTapHold(
        //     button: CommonController.ZoomOutButton,
        //     directionFlag: false,
        //     magnitude: CommonController.MainCameraZoomSpeed,
        //     onButtonDown: CommonController.CameraMovement.ZoomCamera);

        // CommonController.InvokeOnTapHold(
        //     button: CommonController.RotateClockwiseButton,
        //     directionFlag: true,
        //     magnitude: CommonController.MainCameraRotationSpeed,
        //     onButtonDown: CommonController.CameraMovement.RotateCamera);
        // CommonController.InvokeOnTapHold(
        //     button: CommonController.RotateAntiClockwiseButton,
        //     directionFlag: false,
        //     magnitude: CommonController.MainCameraRotationSpeed,
        //     onButtonDown: CommonController.CameraMovement.RotateCamera);

        // CommonController.InvokeOnTapHold(
        //     button: CommonController.TiltUpButton,
        //     directionFlag: true,
        //     magnitude: CommonController.MainCameraTiltSpeed,
        //     onButtonDown: CommonController.CameraMovement.TiltCamera);
        // CommonController.InvokeOnTapHold(
        //     button: CommonController.TiltDownButton,
        //     directionFlag: false,
        //     magnitude: CommonController.MainCameraTiltSpeed,
        //     onButtonDown: CommonController.CameraMovement.TiltCamera);
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

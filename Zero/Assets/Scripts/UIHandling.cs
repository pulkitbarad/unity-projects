using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class UIHandling : MonoBehaviour
{

    private MainActions _mainActions;
    private InputAction _zoomInAction;
    private InputAction _zoomOutAction;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _touch0Action;
    private InputAction _touch1Action;
    private InputAction _singleTouchAction;
    private InputAction _doubleTouchAction;
    private InputAction _roadAction;
    private InputAction _confirmAction;
    private InputAction _cancelAction;


    void Start()
    {
        _mainActions = new MainActions();
        _moveAction = _mainActions.Player.Move;
        _lookAction = _mainActions.Player.Look;
        _zoomOutAction = _mainActions.Player.ZoomOut;
        _zoomInAction = _mainActions.Player.ZoomIn;
        _touch0Action = _mainActions.Player.Touch0Position;
        _touch1Action = _mainActions.Player.Touch1Position;
        _singleTouchAction = _mainActions.Player.SingleTouchContact;
        _doubleTouchAction = _mainActions.Player.DoubleTouchContact;
        _roadAction = _mainActions.Player.Road;
        _confirmAction = _mainActions.Player.Confirm;
        _cancelAction = _mainActions.Player.Cancel;
        _doubleTouchAction.started += OnTouch0Start;
        _doubleTouchAction.started += OnTouch1Start;
        _doubleTouchAction.canceled += OnTouch0End;
        _doubleTouchAction.canceled += OnTouch1End;
        _singleTouchAction.started += OnTouch0Start;
        _singleTouchAction.canceled += OnTouch0End;
        _roadAction.performed += OnRoadPerformed;
        _confirmAction.performed += OnConfirmPerformed;
        _cancelAction.performed += OnCancelPerformed;


        _zoomOutAction.Enable();
        _zoomInAction.Enable();
        _moveAction.Enable();
        _lookAction.Enable();
        _touch0Action.Enable();
        _touch1Action.Enable();
        _singleTouchAction.Enable();
        _doubleTouchAction.Enable();
        _roadAction.Enable();
        _confirmAction.Enable();
        _cancelAction.Enable();
    }

    void Update()
    {

        if (_doubleTouchAction.phase == InputActionPhase.Performed)
        {
            Vector2 currentTouch0 = _touch0Action.ReadValue<Vector2>();
            Vector2 currentTouch1 = _touch1Action.ReadValue<Vector2>();
        }
        else if (_singleTouchAction.phase == InputActionPhase.Performed)
        {
            Vector2 currentTouch0 = _touch0Action.ReadValue<Vector2>();
            CommonController.HandleRoadObjectsDrag(currentTouch0);
        }

        // if (_doubleTouchAction.phase == InputActionPhase.Performed)
        // {
        //     Vector2 currentTouch0 = _touch0Action.ReadValue<Vector2>();
        //     Vector2 currentTouch1 = _touch1Action.ReadValue<Vector2>();
        //     // CommonController.CameraMovement.TiltCamera(currentTouch0, currentTouch1);
        //     CommonController.CameraMovement.ZoomCamera(currentTouch0, currentTouch1);
        // }

        if (_zoomOutAction.phase == InputActionPhase.Performed)
        {
            CommonController.CameraMovement.ZoomCamera(-1f * CommonController.MainCameraZoomSpeed);
        }
        if (_zoomInAction.phase == InputActionPhase.Performed)
        {
            CommonController.CameraMovement.ZoomCamera(CommonController.MainCameraZoomSpeed);
        }
        if (_moveAction.phase == InputActionPhase.Started)
        {
            CommonController.CameraMovement.MoveCamera(_moveAction.ReadValue<Vector2>());
        }
        if (_lookAction.phase == InputActionPhase.Started)
        {
            float verticalValue = _lookAction.ReadValue<Vector2>().y;
            float horizontalValue = _lookAction.ReadValue<Vector2>().x;
            if (Math.Abs(verticalValue) > 0.5)
            {
                CommonController.CameraMovement.TiltCamera(verticalValue);
            }
            if (Math.Abs(horizontalValue) > 0.5)
            {
                CommonController.CameraMovement.RotateCamera(-1f * _lookAction.ReadValue<Vector2>().x);
            }
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

    private void OnRoadPerformed(InputAction.CallbackContext context)
    {
        CommonController.InitializeStartAndEndPositions();


    }

    private void OnConfirmPerformed(InputAction.CallbackContext context)
    {

    }

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        CommonController.IsRoadMenuActive = false;
        CommonController.DeactivateRoadControlPoints();
        Destroy(CommonController.CurrentRoadCenterLine);
    }


    private void OnTouch0Start(InputAction.CallbackContext context)
    {
        CommonController.StartTouch0 = _touch0Action.ReadValue<Vector2>();
    }
    private void OnTouch0End(InputAction.CallbackContext context)
    {
        CommonController.StartTouch0 = Vector2.zero;
        CommonController.ObjectBeingDragged = "";

    }

    private void OnTouch1Start(InputAction.CallbackContext context)
    {
        CommonController.StartTouch1 = _touch1Action.ReadValue<Vector2>();
    }
    private void OnTouch1End(InputAction.CallbackContext context)
    {
        CommonController.StartTouch1 = Vector2.zero;
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

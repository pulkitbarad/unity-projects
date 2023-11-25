using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIHandling : MonoBehaviour
{

    private bool _isZoomInButtonDown = false;
    private bool _isZoomOutButtonDown = false;

    private DefaultActions _defaultActions;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _zoomInAction;
    private InputAction _zoomOutAction;

    void Awake()
    {
        _defaultActions = new DefaultActions();
    }
    // void OnEnable()
    // {
    // }

    // void OnDisable()
    // {

    // }

    void Start()
    {

        _moveAction = _defaultActions.NewActionMap.Move;
        // _defaultActions.NewActionMap.Move.performed += OnMove;
        _moveAction.Enable();
        _lookAction = _defaultActions.NewActionMap.Look;
        // _defaultActions.NewActionMap.Look.performed += OnMove;
        _lookAction.Enable();
        _zoomInAction = _defaultActions.NewActionMap.ZoomIn;
        _defaultActions.NewActionMap.ZoomIn.performed += OnZoomIn;
        _zoomInAction.Enable();
        _zoomOutAction = _defaultActions.NewActionMap.ZoomOut;
        _defaultActions.NewActionMap.ZoomOut.performed += OnZoomOut;
        _zoomOutAction.Enable();
    }
    void FixedUpdate()
    {
        // HandleButtonEvents();

        Vector2 moveDir = _moveAction.ReadValue<Vector2>();
        Vector2 lookDir = _lookAction.ReadValue<Vector2>();
        Debug.Log("moveDir=" + moveDir);
        Debug.Log("lookDir=" + lookDir);

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


    private void OnZoomIn(InputAction.CallbackContext context)
    {
        Debug.Log("zoom in");

    }

    private void OnZoomOut(InputAction.CallbackContext context)
    {
        Debug.Log("zoom out");

    }

    // public void OnMove(InputAction.CallbackContext context)
    // {
    //     // CommonController.CameraMovement.MoveCamera(value.Get<Vector2>());

    //     Debug.Log("move value=" + context.action.ReadValue<Vector2>());

    // }
    // public void OnLook(InputAction.CallbackContext context)
    // {

    //     // CommonController.CameraMovement.TiltCamera(value.Get<Vector2>().y);
    //     CommonController.CameraMovement.RotateCamera(context.action.ReadValue<Vector2>().x);

    // }

    // public void OnZoomIn(InputValue value)
    // {
    //     // CommonController.CameraMovement.TiltCamera(value.Get<Vector2>().y);
    //     // CommonController.CameraMovement.RotateCamera(value.Get<float>());
    //     _isZoomInButtonDown = !_isZoomInButtonDown;
    //     Debug.Log("zoom in value=" + value.Get<float>());
    //     if (_isZoomInButtonDown)
    //     {
    //         Debug.Log("zoom in started");
    //     }
    //     else
    //     {
    //         Debug.Log("zoom in finished");
    //     }

    // }

    // public void OnZoomOut(InputValue value)
    // {
    //     // CommonController.CameraMovement.TiltCamera(value.Get<Vector2>().y);
    //     // CommonController.CameraMovement.RotateCamera(value.Get<float>());
    //     _isZoomOutButtonDown = !_isZoomOutButtonDown;
    //     Debug.Log("zoom out value=" + value.Get<float>());
    //     if (_isZoomOutButtonDown)
    //     {
    //         Debug.Log("zoom out started");
    //     }
    //     else
    //     {
    //         Debug.Log("zoom out finished");

    //     }

    // }


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

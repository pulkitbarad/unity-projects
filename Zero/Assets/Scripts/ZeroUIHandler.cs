using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class ZeroUIHandler
{

    public static ZeroActions _zeroActions;
    public static InputAction _zoomInAction;
    public static InputAction _zoomOutAction;
    public static InputAction _moveAction;
    public static InputAction _lookAction;
    public static InputAction _touch0Action;
    public static InputAction _touch1Action;
    public static InputAction _singleTouchAction;
    public static InputAction _doubleTouchAction;
    public static InputAction _curvedRoadAction;
    public static InputAction _straightRoadAction;
    public static InputAction _confirmAction;
    public static InputAction _cancelAction;
    public static InputAction _writeLogsAction;
    public static bool _isRoadMenuActive = false;
    public static Vector2 _startTouch0 = Vector2.zero;
    public static Vector2 _startTouch1 = Vector2.zero;
    private static string _objectBeingDragged = "";


    public static void Initialise()
    {
        _zeroActions = new ZeroActions();
        _moveAction = _zeroActions.Player.Move;
        _lookAction = _zeroActions.Player.Look;
        _zoomOutAction = _zeroActions.Player.ZoomOut;
        _zoomInAction = _zeroActions.Player.ZoomIn;
        _touch0Action = _zeroActions.Player.Touch0Position;
        _touch1Action = _zeroActions.Player.Touch1Position;
        _singleTouchAction = _zeroActions.Player.SingleTouchContact;
        _doubleTouchAction = _zeroActions.Player.DoubleTouchContact;
        _curvedRoadAction = _zeroActions.Player.CurvedRoad;
        _straightRoadAction = _zeroActions.Player.StraightRoad;
        _confirmAction = _zeroActions.Player.Confirm;
        _cancelAction = _zeroActions.Player.Cancel;
        _writeLogsAction = _zeroActions.Player.WriteLogs;
        _doubleTouchAction.started += OnTouch0Start;
        _doubleTouchAction.started += OnTouch1Start;
        _doubleTouchAction.canceled += OnTouch0End;
        _doubleTouchAction.canceled += OnTouch1End;
        _singleTouchAction.started += OnTouch0Start;
        _singleTouchAction.canceled += OnTouch0End;
        _curvedRoadAction.performed += OnCurvedRoadPerformed;
        _straightRoadAction.performed += OnStraightRoadPerformed;
        _confirmAction.performed += OnConfirmPerformed;
        _cancelAction.performed += OnCancelPerformed;
        _writeLogsAction.performed += OnWriteLogsPerformed;


        _zoomOutAction.Enable();
        _zoomInAction.Enable();
        _moveAction.Enable();
        _lookAction.Enable();
        _touch0Action.Enable();
        _touch1Action.Enable();
        _singleTouchAction.Enable();
        _doubleTouchAction.Enable();
        _curvedRoadAction.Enable();
        _straightRoadAction.Enable();
        _confirmAction.Enable();
        _cancelAction.Enable();
        _writeLogsAction.Enable();
    }

    public static void HandleInputChanges()
    {
        if (_doubleTouchAction.phase == InputActionPhase.Performed)
        {
            Vector2 currentTouch0 = _touch0Action.ReadValue<Vector2>();
            Vector2 currentTouch1 = _touch1Action.ReadValue<Vector2>();
        }
        else if (_singleTouchAction.phase == InputActionPhase.Performed)
        {
            Vector2 currentTouch0 = _touch0Action.ReadValue<Vector2>();
            if (_isRoadMenuActive)
                ZeroRoadBuilder.HandleControlDrag(
                    isCurved: ZeroRoadBuilder.CurrentActiveRoad.IsCurved,
                    touchPosition: currentTouch0);
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
            ZeroCameraMovement.ZoomCamera(-1f * ZeroCameraMovement.MainCameraZoomSpeed);
        }
        if (_zoomInAction.phase == InputActionPhase.Performed)
        {
            ZeroCameraMovement.ZoomCamera(ZeroCameraMovement.MainCameraZoomSpeed);
        }
        if (_moveAction.phase == InputActionPhase.Started)
        {
            ZeroCameraMovement.MoveCamera(_moveAction.ReadValue<Vector2>());
        }
        if (_lookAction.phase == InputActionPhase.Started)
        {
            float verticalValue = _lookAction.ReadValue<Vector2>().y;
            float horizontalValue = _lookAction.ReadValue<Vector2>().x;
            if (Math.Abs(verticalValue) > 0.5)
            {
                ZeroCameraMovement.TiltCamera(verticalValue);
            }
            if (Math.Abs(horizontalValue) > 0.5)
            {
                ZeroCameraMovement.RotateCamera(-1f * _lookAction.ReadValue<Vector2>().x);
            }
        }
    }

    public static void OnCurvedRoadPerformed(InputAction.CallbackContext context)
    {
        _isRoadMenuActive = true;
        ZeroRoadBuilder.StartBuilding(true);
    }

    public static void OnStraightRoadPerformed(InputAction.CallbackContext context)
    {
        _isRoadMenuActive = true;
        ZeroRoadBuilder.StartBuilding(false);
    }

    public static void OnConfirmPerformed(InputAction.CallbackContext context)
    {
        _isRoadMenuActive = false;
        ZeroRoadBuilder.ConfirmBuilding();
    }

    public static void OnCancelPerformed(InputAction.CallbackContext context)
    {
        _isRoadMenuActive = false;
        ZeroRoadBuilder.CancelBuilding();
    }

    public static void OnWriteLogsPerformed(InputAction.CallbackContext context)
    {
        ZeroController.WriteDebugFile();
    }


    public static void OnTouch0Start(InputAction.CallbackContext context)
    {
        StartOfSingleTouchDrag(_touch0Action.ReadValue<Vector2>());
    }
    public static void OnTouch0End(InputAction.CallbackContext context)
    {
        EndOfSingleTouchDrag();
    }

    public static void OnTouch1Start(InputAction.CallbackContext context)
    {
        StartOfMultiTouchDrag(_touch0Action.ReadValue<Vector2>(), _touch1Action.ReadValue<Vector2>());
    }
    public static void OnTouch1End(InputAction.CallbackContext context)
    {
        EndOfMultiTouchDrag();
    }


    public static void StartOfSingleTouchDrag(Vector2 touchPosition)
    {
        _startTouch0 = touchPosition;
    }

    public static void StartOfMultiTouchDrag(Vector2 touch0Position, Vector2 touch1Position)
    {
        _startTouch0 = touch0Position;
        _startTouch1 = touch1Position;
    }

    public static void EndOfSingleTouchDrag()
    {
        _startTouch0 = Vector2.zero;
        _objectBeingDragged = "";
    }

    public static void EndOfMultiTouchDrag()
    {
        _startTouch0 = Vector2.zero;
        _startTouch1 = Vector2.zero;
        _objectBeingDragged = "";
    }

    public static bool HandleGameObjectDrag(GameObject gameObject, Vector2 touchPosition, GameObject followerObject = null)
    {
        if (!EventSystem.current.IsPointerOverGameObject() || _objectBeingDragged.Length > 0)
        {
            Ray touchPointRay = ZeroCameraMovement.MainCamera.ScreenPointToRay(touchPosition);
            gameObject.SetActive(true);
            if ((_objectBeingDragged.Length > 0 && _objectBeingDragged.Equals(gameObject.name))
                || (Physics.Raycast(touchPointRay, out RaycastHit hit)
                && hit.transform == gameObject.transform))
            {
                Vector3 oldPosition = gameObject.transform.position;
                Vector3 newPosition = ZeroCameraMovement.GetTerrainHitPoint(touchPosition);
                newPosition.y = 0;
                gameObject.transform.position = newPosition;
                if (followerObject != null)
                {
                    Vector3 followerOldPosition = followerObject.transform.position;
                    Vector3 followerNewPosition = newPosition - oldPosition + followerOldPosition;

                    followerObject.transform.position = followerNewPosition;

                }
                _objectBeingDragged = gameObject.name;
                return true;
            }
        }
        return false;
    }
    public static void GetControlPoints(Vector3 startGroundPosition, Vector3 endGroundPosition, out Vector3 controlPoint0, out Vector3 controlPoint1)
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

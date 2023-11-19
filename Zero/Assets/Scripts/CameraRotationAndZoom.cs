using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraRotationAndZoom : MonoBehaviour
{
    private bool isZoomInProgress = false;
    private bool isTiltInProgress = false;

    [SerializeField] private Vector2 _range = new(100, 100);

    private void Awake()
    {

    }

    private void Update()
    {
        if (!CommonController.IsRoadMenuActive && CommonController.IsTouchOverNonUI(suppressTouchEndEvent: false))
        {
            HandleTouchZoomAndTilt();
            HandleMouseZoom();
        }
    }
    private void HandleTouchZoomAndTilt()
    {
        if (Input.touchCount == 2)
        {
            var touch0 = Input.GetTouch(0);
            var touch1 = Input.GetTouch(1);

            var maxDeltaMagnitude = Math.Abs(Math.Max(touch0.deltaPosition.magnitude, touch1.deltaPosition.magnitude));

            if (maxDeltaMagnitude <= 0)
                return;

            if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended)
            {
                isZoomInProgress = false;
                isTiltInProgress = false;
            }
            var touch0DeltaPosition = touch0.deltaPosition;
            var touch1DeltaPosition = touch1.deltaPosition;
            var delta0VerticalAngle = Vector2.Angle(touch0DeltaPosition, Vector2.up);
            var delta1VerticalAngle = Vector2.Angle(touch1DeltaPosition, Vector2.up);

            Debug.Log("IsTouchPinchingOut=" + CommonController.CameraMovement.IsTouchPinchingOut(touch0, touch1));
            if (!isZoomInProgress
                && AreBothGesturesVertical(delta0VerticalAngle, delta1VerticalAngle))
            {
                //Lock the current movement for tilt only
                if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
                    isTiltInProgress = true;
                //Vertical tilt gesture
                if (delta0VerticalAngle > 90 || delta1VerticalAngle > 90)
                    CommonController.CameraMovement.TiltCamera(isTiltup: false, magnitude: maxDeltaMagnitude / 100 * CommonController.MainCameraTiltSpeedTouch);
                else
                    CommonController.CameraMovement.TiltCamera(isTiltup: true, magnitude: maxDeltaMagnitude / 100 * CommonController.MainCameraTiltSpeedTouch);
            }
            else if (!isTiltInProgress)
            {

                if (CommonController.CameraMovement.IsTouchPinchingOut(touch0, touch1))
                    //If Zoom-in, inverse the direction
                    CommonController.CameraMovement.ZoomCamera(isZoomIn: true, magnitude: maxDeltaMagnitude / 10 * CommonController.MainCameraZoomSpeedTouch);
                else
                    CommonController.CameraMovement.ZoomCamera(isZoomIn: false, magnitude: maxDeltaMagnitude / 10 * CommonController.MainCameraZoomSpeedTouch);
            }
        }
    }


    private bool AreBothGesturesVertical(float delta0VerticalAngle, float delta1VerticalAngle)
    {
        return (delta0VerticalAngle < CommonController.MainCameraRotateAngleThreshold
                && delta1VerticalAngle < CommonController.MainCameraRotateAngleThreshold)
            || (delta0VerticalAngle > (180 - CommonController.MainCameraRotateAngleThreshold)
                && delta1VerticalAngle > (180 - CommonController.MainCameraRotateAngleThreshold));
    }

    private void HandleMouseZoom()
    {
        Vector3 cameraDirection =
            CommonController
            .MainCameraRoot
            .transform
            .InverseTransformDirection(
                CommonController
                .MainCameraHolder
                .transform
                .forward);

        float _input = Input.GetAxisRaw("Mouse ScrollWheel");
        var currentPosition = CommonController.MainCameraHolder.transform.localPosition;
        Vector3 targetPosition = currentPosition + cameraDirection * (_input * CommonController.MainCameraZoomSpeed);
        // if (IsInBounds(nextTargetPosition)) _targetPosition = nextTargetPosition;
        CommonController.MainCameraHolder.transform.localPosition
            = Vector3.Lerp(
                CommonController.MainCameraHolder.transform.localPosition,
                targetPosition,
                Time.deltaTime * CommonController.MainCameraSmoothing);
    }

    private bool IsInBounds(Vector3 position)
    {
        return position.x > -_range.x &&
               position.x < _range.x &&
               position.z > -_range.y &&
               position.z < _range.y;
    }

    public static bool IsPointerOverGameObject(GameObject gameObject)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults.Any(x => x.gameObject == gameObject);
    }
}

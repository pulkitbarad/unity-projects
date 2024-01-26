using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroCameraMovement
{
    private static float _cameraInitialHeight;

    public static float MainCameraMoveSpeed;
    public static float MainCameraSmoothing;
    public static float MainCameraZoomSpeed;
    public static float MainCameraRotationSpeed;
    public static float MainCameraTiltSpeed;
    public static float MainCameraTiltAngleThreshold;
    public static Camera MainCamera;
    public static GameObject MainCameraAnchor;
    public static GameObject MainCameraHolder;
    public static GameObject MainCameraRoot;

    public static void Initialise()
    {
        _cameraInitialHeight = 1f;
        InitConfig();
        Transform rootTransform = MainCameraRoot.transform;
        rootTransform.position = new Vector3(-67f, 0f, 33f);

        Transform anchorTransform = MainCameraAnchor.transform;
        anchorTransform.localPosition = Vector3.zero;
        anchorTransform.SetParent(rootTransform);

        Transform holderTransform = MainCameraHolder.transform;
        //do not set y position zero, it is used as a divisor in relative object scaling
        holderTransform.localPosition = new Vector3(0, 20, -20);
        holderTransform.eulerAngles = new Vector3(45, 0, 0);
        holderTransform.SetParent(anchorTransform);

        Transform cameraTransform = MainCamera.transform;
        cameraTransform.localPosition = Vector3.zero;
        cameraTransform.SetParent(holderTransform);

        _cameraInitialHeight = holderTransform.localPosition.y;
    }

    private static void InitConfig()
    {
        MainCameraMoveSpeed = 0.5f;
        MainCameraSmoothing = 2;
        MainCameraZoomSpeed = 50;
        MainCameraRotationSpeed = 50;
        MainCameraTiltSpeed = 1f;
        MainCameraTiltAngleThreshold = 10;

    }

    public static void MoveCamera(Vector2 direction)
    {
        Transform rootTransform = MainCameraRoot.transform;
        Vector3 right = rootTransform.right * direction.x;
        Vector3 forward = rootTransform.forward * direction.y;
        var input = (forward + right).normalized;

        float moveSpeed =
            MainCameraMoveSpeed
            * MainCameraHolder.transform.localPosition.y
            / _cameraInitialHeight;

        Vector3 targetCameraPosition =
            rootTransform.position + (moveSpeed * input);

        rootTransform.position
            = Vector3.Lerp(
                a: rootTransform.position,
                b: targetCameraPosition,
                t: Time.deltaTime * 100 * MainCameraSmoothing);
    }

    public static void TiltCamera(Vector2 currentTouch0, Vector2 currentTouch1)
    {
        var touch0Delta = currentTouch0 - ZeroUIHandler._startTouch0;
        var touch1Delta = currentTouch1 - ZeroUIHandler._startTouch1;

        var maxDeltaMagnitude = Math.Abs(Math.Max(touch0Delta.magnitude, touch1Delta.magnitude));
        if (maxDeltaMagnitude < 0)
            return;
        var delta0VerticalAngle = Vector2.Angle(touch0Delta, Vector2.up);
        var delta1VerticalAngle = Vector2.Angle(touch1Delta, Vector2.up);
        if (AreBothGesturesVertical(delta0VerticalAngle, delta1VerticalAngle))
        {

            if (delta0VerticalAngle > 90 || delta1VerticalAngle > 90)
                TiltCamera(Math.Abs(maxDeltaMagnitude) / 100f);
            else
                TiltCamera(Math.Abs(maxDeltaMagnitude) / -100f);
        }
    }

    public static void TiltCamera(float magnitude)
    {
        Transform rootTransform = MainCameraAnchor.transform;
        float currentHorizontalAngle, targetHorizontalAngle;
        currentHorizontalAngle = targetHorizontalAngle = rootTransform.eulerAngles.x;

        targetHorizontalAngle += magnitude * MainCameraTiltSpeed;
        // var targetEurlerAngle = Mathf.Lerp(currentHorizontalAngle, targetHorizontalAngle, Time.deltaTime * MainCameraSmoothing);
        // rootTransform.rotation = Quaternion.AngleAxis(GetValidTiltAngle(targetEurlerAngle), rootTransform.right);
        rootTransform.eulerAngles =
        new Vector3(
            GetValidTiltAngle(targetHorizontalAngle),
            rootTransform.eulerAngles.y,
            rootTransform.eulerAngles.z
        );
    }

    private static float GetValidTiltAngle(float targetEurlerAngle)
    {
        var tempAngle = targetEurlerAngle;
        float holderLocalAngle = MainCameraHolder.transform.localEulerAngles.x;
        if (tempAngle > 180)
            tempAngle -= 360;

        if (tempAngle + holderLocalAngle > 80)
            tempAngle = 80 - holderLocalAngle;
        else if (tempAngle + holderLocalAngle < 25)
            tempAngle = 25 - holderLocalAngle;

        if (tempAngle < 0)
            tempAngle += 360;
        else if (tempAngle > 180)
            tempAngle -= 360;

        return tempAngle;
    }

    public static void RotateCamera(float magnitude)
    {
        RoatateObject(MainCameraRoot, magnitude, MainCameraRotationSpeed, MainCameraSmoothing);
        // RoatateObject(MainCameraAnchor, magnitude, MainCameraRotationSpeed, MainCameraSmoothing);
    }

    private static void RoatateObject(GameObject gameObject, float magnitude, float rotationSpeed, float cameraSmoothing)
    {
        Transform objectTransform = gameObject.transform;
        float currentVerticalAngle, targetVerticalAngle;
        currentVerticalAngle = targetVerticalAngle = objectTransform.eulerAngles.y;
        targetVerticalAngle += magnitude * rotationSpeed;
        var targetEurlerAngle =
            Mathf.Lerp(
                a: currentVerticalAngle,
                b: targetVerticalAngle,
                t: Time.deltaTime * cameraSmoothing);
        objectTransform.rotation = Quaternion.AngleAxis(targetEurlerAngle, objectTransform.up);
    }

    public static void ZoomCamera(Vector2 currentTouch0, Vector2 currentTouch1)
    {
        var touch0Delta = currentTouch0 - ZeroUIHandler._startTouch0;
        var touch1Delta = currentTouch1 - ZeroUIHandler._startTouch1;

        var maxDeltaMagnitude = Math.Abs(Math.Max(touch0Delta.magnitude, touch1Delta.magnitude));
        if (maxDeltaMagnitude < 0)
            return;
        var delta0VerticalAngle = Vector2.Angle(touch0Delta, Vector2.up);
        var delta1VerticalAngle = Vector2.Angle(touch1Delta, Vector2.up);
        if (!AreBothGesturesVertical(delta0VerticalAngle, delta1VerticalAngle))
        {
            if (IsTouchPinchingOut(currentTouch0, currentTouch1))
                ZoomCamera(maxDeltaMagnitude);
            else
                ZoomCamera(-1f * maxDeltaMagnitude);
        }
    }

    public static void ZoomCamera(float magnitude)
    {
        Vector3 cameraDirection =

            MainCameraAnchor
            .transform
            .InverseTransformDirection(
                MainCameraHolder
                .transform
                .forward);

        Vector3 currentPosition = MainCameraHolder.transform.localPosition;
        Vector3 targetPosition;

        targetPosition = currentPosition + magnitude * cameraDirection;

        float tiltAngle = Vector3.Angle(cameraDirection, Vector3.down);
        if (targetPosition.y < 5)
            magnitude = (currentPosition.y - 5) / MathF.Cos(tiltAngle * MathF.PI / 180F);
        else if (targetPosition.y > 1000)
            magnitude = (1000 - currentPosition.y) / MathF.Cos(tiltAngle * MathF.PI / 180F);

        targetPosition = currentPosition + magnitude * cameraDirection;

        MainCameraHolder.transform.localPosition =
            Vector3.Lerp(
                a: currentPosition,
                b: targetPosition,
                t: Time.deltaTime * MainCameraSmoothing);
        ScaleStaticRoadObjects();
    }

    // public static void HandleTouchZoomAndTilt()
    // {
    //     if (Input.touchCount == 2)
    //     {
    //         var touch0 = Input.GetTouch(0);
    //         var touch1 = Input.GetTouch(1);

    //         var maxDeltaMagnitude =
    //             Math.Abs(
    //                 Math.Max(
    //                     touch0.deltaPosition.magnitude,
    //                     touch1.deltaPosition.magnitude));

    //         if (maxDeltaMagnitude <= 0)
    //             return;

    //         if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended)
    //         {
    //             _IsZoomInProgress = false;
    //             _IsTiltInProgress = false;
    //         }
    //         var touch0DeltaPosition = touch0.deltaPosition;
    //         var touch1DeltaPosition = touch1.deltaPosition;
    //         var delta0VerticalAngle = Vector2.Angle(touch0DeltaPosition, Vector2.up);
    //         var delta1VerticalAngle = Vector2.Angle(touch1DeltaPosition, Vector2.up);

    //         if (!_IsZoomInProgress
    //             && AreBothGesturesVertical(delta0VerticalAngle, delta1VerticalAngle))
    //         {
    //             //Lock the current movement for tilt only
    //             if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
    //                 _IsTiltInProgress = true;
    //             //Vertical tilt gesture
    //             if (delta0VerticalAngle > 90 || delta1VerticalAngle > 90)
    //                 CameraMovement.TiltCamera(
    //                     isTiltup: false,
    //                     magnitude: maxDeltaMagnitude / 100 * MainCameraTiltSpeedTouch);
    //             else
    //                 CameraMovement.TiltCamera(
    //                     isTiltup: true,
    //                     magnitude: maxDeltaMagnitude / 100 * MainCameraTiltSpeedTouch);
    //         }
    //         else if (!_IsTiltInProgress)
    //         {

    //             if (CameraMovement.IsTouchPinchingOut(touch0, touch1))
    //                 //If Zoom-in, inverse the direction
    //                 CameraMovement.ZoomCamera(
    //                     isZoomIn: true,
    //                     magnitude: maxDeltaMagnitude / 10 * MainCameraZoomSpeedTouch);
    //             else
    //                 CameraMovement.ZoomCamera(
    //                     isZoomIn: false,
    //                     magnitude: maxDeltaMagnitude / 10 * MainCameraZoomSpeedTouch);
    //         }
    //     }
    // }

    public static void HandleMouseZoom()
    {
        Vector3 cameraDirection =

            MainCameraRoot
            .transform
            .InverseTransformDirection(

                MainCameraHolder
                .transform
                .forward);

        float _input = Input.GetAxisRaw("Mouse ScrollWheel");
        var currentPosition = MainCameraHolder.transform.localPosition;
        Vector3 targetPosition =
            currentPosition
            + (cameraDirection
                * _input * MainCameraZoomSpeed);
        // if (IsInBounds(nextTargetPosition)) _targetPosition = nextTargetPosition;
        MainCameraHolder.transform.localPosition
            = Vector3.Lerp(
                MainCameraHolder.transform.localPosition,
                targetPosition,
                Time.deltaTime * MainCameraSmoothing);
    }

    public static bool AreBothGesturesVertical(float delta0VerticalAngle, float delta1VerticalAngle)
    {
        return (delta0VerticalAngle < MainCameraTiltAngleThreshold
                && delta1VerticalAngle < MainCameraTiltAngleThreshold)
            || (delta0VerticalAngle > (180 - MainCameraTiltAngleThreshold)
                && delta1VerticalAngle > (180 - MainCameraTiltAngleThreshold));
    }

    public static bool IsTouchPinchingOut(Vector2 touch0, Vector2 touch1)
    {
        return (ZeroUIHandler._startTouch0 - ZeroUIHandler._startTouch1).magnitude < (touch0 - touch1).magnitude;
    }

    public static Vector3 GetTerrainHitPoint(Vector2 origin)
    {
        Vector3 groundPosition = Vector3.zero;
        if (
            Physics.Raycast(ray: MainCamera.ScreenPointToRay(origin),
                hitInfo: out RaycastHit _rayHit,
                maxDistance: MainCamera.farClipPlane,
                layerMask: LayerMask.GetMask("Ground")))
        {
            groundPosition = _rayHit.point;

        }
        return groundPosition;
    }
    public static void ScaleStaticRoadObjects()
    {
        ScaleStaticObject(ZeroRoadBuilder.StartObject);
        ScaleStaticObject(ZeroRoadBuilder.ControlObject);
        ScaleStaticObject(ZeroRoadBuilder.EndObject);
    }

    public static void ScaleStaticObject(GameObject gameObject)
    {
        float magnitude = MainCameraHolder.transform.localPosition.y / _cameraInitialHeight;

        if (ZeroRoadBuilder.InitialStaticLocalScale.ContainsKey(gameObject.name))
        {
            Vector3 initialScale = ZeroRoadBuilder.InitialStaticLocalScale[gameObject.name];
            gameObject.transform.localScale =
            new Vector3(
               initialScale.x * magnitude,
               initialScale.y,
               initialScale.z * magnitude);
        }
    }
}

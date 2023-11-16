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
    private Vector3 _cameraDirection;

    private Vector3 _targetPosition;

    private float _targetVerticalAngle;
    private float _currentVerticalAngle;

    private float _targetHorizontalAngle;
    private float _currentHorizontalAngle;
    private bool isZoomInProgress = false;
    private bool isRotateInProgress = false;
    private bool isTiltInProgress = false;
    private Touch _startTouch0;
    private Touch _startTouch1;

    [SerializeField] private Vector2 _range = new(100, 100);

    void Start()
    {

    }
    private void Awake()
    {
        _cameraDirection = CommonController.MainCameraRoot.transform.InverseTransformDirection(CommonController.MainCameraHolder.transform.forward);

        _targetPosition = CommonController.MainCameraHolder.transform.localPosition;
        _targetVerticalAngle = CommonController.MainCameraHolder.transform.eulerAngles.y;
        _currentVerticalAngle = _targetVerticalAngle;
        _targetHorizontalAngle = CommonController.MainCameraHolder.transform.eulerAngles.x;
        _currentHorizontalAngle = _targetHorizontalAngle;
        _startTouch0 = new Touch
        {
            deltaTime = -1000
        };
        _startTouch1 = new Touch
        {
            deltaTime = -1000
        };

    }

    private void Update()
    {

        if (!CommonController.IsRoadMenuActive && CommonController.IsTouchOverNonUI(suppressTouchEndEvent: false))
        {
            HandleMouseRotation();
            HandleTouchRotation();
            HandleMouseZoom();
        }

    }
    private void HandleTouchRotation()
    {
        if (Input.touchCount == 2)
        {
            var touch0 = Input.GetTouch(0);
            var touch1 = Input.GetTouch(1);
            Debug.Log("_startTouch0.deltaTime="+ _startTouch0.deltaTime);
            Debug.Log("_startTouch1.deltaTime=" + _startTouch1.deltaTime);
            if (_startTouch0.deltaTime == -1000)
            {
                _startTouch0 = touch0;

            }
            if (_startTouch1.deltaTime == -1000)
            {
                _startTouch1 = touch1;

            }
            var maxDeltaMagnitude = Math.Max(touch0.deltaPosition.magnitude, touch1.deltaPosition.magnitude);

            if (maxDeltaMagnitude <= 0)
                return;
          
            if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended)
            {
                // Debug.Log("Line Drawn");
                // _customDebugger.RenderLine("Touch0", Color.red, lineWidth: 10f, pointSize: 20f, _startTouch0.position, touch0.position);
                // _customDebugger.RenderLine("Touch1", Color.yellow, lineWidth: 10f, pointSize: 20f, _startTouch1.position, touch1.position);

                // Debug.Log("Touch ended");
                isZoomInProgress = false;
                isRotateInProgress = false;
                isTiltInProgress = false;
                _startTouch0 = new Touch
                {
                    deltaTime = -1000
                };
                _startTouch1 = new Touch
                {
                    deltaTime = -1000
                };

            }
            // var touch0DeltaPosition = touch0.position - _startTouch0.position;
            // var touch1DeltaPosition = touch0.position - _startTouch0.position;
            var touch0DeltaPosition = touch0.deltaPosition;
            var touch1DeltaPosition = touch1.deltaPosition;
            var delta0VerticalAngle = Vector2.Angle(touch0DeltaPosition, Vector2.up);
            var delta1VerticalAngle = Vector2.Angle(touch1DeltaPosition, Vector2.up);
            var delta0HorizontalAngle = Vector2.Angle(touch0DeltaPosition, Vector2.right);
            var delta1HorizontalAngle = Vector2.Angle(touch1DeltaPosition, Vector2.right);
            var twoDeltaAngle = Vector2.Angle(touch1DeltaPosition, touch0DeltaPosition);


            var pinchParallelDistance =
                        ParallelDistance(touch0, touch1, _startTouch0, _startTouch1);

            if (!isRotateInProgress && !isZoomInProgress
                && AreBothGesturesVertical(delta0VerticalAngle, delta1VerticalAngle)
                && pinchParallelDistance > CommonController.MainCameraPinchDistanceThreshold)
            {
                //Lock the current movement for rotate only
                if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
                    isTiltInProgress = true;
                //Vertical tilt gesture
                if (delta0VerticalAngle > 90 || delta1VerticalAngle > 90)
                    _targetHorizontalAngle += Math.Abs(maxDeltaMagnitude) / 100 * CommonController.MainCameraTiltSpeed;
                else
                    _targetHorizontalAngle -= Math.Abs(maxDeltaMagnitude) / 100 * CommonController.MainCameraTiltSpeed;

                if (_targetHorizontalAngle > 90)
                    _targetHorizontalAngle = 90;
                else if (_targetHorizontalAngle < 0)
                    _targetHorizontalAngle = 0;

                // _currentHorizontalAngle = Mathf.Lerp(_currentHorizontalAngle, _targetHorizontalAngle, Time.deltaTime * CommonController.MainCameraSmoothing);
                // _cameraHolder.transform.rotation = Quaternion.AngleAxis(_currentHorizontalAngle, Vector3.right);
                CommonController.MainCameraHolder.transform.eulerAngles =
                    new Vector3(_targetHorizontalAngle,
                        CommonController.MainCameraHolder.transform.eulerAngles.y,
                        CommonController.MainCameraHolder.transform.eulerAngles.z
                        );
            }
            else if (!isTiltInProgress && !isZoomInProgress
                   && IsOneGestureHorizontal(delta0HorizontalAngle)
                   && pinchParallelDistance > CommonController.MainCameraPinchDistanceThreshold)
            {
                //Lock the current movement for rotate only
                if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
                    isRotateInProgress = true;

                //Horizontal rotate gesture
                if ((touch0.position.y > touch1.position.y && delta0HorizontalAngle > 90)
                    || (touch0.position.y < touch1.position.y && delta1HorizontalAngle > 90))
                    _targetVerticalAngle += Math.Abs(maxDeltaMagnitude) / 100 * CommonController.MainCameraRotationSpeed;
                else
                    _targetVerticalAngle -= Math.Abs(maxDeltaMagnitude) / 100 * CommonController.MainCameraRotationSpeed;

                _currentVerticalAngle = Mathf.Lerp(_currentVerticalAngle, _targetVerticalAngle, Time.deltaTime * CommonController.MainCameraSmoothing);
                transform.rotation = Quaternion.AngleAxis(_currentVerticalAngle, Vector3.up);
            }
            else if (!isTiltInProgress && !isZoomInProgress
                    && IsOneGestureVertical(delta0VerticalAngle)
                    && pinchParallelDistance > CommonController.MainCameraPinchDistanceThreshold)
            {
                //Lock the current movement for rotate only
                if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
                    isRotateInProgress = true;

                //Vertical rotate gesture
                if ((touch0.position.x > touch1.position.x && delta0VerticalAngle > 90)
                    || (touch0.position.x < touch1.position.x && delta1VerticalAngle > 90))
                    _targetVerticalAngle -= Math.Abs(maxDeltaMagnitude) / 100 * CommonController.MainCameraRotationSpeed;
                else
                    _targetVerticalAngle += Math.Abs(maxDeltaMagnitude) / 100 * CommonController.MainCameraRotationSpeed;

                _currentVerticalAngle = Mathf.Lerp(_currentVerticalAngle, _targetVerticalAngle, Time.deltaTime * CommonController.MainCameraSmoothing);
                transform.rotation = Quaternion.AngleAxis(_currentVerticalAngle, Vector3.up);
            }
            else if (!isTiltInProgress && !isRotateInProgress
                    && (twoDeltaAngle < CommonController.MainCameraRotateAngleThreshold || twoDeltaAngle > 180 - CommonController.MainCameraRotateAngleThreshold)
                    && pinchParallelDistance < CommonController.MainCameraPinchDistanceThreshold)
            {

                //Lock the current movement for zoom only
                if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
                    isZoomInProgress = true;
                //Zoom gesture

                Vector3 nextTargetPosition;

                if (IsTouchPinchingOut(touch0, touch1, _startTouch0, _startTouch1))
                    //If Zoom-in, inverse the direction
                    nextTargetPosition = _targetPosition + _cameraDirection * (maxDeltaMagnitude / 10 * CommonController.MainCameraZoomSpeed);
                else
                    nextTargetPosition = _targetPosition - _cameraDirection * (maxDeltaMagnitude / 10 * CommonController.MainCameraZoomSpeed);

                // if (IsInBounds(nextTargetPosition)) 
                _targetPosition = nextTargetPosition;
                CommonController.MainCameraHolder.transform.localPosition = Vector3.Lerp(CommonController.MainCameraHolder.transform.localPosition, nextTargetPosition, Time.deltaTime * CommonController.MainCameraSmoothing);
            }
        }
    }
    private bool AreBothGesturesVertical(float delta0VerticalAngle, float delta1VerticalAngle)
    {
        return (delta0VerticalAngle < CommonController.MainCameraRotateAngleThreshold && delta1VerticalAngle < CommonController.MainCameraRotateAngleThreshold)
                || delta0VerticalAngle > (180 - CommonController.MainCameraRotateAngleThreshold) && delta1VerticalAngle > (180 - CommonController.MainCameraRotateAngleThreshold);
    }
    private bool IsOneGestureVertical(float delta0VerticalAngle)
    {
        return delta0VerticalAngle < CommonController.MainCameraRotateAngleThreshold || delta0VerticalAngle > (180 - CommonController.MainCameraRotateAngleThreshold);
    }
    private bool IsOneGestureHorizontal(float delta0HorizontalAngle)
    {
        return delta0HorizontalAngle < CommonController.MainCameraRotateAngleThreshold || delta0HorizontalAngle > (180 - CommonController.MainCameraRotateAngleThreshold);
    }
    private double ParallelDistance(params Touch[] touches)
    {
        if (touches.Length < 4)
            throw new Exception("Invalid number of arguments for ParallelDistance");
        Vector2 touch0, startTouch0, touch1, startTouch1;
        Vector2[] inputVectors = new Vector2[4];
        for (var index = 0; index < touches.Length; index += 1)
        {
            var touchPosition = touches[index].position;
            inputVectors[index] = new Vector2(touchPosition.x, touchPosition.y);
        }
        touch0 = inputVectors[0];
        touch1 = inputVectors[1];
        startTouch0 = inputVectors[2];
        startTouch1 = inputVectors[3];

        var vectorStartPoint1to0 = startTouch0 - startTouch1;
        var vectorStartPoint0to1 = -vectorStartPoint1to0;
        var vectorCurrentPoints1to0 = touch0 - touch1;
        var vectorCurrentPoints0to1 = -vectorCurrentPoints1to0;
        var vectorDelta0 = touch0 - startTouch0;
        var vectorDelta0Reversed = -vectorDelta0;
        var vectorDelta1 = touch1 - startTouch1;
        var vectorDelta1Reversed = -vectorDelta1;
        if (vectorStartPoint0to1.magnitude < vectorCurrentPoints0to1.magnitude)
        {
            var distanceFromProjectionOn0 =
                Math.Abs(
                    vectorCurrentPoints0to1.magnitude
                    * Math.Sin(
                        Math.PI / 180 * Vector2.Angle(vectorCurrentPoints0to1,
                            vectorDelta0)));

            var distanceFromProjectionOn1 =
                Math.Abs(
                    vectorCurrentPoints0to1.magnitude
                    * Math.Sin(
                        Math.PI / 180 * Vector2.Angle(vectorCurrentPoints1to0,
                        vectorDelta1)));

            return Math.Min(distanceFromProjectionOn1, distanceFromProjectionOn0);
        }
        else
        {
            var distanceFromProjectionOn0 =
                Math.Abs(
                    vectorCurrentPoints0to1.magnitude
                    * Math.Sin(
                    Math.PI / 180 * Vector2.Angle(vectorStartPoint0to1,
                            vectorDelta0Reversed)));

            var distanceFromProjectionOn1 =
                Math.Abs(
                    vectorCurrentPoints0to1.magnitude
                    * Math.Sin(
                        Math.PI / 180 * Vector2.Angle(vectorStartPoint1to0,
                            vectorDelta1Reversed)));

            return Math.Min(distanceFromProjectionOn1, distanceFromProjectionOn0);
        }

    }
    public bool IsTouchPinchingOut(Touch touch0, Touch touch1, Touch startTouch0, Touch startTouch1)
    {
        return (startTouch0.position - startTouch1.position).magnitude < (touch0.position - touch1.position).magnitude;
    }
    public float ParallelDistance(Transform a, Transform b)
    {
        var positionOfBInAsSpace = a.TransformPoint(b.position);
        return Mathf.Abs(positionOfBInAsSpace.x);
    }

    private void HandleMouseRotation()
    {
        if (Input.GetMouseButton(1))
        {
            //Support for mouse rotation
            _targetVerticalAngle += Input.GetAxisRaw("Mouse X") * CommonController.MainCameraRotationSpeed;

            _currentVerticalAngle = Mathf.Lerp(_currentVerticalAngle, _targetVerticalAngle, Time.deltaTime * CommonController.MainCameraSmoothing);
            transform.rotation = Quaternion.AngleAxis(_currentVerticalAngle, Vector3.up);
        }

    }
    private void HandleMouseZoom()
    {
        float _input = Input.GetAxisRaw("Mouse ScrollWheel");
        Vector3 nextTargetPosition = _targetPosition + _cameraDirection * (_input * CommonController.MainCameraZoomSpeed);
        // if (IsInBounds(nextTargetPosition)) _targetPosition = nextTargetPosition;
        _targetPosition = nextTargetPosition;
        CommonController.MainCameraHolder.transform.localPosition = Vector3.Lerp(CommonController.MainCameraHolder.transform.localPosition, _targetPosition, Time.deltaTime * CommonController.MainCameraSmoothing);
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

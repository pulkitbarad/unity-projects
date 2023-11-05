using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotationAndZoom : MonoBehaviour
{
    [SerializeField] private float _zoomSpeed = 2f;
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private float _smoothing = 2f;
    [SerializeField] private float _pinchDistanceThreshold = 50f;
    [SerializeField] private float _rotateAngleThreshold = 50f;
    [SerializeField] private Transform _cameraHolder;
    private Vector3 _cameraDirection => transform.InverseTransformDirection(_cameraHolder.forward);

    private Vector3 _targetPosition;

    private float _targetVerticalAngle;
    private float _currentVerticalAngle;

    private float _targetHorizontalAngle;
    private float _currentHorizontalAngle;

    [SerializeField] private Vector2 _range = new(100, 100);

    private void Awake()
    {
        _targetPosition = _cameraHolder.localPosition;
        _targetVerticalAngle = transform.eulerAngles.y;
        _currentVerticalAngle = _targetVerticalAngle;
        _targetHorizontalAngle = transform.eulerAngles.x;
        _currentHorizontalAngle = _targetVerticalAngle;
    }

    private void HandleTouchRotation()
    {
        if (Input.touchCount == 2)
        {
            var touch0 = Input.GetTouch(0);
            var touch1 = Input.GetTouch(1);
            var delta0VerticalAngle = Vector2.Angle(touch0.deltaPosition, Vector2.up);
            var delta1VerticalAngle = Vector2.Angle(touch1.deltaPosition, Vector2.up);
            var delta0HorizontalAngle = Vector2.Angle(touch0.deltaPosition, Vector2.right);
            var delta1HorizontalAngle = Vector2.Angle(touch1.deltaPosition, Vector2.right);
            var twoDeltaAngle = Vector2.Angle(touch1.deltaPosition, touch0.deltaPosition);
            var maxDeltaMagnitude = Math.Max(touch0.deltaPosition.magnitude, touch0.deltaPosition.magnitude);
            var touchHorizontalStartDistance = touch0.position.x - touch1.position.x;
            var touchVerticalStartDistance = touch0.position.y - touch1.position.y;

            if ((delta0VerticalAngle < _rotateAngleThreshold / 2
                    || delta0VerticalAngle > (180 - _rotateAngleThreshold / 2))
                    && twoDeltaAngle < _rotateAngleThreshold / 2)
            {
                //Tilt
                if (delta0VerticalAngle > 90 || delta1VerticalAngle > 90)
                    _targetHorizontalAngle -= Math.Abs(maxDeltaMagnitude) / 100 * _rotationSpeed;
                else
                    _targetHorizontalAngle += Math.Abs(maxDeltaMagnitude) / 100 * _rotationSpeed;

                _targetHorizontalAngle = Math.Abs(_targetHorizontalAngle % 90);
                Debug.Log("_targetHorizontalAngle=" + _targetHorizontalAngle);

                _currentHorizontalAngle = Mathf.Lerp(_currentHorizontalAngle, _targetHorizontalAngle, Time.deltaTime * _smoothing);
                transform.rotation = Quaternion.AngleAxis(_currentHorizontalAngle, Vector3.right);
            }
            else if ((delta0HorizontalAngle < _rotateAngleThreshold / 2
                    || delta0HorizontalAngle > (180 - _rotateAngleThreshold / 2))
                    && Math.Abs(touchVerticalStartDistance) > _pinchDistanceThreshold)
            {
                //Rotate
                if ((touchVerticalStartDistance > 0 && delta0HorizontalAngle > 90)
                    || (touchVerticalStartDistance < 0 && delta1HorizontalAngle > 90))
                    _targetVerticalAngle -= Math.Abs(maxDeltaMagnitude) / 100 * _rotationSpeed;
                else
                    _targetVerticalAngle += Math.Abs(maxDeltaMagnitude) / 100 * _rotationSpeed;

                _currentVerticalAngle = Mathf.Lerp(_currentVerticalAngle, _targetVerticalAngle, Time.deltaTime * _smoothing);
                transform.rotation = Quaternion.AngleAxis(_currentVerticalAngle, Vector3.up);
            }
            else
            {
                //Zoom

                // V(A0) = V(delta A) - V(A1)
                // V(B0) = V(delta B) - V(B1)
                // if |V(A0)-V(B0)| < |V(A1)-V(B1)|, it means current position drifted away form the previous positions.
                // Therefore it's a zoom-out event.
                var va0 = touch0.deltaPosition - touch0.position;
                var vb0 = touch1.deltaPosition - touch1.position;
                var va1 = touch0.position;
                var vb1 = touch1.position;

                Vector3 nextTargetPosition;

                if ((vb0 - va0).magnitude > (vb1 - va1).magnitude)
                    //If Zoom-in, inverse the direction
                    nextTargetPosition = _targetPosition - _cameraDirection * (maxDeltaMagnitude / 10 * _zoomSpeed);
                else
                    nextTargetPosition = _targetPosition + _cameraDirection * (maxDeltaMagnitude / 10 * _zoomSpeed);

                // if (IsInBounds(nextTargetPosition)) 
                _targetPosition = nextTargetPosition;
                _cameraHolder.localPosition = Vector3.Lerp(_cameraHolder.localPosition, nextTargetPosition, Time.deltaTime * _smoothing);
            }
        }
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
            _targetVerticalAngle += Input.GetAxisRaw("Mouse X") * _rotationSpeed;

            _currentVerticalAngle = Mathf.Lerp(_currentVerticalAngle, _targetVerticalAngle, Time.deltaTime * _smoothing);
            transform.rotation = Quaternion.AngleAxis(_currentVerticalAngle, Vector3.up);
        }

    }
    private void HandleMouseZoom()
    {
        float _input = Input.GetAxisRaw("Mouse ScrollWheel");
        Vector3 nextTargetPosition = _targetPosition + _cameraDirection * (_input * _zoomSpeed);
        // if (IsInBounds(nextTargetPosition)) _targetPosition = nextTargetPosition;
        _targetPosition = nextTargetPosition;
        _cameraHolder.localPosition = Vector3.Lerp(_cameraHolder.localPosition, _targetPosition, Time.deltaTime * _smoothing);
    }

    private bool IsInBounds(Vector3 position)
    {
        return position.x > -_range.x &&
               position.x < _range.x &&
               position.z > -_range.y &&
               position.z < _range.y;
    }

    private void Update()
    {
        HandleTouchRotation();
        HandleMouseRotation();
        HandleMouseZoom();
    }
}

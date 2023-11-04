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

    private float _targetAngle;
    private float _currentAngle;

    [SerializeField] private Vector2 _range = new(100, 100);

    private void Awake()
    {
        _targetPosition = _cameraHolder.localPosition;
        _targetAngle = transform.eulerAngles.y;
        _currentAngle = _targetAngle;
    }

    private void HandleTouchRotation()
    {
        if (Input.touchCount == 2)
        {
            var touch0 = Input.GetTouch(0);
            var touch1 = Input.GetTouch(1);
            var delta0VerticalAngle = Vector2.Angle(touch0.deltaPosition, Vector2.up);
            var delta1VerticalAngle = Vector2.Angle(touch1.deltaPosition, Vector2.up);
            var maxDeltaMagnitude = Math.Max(touch0.deltaPosition.magnitude, touch0.deltaPosition.magnitude);
            var touchStartDistance = touch0.position.x - touch1.position.x;

            if ((delta0VerticalAngle < _rotateAngleThreshold / 2
                    || delta0VerticalAngle > (180 - _rotateAngleThreshold / 2))
                    && Math.Abs(touchStartDistance) > _pinchDistanceThreshold)
            {
                //Rotate
                if ((touchStartDistance > 0 && delta0VerticalAngle > 90)
                    || (touchStartDistance < 0 && delta1VerticalAngle > 90))
                    _targetAngle -= Math.Abs(maxDeltaMagnitude) / 100 * _rotationSpeed;
                else
                    _targetAngle += Math.Abs(maxDeltaMagnitude) / 100 * _rotationSpeed;

                _currentAngle = Mathf.Lerp(_currentAngle, _targetAngle, Time.deltaTime * _smoothing);
                transform.rotation = Quaternion.AngleAxis(_currentAngle, Vector3.up);
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
            _targetAngle += Input.GetAxisRaw("Mouse X") * _rotationSpeed;

            _currentAngle = Mathf.Lerp(_currentAngle, _targetAngle, Time.deltaTime * _smoothing);
            transform.rotation = Quaternion.AngleAxis(_currentAngle, Vector3.up);
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

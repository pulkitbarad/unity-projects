using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotion : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private float _smoothing = 2f;
    [SerializeField] private Vector2 _range = new(100, 100);

    private Vector3 _targetPosition;

    private void Awake()
    {
        _targetPosition = transform.position;
    }
    void Start()
    {

    }
    private void Update()
    {
        if (!CommonController.IsSingleTouchLocked)
        {

            CommonController.IsSingleTouchLocked = true;
            HandleMove();
            CommonController.IsSingleTouchLocked = false;
        }
    }

    private void HandleMove()
    {

        if (Input.touchCount == 1)
        {
            var touch = Input.GetTouch(0);

            Vector3 right = -transform.right * touch.deltaPosition.x;
            Vector3 forward = -transform.forward * touch.deltaPosition.y;
            var input = (forward + right);
            Vector3 nextTargetPosition = _targetPosition + input / 10 * _moveSpeed;
            // if (IsInBounds(nextTargetPosition))
            _targetPosition = nextTargetPosition;
            transform.position = Vector3.Lerp(transform.position, nextTargetPosition, Time.deltaTime * 100 * _smoothing);
            // transform.position = nextTargetPosition;

        }
        else
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            Vector3 right = transform.right * x;
            Vector3 forward = transform.forward * z;
            var input = (forward + right).normalized;

            Vector3 nextTargetPosition = _targetPosition + input * _moveSpeed;
            // if (IsInBounds(nextTargetPosition)) 
            _targetPosition = nextTargetPosition;
            transform.position = Vector3.Lerp(transform.position, nextTargetPosition, Time.deltaTime * 1);
        }
    }


    private bool IsInBounds(Vector3 position)
    {
        return position.x > -_range.x &&
               position.x < _range.x &&
               position.z > -_range.y &&
               position.z < _range.y;
    }

}

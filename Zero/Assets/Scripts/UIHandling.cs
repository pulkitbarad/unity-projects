using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHandling : MonoBehaviour
{

    private bool _waitForRoadBuildStart = false;

    [SerializeField] private Button _buttonRoad;

    [SerializeField] private Camera _mainCamera;
    [SerializeField] private GameObject _mainCameraHolder;

    void Awake()
    {
        _mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        _mainCameraHolder = GameObject.Find("CameraHolder");

    }
    void Start()
    {
        Button btn = _buttonRoad.GetComponent<Button>();
        btn.onClick.AddListener(OnClickButtonRoad);
    }

    void Update()
    {
        if (!CommonController.IsSingleTouchLocked && !CommonController.IsMultiTouchLocked)
        {

            CommonController.IsSingleTouchLocked = true;
            CommonController.IsMultiTouchLocked = true;
            HandleRoadBuilding();
            CommonController.IsSingleTouchLocked = false;
            CommonController.IsMultiTouchLocked = false;
        }
    }

    void OnClickButtonRoad()
    {
        if (!_waitForRoadBuildStart)
            _waitForRoadBuildStart = true;
        Debug.Log("Tap on screen to build road");
    }
    void HandleRoadBuilding()
    {

        if (_waitForRoadBuildStart && Input.touchCount >= 1)
        {
            var touch0 = Input.GetTouch(0).position;
            Debug.Log("Input.GetTouch(0).type=" + Input.GetTouch(0).type);
            Vector2 touch1 = new Vector2(0, 0);
            if (Input.touchCount == 2)
                touch1 = Input.GetTouch(1).position;
            else
            {
                if (touch0.x > Screen.width * 1.20f)
                    touch1.x = touch0.x - Screen.width * 0.20f;
                else
                    touch1.x = touch0.x + Screen.width * 0.20f;
                if (touch0.y > Screen.width * 1.20f)
                    touch1.y = touch0.y - Screen.width * 0.20f;
                else
                    touch1.y = touch0.y + Screen.width * 0.20f;

            }
            var touch0PointWorld = _mainCamera.ScreenToWorldPoint(touch0);
            var touch1PointWorld = _mainCamera.ScreenToWorldPoint(touch1);
            Physics.Raycast(touch0PointWorld, _mainCameraHolder.transform.forward, out RaycastHit startPointHit);
            Physics.Raycast(touch0PointWorld, _mainCameraHolder.transform.forward, out RaycastHit endPointHit);
            var startPointWorld = startPointHit.point;
            var endPointWorld = endPointHit.point;
            CommonController.RenderLine("startRay", Color.yellow, touch0PointWorld, startPointWorld);
            CommonController.RenderLine("endRay", Color.red, touch1PointWorld, endPointWorld);
            Debug.Log("startPointWorld = " + startPointWorld);
            Debug.Log("endPointWorld = " + endPointWorld);
            CommonController.DrawPointSphere(startPointWorld, color: Color.blue);
            CommonController.DrawPointSphere(endPointWorld, color: Color.green);
            _waitForRoadBuildStart = false;
        }
    }
}

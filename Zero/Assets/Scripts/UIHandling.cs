using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.VisualScripting;

public class UIHandling : MonoBehaviour
{


    [SerializeField] private Button _buttonRoad;

    [SerializeField] private Camera _mainCamera;
    [SerializeField] private GameObject _mainCameraHolder;
    [SerializeField] private GameObject _mainCameraRoot;

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
        if (CommonController.IsRoadBuildEnabled && CommonController.IsTouchOverNonUI())
        {
            HandleRoadBuilding();
        }
    }

    void OnClickButtonRoad()
    {
        CommonController.IsRoadBuildEnabled = true;

        Debug.Log("Tap on screen to build road");
    }

    private void HandleRoadBuilding()
    {
        var touch0 = Input.GetTouch(0);
        var touch0Position = touch0.position;
        Vector2 touch1Position = new Vector2(0, 0);
        if (Input.touchCount == 2)
            touch1Position = Input.GetTouch(1).position;
        else
        {
            if (touch0Position.x > Screen.width * 1.1f)
                touch1Position.x = touch0Position.x - Screen.width * 0.1f;
            else
                touch1Position.x = touch0Position.x + Screen.width * 0.1f;
            if (touch0Position.y > Screen.width * 1.1f)
                touch1Position.y = touch0Position.y - Screen.width * 0.1f;
            else
                touch1Position.y = touch0Position.y + Screen.width * 0.1f;

        }
        // CommonController.DrawPointSphere(new Vector3(20, 5, -434), color: Color.green, size: 10f);
        // CommonController.DrawPointSphere(new Vector3(100, 5, -434), color: Color.blue, size: 10f);
        Debug.Log("touch0Position=" + touch0Position);
        Debug.Log("touch1Position=" + touch1Position);
        GetTerrainHitPoint(touch0Position, color: Color.green, size: 20f);
        GetTerrainHitPoint(touch1Position, color: Color.blue, size: 20f);

        CommonController.IsRoadBuildEnabled = false;
    }

    private void GetTerrainHitPoint(Vector2 origin, Color color, float size)
    {
        var didRayHit = Physics.Raycast(_mainCamera.ScreenPointToRay(origin),
                                        out RaycastHit _rayHit,
                                        _mainCamera.farClipPlane,
                                        LayerMask.GetMask("Ground"));

        if (didRayHit)
        {
            var pointNewLocation = _rayHit.point;
            pointNewLocation.x += size / 2;
            pointNewLocation.y += size / 2;
            pointNewLocation.z += size / 2;
            CommonController.DrawPointSphere(pointNewLocation, color: color, size: size);
        }

    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHandling : MonoBehaviour
{


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
        // //TO-DO: Think about race condition between multiple touch handlers.
        // if (Input.touchCount > 1)
        //     CommonController.IsMultiTouchConsumed = false;
        // else if (Input.touchCount == 1)
        //     CommonController.IsSingleTouchConsumed = false;

        HandleRoadBuilding();
    }

    void OnClickButtonRoad()
    {
        // if (Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            CommonController.IsRoadBuildEnabled = true;

        }
        Debug.Log("Tap on screen to build road");
        Debug.Log("1 [phase]=" + Input.GetTouch(0).phase);
    }
    private void HandleRoadBuilding()
    {
        if (CommonController.IsRoadBuildEnabled)
        {
            if (Input.touchCount >= 1)
            {

                var touch0 = Input.GetTouch(0);
                Debug.Log("3 touch0.phase=" + touch0.phase);
                if (touch0.phase == TouchPhase.Began)
                {
                    var touch0Position = touch0.position;
                    Vector2 touch1Position = new Vector2(0, 0);
                    if (Input.touchCount == 2)
                        touch1Position = Input.GetTouch(1).position;
                    else
                    {
                        if (touch0Position.x > Screen.width * 1.20f)
                            touch1Position.x = touch0Position.x - Screen.width * 0.20f;
                        else
                            touch1Position.x = touch0Position.x + Screen.width * 0.20f;
                        if (touch0Position.y > Screen.width * 1.20f)
                            touch1Position.y = touch0Position.y - Screen.width * 0.20f;
                        else
                            touch1Position.y = touch0Position.y + Screen.width * 0.20f;

                    }
                    DrawHitPoint(touch0Position, Color.blue, 20f);
                    DrawHitPoint(touch1Position, Color.green, 20f);
                }
                else if (touch0.phase == TouchPhase.Ended)
                {
                    CommonController.IsRoadBuildEnabled = false;
                }

            }
        }
    }

    private void DrawHitPoint(Vector2 touchPosition, Color color, float size)
    {
        var touchPositionWorld = _mainCamera.ScreenToWorldPoint(touchPosition);
        Physics.Raycast(touchPositionWorld, _mainCameraHolder.transform.forward, out RaycastHit touchHit);
        var touchHitObjectName = touchHit.transform.gameObject.name;
        var correctedColor = color;
        if (String.IsNullOrEmpty(touchHitObjectName)
         && String.Equals(touchHitObjectName, "Terrain", System.StringComparison.OrdinalIgnoreCase))
        {
            correctedColor = Color.red;
        }
        var pointNewLocation = touchHit.transform.position;
        pointNewLocation.x -= size / 2;
        pointNewLocation.y -= size / 2;
        pointNewLocation.z -= size / 2;
        CommonController.DrawPointSphere(pointNewLocation, color: color, size: size);
        // CommonController.RenderLine("endRay", Color.red, touch1PointWorld, endPointWorld);
    }
}

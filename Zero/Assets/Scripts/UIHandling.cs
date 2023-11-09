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
            if (touch0Position.x > Screen.width * 1.20f)
                touch1Position.x = touch0Position.x - Screen.width * 0.20f;
            else
                touch1Position.x = touch0Position.x + Screen.width * 0.20f;
            if (touch0Position.y > Screen.width * 1.20f)
                touch1Position.y = touch0Position.y - Screen.width * 0.20f;
            else
                touch1Position.y = touch0Position.y + Screen.width * 0.20f;

        }
        CommonController.DrawPointSphere(new Vector3(20, 5, -434), color: Color.green, size: 10f);
        CommonController.DrawPointSphere(new Vector3(100, 5, -434), color: Color.blue, size: 10f);
        CommonController.IsRoadBuildEnabled = false;
        // else if (touch0.phase == TouchPhase.Ended)
        // {
        //     CommonController.IsRoadBuildEnabled = false;
        // }

    }

    private void DrawHitPoint(Vector2 touchPosition, Color color, float size)
    {
        var touchPositionWorld = _mainCamera.ScreenToWorldPoint(touchPosition);
        var sphereCollisionCasts = Physics.SphereCastAll(touchPositionWorld, size / 2, _mainCameraHolder.transform.forward);
        Debug.Log("collision count=" + sphereCollisionCasts.Length);

        foreach (var sphereCollisionCast in sphereCollisionCasts)
        {
            Debug.Log("collision=" + sphereCollisionCast.point);

            var correctedColor = color;

            var collidingObjectName = sphereCollisionCast.collider.gameObject.name;
            if (!String.IsNullOrEmpty(collidingObjectName)
            && collidingObjectName.Equals("Terrain", StringComparison.InvariantCultureIgnoreCase)
             )
            {
                Debug.Log("collidingObjectName=" + collidingObjectName);
                var pointNewLocation = sphereCollisionCast.point;
                pointNewLocation.x += size / 2;
                pointNewLocation.y += size / 2;
                pointNewLocation.z += size / 2;
                CommonController.DrawPointSphere(sphereCollisionCast.point, color: correctedColor, size: size);
            }
        }
        // CommonController.RenderLine("RayLine", Color.red, touchPosition, touchHit.point);
    }
}

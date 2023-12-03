using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using System;
using UnityEngine.SocialPlatforms;
using UnityEngine.AI;

public class CommonController : MonoBehaviour
{

    private static string _objectBeingDragged = "";
    public static Vector2 _startTouch0 = Vector2.zero;
    public static Vector2 _startTouch1 = Vector2.zero;

    public static readonly List<CustomRoad> ExistingRoads = new();
    public static CustomRoad CurrentActiveRoad;



    // Start is called before the first frame update
    void Start()
    {
        CurrentActiveRoad = new CustomRoad();
    }

    public static void ResetGameObject(GameObject gameObject)
    {
        gameObject.transform.position = Vector3.zero;
        gameObject.SetActive(false);
    }


    public static void StartOfSingleTouchDrag(Vector2 touchPosition)
    {
        _startTouch0 = touchPosition;
    }

    public static void StartOfMultiTouchDrag(Vector2 touch0Position, Vector2 touch1Position)
    {
        _startTouch0 = touch0Position;
        _startTouch1 = touch1Position;
    }

    public static void EndOfSingleTouchDrag()
    {
        _startTouch0 = Vector2.zero;
        _objectBeingDragged = "";
    }

    public static void EndOfMultiTouchDrag()
    {
        _startTouch0 = Vector2.zero;
        _startTouch1 = Vector2.zero;
        _objectBeingDragged = "";
    }

    public static bool HandleGameObjectDrag(GameObject gameObject, Vector2 touchPosition)
    {
        if (!EventSystem.current.IsPointerOverGameObject() || _objectBeingDragged.Length > 0)
        {
            Ray touchPointRay = CameraMovement.MainCamera.ScreenPointToRay(touchPosition);
            gameObject.SetActive(true);
            if (
                (_objectBeingDragged.Length > 0 && _objectBeingDragged.Equals(gameObject.name))
                || (Physics.Raycast(touchPointRay, out RaycastHit hit)
                && hit.transform == gameObject.transform))
            {
                gameObject.transform.position = CameraMovement.GetTerrainHitPoint(touchPosition);
                _objectBeingDragged = gameObject.name;
                return true;
            }
            Physics.Raycast(touchPointRay, out RaycastHit hit2);
        }
        return false;
    }

    public static Vector2 GetScreenCenterPoint()
    {
        return new Vector2(Screen.width / 2, Screen.height / 2);
    }

    public static void StartRoadConstruction(bool isCurved)
    {
        CurrentActiveRoad.StartConstruction(isCurved);
    }

    public static void ConfirmRoadConstruction()
    {
        CurrentActiveRoad.CancelConstruction();
        ExistingRoads.Add(CurrentActiveRoad);
    }

    public static void CancelRoadConstruction()
    {
        CurrentActiveRoad.CancelConstruction();
    }

    public class CustomRoad
    {
    }


    public static class CameraMovement
    {
    }

    public static class CurvedLine
    {

    }

    public static class CustomRendrer
    {
    }

}

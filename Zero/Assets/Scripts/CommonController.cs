using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using System;
using UnityEngine.SocialPlatforms;
using UnityEngine.AI;
using UnityEditor;

public class CommonController : MonoBehaviour
{

    private static string _objectBeingDragged = "";
    public static Vector2 _startTouch0 = Vector2.zero;
    public static Vector2 _startTouch1 = Vector2.zero;




    // Start is called before the first frame update
    void Start()
    {
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

    public static GameObject FindGameObject(string objectName, bool findDisabled)
    {
        if (findDisabled)
        {
            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (!EditorUtility.IsPersistent(go.transform.root.gameObject)
                     && !(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave))
                {
                    if (go.name == objectName)
                        return go;
                }
            }
            return null;
        }
        else
            return GameObject.Find(objectName);
    }
}

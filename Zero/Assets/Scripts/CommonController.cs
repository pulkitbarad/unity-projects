using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using System;
using UnityEngine.SocialPlatforms;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.PackageManager;
using System.Text;

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

    public static bool HandleGameObjectDrag(GameObject gameObject, Vector2 touchPosition, GameObject followerObject = null)
    {
        if (!EventSystem.current.IsPointerOverGameObject() || _objectBeingDragged.Length > 0)
        {
            Ray touchPointRay = CameraMovement.MainCamera.ScreenPointToRay(touchPosition);
            gameObject.SetActive(true);
            if ((_objectBeingDragged.Length > 0 && _objectBeingDragged.Equals(gameObject.name))
                || (Physics.Raycast(touchPointRay, out RaycastHit hit)
                && hit.transform == gameObject.transform))
            {
                Vector3 oldPosition = gameObject.transform.position;
                Vector3 newPosition = CameraMovement.GetTerrainHitPoint(touchPosition);
                gameObject.transform.position = newPosition;
                if (followerObject != null)
                {
                    Vector3 followerOldPosition = followerObject.transform.position;
                    Vector3 followerNewPosition = newPosition - oldPosition + followerOldPosition;

                    followerObject.transform.position = followerNewPosition;

                }
                _objectBeingDragged = gameObject.name;
                return true;
            }
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

    // public static bool AreSegmentsIntersecting(){

    // }

    public static bool AreSegmentsIntersecting(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2){

        Vector3 backward2  = start2-end2;
        Vector3 bound1 = start2 - start1;
        Vector3 bound2 = start2 - end1;

        float boundAngle = Vector3.Angle(bound1,bound2);
        return Vector3.Angle(backward2,bound1) + Vector3.Angle(backward2,bound2) == boundAngle;

    }
    
    public static string GetPositionHexCode(params Vector3[] positions)
    {
        Vector3 position = Vector3.zero;
        for (int i = 0; i < positions.Length; i++)
        {
            position += positions[i];
        }
        float coordinates = position.x  + position.y  + position.z;
        return BitConverter.ToString(Encoding.Default.GetBytes(coordinates.ToString())).Replace("-", "");
    }
    // private static string GetNumerics(string input){
    //     return new string(input.Where(c => char.IsDigit(c)).ToArray());
    // }
}

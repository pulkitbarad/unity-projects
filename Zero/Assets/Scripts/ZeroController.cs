using System.Linq;
using UnityEngine;
using System;
using UnityEditor;
using System.Text;

public class ZeroController : MonoBehaviour
{
    [SerializeField] public float MainCameraMoveSpeed;// = 0.5f;
    [SerializeField] public float MainCameraSmoothing;// = 2f;
    [SerializeField] public float MainCameraZoomSpeed;// = 50f;
    [SerializeField] public float MainCameraRotationSpeed;// = 50f;
    [SerializeField] public float MainCameraTiltSpeed;// = 50f;
    [SerializeField] public float MainCameraTiltAngleThreshold;// = 10f;
    [SerializeField] public float RoadMaxChangeInAngle;// = 15f;
    [SerializeField] public int RoadMaxVertexCount;// = 30;
    [SerializeField] public int RoadMinVertexCount;// = 6;
    [SerializeField] public float RoadSegmentMinLength;// = 3;
    [SerializeField] public float RoadLaneHeight;// = 0.25;
    [SerializeField] public float RoadLaneWidth;// = 3;
    [SerializeField] public float RoadSideWalkHeight;// = 0.3;

    [SerializeField] public GameObject MainCameraRoot;
    [SerializeField] public GameObject MainCameraAnchor;
    [SerializeField] public GameObject MainCameraHolder;
    [SerializeField] public Camera MainCamera;

    void Awake()
    {
        ZeroCameraMovement.MainCameraMoveSpeed = MainCameraMoveSpeed;
        ZeroCameraMovement.MainCameraSmoothing = MainCameraSmoothing;
        ZeroCameraMovement.MainCameraZoomSpeed = MainCameraZoomSpeed;
        ZeroCameraMovement.MainCameraRotationSpeed = MainCameraRotationSpeed;
        ZeroCameraMovement.MainCameraTiltSpeed = MainCameraTiltSpeed;
        ZeroCameraMovement.MainCameraTiltAngleThreshold = MainCameraTiltAngleThreshold;
        ZeroRoadBuilder.RoadMaxChangeInAngle = RoadMaxChangeInAngle;
        ZeroRoadBuilder.RoadMaxVertexCount = RoadMaxVertexCount;
        ZeroRoadBuilder.RoadMinVertexCount = RoadMinVertexCount;
        ZeroRoadBuilder.RoadSegmentMinLength = RoadSegmentMinLength;
        ZeroRoadBuilder.RoadLaneHeight = RoadLaneHeight;
        ZeroRoadBuilder.RoadLaneWidth = RoadLaneWidth;
        ZeroRoadBuilder.RoadSideWalkHeight = RoadSideWalkHeight;

        ZeroCameraMovement.MainCamera = MainCamera;
        ZeroCameraMovement.MainCameraHolder = MainCameraHolder;
        ZeroCameraMovement.MainCameraRoot = MainCameraRoot;
        ZeroCameraMovement.MainCameraAnchor = MainCameraAnchor;
    }


    void Start()
    {
        ZeroCameraMovement.Initialise();
        ZeroRenderer.Initialise();
        ZeroUIHandler.Initialise();
        ZeroRoadBuilder.Initialise();
    }

    void Update()
    {
        ZeroUIHandler.HandleInputChanges();
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


    // public static string GetPositionHexCode(params Vector3[] positions)
    // {
    //     Vector3 position = Vector3.zero;
    //     for (int i = 0; i < positions.Length; i++)
    //     {
    //         position += positions[i];
    //     }
    //     float coordinates = position.x + position.y + position.z;
    //     return BitConverter.ToString(Encoding.Default.GetBytes(coordinates.ToString())).Replace("-", "");
    // } 
}

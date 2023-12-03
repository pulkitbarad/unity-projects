using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomRoadBuilder : MonoBehaviour
{

    public static GameObject StaticObjectParent;
    public static GameObject StartObject;
    public static GameObject ControlObject;
    public static GameObject EndObject;
    public static GameObject CenterLineObject;
    public static GameObject LeftLineObject;
    public static GameObject RightLineObject;
    private static readonly Dictionary<string, Vector3> _initialStaticLocalScale = new();

    void Start()
    {
        StaticObjectParent = new GameObject("StaticObjects");

    }

    public static void ScaleStaticObjects(Camera mainCameraHolder, float cameraInitialHeight)
    {
        ScaleStaticObject(StartObject, mainCameraHolder, cameraInitialHeight);
        ScaleStaticObject(ControlObject, mainCameraHolder, cameraInitialHeight);
        ScaleStaticObject(EndObject, mainCameraHolder, cameraInitialHeight);
    }

    public static void ScaleStaticObject(GameObject gameObject, Camera mainCameraHolder, float cameraInitialHeight)
    {
        float magnitude = mainCameraHolder.transform.localPosition.y / cameraInitialHeight;

        if (_initialStaticLocalScale.ContainsKey(gameObject.name))
        {
            Vector3 initialScale = _initialStaticLocalScale[gameObject.name];
            gameObject.transform.localScale =
            new Vector3(
               initialScale.x * magnitude,
               initialScale.y,
               initialScale.z * magnitude);
        }
    }
    public class CustomRoad
    {
        public Vector2 StartPosition;
        public Vector2 ControlPosition;
        public Vector2 EndPosition;
        public CustomRoadLine CenterLine;
        public List<CustomRoadLine> EdgeLines = new();
        public int RoadWidth;
        public int VertexCount = 20;
        public bool IsCurved = true;
        public bool IsUnderConstruction;

        public void InitializeCustomRoad()
        {
            StartObject = InitializeStaticObject(objectName: "RoadStart", false, size: 10, color: new Color(0.25f, 0.35f, 0.30f));
            ControlObject = InitializeStaticObject(objectName: "RoadControl", false, size: 10, color: new Color(0, 1, 0.20f));
            EndObject = InitializeStaticObject(objectName: "RoadEnd", false, size: 10, color: new Color(0.70f, 0.45f, 0f));

            RebuildRoad(forceRebuild: true, activate: false, touchPosition: Vector2.zero);
        }
        public static GameObject InitializeStaticObject(string objectName, bool activate, float size, Color? color)
        {
            GameObject gameObject = GameObject.Find(objectName);
            if (gameObject == null)
            {
                gameObject = CustomRendrer.RenderCylinder(objectName: objectName, position: Vector3.zero, size: size, color: color);
                gameObject.transform.SetParent(StaticObjectParent.transform);
            }
            gameObject.SetActive(activate);
            _initialStaticLocalScale.Add(gameObject.name, gameObject.transform.localScale);
            return gameObject;
        }

        public void StartConstruction(bool isCurved)
        {
            if (!isCurved)
                VertexCount = 3;
            IsCurved = isCurved;

            InitializeControlPositions();

            //Make control objects visible            
            StartObject.SetActive(true);
            EndObject.SetActive(true);
            Debug.Log("IsCurved=" + IsCurved);
            if (IsCurved)
                ControlObject.SetActive(true);

            //Make line objects visible            
            CenterLineObject.SetActive(true);
            LeftLineObject.SetActive(true);
            RightLineObject.SetActive(true);

            IsUnderConstruction = true;
        }

        private void InitializeControlPositions()
        {
            Vector3 startObjectPosition, endObjectPosition;
            startObjectPosition = CameraMovement.GetTerrainHitPoint(CommonController.GetScreenCenterPoint());
            StartObject.transform.position = endObjectPosition = startObjectPosition;
            endObjectPosition += 200f * CameraMovement.MainCameraRoot.transform.right;
            EndObject.transform.position = endObjectPosition;
            var startToEndDirection = endObjectPosition - startObjectPosition;
            var startToEndDistance = startToEndDirection.magnitude;


            if (ControlObject.transform.position == Vector3.zero)
            {
                Vector3 midPointVector = 0.5f * startToEndDistance * startToEndDirection.normalized;
                if (IsCurved)
                    ControlObject.transform.position =
                        startObjectPosition + Quaternion.AngleAxis(45, Vector3.up) * midPointVector;
                else
                    ControlObject.transform.position =
                        startObjectPosition + midPointVector;
            }
        }

        public void CancelConstruction()
        {
            StartObject.SetActive(false);
            ControlObject.SetActive(false);
            EndObject.SetActive(false);
            CenterLineObject.SetActive(false);
            LeftLineObject.SetActive(false);
            RightLineObject.SetActive(false);
            IsUnderConstruction = false;
        }

        public void RebuildRoad(CustomRoad road, bool forceRebuild, bool activate, Vector2 touchPosition)
        {
            if (forceRebuild || IsRebuildRoadRequired(touchPosition))
            {
                CurvedLine.FindBazierLinePoints(
                    roadStartObject: StartObject,
                    roadControlObject: ControlObject,
                    roadEndObject: EndObject,
                    vertexCount: 10,
                    roadWidth: 10,
                    centerLinePoints: out List<Vector3> centerLinePoints,
                    leftLinePoints: out List<Vector3> leftLinePoints,
                    rightLinePoints: out List<Vector3> rightLinePoints);
                    
                RenderRoadLines(activate);
            }
        }

        private bool IsRebuildRoadRequired(Vector2 touchPosition)
        {

            if (!EventSystem.current.IsPointerOverGameObject())
            {
                var roadStartChanged =
                    !StartObject.transform.position.Equals(Vector3.zero)
                    && CommonController.HandleGameObjectDrag(StartObject, touchPosition);

                var roadControlChanged =
                    IsCurved
                    && !ControlObject.transform.position.Equals(Vector3.zero)
                    && CommonController.HandleGameObjectDrag(ControlObject, touchPosition);

                var roadEndChanged =
                    !EndObject.transform.position.Equals(Vector3.zero)
                    && CommonController.HandleGameObjectDrag(EndObject, touchPosition);
                return roadStartChanged || roadControlChanged || roadEndChanged;
            }
            else return false;
        }


        public static void RenderRoadLines(bool activate)
        {
            CenterLineObject = CustomRendrer.RenderLine(
                    name: "Road_center",
                    color: Color.green,
                    width: 10,
                    pointSize: 20,
                    linePoints: CenterLine.Vertices.ToArray());
            CenterLineObject.SetActive(activate);
            CenterLineObject.transform.SetParent(StaticObjectParent.transform);

            LeftLineObject =
            CustomRendrer.RenderLine(
                name: "Road_left_edge",
                color: Color.yellow,
                width: 10,
                pointSize: 20,
                linePoints: edgeLine.Vertices.ToArray());
            LeftLineObject.SetActive(activate);
            LeftLineObject.transform.SetParent(StaticObjectParent.transform);
            RightLineObject =
            CustomRendrer.RenderLine(
                name: "Road_right_edge",
                color: Color.red,
                width: 10,
                pointSize: 20,
                linePoints: edgeLine.Vertices.ToArray());
            RightLineObject.SetActive(activate);
            RightLineObject.transform.SetParent(StaticObjectParent.transform);

        }
    }


    public class CustomRoadLine
    {
        public Vector3 StartPoint;
        public Vector3 EndPoint;
        public List<Vector3> Vertices = new();
    }
}

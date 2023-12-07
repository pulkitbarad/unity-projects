using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomRoadBuilder : MonoBehaviour
{
    public static GameObject StaticParent;
    public static GameObject StartObject;
    public static GameObject ControlObject;
    public static GameObject EndObject;
    public static GameObject CenterLineObject;
    public static GameObject LeftLineObject;
    public static GameObject RightLineObject;
    public static readonly Dictionary<string, Vector3> InitialStaticLocalScale = new();
    public static readonly List<CustomRoad> ExistingRoads = new();
    public static CustomRoad CurrentActiveRoad;

    void Start()
    {
        StaticParent = new GameObject("StaticObjects");
        CurrentActiveRoad = new CustomRoad();
        CurrentActiveRoad.HideControlObjects();
    }

    public class CustomRoad
    {
        public Vector3 StartPosition;
        public Vector3 CurveControlPosition;
        public Vector3 EndPosition;
        public CustomRoadLine CenterLine;
        public CustomRoadLine LeftEdge;
        public CustomRoadLine RightEdge;
        public int RoadWidth = 10;
        public int VertexCount = 20;
        public bool IsCurved = true;

        public CustomRoad()
        {

            if (!this.IsCurved)
                VertexCount = 3;

            InitControlObjects();
            Rebuild(forceRebuild: true, touchPosition: Vector2.zero);
        }

        public void InitControlObjects()
        {
            StartObject = InitStaticObject(
                objectName: "RoadStart",
                 size: 10,
                  color: new Color(0.25f, 0.35f, 0.30f));
            ControlObject = InitStaticObject(
                objectName: "RoadControl",
                size: 10,
                color: new Color(0, 1, 0.20f));
            EndObject = InitStaticObject(
                objectName: "RoadEnd",
                size: 10,
                color: new Color(0.70f, 0.45f, 0f));
            this.CenterLine = new();
            this.LeftEdge = new();
            this.RightEdge = new();

            this.StartPosition = this.EndPosition = CameraMovement.GetTerrainHitPoint(CommonController.GetScreenCenterPoint());
            this.EndPosition += 200f * CameraMovement.MainCameraRoot.transform.right;
            this.CurveControlPosition = InitCurveControlPosition();
            StartObject.transform.position = StartPosition;
            EndObject.transform.position = this.EndPosition;
            ControlObject.transform.position = InitCurveControlPosition();
        }

        private Vector3 InitCurveControlPosition()
        {
            var startToEndDirection = this.EndPosition - this.StartPosition;
            var startToEndDistance = startToEndDirection.magnitude;
            Vector3 midPointVector = 0.5f * startToEndDistance * startToEndDirection.normalized;
            if (IsCurved)
                return this.StartPosition + Quaternion.AngleAxis(45, Vector3.up) * midPointVector;
            else

                return this.StartPosition + midPointVector;

        }

        public GameObject InitStaticObject(string objectName, float size, Color? color)
        {

            GameObject gameObject = CommonController.FindGameObject(objectName, true);

            if (gameObject == null)
            {
                gameObject = CustomRenderer.RenderCylinder(objectName: objectName, position: Vector3.zero, size: size, color: color);
                gameObject.transform.SetParent(StaticParent.transform);
                InitialStaticLocalScale.Add(gameObject.name, gameObject.transform.localScale);
            }
            return gameObject;
        }

        public void ShowControlObjects()
        {
            //Make control objects visible            
            StartObject.SetActive(true);
            EndObject.SetActive(true);
            Debug.Log("IsCurved=" + IsCurved);
            if (this.IsCurved)
                ControlObject.SetActive(true);

            //Make line objects visible            
            CenterLineObject.SetActive(true);
            LeftLineObject.SetActive(true);
            RightLineObject.SetActive(true);
        }

        public void HideControlObjects()
        {
            StartObject.SetActive(false);
            ControlObject.SetActive(false);
            EndObject.SetActive(false);
            CenterLineObject.SetActive(false);
            LeftLineObject.SetActive(false);
            RightLineObject.SetActive(false);
        }

        public void StartBuilding(bool isCurved)
        {
            CurrentActiveRoad = new CustomRoad();
            CurrentActiveRoad.ShowControlObjects();
        }

        public void ConfirmBuilding()
        {
            string suffix = "_" + CurrentActiveRoad.StartPosition.x;
            CustomRenderer.RenderLine(
                name: "RoadCenterLine" + suffix,
                color: Color.yellow,
                linePoints: CurrentActiveRoad.CenterLine.Vertices.ToArray()).transform.SetParent(StaticParent.transform);
            CustomRenderer.RenderLine(
                name: "RoadLeftEdge" + suffix,
                color: Color.yellow,
                linePoints: CurrentActiveRoad.LeftEdge.Vertices.ToArray()).transform.SetParent(StaticParent.transform);
            CustomRenderer.RenderLine(
                name: "RoadRightEdge" + suffix,
                color: Color.yellow,
                linePoints: CurrentActiveRoad.RightEdge.Vertices.ToArray()).transform.SetParent(StaticParent.transform);
            CurrentActiveRoad.HideControlObjects();
            ExistingRoads.Add(CurrentActiveRoad);
        }

        public void CancelBuilding()
        {
            CurrentActiveRoad.HideControlObjects();
        }

        public void Rebuild(
            bool forceRebuild,
            Vector2 touchPosition)
        {
            if (forceRebuild || IsRebuildRequired(touchPosition))
            {
                CurvedLine.FindBazierLinePoints(
                    startPosition: StartObject.transform.position,
                    controlPosition: ControlObject.transform.position,
                    endPosition: EndObject.transform.position,
                    vertexCount: this.VertexCount,
                    roadWidth: this.RoadWidth,
                    centerLinePoints: out this.CenterLine.Vertices,
                    leftLinePoints: out this.LeftEdge.Vertices,
                    rightLinePoints: out this.RightEdge.Vertices);

                RenderRoadLines();
            }
        }

        private bool IsRebuildRequired(Vector2 touchPosition)
        {

            if (!EventSystem.current.IsPointerOverGameObject())
            {
                var roadStartChanged =
                    !this.StartPosition.Equals(Vector3.zero)
                    && CommonController.HandleGameObjectDrag(StartObject, touchPosition, ControlObject);

                var roadControlChanged =
                    this.IsCurved
                    && !this.CurveControlPosition.Equals(Vector3.zero)
                    && CommonController.HandleGameObjectDrag(ControlObject, touchPosition);

                var roadEndChanged =
                    !this.EndPosition.Equals(Vector3.zero)
                    && CommonController.HandleGameObjectDrag(EndObject, touchPosition, ControlObject);

                return roadStartChanged || roadControlChanged || roadEndChanged;
            }
            else return false;
        }

        public void RenderRoadLines()
        {
            CenterLineObject = CustomRenderer.RenderLine(
                    name: "RoadCenterLine",
                    color: Color.green,
                    width: 2,
                    pointSize: 5,
                    parentTransform: StaticParent.transform,
                    renderPoints: true,
                    linePoints: this.CenterLine.Vertices.ToArray());

            LeftLineObject =
            CustomRenderer.RenderLine(
                name: "RoadLeftEdge",
                color: Color.yellow,
                width: 2,
                pointSize: 5,
                parentTransform: StaticParent.transform,
                renderPoints: true,
                linePoints: this.LeftEdge.Vertices.ToArray());

            RightLineObject =
            CustomRenderer.RenderLine(
                name: "RoadRightEdge",
                color: Color.red,
                width: 2,
                pointSize: 5,
                parentTransform: StaticParent.transform,
                renderPoints: true,
                linePoints: this.RightEdge.Vertices.ToArray());
        }
    }


    public class CustomRoadLine
    {
        public Vector3 StartPoint;
        public Vector3 EndPoint;
        public List<Vector3> Vertices;

        public CustomRoadLine()
        {
            this.StartPoint = Vector3.zero;
            this.EndPoint = Vector3.zero;
            this.Vertices = new();
        }
    }
}

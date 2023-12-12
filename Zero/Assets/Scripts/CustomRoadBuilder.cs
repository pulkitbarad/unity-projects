using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomRoadBuilder : MonoBehaviour
{
    public static GameObject RoadControlsParent;
    public static GameObject BuiltRoadsParent;
    public static GameObject StartObject;
    public static GameObject ControlObject;
    public static GameObject EndObject;
    public static GameObject CenterLineObject;
    public static GameObject LeftLineObject;
    public static GameObject RightLineObject;
    public static readonly Dictionary<string, Vector3> InitialStaticLocalScale = new();
    public static readonly List<CustomRoad> BuiltRoads = new();
    public static CustomRoad CurrentActiveRoad;

    void Start()
    {
        RoadControlsParent = new GameObject("RoadControls");
        BuiltRoadsParent = new GameObject("BuiltRoads");
        CurrentActiveRoad = new CustomRoad(true);
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
        public int RoadHeight = 2;
        public int VertexCount = 20;
        public bool IsCurved = true;

        public CustomRoad(bool isCurved)
        {
            this.IsCurved = isCurved;

            if (!this.IsCurved)
                VertexCount = 4;

            InitControlObjects();
            Rebuild(forceRebuild: true, touchPosition: Vector2.zero);
        }

        public void InitControlObjects()
        {
            StartObject = InitStaticObject(
                objectName: "RoadStart",
                 size: 10,
                  color: new Color(0.25f, 0.35f, 0.30f));
            if (this.IsCurved)
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
            StartObject.transform.position = this.StartPosition;
            EndObject.transform.position = this.EndPosition;
            if (this.IsCurved)
            {
                this.CurveControlPosition = InitCurveControlPosition();
                ControlObject.transform.position = InitCurveControlPosition();
            }
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
                gameObject.transform.SetParent(RoadControlsParent.transform);
                InitialStaticLocalScale.Add(gameObject.name, gameObject.transform.localScale);
            }
            return gameObject;
        }

        public void ShowControlObjects()
        {
            //Make control objects visible            
            StartObject.SetActive(true);
            EndObject.SetActive(true);
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
            CurrentActiveRoad = new CustomRoad(isCurved);
            CurrentActiveRoad.ShowControlObjects();
        }

        public void ConfirmBuilding()
        {
            var suffix = BuiltRoads.Count;
            Transform roadParentTransform = new GameObject("Road" + suffix).transform;

            roadParentTransform.SetParent(BuiltRoadsParent.transform);
            CustomRenderer.RenderLine(
                name: "RoadCenterLine" + suffix,
                color: Color.yellow,
                linePoints: CurrentActiveRoad.CenterLine.Vertices.ToArray()).transform.SetParent(roadParentTransform);
            CustomRenderer.RenderLine(
                name: "RoadLeftEdge" + suffix,
                color: Color.yellow,
                linePoints: CurrentActiveRoad.LeftEdge.Vertices.ToArray()).transform.SetParent(roadParentTransform);
            CustomRenderer.RenderLine(
                name: "RoadRightEdge" + suffix,
                color: Color.yellow,
                linePoints: CurrentActiveRoad.RightEdge.Vertices.ToArray()).transform.SetParent(roadParentTransform);
            CurrentActiveRoad.HideControlObjects();
            RenderRoadMesh();
            BuiltRoads.Add(CurrentActiveRoad);
        }

        private void RenderRoadMesh()
        {
            List<Vector3> centerVertices = CurrentActiveRoad.CenterLine.Vertices;
            string roadObjectName = "Road" + BuiltRoads.Count;
            for (int i = 1; i < centerVertices.Count; i++)
            {

                GameObject segmentObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                segmentObject.name = roadObjectName + "Segment" + i;
                Vector3 forwardDirection = centerVertices[i] - centerVertices[i - 1];
                segmentObject.transform.position =
                    centerVertices[i - 1]
                    + forwardDirection * 0.5f
                    + Vector3.up * CurrentActiveRoad.RoadHeight;
                segmentObject.transform.localScale = new Vector3(forwardDirection.magnitude, CurrentActiveRoad.RoadHeight, CurrentActiveRoad.RoadWidth);
                float angleWithRight = Vector3.Angle(forwardDirection, segmentObject.transform.right);
                segmentObject.transform.rotation = Quaternion.AngleAxis(-angleWithRight, Vector3.up);
                // var headingChange = Quaternion.FromToRotation(segmentObject.transform.forward, forwardDirection);
                // segmentObject.transform.localRotation *= headingChange;
                // segmentObject.AddComponent<BoxCollider>();

                // Vector3[] points = new Vector3[]
                //     { rightVertices[i - 1], rightVertices[i],leftVertices[i - 1], leftVertices[i] };
                // Mesh mesh = new();
                // segmentObject.GetComponent<MeshFilter>().mesh = mesh;
                // mesh.vertices = points;
                // mesh.triangles = new int[] { 0, 2, 3, 3, 1, 0 };
                // Vector3 upVector = Vector3.Cross(points[0] - points[2], points[3] - points[2]);
                // Vector3[] normals = new Vector3[] { upVector, upVector, upVector, upVector, upVector, upVector };
                // mesh.normals = new Vector3[] { upVector, upVector, upVector, upVector , upVector, upVector };
                // mesh.uv = new Vector2[] { Vector2.right, Vector2.one, Vector2.zero, Vector2.up };

            }
        }

        private Vector3 GetPointNormal(Vector3 originVertex, Vector3 destinationVertex, Vector3 adjacentVertex)
        {
            return Vector3.Cross(destinationVertex - originVertex, adjacentVertex - originVertex);
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
                Vector3[] controlPoints;
                if (this.IsCurved)
                {
                    controlPoints = new Vector3[3];
                    controlPoints[0] = StartObject.transform.position;
                    controlPoints[1] = ControlObject.transform.position;
                    controlPoints[2] = EndObject.transform.position;
                }
                else
                {
                    controlPoints = new Vector3[2];
                    controlPoints[0] = StartObject.transform.position;
                    controlPoints[1] = EndObject.transform.position;
                }

                CurvedLine.FindBazierLinePoints(
                    vertexCount: this.VertexCount,
                    roadWidth: this.RoadWidth,
                    centerLinePoints: out this.CenterLine.Vertices,
                    leftLinePoints: out this.LeftEdge.Vertices,
                    rightLinePoints: out this.RightEdge.Vertices,
                    controlPoints);

                RenderRoadLines();
            }
        }

        private bool IsRebuildRequired(Vector2 touchPosition)
        {

            if (!EventSystem.current.IsPointerOverGameObject())
            {
                var roadStartChanged =
                    !this.StartPosition.Equals(Vector3.zero)
                    && CommonController.HandleGameObjectDrag(StartObject, touchPosition);

                var roadControlChanged =
                    this.IsCurved
                    && !this.CurveControlPosition.Equals(Vector3.zero)
                    && CommonController.HandleGameObjectDrag(ControlObject, touchPosition);

                var roadEndChanged =
                    !this.EndPosition.Equals(Vector3.zero)
                    && CommonController.HandleGameObjectDrag(EndObject, touchPosition);

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
                    parentTransform: RoadControlsParent.transform,
                    renderPoints: true,
                    linePoints: this.CenterLine.Vertices.ToArray());

            LeftLineObject =
            CustomRenderer.RenderLine(
                name: "RoadLeftEdge",
                color: Color.yellow,
                width: 2,
                pointSize: 5,
                parentTransform: RoadControlsParent.transform,
                renderPoints: true,
                linePoints: this.LeftEdge.Vertices.ToArray());

            RightLineObject =
            CustomRenderer.RenderLine(
                name: "RoadRightEdge",
                color: Color.red,
                width: 2,
                pointSize: 5,
                parentTransform: RoadControlsParent.transform,
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

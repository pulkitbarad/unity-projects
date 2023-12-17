using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class CustomRoadBuilder : MonoBehaviour
{
    public static GameObject RoadControlsParent;
    public static GameObject BuiltRoadsParent;
    public static GameObject BuiltRoadSegmentsParent;
    public static GameObject BuiltIntersectionsParent;
    public static GameObject StartObject;
    public static GameObject ControlObject;
    public static GameObject EndObject;
    public static GameObject LeftLineObject;
    public static GameObject RightLineObject;
    public static CustomRoad CurrentActiveRoad;
    public static readonly Dictionary<string, Vector3> InitialStaticLocalScale = new();
    public static readonly Dictionary<string, CustomRoadSegment> BuiltRoadSegments = new();
    public static readonly Dictionary<string, CustomRoad> BuiltRoads = new();
    public static readonly Dictionary<string, CustomRoadIntersection> BuiltIntersections = new();

    void Start()
    {
        RoadControlsParent = new GameObject("RoadControls");
        BuiltRoadsParent = new GameObject("BuiltRoads");
        BuiltRoadSegmentsParent = new GameObject("BuiltRoadSegments");
        BuiltIntersectionsParent = new GameObject("BuiltIntersections");
        InitControlObjects(true);
        HideControlObjects();
    }
    public static void InitControlObjects(bool isCurved)
    {
        StartObject = InitStaticObject(
            objectName: "RoadStart",
             size: 10,
              color: new Color(0.25f, 0.35f, 0.30f));
        if (isCurved)
            ControlObject = InitStaticObject(
                objectName: "RoadControl",
                size: 10,
                color: new Color(0, 1, 0.20f));
        EndObject = InitStaticObject(
            objectName: "RoadEnd",
            size: 10,
            color: new Color(0.70f, 0.45f, 0f));

        LeftLineObject =
        CustomRenderer.GetLineObject(
            name: "RoadLeftEdge",
            parentTransform: RoadControlsParent.transform);

        RightLineObject =
        CustomRenderer.GetLineObject(
            name: "RoadRightEdge",
            parentTransform: RoadControlsParent.transform);
    }

    public static GameObject InitStaticObject(string objectName, float size, Color? color)
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

    private static Vector3 InitCurveControlPosition(bool isCurved)
    {
        Vector3 startPosition = StartObject.transform.position;
        var startToEndDirection = EndObject.transform.position - startPosition;
        var startToEndDistance = startToEndDirection.magnitude;
        Vector3 midPointVector = 0.5f * startToEndDistance * startToEndDirection.normalized;
        if (isCurved)
            return startPosition + Quaternion.AngleAxis(45, Vector3.up) * midPointVector;
        else
            return startPosition + midPointVector;
    }

    public static void ShowControlObjects(bool isCurved)
    {
        //Make control objects visible            
        StartObject.SetActive(true);
        EndObject.SetActive(true);
        if (isCurved)
            ControlObject.SetActive(true);
        LeftLineObject.SetActive(true);
        RightLineObject.SetActive(true);
    }

    public static void HideControlObjects()
    {
        StartObject.SetActive(false);
        ControlObject.SetActive(false);
        EndObject.SetActive(false);
        LeftLineObject.SetActive(false);
        RightLineObject.SetActive(false);
    }

    public static void StartBuilding(bool isCurved)
    {
        CurrentActiveRoad = new CustomRoad("Road" + BuiltRoads.Count, isCurved);
        ShowControlObjects(isCurved);
    }

    public static void ConfirmBuilding()
    {
        HideControlObjects();
        CurrentActiveRoad.RenderRoad();
    }
    public static void CancelBuilding()
    {
        HideControlObjects();
    }

    public static void RepositionControlObjects(bool isCurved)
    {
        Vector3 startPosition = CameraMovement.GetTerrainHitPoint(CommonController.GetScreenCenterPoint());

        StartObject.transform.position = startPosition;
        EndObject.transform.position = startPosition + 200f * CameraMovement.MainCameraRoot.transform.right;
        if (isCurved)
        {
            ControlObject.transform.position = InitCurveControlPosition(isCurved);
        }
        LeftLineObject =
        CustomRenderer.GetLineObject(
            name: "RoadLeftEdge",
            parentTransform: RoadControlsParent.transform);

        RightLineObject =
        CustomRenderer.GetLineObject(
            name: "RoadRightEdge",
            parentTransform: RoadControlsParent.transform);
    }

    public class CustomRoad
    {
        public string Name;
        public int RoadWidth = 10;
        public int RoadHeight = 2;
        public int VertexCount = 20;
        public bool IsCurved = true;
        public List<CustomRoadSegment> Segments = new();
        public GameObject RoadObject;

        public CustomRoad(string name, bool isCurved)
        {
            this.Name = name;
            this.IsCurved = isCurved;
            this.RoadObject = CommonController.FindGameObject(name, true) ?? new GameObject("Road" + BuiltRoads.Count);
            this.RoadObject.transform.SetParent(BuiltRoadsParent.transform);

            if (!this.IsCurved)
                VertexCount = 4;

            CustomRoadBuilder.RepositionControlObjects(isCurved);
            this.Rebuild(forceRebuild: true, touchPosition: Vector2.zero);
        }

        public void Rebuild(
            bool forceRebuild,
            Vector2 touchPosition)
        {
            if (forceRebuild || this.IsRebuildRequired(touchPosition))
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
                    centerLinePoints: out List<Vector3> centerVertices,
                    controlPoints);

                this.Segments = new();
                for (int i = 0; i < centerVertices.Count - 1; i++)
                {
                    string roadSegmentName = String.Concat("RoadSegment", (CustomRoadBuilder.BuiltRoadSegments.Count + i).ToString());
                    this.Segments.Add(
                        new CustomRoadSegment(
                                name: roadSegmentName,
                                centerStart: centerVertices[i],
                                centerEnd: centerVertices[i + 1],
                                width: this.RoadWidth,
                                height: this.RoadHeight,
                                parentRoad: this,
                                renderSegment: false)
                    );
                }
                this.RenderRoadLines();
            }
        }

        private bool IsRebuildRequired(Vector2 touchPosition)
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                var roadStartChanged =
                    !StartObject.transform.position.Equals(Vector3.zero)
                    && CommonController.HandleGameObjectDrag(StartObject, touchPosition);

                var roadControlChanged =
                    this.IsCurved
                    && !ControlObject.transform.position.Equals(Vector3.zero)
                    && CommonController.HandleGameObjectDrag(ControlObject, touchPosition);

                var roadEndChanged =
                    !EndObject.transform.position.Equals(Vector3.zero)
                    && CommonController.HandleGameObjectDrag(EndObject, touchPosition);

                return roadStartChanged || roadControlChanged || roadEndChanged;
            }
            else return false;
        }

        public void RenderRoad()
        {
            this.Segments.ForEach(
                (segment) =>
                {
                    BuiltRoadSegments[segment.Name] = segment;
                    segment.InitSegmentObject();
                }
            );
            this.RenderIntersections();
            BuiltRoads[this.Name] = this;
        }

        public void RenderRoadLines()
        {
            List<Vector3> leftVertices = new();
            List<Vector3> rightVertices = new();
            for (int i = 0; i < this.Segments.Count; i++)
            {
                CustomRoadSegment segment = this.Segments[i];
                Vector3[] bounds = segment.TopPlane;
                leftVertices.Add(bounds[0]);
                leftVertices.Add(bounds[2]);
                rightVertices.Add(bounds[1]);
                rightVertices.Add(bounds[3]);
            }

            LeftLineObject =
            CustomRenderer.RenderLine(
                name: "RoadLeftEdge",
                color: Color.yellow,
                width: 2,
                pointSize: 5,
                parentTransform: RoadControlsParent.transform,
                renderPoints: true,
                linePoints: leftVertices.ToArray());

            RightLineObject =
            CustomRenderer.RenderLine(
                name: "RoadRightEdge",
                color: Color.red,
                width: 2,
                pointSize: 5,
                parentTransform: RoadControlsParent.transform,
                renderPoints: true,
                linePoints: rightVertices.ToArray());
        }

        // private void RenderRoadMesh()
        // {

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
        // }

        public void RenderIntersections()
        {
            List<Vector3> leftCollisionPoints = new();
            List<Vector3> rightCollisionPoints = new();

            int activeRoadSegmentCount = this.Segments.Count;
            for (int segmentIndex = 0; segmentIndex < activeRoadSegmentCount; segmentIndex++)
            {
                CustomRoadSegment segment = this.Segments[segmentIndex];
                Vector3[] segmentBounds = segment.TopPlane;
                Collider[] overalppingColliders =
                    Physics.OverlapBox(
                        segment.SegmentObject.transform.position,
                        segment.SegmentObject.transform.localScale / 2);

                if (overalppingColliders.Length > 0)
                {
                    foreach (Collider collider in overalppingColliders)
                    {
                        collider.enabled = false;
                    }
                    foreach (Collider collider in overalppingColliders)
                    {
                        collider.enabled = true;
                        string colliderSegmentName = collider.gameObject.name;
                        if (BuiltRoadSegments.ContainsKey(colliderSegmentName))
                        {
                            Vector3[] colliderBounds = BuiltRoadSegments[colliderSegmentName].TopPlane;
                            if (colliderBounds.Length > 0)
                            {
                                List<List<Vector3>> segmentCollisionPoints = this.FindCollisionPoints(
                                    maxDistance: segment.Length,
                                    colliderBounds: colliderBounds,
                                    segmentBounds: segmentBounds);
                                leftCollisionPoints.AddRange(segmentCollisionPoints[0]);
                                rightCollisionPoints.AddRange(segmentCollisionPoints[1]);
                            }
                        }
                        collider.enabled = false;
                    }
                    foreach (Collider collider in overalppingColliders)
                    {
                        collider.enabled = true;
                    }
                }
            }

            for (int collisionPointIndex = 0;
                collisionPointIndex < leftCollisionPoints.Count - 2;
                collisionPointIndex += 2)
            {
                // CustomRenderer.RenderSphere(
                //     position: leftCollisionPoints[collisionPointIndex],
                //     sphereName: this.Name + "LeftPoint" + collisionPointIndex,
                //     color: Color.green);
            }
            for (int collisionPointIndex = 0;
                collisionPointIndex < rightCollisionPoints.Count - 2;
                collisionPointIndex += 2)
            {
                // CustomRenderer.RenderSphere(
                //     position: rightCollisionPoints[collisionPointIndex],
                //     sphereName: this.Name + "RightPoint" + collisionPointIndex,
                //     color: Color.red);
            }
        }

        private List<List<Vector3>> FindCollisionPoints(
            float maxDistance,
            Vector3[] colliderBounds,
            Vector3[] segmentBounds)
        {
            List<Vector3> leftCollisionPoints = new();
            List<Vector3> rightCollisionPoints = new();
            if (!this.IsPartialOverlap(colliderBounds, segmentBounds))
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector3 origin;
                    Vector3 direction;
                    origin = segmentBounds[i];
                    if (i < 2)
                        direction = segmentBounds[i + 2] - segmentBounds[i];
                    else
                        direction = segmentBounds[i - 2] - segmentBounds[i];
                    if (!Physics.Raycast(
                                origin: origin,
                                direction: direction,
                                hitInfo: out RaycastHit _rayHit2,
                                maxDistance: maxDistance))
                    {
                        CustomRenderer.RenderSphere(origin, color: Color.blue);
                        CustomRenderer.RenderSphere(direction + origin, color: Color.yellow);
                    }

                    if (Physics.Raycast(
                            origin: origin,
                            direction: direction,
                            hitInfo: out RaycastHit _rayHit,
                            maxDistance: maxDistance))
                    {
                        if (i % 2 == 1)
                            leftCollisionPoints.Add(_rayHit.point);
                        else
                            rightCollisionPoints.Add(_rayHit.point);
                        CustomRenderer.RenderSphere(_rayHit.point, color: Color.green);
                        maxDistance -= (_rayHit.point - origin).magnitude;
                        origin = _rayHit.point;
                    }
                }
            }
            return new List<List<Vector3>>() { leftCollisionPoints, rightCollisionPoints };
        }

        private bool IsPartialOverlap(Vector3[] boundsMain, Vector3[] boundsRef)
        {
            for (int i = 0; i < 4; i++)
                if (!IsPointInsideBounds(boundsMain[i], boundsRef))
                    return false;
            return true;
        }

        private bool IsPointInsideBounds(Vector3 point, Vector3[] bounds, float margin = 5)
        {
            float maxX = bounds[0].x;
            float maxY = bounds[0].y;
            float minX = bounds[0].x;
            float minY = bounds[0].y;

            for (int i = 1; i < 4; i++)
            {
                if (bounds[i].x > maxX) maxX = bounds[i].x;
                if (bounds[i].y > maxY) maxY = bounds[i].y;
                //
                if (bounds[i].x < minX) minX = bounds[i].x;
                if (bounds[i].y < minY) minY = bounds[i].y;
            }
            return point.x > minX && point.x < maxX && point.y > minY && point.y < maxY;
        }
    }

    public class CustomRoadIntersection
    {
        public string Name;
        public Vector3[] Vertices;
        public CustomRoadIntersection(string name, Vector3[] vertices)
        {
            this.Name = name;
            this.Vertices = vertices;
        }
    }

    public class CustomRoadSegment
    {
        public string Name;
        public Vector3[] TopPlane;
        public Vector3[] BottomPlane;
        public Vector3 Center;
        public Vector3 Forward;
        public float Width;
        public float Height;
        public float Length;
        public GameObject SegmentObject;
        public CustomRoad ParentRoad;

        public CustomRoadSegment(
             string name,
             float width,
             float height,
             Vector3 centerStart,
             Vector3 centerEnd,
             CustomRoad parentRoad,
             bool renderSegment)
        {
            this.Name = name;
            this.Width = width;
            this.Height = height;
            this.ParentRoad = parentRoad;
            this.Forward = centerEnd - centerStart;
            this.Length = this.Forward.magnitude;
            this.InitCenter(centerStart);
            this.InitPlanes(centerStart, centerEnd);
            if (renderSegment)
                this.InitSegmentObject();
        }

        private void InitPlanes(Vector3 centerStart, Vector3 centerEnd)
        {
            Vector3 forward = centerEnd - centerStart;
            Vector3 left = Vector3.Cross(forward, Vector3.up).normalized;
            Vector3 halfLeft = left * this.Width / 2;
            Vector3 leftStart = centerStart + halfLeft;
            Vector3 leftEnd = centerEnd + halfLeft;
            Vector3 rightStart = centerStart - halfLeft;
            Vector3 rightEnd = centerEnd - halfLeft;

            this.TopPlane = new Vector3[]{
                leftStart,
                rightStart,
                leftEnd,
                rightEnd
            };

            this.BottomPlane = new Vector3[]{
                leftStart,
                rightStart,
                leftEnd,
                rightEnd
            };

            for (int i = 0; i < this.TopPlane.Length; i++)
            {
                this.TopPlane[i] += 0.5f * this.Height * Vector3.up;
                this.BottomPlane[i] -= 0.5f * this.Height * Vector3.up;
            }
        }

        private void InitCenter(Vector3 centerStart)
        {
            this.Center = centerStart + this.Forward / 2 + 0.5f * this.Height * Vector3.up;
        }

        public void InitSegmentObject()
        {
            GameObject segmentObject = CommonController.FindGameObject(this.Name, true) ?? GameObject.CreatePrimitive(PrimitiveType.Cube);
            segmentObject.name = this.Name;
            segmentObject.transform.position = this.Center;
            segmentObject.transform.localScale = new Vector3(this.Width, this.Height, this.Length);
            var headingChange = Quaternion.FromToRotation(segmentObject.transform.forward, this.Forward);
            segmentObject.transform.localRotation *= headingChange;
            segmentObject.transform.SetParent(BuiltRoadSegmentsParent.transform);
            this.SegmentObject = segmentObject;
        }
    }
}

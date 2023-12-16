using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public static readonly Dictionary<string, CustomRoadSegment> BuiltRoadSegments = new();
    public static CustomRoad CurrentActiveRoad;

    void Start()
    {
        RoadControlsParent = new GameObject("RoadControls");
        BuiltRoadsParent = new GameObject("BuiltRoads");
        // CurrentActiveRoad.HideControlObjects();
    }

    public class CustomRoad
    {
        public Vector3 StartPosition;
        public Vector3 CurveControlPosition;
        public Vector3 EndPosition;
        public int RoadWidth = 10;
        public int RoadHeight = 2;
        public int VertexCount = 20;
        public bool IsCurved = true;

        public List<CustomRoadSegment> Segments = new();

        public GameObject RoadObject;

        public CustomRoad(bool isCurved, GameObject roadObject)
        {
            this.IsCurved = isCurved;
            this.RoadObject = roadObject;

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
            CurrentActiveRoad = new CustomRoad(isCurved, new GameObject("Road" + BuiltRoads.Count));
            CurrentActiveRoad.ShowControlObjects();
        }

        public void ConfirmBuilding()
        {
            var suffix = BuiltRoads.Count;
            Transform roadParentTransform = new GameObject("Road" + suffix).transform;

            roadParentTransform.SetParent(BuiltRoadsParent.transform);
            CurrentActiveRoad.HideControlObjects();
            CurrentActiveRoad.Segments.ForEach(
                segment =>
                BuiltRoadSegments.Add(segment.SegmentObject.name, segment)
            );
            BuiltRoads.Add(CurrentActiveRoad);
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

                List<Vector3> centerVertices = new();

                CurvedLine.FindBazierLinePoints(
                    vertexCount: this.VertexCount,
                    roadWidth: this.RoadWidth,
                    centerLinePoints: out centerVertices,
                    controlPoints);

                this.Segments = new();

                for (int i = 0; i < centerVertices.Count - 1; i++)
                {
                    this.Segments.Add(
                        new CustomRoadSegment(
                            roadObjectName: this.RoadObject.name,
                            segmentIndex: i,
                            centerStart: centerVertices[i],
                            centerEnd: centerVertices[i + 1],
                            width: this.RoadWidth,
                            height: this.RoadHeight
                        )
                    );
                }
                RenderRoadMesh();
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


        private void RenderRoadMesh()
        {
            string roadObjectName = "Road" + BuiltRoads.Count;
            int activeRoadSegmentCount = CurrentActiveRoad.Segments.Count;
            for (int i = 0; i < activeRoadSegmentCount - 1; i++)
            {

                CustomRoadSegment segmentForward = CurrentActiveRoad.Segments[i];
                Collider[] overalppingColliders =
                    Physics.OverlapBox(
                        segmentForward.SegmentObject.transform.position,
                        segmentForward.SegmentObject.transform.localScale / 2);
                Vector3[] collisionPoints = new Vector3[]{
                            Vector3.positiveInfinity,
                            Vector3.positiveInfinity,
                            Vector3.positiveInfinity,
                            Vector3.positiveInfinity
                        };

                if (overalppingColliders.Length > 0)
                {
                    CustomRoadSegment segmentReverse = CurrentActiveRoad.Segments[activeRoadSegmentCount - i];
                    foreach (Collider collider in overalppingColliders)
                    {
                        string colliderSegmentName = collider.gameObject.name;
                        if (BuiltRoadSegments.ContainsKey(collider.gameObject.name))
                        {
                            Vector3[] segmentReverseBounds = segmentReverse.TopPlane;
                            Vector3[] colliderBounds = segmentReverse.TopPlane;
                            Vector3 leftStart = segmentReverseBounds[0];
                            Vector3 rightStart = segmentReverseBounds[1];
                            Vector3 leftEnd = segmentReverseBounds[2];
                            Vector3 rightEnd = segmentReverseBounds[3];
                            collisionPoints[1] = FindCollisionPoint(
                                origin: leftEnd,
                                direction: leftStart - leftEnd,
                                maxDistance: segmentReverse.Length,
                                colliderBounds: colliderBounds,
                                segmentBounds: segmentReverseBounds);
                            collisionPoints[3] = FindCollisionPoint(
                                origin: rightEnd,
                                direction: rightStart - rightEnd,
                                maxDistance: segmentReverse.Length,
                                colliderBounds: colliderBounds,
                                segmentBounds: segmentReverseBounds);
                        }
                    }
                }

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

        private Vector3 FindCollisionPoint(
            Vector3 origin,
            Vector3 direction,
            float maxDistance,
            Vector3[] colliderBounds,
            Vector3[] segmentBounds)
        {
            if (!IsPartialOverlap(colliderBounds, segmentBounds))
            {
                if (!IsPointInsideBounds(origin, colliderBounds))
                {
                    if (
                        Physics.Raycast(origin,
                        direction: direction,
                            hitInfo: out RaycastHit _rayHit,
                            maxDistance: maxDistance))
                    {
                        return _rayHit.point;
                    }
                }
            }

            return Vector3.positiveInfinity;
        }

        private bool IsPartialOverlap(Vector3[] boundsMain, Vector3[] boundsRef)
        {
            for (int i = 0; i < 4; i++)
                if (!IsPointInsideBounds(boundsMain[i], boundsRef))
                    return false;
            return true;
        }

        private bool IsPointInsideBounds(Vector3 point, Vector3[] bounds)
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

        private Vector3 GetCollisionPoint(Vector3 edgeStart, Vector3 edgeEnd, Vector3 colliderCenter)
        {

            Vector3 edgeForward = edgeEnd - edgeStart;
            float angleToCollider = Vector3.Angle(edgeForward, colliderCenter - edgeStart);
            if (angleToCollider > 90f)
                return -edgeForward;
            else
                return edgeForward;
        }

        public void CancelBuilding()
        {
            CurrentActiveRoad.HideControlObjects();
        }

    }


    public class CustomRoadSegment
    {
        public Vector3[] TopPlane;
        public Vector3[] BottomPlane;
        public Vector3 Center;
        public float Length;
        public GameObject SegmentObject;

        public CustomRoadSegment(
            string roadObjectName,
            int segmentIndex,
            Vector3 centerStart,
            Vector3 centerEnd,
            float width,
            float height)
        {

            Vector3 forward = centerEnd - centerStart;
            Vector3 left = Vector3.Cross(forward, Vector3.up).normalized;
            Vector3 halfLeft = left * width / 2;
            Vector3 leftStart = centerStart + halfLeft;
            Vector3 leftEnd = centerEnd + halfLeft;
            Vector3 rightStart = centerStart - halfLeft;
            Vector3 rightEnd = centerEnd - halfLeft;
            this.Center = centerStart + forward / 2 + 0.5f * height * Vector3.up;
            this.Length = forward.magnitude;
            this.TopPlane = new Vector3[]{
                leftStart,
                leftEnd,
                rightStart,
                rightEnd
            };
            this.BottomPlane = new Vector3[]{
                leftStart,
                leftEnd,
                rightStart,
                rightEnd
            };

            for (int i = 0; i < this.TopPlane.Length; i++)
            {
                this.TopPlane[i] += 0.5f * height * Vector3.up;
                this.BottomPlane[i] -= 0.5f * height * Vector3.up;
            }

            GameObject segmentObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segmentObject.name = roadObjectName + "Segment" + segmentIndex;
            segmentObject.layer = LayerMask.GetMask("RoadSegment");
            segmentObject.transform.position = this.Center;
            segmentObject.transform.localScale = new Vector3(width, height, forward.magnitude);
            var headingChange = Quaternion.FromToRotation(segmentObject.transform.forward, forward);
            segmentObject.transform.localRotation *= headingChange;
            this.SegmentObject = segmentObject;
        }
    }
}

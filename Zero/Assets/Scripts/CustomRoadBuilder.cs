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
    public static float RoadMaxChangeInAngle;
    public static int RoadMaxVertexCount;
    public static int RoadMinVertexCount;
    public static float RoadSegmentMinLength;
    public static float RoadLaneHeight = 1f;
    public static float RoadLaneWidth = 3f;
    public static float RoadSideWalkHeight = 1.5f;
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
             size: 2,
              color: new UnityEngine.Color(0.25f, 0.35f, 0.30f));
        if (isCurved)
            ControlObject = InitStaticObject(
                objectName: "RoadControl",
                size: 2,
                color: new UnityEngine.Color(0, 1, 0.20f));
        EndObject = InitStaticObject(
            objectName: "RoadEnd",
            size: 2,
            color: new UnityEngine.Color(0.70f, 0.45f, 0f));

        LeftLineObject =
        CustomRenderer.GetLineObject(
            name: "RoadLeftEdge",
            parentTransform: RoadControlsParent.transform);

        RightLineObject =
        CustomRenderer.GetLineObject(
            name: "RoadRightEdge",
            parentTransform: RoadControlsParent.transform);
    }

    public static GameObject InitStaticObject(
        string objectName,
        float size,
        UnityEngine.Color? color)
    {

        GameObject gameObject = CommonController.FindGameObject(objectName, true);

        if (gameObject == null)
        {
            gameObject =
            CustomRenderer.RenderCylinder(
                objectName: objectName,
                position: Vector3.zero,
                size: size,
                color: color);
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
        CurrentActiveRoad = new CustomRoad(
            name: "Road" + BuiltRoads.Count,
            isCurved: isCurved,
            hasBusLane: true,
            numberOfLanes: 2,
            height: CustomRoadBuilder.RoadLaneHeight,
            sidewalkHeight: CustomRoadBuilder.RoadSideWalkHeight);
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
        Vector3 startPosition =
            CameraMovement
            .GetTerrainHitPoint(CommonController.GetScreenCenterPoint());

        StartObject.transform.position = startPosition;
        EndObject.transform.position =
            startPosition + 20f * CameraMovement.MainCameraRoot.transform.right;
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

    private static Vector3[][] GetIntersectionPoints(
        string roadName,
        CustomRoadSegment segment,
        int sideOfTheRoad)
    {
        Vector3[] segmentBounds = segment.TopPlane;

        List<Collider> partialOverlaps = GetPartialOverlaps(roadName, segment);

        List<Vector3> leftIntersectionPoints = new();
        List<Vector3> rightIntersectionPoints = new();
        if (partialOverlaps.Count > 0)
        {
            if (sideOfTheRoad == 0 || sideOfTheRoad == 2)
            {

                leftIntersectionPoints.AddRange(
                    CustomRoadBuilder.GetCollisionPoints(
                        origin: segmentBounds[0],
                        end: segmentBounds[2],
                        overalppingColliders: partialOverlaps));
                leftIntersectionPoints.AddRange(
                    CustomRoadBuilder.GetCollisionPoints(
                        origin: segmentBounds[2],
                        end: segmentBounds[0],
                        overalppingColliders: partialOverlaps));
            }
            if (sideOfTheRoad == 0 || sideOfTheRoad == 2)
            {

                rightIntersectionPoints.AddRange(
                    CustomRoadBuilder.GetCollisionPoints(
                        origin: segmentBounds[1],
                        end: segmentBounds[3],
                        overalppingColliders: partialOverlaps));
                rightIntersectionPoints.AddRange(
                    CustomRoadBuilder.GetCollisionPoints(
                        origin: segmentBounds[3],
                        end: segmentBounds[1],
                        overalppingColliders: partialOverlaps));
            }

            if (sideOfTheRoad == 2)
            {
                return new Vector3[][]{
                    leftIntersectionPoints
                    .OrderBy(e=> (segmentBounds[0]-e).magnitude).ToArray(),
                    rightIntersectionPoints
                    .OrderBy(e=> (segmentBounds[1]-e).magnitude).ToArray()
                };
            }
            else if (sideOfTheRoad == 0)
            {
                return new Vector3[][]{
                    leftIntersectionPoints
                    .OrderBy(e=> (segmentBounds[0]-e).magnitude).ToArray()
                };
            }
            else
            {
                return new Vector3[][]{
                    rightIntersectionPoints
                    .OrderBy(e=> (segmentBounds[1]-e).magnitude).ToArray()
                };
            }
        }
        return new Vector3[][] { };
    }

    private static List<Collider> GetPartialOverlaps(
        string roadName,
        CustomRoadSegment segment)
    {
        Vector3[] segmentBounds = segment.TopPlane;
        Collider[] overlaps = Physics.OverlapBox(
            center: segment.SegmentObject.transform.position,
            halfExtents: segment.SegmentObject.transform.localScale / 2,
            orientation: segment.SegmentObject.transform.rotation,
            layerMask: LayerMask.GetMask("RoadEdgeLaneSegment"));
        List<Collider> partialOverlaps = new();

        foreach (Collider collider in overlaps)
        {
            string colliderGameObjectName = collider.gameObject.name;

            if (BuiltRoadSegments.ContainsKey(colliderGameObjectName)
                && !BuiltRoadSegments[colliderGameObjectName]
                    .ParentLane.ParentRoad.Name.Equals(roadName)
                && !CustomRoadBuilder.IsColliderWithinbounds(collider, segmentBounds))
            {
                partialOverlaps.Add(collider);
            }
        }
        return partialOverlaps;

    }

    private static List<Vector3> GetCollisionPoints(
        Vector3 origin,
        Vector3 end,
        List<Collider> overalppingColliders)
    {

        List<Vector3> collisionPoints = new();
        foreach (Collider collider in overalppingColliders)
        {
            Vector3[] colliderBounds =
                BuiltRoadSegments[collider.gameObject.name].TopPlane;

            Vector3 direction = end - origin;
            if (collider.Raycast(
                    ray: new Ray(origin, direction),
                    hitInfo: out RaycastHit rayHitInfo,
                    maxDistance: direction.magnitude)
                && !IsPointOnLineSegment(
                        rayHitInfo.point,
                        colliderBounds[0],
                        colliderBounds[1])
                && !IsPointOnLineSegment(
                        rayHitInfo.point,
                        colliderBounds[2],
                        colliderBounds[3]))
            {
                collisionPoints.Add(rayHitInfo.point);
                CustomRenderer.RenderSphere(rayHitInfo.point, color: UnityEngine.Color.green);
            }
        }
        return collisionPoints;
    }

    private static bool IsColliderWithinbounds(Collider collider, Vector3[] bounds)
    {
        Vector3[] colliderBounds = BuiltRoadSegments[collider.gameObject.name].TopPlane;
        return colliderBounds.Length > 0 && IsRectWithinBounds(colliderBounds, bounds);
    }

    private static bool IsRectWithinBounds(Vector3[] rectangle, Vector3[] bounds)
    {
        for (int i = 0; i < 4; i++)
            if (!IsPointInsideBounds(rectangle[i], bounds))
                return false;
        return true;
    }

    private static bool IsPointOnLineSegment(Vector3 point, Vector3 start, Vector3 end)
    {
        return Math.Round(Vector3.Cross(point - start, end - point).magnitude, 2) == 0;

    }
    private static bool IsPointInsideBounds(Vector3 point, Vector3[] bounds, float margin = 5)
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

    private static Vector3[] GetLeftParallelLine(
        Vector3[] vertices,
        float distance)
    {
        return GetParallelLines(vertices, distance)[0];
    }

    private static Vector3[] GetRightParallelLine(
        Vector3[] vertices,
        float distance)
    {
        return GetParallelLines(vertices, distance)[1];
    }

    private static Vector3[][] GetParallelLines(
        Vector3[] vertices,
        float distance)
    {
        Vector3[] leftLine = new Vector3[vertices.Length];
        Vector3[] rightLine = new Vector3[vertices.Length];
        Vector3[] parallelPoints = new Vector3[2];
        for (int i = 1; i < vertices.Length; i++)
        {
            parallelPoints = GetParallelPoints(
                originPoint: vertices[i - 1],
                targetPoint: vertices[i],
                distance: distance);

            leftLine[i - 1] = parallelPoints[0];
            rightLine[i - 1] = parallelPoints[1];
            if (i == vertices.Length - 1)
            {
                parallelPoints = GetParallelPoints(
                    originPoint: vertices[i],
                    targetPoint: vertices[i - 1],
                    distance: distance);
                leftLine[i] = parallelPoints[1];
                rightLine[i] = parallelPoints[0];
            }
        }
        return new Vector3[][] { leftLine, rightLine };
    }

    private static Vector3 GetUpVector(
        Vector3 start,
        Vector3 end)
    {
        Vector3 forward = start - end;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        return Vector3.Cross(forward, right).normalized;
    }

    private static Vector3[] GetParallelPoints(
        Vector3 originPoint,
        Vector3 targetPoint,
        float distance)
    {
        Vector3 forwardVector = targetPoint - originPoint;
        Vector3 upVector = GetUpVector(originPoint, targetPoint);
        Vector3 leftVector = Vector3.Cross(forwardVector, upVector).normalized;
        var leftPoint = originPoint + (leftVector * distance);
        var rightPoint = originPoint - (leftVector * distance);
        return new Vector3[] { leftPoint, rightPoint };
    }

    public class CustomRoad
    {
        public string Name;
        public float Width;
        public float Height;
        public float SidewalkHeight;
        public int VertexCount;
        public int NumberOfLanes;
        public bool IsCurved = true;
        public bool HasBusLane = false;
        public CustomRoadLane[] Lanes;
        public CustomRoadLane[] Sidewalks;
        public GameObject RoadObject;

        public CustomRoad(
            string name,
            bool isCurved,
            bool hasBusLane,
            int numberOfLanes,
            float height,
            float sidewalkHeight)
        {
            this.Name = name;
            this.IsCurved = isCurved;
            this.NumberOfLanes = numberOfLanes;
            this.Width = numberOfLanes * CustomRoadBuilder.RoadLaneWidth;
            this.Height = height;
            this.SidewalkHeight = sidewalkHeight;
            this.HasBusLane = hasBusLane && numberOfLanes > 1;

            this.RoadObject =
                CommonController.FindGameObject(name, true)
                    ?? new GameObject("Road" + BuiltRoads.Count);
            this.RoadObject.transform.localScale = Vector3.zero;
            this.RoadObject.transform.SetParent(BuiltRoadsParent.transform);

            if (!this.IsCurved)
                VertexCount = 4;

            CustomRoadBuilder.RepositionControlObjects(isCurved);
            this.Build(forceBuild: true, touchPosition: Vector2.zero);
        }

        public void Build(
            bool forceBuild,
            Vector2 touchPosition)
        {
            if (forceBuild || this.IsBuildRequired(touchPosition))
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

                Vector3[] centerVertices = CurvedLine.FindBazierLinePoints(controlPoints);

                this.Lanes =
                    GetLanes(
                        leftMostVertices:
                            GetLeftParallelLine(
                                vertices: centerVertices,
                                distance: 0.5f * this.Width)
                            );
                this.Sidewalks = this.GetSideWalks(centerVertices, this.NumberOfLanes);

                this.RenderRoadLines(
                    leftSegments: this.Lanes[0].Segments,
                    rightSegments: this.Lanes[^1].Segments);
            }
        }

        private CustomRoadLane[] GetLanes(Vector3[] leftMostVertices)
        {
            CustomRoadLane[] lanes = new CustomRoadLane[this.NumberOfLanes];
            for (int laneIndex = 0; laneIndex < this.NumberOfLanes; laneIndex++)
            {
                float distanceFromLeftMostLane =
                    (2 * laneIndex + 1) / 2f * CustomRoadBuilder.RoadLaneWidth;
                string laneName = this.Name + "Lane" + laneIndex;
                CustomRoadSegment[] roadSegments =
                    GetRoadSegments(
                        laneName: laneName,
                        width: CustomRoadBuilder.RoadLaneWidth,
                        height: CustomRoadBuilder.RoadLaneHeight,
                        centerVertices:
                            GetRightParallelLine(
                                vertices: leftMostVertices,
                                distance: distanceFromLeftMostLane));
                CustomRoadLane newLane =
                new(
                    name: laneName,
                    isEdgeLane: laneIndex == 0 || laneIndex == this.NumberOfLanes - 1,
                    parentRoad: this,
                    segments: roadSegments);
                lanes[laneIndex] = newLane;
            }
            return lanes;
        }

        private CustomRoadLane[] GetSideWalks(Vector3[] centerVertices, int lanesBuilt)
        {
            CustomRoadLane[] lanes = new CustomRoadLane[2];

            float distanceToSideWalkCenter =
                (this.NumberOfLanes + 1) * (0.5f * CustomRoadBuilder.RoadLaneWidth);

            Vector3[][] sideWalkCenterVertices =
                GetParallelLines(
                    vertices: centerVertices,
                    distance: distanceToSideWalkCenter);

            CustomRoadSegment[] leftSideWalkSegments =
                GetRoadSegments(
                    laneName: this.Name + "Lane" + lanesBuilt,
                    width: CustomRoadBuilder.RoadLaneWidth,
                    height: this.SidewalkHeight,
                    centerVertices: sideWalkCenterVertices[0]);

            CustomRoadSegment[] rightSideWalkSegments =
                GetRoadSegments(
                    laneName: this.Name + "Lane" + (lanesBuilt + 1).ToString(),
                    CustomRoadBuilder.RoadLaneWidth,
                    this.SidewalkHeight,
                    centerVertices: sideWalkCenterVertices[1]);

            lanes[0] = new(name: this.Name, isEdgeLane: false, parentRoad: this, segments: leftSideWalkSegments);
            lanes[1] = new(name: this.Name, isEdgeLane: false, parentRoad: this, segments: rightSideWalkSegments);
            return lanes;
        }

        private CustomRoadSegment[] GetRoadSegments(string laneName, float width, float height, Vector3[] centerVertices)
        {
            CustomRoadSegment[] segments = new CustomRoadSegment[centerVertices.Length - 1];
            for (int vertexIndex = 0; vertexIndex < centerVertices.Length - 1; vertexIndex++)
            {
                string roadSegmentName =
                String.Concat(
                    laneName,
                    "Segment",
                    (CustomRoadBuilder.BuiltRoadSegments.Count + vertexIndex).ToString());
                Vector3 currVertex = centerVertices[vertexIndex];
                Vector3 nextVertex = centerVertices[vertexIndex + 1];
                Vector3 secondNextVertex =
                    centerVertices[
                        vertexIndex == centerVertices.Length - 2
                        ? vertexIndex + 1 : vertexIndex + 2];

                CustomRoadSegment newSegment = new(
                            name: roadSegmentName,
                            centerStart: currVertex,
                            centerEnd: nextVertex,
                            nextCenterEnd: secondNextVertex,
                            width: width,
                            height: height,
                            renderSegment: false);
                segments[vertexIndex] = newSegment;
            }
            return segments;
        }

        private bool IsBuildRequired(Vector2 touchPosition)
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
            this.RenderLanes();
            this.RenderSidewalks();
            this.RenderIntersections(
                leftSegments: this.Lanes[0].Segments,
                rightSegments: this.Lanes[^1].Segments);
            BuiltRoads[this.Name] = this;
        }

        private void RenderLanes()
        {
            for (int laneIndex = 0; laneIndex < this.Lanes.Length; laneIndex++)
            {
                RenderLane(this.Lanes[laneIndex].Segments);
            }
        }

        private void RenderSidewalks()
        {
            for (int laneIndex = 0; laneIndex < this.Sidewalks.Length; laneIndex++)
            {
                RenderLane(this.Sidewalks[laneIndex].Segments);
            }
        }
        private void RenderLane(CustomRoadSegment[] segments)
        {
            for (int segmentIndex = 0; segmentIndex < segments.Length; segmentIndex++)
            {
                CustomRoadSegment segment = segments[segmentIndex];
                BuiltRoadSegments[segment.Name] = segment;
                segment.InitSegmentObject();
            }
        }

        public void RenderRoadLines(CustomRoadSegment[] leftSegments, CustomRoadSegment[] rightSegments)
        {
            Vector3[] leftVertices = new Vector3[leftSegments.Length];
            Vector3[] rightVertices = new Vector3[leftSegments.Length];
            for (int i = 0; i < leftSegments.Length; i++)
            {
                leftVertices[i] = leftSegments[i].TopPlane[0];
                leftVertices[i] = leftSegments[i].TopPlane[2];
                rightVertices[i] = rightSegments[i].TopPlane[1];
                rightVertices[i] = rightSegments[i].TopPlane[3];
            }

            LeftLineObject =
            CustomRenderer.RenderLine(
                name: "RoadLeftEdge",
                color: UnityEngine.Color.yellow,
                width: this.Width,
                pointSize: 0.5f,
                parentTransform: RoadControlsParent.transform,
                renderPoints: false,
                linePoints: leftVertices);

            RightLineObject =
            CustomRenderer.RenderLine(
                name: "RoadRightEdge",
                color: UnityEngine.Color.red,
                width: this.Width,
                pointSize: 0.5f,
                parentTransform: RoadControlsParent.transform,
                renderPoints: false,
                linePoints: rightVertices);
        }

        public bool RenderIntersections(CustomRoadSegment[] leftSegments, CustomRoadSegment[] rightSegments)
        {
            int activeRoadSegmentCount = leftSegments.Length;
            for (int segmentIndex = 0; segmentIndex < activeRoadSegmentCount; segmentIndex++)
            {
                CustomRoadSegment leftSegment = leftSegments[segmentIndex];
                CustomRoadSegment rightSegment = rightSegments[segmentIndex];
                Vector3[][] leftIntersectionPoints =
                    CustomRoadBuilder.GetIntersectionPoints(
                        roadName: this.Name,
                        segment: leftSegment,
                        sideOfTheRoad: 0);
                Vector3[][] rightIntersectionPoints =
                    CustomRoadBuilder.GetIntersectionPoints(
                        roadName: this.Name,
                        segment: rightSegment,
                        sideOfTheRoad: 1);
                if (leftIntersectionPoints.Length != rightIntersectionPoints.Length)
                    return false;
            }
            return true;
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

    public class CustomRoadLane
    {
        public string Name;
        public bool IsEdgeLane;
        public CustomRoadSegment[] Segments;
        public GameObject LaneObject;
        public CustomRoad ParentRoad;

        public CustomRoadLane(string name, bool isEdgeLane, CustomRoad parentRoad, CustomRoadSegment[] segments)
        {
            this.Name = name;
            this.IsEdgeLane = isEdgeLane;
            this.ParentRoad = parentRoad;
            this.Segments = segments;
            InitLaneObject(segments[segments.Length > 2 ? segments.Length / 2 - 1 : 0].Center);
            AssignParentToSegments();
        }

        public void AssignParentToSegments()
        {
            for (int i = 0; i < this.Segments.Length; i++)
            {
                this.Segments[i].ParentLane = this;
            }
        }

        public void InitLaneObject(Vector3 position)
        {
            GameObject laneObject =
                CommonController.FindGameObject(this.Name, true)
                ?? new GameObject();

            laneObject.name = this.Name;
            laneObject.transform.position = position;
            laneObject.transform.SetParent(this.ParentRoad.RoadObject.transform);
            this.LaneObject = laneObject;
        }
    }

    public class CustomRoadSegment
    {
        public string Name;
        public Vector3[] TopPlane;
        public Vector3[] BottomPlane;
        public Vector3 Center;
        public Vector3 Forward;
        public Vector3 Up;
        public float Width;
        public float Height;
        public float Length;
        public Vector3 CenterStart;
        public Vector3 CenterEnd;
        public Vector3 NextCenterEnd;
        public GameObject SegmentObject;
        public CustomRoadLane ParentLane;

        public CustomRoadSegment(
             string name,
             float width,
             float height,
             Vector3 centerStart,
             Vector3 centerEnd,
             Vector3 nextCenterEnd,
             bool renderSegment)
        {
            this.Name = name;
            this.Width = width;
            this.Height = height;
            this.CenterStart = centerStart;
            this.CenterEnd = centerEnd;
            this.NextCenterEnd = nextCenterEnd;

            this.Up = CustomRoadBuilder.GetUpVector(
                start: centerStart,
                end: centerEnd);

            float extension = GetSegmentExtension(
                width: width,
                centerStart: centerStart,
                centerEnd: centerEnd,
                nextCenterEnd: nextCenterEnd);

            this.Forward = this.CenterEnd - this.CenterStart;
            if (extension > 0)
                this.CenterEnd += this.Forward.normalized * extension;

            this.Center = GetSegmentCenter(
                height: height,
                centerStart: this.CenterStart,
                centerEnd: this.CenterEnd,
                up: this.Up);

            this.Forward = this.CenterEnd - this.CenterStart;
            this.Length = this.Forward.magnitude;

            Vector3[][] planes =
                this.GetPlanes(
                    width: this.Width,
                    height: this.Height,
                    centerStart: this.CenterStart,
                    centerEnd: this.CenterEnd,
                    up: this.Up);

            this.TopPlane = planes[0];
            this.BottomPlane = planes[1];
            if (renderSegment)
                this.InitSegmentObject();
        }

        private Vector3 GetSegmentCenter(
            float height,
            Vector3 centerStart,
            Vector3 centerEnd,
            Vector3 up)
        {
            return centerStart + (centerEnd - centerStart) / 2 + 0.5f * height * up;
        }

        private float GetSegmentExtension(
            float width,
            Vector3 centerStart,
            Vector3 centerEnd,
            Vector3 nextCenterEnd)
        {

            if (nextCenterEnd.Equals(centerEnd))
                return 0;
            else
            {
                float angleBetweenSegments =
                 Math.Abs(
                    Vector3.Angle(
                        nextCenterEnd - centerEnd,
                        centerEnd - centerStart));
                if (angleBetweenSegments > 180)
                    angleBetweenSegments -= 180;

                return
                    width / 2
                    * Math.Abs(
                        MathF.Sin(
                            angleBetweenSegments * MathF.PI / 180f));
            }
        }

        private Vector3[][] GetPlanes(
            float width,
            float height,
            Vector3 centerStart,
            Vector3 centerEnd,
            Vector3 up)
        {
            Vector3[] startPoints = CustomRoadBuilder.GetParallelPoints(
                    originPoint: centerStart,
                    targetPoint: centerEnd,
                    distance: width * 0.5f);
            Vector3[] endPoints = CustomRoadBuilder.GetParallelPoints(
                    originPoint: centerEnd,
                    targetPoint: centerStart,
                    distance: width * 0.5f);
            Vector3[] topPlane = new Vector3[4];
            topPlane[0] = startPoints[0];
            topPlane[1] = startPoints[1];
            topPlane[2] = endPoints[1];
            topPlane[3] = endPoints[0];
            Vector3[] bottomPlane = (Vector3[])topPlane.Clone();

            for (int i = 0; i < topPlane.Length; i++)
            {
                topPlane[i] += 0.5f * height * up;
                bottomPlane[i] -= 0.5f * height * up;
            }
            Debug.Log("centerStart=" + centerStart);
            Debug.Log("centerEnd=" + centerEnd);
            Debug.Log("topPlane[0]=" + topPlane[0]);
            Debug.Log("topPlane[1]=" + topPlane[1]);
            Debug.Log("topPlane[2]=" + topPlane[2]);
            Debug.Log("topPlane[3]=" + topPlane[3]);
            return new Vector3[][] { topPlane, bottomPlane };
        }

        public void InitSegmentObject()
        {
            GameObject segmentObject =
                CommonController.FindGameObject(this.Name, true)
                ?? GameObject.CreatePrimitive(PrimitiveType.Cube);
            var headingChange =
                Quaternion.FromToRotation(
                    segmentObject.transform.forward,
                    this.Forward);

            segmentObject.name = this.Name;
            segmentObject.transform.position = this.Center;
            segmentObject.transform.localScale = new Vector3(this.Width, this.Height, this.Length);
            segmentObject.transform.localRotation *= headingChange;
            segmentObject.transform.SetParent(BuiltRoadSegmentsParent.transform);
            segmentObject.layer = LayerMask.NameToLayer("RoadEdgeLaneSegment");

            // AddBoxCollider(
            //     width: this.Width,
            //     height: this.Height,
            //     length: this.Length,
            //     center: Vector3.zero,
            //     segmentObject: segmentObject);

            // RenderMesh(
            //     topPlane: this.TopPlane,
            //     bottomPlane: this.BottomPlane,
            //     segment: segmentObject);

            this.SegmentObject = segmentObject;
        }

        private void AddBoxCollider(
            Vector3 center,
            GameObject segmentObject)
        {
            Vector3 updCenter = center;
            BoxCollider boxCollider = segmentObject.AddComponent<BoxCollider>();
            boxCollider.center = updCenter;
            boxCollider.size = new Vector3(1, 1, 2);
        }

        private void RenderMesh(Vector3[] topPlane, Vector3[] bottomPlane, GameObject segment)
        {
            Vector3[] allVertices = new Vector3[2 * bottomPlane.Length];

            for (int i = 0; i < topPlane.Length; i++)
            {
                allVertices[i] =
                    segment.transform.InverseTransformPoint(topPlane[i]);
                allVertices[i + topPlane.Length] =
                    segment.transform.InverseTransformPoint(bottomPlane[i]);
            }

            Mesh mesh = new() { name = "Generated" };
            segment.AddComponent<MeshRenderer>();
            mesh.vertices =
                new Vector3[]{
                    //Top
                    new (-0.5f,0.5f,1),
                    new (0.5f,0.5f,1),
                    new (0.5f,0.5f,-1),
                    new (-0.5f,0.5f,-1),
                    new (-0.5f,0.5f,-1),
                    new (-0.5f,-0.5f,1),
                    //Bottom
                    new (0.5f,-0.5f,1),
                    new (0.5f,-0.5f,-1),
                    new (-0.5f,-0.5f,-1),
                    new (-0.5f,-0.5f,-1),
                    };
            mesh.triangles = new int[] { 
                //Top
                0,1, 2,
                0, 2, 3, 
                //Bottom
                4,5,6,
                4,6,7
                };
            // mesh.vertices = allVertices;

            // mesh.triangles = new int[] { 
            //     //Top
            //     1,0,2,
            //     1,2,3, 
            //     //Bottom
            //     5,4,6,
            //     5,6,7,
            //     //Left Side
            //     6,2,0,
            //     6,0,4,
            //     //Right Side
            //     5,1,3,
            //     5,3,7,
            //     //Front Side
            //     4,0,1,
            //     4,1,5,
            //     //Back Side
            //     7,3,2,
            //     7,2,6,
            // };
            segment.AddComponent<MeshFilter>().mesh = mesh;
            mesh.RecalculateBounds();
        }
    }
}

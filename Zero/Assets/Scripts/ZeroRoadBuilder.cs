using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class ZeroRoadBuilder
{
    public static Material RoadSegmentMaterial;
    public static float RoadChangeAngleThreshold;
    public static float RoadChangeAngleMax;
    public static int RoadMaxVertexCount;
    public static int RoadMinVertexCount;
    public static float RoadSegmentMinLength;
    public static float RoadLaneHeight;
    public static float RoadLaneWidth;
    public static float RoadSideWalkHeight;
    public static float RoadCrossWalkLength;
    public static GameObject RoadControlsParent;
    public static GameObject BuiltRoadsParent;
    public static GameObject BuiltRoadSegmentsParent;
    public static GameObject BuiltIntersectionsParent;
    public static GameObject StartObject;
    public static GameObject ControlObject;
    public static GameObject EndObject;
    public static ZeroRoad ActiveRoad;
    public static Dictionary<string, ZeroRoadIntersection> ActiveIntersections = new();
    public static readonly Dictionary<string, Vector3> InitialStaticLocalScale = new();
    public static readonly Dictionary<string, ZeroRoad> BuiltRoadsByName = new();
    public static readonly Dictionary<string, List<ZeroRoadSegment>> BuiltRoadSegmentsByLane = new();
    public static readonly Dictionary<string, ZeroRoadSegment> BuiltRoadSegmentsByName = new();
    public static readonly Dictionary<string, ZeroRoadIntersection> BuiltRoadIntersections = new();
    public static string RoadStartObjectName = "RoadStart";
    public static string RoadControlObjectName = "RoadControl";
    public static string RoadEndObjectName = "RoadEnd";
    public static string RoadLeftEdgeObjectName = "RoadLeftEdge";
    public static string RoadRightEdgeObjectName = "RoadRightEdge";
    public static string RoadControlsObjectName = "RoadControls";
    public static string BuiltRoadsObjectName = "BuiltRoads";
    public static string BuiltRoadSegmentsObjectName = "BuiltRoadSegments";
    public static string BuiltIntersectionsObjectName = "BuiltIntersections";
    public static string RoadEdgeLaneMaskName = "RoadEdgeLaneMask";
    public static string RoadSidewalkMaskName = "RoadSidewalkMask";

    public static void Initialise()
    {
        InitialiseConfig();
        RoadControlsParent = new GameObject(RoadControlsObjectName);
        BuiltRoadsParent = new GameObject(BuiltRoadsObjectName);
        BuiltRoadSegmentsParent = new GameObject(BuiltRoadSegmentsObjectName);
        BuiltIntersectionsParent = new GameObject(BuiltIntersectionsObjectName);
        if (ZeroController.IsPlayMode)
            InitControlObjects(true);
    }
    private static void InitialiseConfig()
    {
        ZeroRoadBuilder.RoadChangeAngleThreshold = 10;
        ZeroRoadBuilder.RoadChangeAngleMax = 20;
        ZeroRoadBuilder.RoadMaxVertexCount = 30;
        ZeroRoadBuilder.RoadMinVertexCount = 6;
        ZeroRoadBuilder.RoadSegmentMinLength = 3;
        ZeroRoadBuilder.RoadLaneHeight = 0.02f;
        ZeroRoadBuilder.RoadLaneWidth = 3;
        ZeroRoadBuilder.RoadSideWalkHeight = 0.3f;
    }

    public static void InitControlObjects(bool isCurved)
    {
        StartObject = InitStaticObject(
            objectName: RoadStartObjectName,
             size: 2,
              color: new UnityEngine.Color(0.25f, 0.35f, 0.30f));
        if (isCurved)
            ControlObject = InitStaticObject(
                objectName: RoadControlObjectName,
                size: 2,
                color: new UnityEngine.Color(0, 1, 0.20f));
        EndObject = InitStaticObject(
            objectName: RoadEndObjectName,
            size: 2,
            color: new UnityEngine.Color(0.70f, 0.45f, 0f));
        HideControlObjects();
    }

    public static GameObject InitStaticObject(
        string objectName,
        float size,
        UnityEngine.Color? color)
    {
        GameObject cylinderObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinderObject.name = objectName;
        cylinderObject.transform.localScale = new Vector3(size, 0.5f, size);
        cylinderObject.transform.position = Vector3.zero;
        var renderer = cylinderObject.GetComponent<Renderer>();
        renderer.material.color = color ?? UnityEngine.Color.yellow;

        cylinderObject.transform.SetParent(RoadControlsParent.transform);
        InitialStaticLocalScale.Add(cylinderObject.name, cylinderObject.transform.localScale);
        return cylinderObject;
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

    public static void StartBuilding(bool isCurved)
    {
        ActiveRoad = new ZeroRoad(
            isCurved: isCurved,
            hasBusLane: true,
            numberOfLanesExclSidewalks: 2,
            height: ZeroRoadBuilder.RoadLaneHeight,
            sidewalkHeight: ZeroRoadBuilder.RoadSideWalkHeight,
            controlPoints: ZeroRoadBuilder.ResetControlObjects(isCurved));
    }


    public static void CancelBuilding()
    {
        HideControlObjects();
        ZeroRoadBuilder.ActiveIntersections.Clear();
        ActiveRoad?.Hide();
        ActiveRoad = null;
    }

    public static void ConfirmBuilding()
    {
        ActiveRoad.LogRoadPositions();
        foreach (var intersectionTempName in ZeroRoadBuilder.ActiveIntersections.Keys)
        {
            ZeroRoadIntersection intersection = ZeroRoadBuilder.ActiveIntersections[intersectionTempName];
            intersection.Name = "RI" + ZeroRoadBuilder.BuiltRoadIntersections.Count();
            ZeroRoadBuilder.BuiltRoadIntersections[intersectionTempName] = intersection;
        }
        ZeroRoadBuilder.ActiveIntersections.Clear();
        HideControlObjects();
        ActiveRoad = null;
    }

    public static void HideControlObjects()
    {
        StartObject.SetActive(false);
        ControlObject.SetActive(false);
        EndObject.SetActive(false);
    }


    public static Vector3[] ResetControlObjects(bool isCurved)
    {
        List<Vector3> controlPoints = new();


        Vector3 startPosition =
            ZeroCameraMovement
            .GetTerrainHitPoint(GetScreenCenterPoint());
        startPosition.y = 0;
        StartObject.transform.position = startPosition;
        StartObject.SetActive(true);
        controlPoints.Add(startPosition);

        if (isCurved)
        {
            Vector3 curveControlPosition = InitCurveControlPosition(isCurved);
            ControlObject.transform.position = curveControlPosition;
            controlPoints.Add(curveControlPosition);
            ControlObject.SetActive(true);
        }
        else
            ControlObject.SetActive(false);

        Vector3 endPosition =
            startPosition + 20f * ZeroCameraMovement.MainCameraRoot.transform.right;
        EndObject.transform.position = endPosition;
        EndObject.SetActive(true);
        controlPoints.Add(endPosition);

        return controlPoints.ToArray();
    }


    public static void HandleControlDrag(bool isCurved, Vector2 touchPosition)
    {
        List<Vector3> controlPoints = new();

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            var roadStartChanged =
                !ZeroRoadBuilder.StartObject.transform.position.Equals(Vector3.zero)
                && ZeroUIHandler.HandleGameObjectDrag(ZeroRoadBuilder.StartObject, touchPosition);

            var roadControlChanged =
                isCurved
                && !ZeroRoadBuilder.ControlObject.transform.position.Equals(Vector3.zero)
                && ZeroUIHandler.HandleGameObjectDrag(ZeroRoadBuilder.ControlObject, touchPosition);

            var roadEndChanged =
                !ZeroRoadBuilder.EndObject.transform.position.Equals(Vector3.zero)
                && ZeroUIHandler.HandleGameObjectDrag(ZeroRoadBuilder.EndObject, touchPosition);

            controlPoints.Add(ZeroRoadBuilder.StartObject.transform.position);

            if (isCurved)
                controlPoints.Add(ZeroRoadBuilder.ControlObject.transform.position);
            controlPoints.Add(ZeroRoadBuilder.EndObject.transform.position);


            if (roadStartChanged || roadControlChanged || roadEndChanged)
                ZeroRoadBuilder.ActiveRoad.Build(controlPoints.ToArray());
        }
    }

    // public class CustomRoadTIntersection
    // {
    //     public ZeroRoadLaneIntersection StartSidewalkOverlap;
    //     public ZeroRoadLaneIntersection EndSidewalkOverlap;
    //     public CustomRoadTIntersection(
    //         ZeroRoadLaneIntersection startSidewalkOverlap,
    //         ZeroRoadLaneIntersection endSidewalkOverlap)
    //     {
    //         this.StartSidewalkOverlap = startSidewalkOverlap;
    //         this.EndSidewalkOverlap = endSidewalkOverlap;
    //     }
    // }
    // public class CustomRoadXIntersectionGrid
    // {
    //     public ZeroParallelogram[] LeftStart;
    //     public ZeroParallelogram[] LeftCenter;
    //     public ZeroParallelogram[] LeftEnd;
    //     public ZeroParallelogram[] CenterStart;
    //     public ZeroParallelogram[] Center;
    //     public ZeroParallelogram[] CenterEnd;
    //     public ZeroParallelogram[] RightStart;
    //     public ZeroParallelogram[] RightCenter;
    //     public ZeroParallelogram[] RightEnd;

    //     public CustomRoadXIntersectionGrid(
    //         ZeroParallelogram[] leftStart,
    //         ZeroParallelogram[] leftCenter,
    //         ZeroParallelogram[] leftEnd,
    //         ZeroParallelogram[] centerStart,
    //         ZeroParallelogram[] center,
    //         ZeroParallelogram[] centerEnd,
    //         ZeroParallelogram[] rightStart,
    //         ZeroParallelogram[] rightCenter,
    //         ZeroParallelogram[] rightEnd
    //     )
    //     {
    //         this.LeftStart = leftStart;
    //         this.LeftCenter = leftCenter;
    //         this.LeftEnd = leftEnd;
    //         this.CenterStart = centerStart;
    //         this.Center = center;
    //         this.CenterEnd = centerEnd;
    //         this.RightStart = rightStart;
    //         this.RightCenter = rightCenter;
    //         this.RightEnd = rightEnd;
    //     }
    // }

    public static Vector2 GetScreenCenterPoint()
    {
        return new Vector2(Screen.width / 2, Screen.height / 2);
    }


}

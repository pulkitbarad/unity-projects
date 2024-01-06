using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ZeroRoadBuilder
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
    public static ZeroRoad CurrentActiveRoad;
    public static readonly Dictionary<string, Vector3> InitialStaticLocalScale = new();
    public static readonly Dictionary<string, ZeroRoad> BuiltRoads = new();
    public static readonly Dictionary<string, ZeroRoadSegment> BuiltRoadSegments = new();
    // public static readonly Dictionary<string, CustomRoadTIntersection> BuiltTIntersections = new();
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
        RoadControlsParent = new GameObject(RoadControlsObjectName);
        BuiltRoadsParent = new GameObject(BuiltRoadsObjectName);
        BuiltRoadSegmentsParent = new GameObject(BuiltRoadSegmentsObjectName);
        BuiltIntersectionsParent = new GameObject(BuiltIntersectionsObjectName);
        InitControlObjects(true);
        HideControlObjects();

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

    }

    public static GameObject InitStaticObject(
        string objectName,
        float size,
        UnityEngine.Color? color)
    {

        GameObject gameObject = ZeroObjectManager.GetNewObject(objectName, ZeroObjectManager.PoolType.STATIC_CYLINDER);

        if (gameObject == null)
        {
            gameObject =
            ZeroRenderer.RenderCylinder(
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
    }

    public static void HideControlObjects()
    {
        StartObject.SetActive(false);
        ControlObject.SetActive(false);
        EndObject.SetActive(false);
    }

    public static void StartBuilding(bool isCurved)
    {
        HideControlObjects();
        CurrentActiveRoad?.Hide();
        CurrentActiveRoad = new ZeroRoad(
            isCurved: isCurved,
            hasBusLane: true,
            numberOfLanes: 2,
            height: ZeroRoadBuilder.RoadLaneHeight,
            sidewalkHeight: ZeroRoadBuilder.RoadSideWalkHeight);
        ShowControlObjects(isCurved);
    }

    public static void ConfirmBuilding()
    {
        HideControlObjects();
        CurrentActiveRoad = null;
    }
    public static void CancelBuilding()
    {
        CurrentActiveRoad.Hide();
        CurrentActiveRoad = null;
        HideControlObjects();
    }

    public static void RepositionControlObjects(bool isCurved)
    {
        Vector3 startPosition =
            ZeroCameraMovement
            .GetTerrainHitPoint(GetScreenCenterPoint());

        StartObject.transform.position = startPosition;
        EndObject.transform.position =
            startPosition + 20f * ZeroCameraMovement.MainCameraRoot.transform.right;
        if (isCurved)
        {
            ControlObject.transform.position = InitCurveControlPosition(isCurved);
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

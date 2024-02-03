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
    public static float RoadMinimumLength;
    public static GameObject RoadControlsParent;
    public static GameObject BuiltRoadsParent;
    public static GameObject BuiltRoadSegmentsParent;
    public static GameObject BuiltIntersectionsParent;
    public static GameObject StartObject;
    public static GameObject ControlObject;
    public static GameObject EndObject;
    public static ZeroRoad ActivePrimaryRoad;
    public static Dictionary<string, ZeroRoad> ActiveSecondaryRoads;
    public static Dictionary<string, ZeroRoadIntersection> ActiveIntersections;
    public static Dictionary<string, Vector3> InitialStaticLocalScale;
    public static Dictionary<string, ZeroRoad> BuiltRoadsByName;
    public static Dictionary<string, List<ZeroRoadSegment>> BuiltRoadSegmentsByLane;
    public static Dictionary<string, ZeroRoadSegment> BuiltRoadSegmentsByName;
    public static Dictionary<string, ZeroRoadIntersection> BuiltRoadIntersections;
    public static string RoadStartObjectName = "RoadStart";
    public static string RoadControlObjectName = "RoadControl";
    public static string RoadEndObjectName = "RoadEnd";
    public static string RoadLeftEdgeObjectName = "RoadLeftEdge";
    public static string RoadRightEdgeObjectName = "RoadRightEdge";
    public static string RoadControlsObjectName = "RoadControls";
    public static string BuiltRoadsObjectName = "BuiltRoads";
    public static string BuiltRoadSegmentsObjectName = "BuiltRoadSegments";
    public static string BuiltIntersectionsObjectName = "BuiltIntersections";
    public static string RoadLaneMaskName = "RoadLaneMask";
    public static string RoadSidewalkMaskName = "RoadSidewalkMask";

    public static void Initialise()
    {
        ActiveIntersections = new();
        InitialStaticLocalScale = new();
        BuiltRoadsByName = new();
        BuiltRoadSegmentsByLane = new();
        BuiltRoadSegmentsByName = new();
        BuiltRoadIntersections = new();
        ActiveSecondaryRoads = new();

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
        RoadChangeAngleThreshold = 10;
        RoadChangeAngleMax = 20;
        RoadMaxVertexCount = 30;
        RoadMinVertexCount = 6;
        RoadSegmentMinLength = 3;
        RoadLaneHeight = 0.02f;
        RoadLaneWidth = 3;
        RoadSideWalkHeight = 0.3f;
        RoadMinimumLength = 1f;
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
        renderer.material.color = color ?? Color.yellow;

        cylinderObject.transform.SetParent(RoadControlsParent.transform);
        InitialStaticLocalScale.Add(cylinderObject.name, cylinderObject.transform.localScale);
        return cylinderObject;
    }

    private static Vector3 InitCurveControlPosition(bool isCurved)
    {

        Vector3 startPosition = StartObject.transform.position;
        var startToEnd = EndObject.transform.position - startPosition;
        Vector3 scaledStartToEnd = 0.5f * startToEnd;

        if (isCurved)
            return startPosition + Quaternion.AngleAxis(-45, Vector3.up) * scaledStartToEnd;
        else
            return startPosition + scaledStartToEnd;
    }

    public static void StartBuilding(bool isCurved, Vector3[] controlPoints)
    {
        ResetActiveRoadConfig(hideActiveRoad: false);
        ActivePrimaryRoad = new ZeroRoad(
            isCurved: isCurved,
            hasBusLane: true,
            numberOfLanesExclSidewalks: 2,
            height: RoadLaneHeight,
            sidewalkHeight: RoadSideWalkHeight,
            forceSyncTransform: false,
            controlPoints: controlPoints);
    }


    private static void ResetActiveRoadConfig(bool hideActiveRoad)
    {
        ActiveIntersections.Clear();
        if (hideActiveRoad)
            ActivePrimaryRoad?.Hide();
        foreach (var newSecondaryRoad in ActiveSecondaryRoads.Values)
            newSecondaryRoad.Hide();
        ActivePrimaryRoad = null;
        ActiveSecondaryRoads = new();
    }
    public static void CancelBuilding()
    {
        HideControlObjects();
        ResetActiveRoadConfig(true);
    }

    public static void ConfirmBuilding()
    {
        BuiltRoadsByName[ActivePrimaryRoad.Name] = ActivePrimaryRoad;
        foreach (var intersectionTempName in ActiveIntersections.Keys)
            BuiltRoadIntersections[intersectionTempName] = ActiveIntersections[intersectionTempName];
        if (ZeroController.TestToGenerateData.Length > 0)
        {
            ZeroController.AppendToTestDataFile(ActivePrimaryRoad.GenerateTestData(ZeroController.TestToGenerateData));
            foreach (var newSecondaryRoad in ActiveSecondaryRoads.Values)
                ZeroController.AppendToTestDataFile(newSecondaryRoad.GenerateTestData(ZeroController.TestToGenerateData));
        }

        HideControlObjects();
        ResetActiveRoadConfig(false);
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
        Vector3 endPosition =
            startPosition + 50f * ZeroCameraMovement.MainCameraRoot.transform.right;
        EndObject.transform.position = endPosition;
        EndObject.SetActive(true);

        Vector3 curveControlPosition = Vector3.zero;
        if (isCurved)
        {
            curveControlPosition = InitCurveControlPosition(isCurved);
            ControlObject.transform.position = curveControlPosition;

            ControlObject.SetActive(true);
        }
        else
            ControlObject.SetActive(false);

        controlPoints.Add(startPosition);
        if (isCurved)
            controlPoints.Add(curveControlPosition);

        controlPoints.Add(endPosition);

        return controlPoints.ToArray();
    }


    public static void HandleControlDrag(Vector2 touchPosition)
    {
        List<Vector3> controlPoints = new();


        if (!EventSystem.current.IsPointerOverGameObject())
        {
            var roadStartChanged =
                !StartObject.transform.position.Equals(Vector3.zero)
                && ZeroUIHandler.HandleGameObjectDrag(StartObject, touchPosition);

            var roadControlChanged =

                ActivePrimaryRoad.IsCurved
                && !ControlObject.transform.position.Equals(Vector3.zero)
                && ZeroUIHandler.HandleGameObjectDrag(ControlObject, touchPosition);

            var roadEndChanged =
                !EndObject.transform.position.Equals(Vector3.zero)
                && ZeroUIHandler.HandleGameObjectDrag(EndObject, touchPosition);

            controlPoints.Add(StartObject.transform.position);

            if (ActivePrimaryRoad.IsCurved)
                controlPoints.Add(ControlObject.transform.position);
            controlPoints.Add(EndObject.transform.position);

            if (roadStartChanged || roadControlChanged || roadEndChanged)
                StartBuilding(ActivePrimaryRoad.IsCurved, controlPoints.ToArray());
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

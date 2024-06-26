using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ZeroRoadIntersection
{
    public string Name;
    public Vector3[][] Sidewalks;
    public Vector3[][] CrossWalks;
    public Vector3[][] MainSquare;
    public Vector3[][] RoadEdges;
    public Vector3[] EdgeMidpoints;
    public Vector3[] BranchDownVertices;
    public Vector3[] BranchUpVertices;
    public Vector3[] BranchLeftVertices;
    public Vector3[] BranchRightVertices;
    public ZeroLaneIntersection[] LaneIntersections;
    public Vector3[][] LaneIntersectionPoints;
    public ZeroRoad PrimaryRoad;
    public ZeroRoad IntersectingRoad;
    public float PrimaryRoadLengthSoFar;
    public float Height;
    public int RoadIntersectionType;
    public bool IsValid;
    public static int ROAD_INTERSESCTION_TYPE_LEFT = 0;
    public static int ROAD_INTERSESCTION_TYPE_UP = 1;
    public static int ROAD_INTERSESCTION_TYPE_RIGHT = 2;
    public static int ROAD_INTERSESCTION_TYPE_DOWN = 3;
    public static int ROAD_INTERSESCTION_TYPE_X = 4;

    public ZeroRoadIntersection(
        string name,
        float height,
        int roadIntersectionType,
        ZeroRoad primaryRoad,
        ZeroRoad intersectingRoad,
        ZeroLaneIntersection[] laneIntersections)
    {
        Name = name;
        LaneIntersections = laneIntersections;
        RoadIntersectionType = roadIntersectionType;
        LaneIntersectionPoints =
            LaneIntersections
            .Select(e =>
                e.CollisonPoints.Select(c => c.CollisionPoint).ToArray()).ToArray();
        Height = height;
        PrimaryRoadLengthSoFar = LaneIntersections[0].PrimaryRoadLengthSoFar;
        IsValid = false;
        PrimaryRoad = primaryRoad;
        IntersectingRoad = intersectingRoad;

        GetIntersectionModules();

    }

    private void GetIntersectionModules()
    {
        List<Vector3[]> sidewalks = new();
        List<Vector3[]> crosswalks = new();
        List<Vector3[]> mainSquares = new();
        List<Vector3[]> roadEdges = new();
        List<Vector3> edgeMidpoints = new();
        List<Vector3> middleSquare = new();

        int n = LaneIntersections.Length;
        for (int intersectionSideIndex = 0; intersectionSideIndex < n; intersectionSideIndex++)
        {
            GetLRE(
                intersectionSideIndex: intersectionSideIndex,
                n: n,
                L: out Vector3[] L,
                R: out Vector3[] R,
                E: out Vector3[] E,
                _L: out ZeroCollisionInfo[] _L,
                _R: out ZeroCollisionInfo[] _R);
            Vector3 edgeMidpoint = E[0] + 0.5f * (E[3] - E[0]);

            int leftIntersectionPosition =
                GetLeftIntersectionPosition(intersectionSideIndex);

            bool isBranchOppositeToRoad =
                (leftIntersectionPosition == 2)
                || ((leftIntersectionPosition == 1 || leftIntersectionPosition == 3)
                    && _L[0].CollidingSegment.ParentLane.IsRightSidewalk);

            PopulateBranchVertices(
               leftIntersectionPosition: leftIntersectionPosition,
               edgeMidPoint: edgeMidpoint,
               L0: L[0],
               R3: R[3],
               isBranchOppositeToRoad: isBranchOppositeToRoad,
               out IsValid);

            if (!IsValid) break;

            Vector3[] mainSquare = new Vector3[] { E[0], L[1], L[2], R[1], R[2], E[3] };
            Vector3[] crosswalk = new Vector3[] { L[3], L[2], R[1], R[0] };
            middleSquare.Add(L[2]);
            mainSquares.Add(mainSquare);
            crosswalks.Add(crosswalk);
            sidewalks.AddRange(
                GetSidewalks(
                    i: intersectionSideIndex,
                    L: L,
                    R: R,
                    E: E)
                .Where(e => e.Length > 2));
            roadEdges.Add(E);
            edgeMidpoints.Add(edgeMidpoint);

            //If there are only two lane intersections (it's a T intersection), 
            //then there should be only one iteration of this loop.
            if (n == 2) intersectionSideIndex++;
        }
        if (n == 4)
            mainSquares.Add(middleSquare.ToArray());
        //
        Sidewalks = sidewalks.ToArray();
        CrossWalks = crosswalks.ToArray();
        MainSquare = mainSquares.ToArray();
        RoadEdges = roadEdges.ToArray();
        EdgeMidpoints = edgeMidpoints.ToArray();
    }

    private void GetLRE(
        int intersectionSideIndex,
        int n,
        out Vector3[] L,
        out Vector3[] R,
        out Vector3[] E,
        out ZeroCollisionInfo[] _L,
        out ZeroCollisionInfo[] _R)
    {
        ZeroLaneIntersection leftIntersection =
            LaneIntersections[intersectionSideIndex];
        ZeroLaneIntersection rightIntersection =
            LaneIntersections[(intersectionSideIndex + n - 1) % n];
        L = new Vector3[4];
        R = new Vector3[4];
        E = new Vector3[4];
        _L = new ZeroCollisionInfo[4];
        _R = new ZeroCollisionInfo[4];

        int leftIntersectionPosition = GetLeftIntersectionPosition(intersectionSideIndex);
        for (int j = 0; j < 4; j++)
        {
            _L[j] = leftIntersection.CollisonPoints[(j + leftIntersectionPosition) % 4];
            _R[j] = rightIntersection.CollisonPoints[(j + leftIntersectionPosition) % 4];
            L[j] = _L[j].CollisionPoint;
            R[j] = _R[j].CollisionPoint;
        }
        Vector3[] _E;
        ZeroCollisionInfo leftCollisionInfo = _L[0];
        if (Vector3.Angle(L[1] - L[0], L[3] - L[0]) > 90)
        {
            _E = GetRoadEdgePoints(
             leftIntersectionPosition: leftIntersectionPosition,
             leftCollisionInfo: leftCollisionInfo,
             edgePointIndex: 1,
             edgePoint: L[3]
            );
            if (IsEdgeValid(L, R, _E))
            {
                E = _E;
                return;
            }
        }
        else
        {
            _E = GetRoadEdgePoints(
             leftIntersectionPosition: leftIntersectionPosition,
             leftCollisionInfo: leftCollisionInfo,
             edgePointIndex: 0,
             edgePoint: L[0]
            );
            if (IsEdgeValid(L, R, _E))
            {
                E = _E;
                return;
            }
        }
        if (Vector3.Angle(R[1] - R[0], R[3] - R[0]) > 90)
        {
            _E = GetRoadEdgePoints(
             leftIntersectionPosition: leftIntersectionPosition,
             leftCollisionInfo: leftCollisionInfo,
             edgePointIndex: 3,
             edgePoint: R[3]
            );
            if (IsEdgeValid(L, R, _E))
            {
                E = _E;
                return;
            }
        }
        else
        {
            _E = GetRoadEdgePoints(
              leftIntersectionPosition: leftIntersectionPosition,
              leftCollisionInfo: leftCollisionInfo,
              edgePointIndex: 2,
              edgePoint: R[0]
             );
            if (IsEdgeValid(L, R, _E))
            {
                E = _E;
                return;
            }
        }
    }

    private int GetLeftIntersectionPosition(int intersectionSideIndex)
    {
        if (RoadIntersectionType == ROAD_INTERSESCTION_TYPE_X)
            return intersectionSideIndex;
        else if (RoadIntersectionType == ROAD_INTERSESCTION_TYPE_LEFT)
            return 0;
        else if (RoadIntersectionType == ROAD_INTERSESCTION_TYPE_UP)
            return 1;
        else if (RoadIntersectionType == ROAD_INTERSESCTION_TYPE_RIGHT)
            return 2;
        // else if (this.RoadIntersectionType == INTERSESCTION_TYPE_T_DOWN)
        else
            return 3;
    }


    private void PopulateBranchVertices(
        int leftIntersectionPosition,
        Vector3 edgeMidPoint,
        Vector3 L0,
        Vector3 R3,
        bool isBranchOppositeToRoad,
        out bool isValid)
    {
        Vector3 __edgeMidPoint = edgeMidPoint - 0.5f * Height * Vector3.up;
        if (leftIntersectionPosition == 0)
            GetBranchVertices(
                edgeMidPoint: __edgeMidPoint,
                L0: L0,
                R3: R3,
                oldCenterVertices: PrimaryRoad.CenterVertices,
                isBranchOppositeToRoad: isBranchOppositeToRoad,
                newCenterVertices: out BranchLeftVertices,
                isIntersectionValid: out isValid);

        else if (leftIntersectionPosition == 2)
            GetBranchVertices(
                edgeMidPoint: __edgeMidPoint,
                L0: L0,
                R3: R3,
                oldCenterVertices: PrimaryRoad.CenterVertices,
                isBranchOppositeToRoad: isBranchOppositeToRoad,
                newCenterVertices: out BranchRightVertices,
                isIntersectionValid: out isValid);
        else if (leftIntersectionPosition == 1)
            GetBranchVertices(
                edgeMidPoint: __edgeMidPoint,
                L0: L0,
                R3: R3,
                oldCenterVertices: IntersectingRoad.CenterVertices,
                isBranchOppositeToRoad: isBranchOppositeToRoad,
                newCenterVertices: out BranchUpVertices,
                isIntersectionValid: out isValid);
        // else if (leftIntersectionPosition == 3)
        else
            GetBranchVertices(
                edgeMidPoint: __edgeMidPoint,
                L0: L0,
                R3: R3,
                oldCenterVertices: IntersectingRoad.CenterVertices,
                isBranchOppositeToRoad: isBranchOppositeToRoad,
                newCenterVertices: out BranchDownVertices,
                isIntersectionValid: out isValid);

    }

    private void GetBranchVertices(
        Vector3 edgeMidPoint,
        Vector3 L0,
        Vector3 R3,
        Vector3[] oldCenterVertices,
        bool isBranchOppositeToRoad,
        out Vector3[] newCenterVertices,
        out bool isIntersectionValid)
    {
        Vector3 __L0 = L0 - 0.5f * Height * Vector3.up;
        Vector3 __R3 = R3 - 0.5f * Height * Vector3.up;
        List<Vector3> newCenterVerticesList = new();

        Vector3[] currentCenterVertices = oldCenterVertices;

        if (isBranchOppositeToRoad)
            currentCenterVertices = oldCenterVertices.Reverse().ToArray();

        float thresholdAngle = Vector3.Angle(__L0 - edgeMidPoint, __R3 - edgeMidPoint);
        for (int i = 0; i < currentCenterVertices.Count(); i++)
        {
            if (i > 0)
            {
                float AngleBetweenCurrToL0AndReverse =
                    Vector3.Angle(
                        currentCenterVertices[i - 1] - currentCenterVertices[i],
                        __L0 - currentCenterVertices[i]);
                if (AngleBetweenCurrToL0AndReverse < 90)
                {
                    //Current vertex is at least the second vertex of the road 
                    //And it is on the right of the road edge
                    //normal case=> the previous vertex was the new end vertex
                    break;
                }
            }
            float currentAngleWithEdgePoints =
                Vector3.Angle(
                    __L0 - currentCenterVertices[i],
                    __R3 - currentCenterVertices[i]);
            if (currentAngleWithEdgePoints >= thresholdAngle)
            {
                //Current vertex is right of the road edge 
                if (i == 0)
                    //invalid: if start of the road is right of the road edge 
                    isIntersectionValid = false;
                else
                    //normal case=> the previous vertex was the new end vertex. 
                    break;
            }
            newCenterVerticesList.Add(currentCenterVertices[i]);
        }
        newCenterVerticesList.Add(edgeMidPoint);

        // RenderVertices(newCenterVerticesList.ToArray(), Name + leftIntersectionPosition + "New", Color.yellow);
        if (newCenterVerticesList.Count() > 1)
        {
            isIntersectionValid = true;
            newCenterVertices = newCenterVerticesList.ToArray();
        }
        //error case=> new road does not have any valid vertex 
        else isIntersectionValid = false;
        newCenterVertices = newCenterVerticesList.ToArray();
    }


    private Vector3[] GetRoadEdgePoints(
        int leftIntersectionPosition,
        ZeroCollisionInfo leftCollisionInfo,
        int edgePointIndex,
        Vector3 edgePoint)
    {
        GetEdgeDirectionAndRoadWidth(
            leftCollisionInfo: leftCollisionInfo,
            leftIntersectionPosition: leftIntersectionPosition,
            edgeDirectionL2R: out Vector3 edgeDirectionL2R,
            roadWidthExclSidewalks: out float roadWidthExclSidewalks);

        float laneWidth = ZeroRoadBuilder.RoadLaneWidth;
        Vector3[] edgePoints = new Vector3[4];
        edgePoints[edgePointIndex] = edgePoint;

        if (edgePointIndex == 0)
        {
            edgePoints[1] = edgePoints[0] + laneWidth * edgeDirectionL2R;
            edgePoints[2] = edgePoints[1] + roadWidthExclSidewalks * edgeDirectionL2R;
            edgePoints[3] = edgePoints[2] + laneWidth * edgeDirectionL2R;
        }
        else if (edgePointIndex == 1)
        {
            edgePoints[0] = edgePoints[1] - laneWidth * edgeDirectionL2R;
            edgePoints[2] = edgePoints[1] + roadWidthExclSidewalks * edgeDirectionL2R;
            edgePoints[3] = edgePoints[2] + laneWidth * edgeDirectionL2R;
        }
        else if (edgePointIndex == 2)
        {
            edgePoints[1] = edgePoints[2] - roadWidthExclSidewalks * edgeDirectionL2R;
            edgePoints[0] = edgePoints[1] - laneWidth * edgeDirectionL2R;
            edgePoints[3] = edgePoints[2] + laneWidth * edgeDirectionL2R;
        }
        else
        {
            edgePoints[2] = edgePoints[3] - laneWidth * edgeDirectionL2R;
            edgePoints[1] = edgePoints[2] - roadWidthExclSidewalks * edgeDirectionL2R;
            edgePoints[0] = edgePoints[1] - laneWidth * edgeDirectionL2R;
        }
        return edgePoints;
    }

    private void GetEdgeDirectionAndRoadWidth(
        ZeroCollisionInfo leftCollisionInfo,
        int leftIntersectionPosition,
        out Vector3 edgeDirectionL2R,
        out float roadWidthExclSidewalks)
    {
        ZeroRoadSegment leftSegment;

        if (leftIntersectionPosition % 2 == 0)
            leftSegment = leftCollisionInfo.PrimarySegment;
        else
            leftSegment = leftCollisionInfo.CollidingSegment;

        edgeDirectionL2R = leftSegment.DirectionL2R;
        roadWidthExclSidewalks = leftSegment.ParentLane.ParentRoad.WidthExclSidewalks;

        if (leftSegment.ParentLane.IsRightSidewalk)
            edgeDirectionL2R = -edgeDirectionL2R;
    }

    private bool IsEdgeValid(
        Vector3[] L,
        Vector3[] R,
        Vector3[] E
    )
    {
        Vector3 forward = Vector3.Cross(E[1] - E[0], Vector3.up);

        if (!ArePointsCloseEnough(E[0], L[0])
            && Vector3.Angle(L[0] - E[0], forward) > 90)
            return false;

        if (!ArePointsCloseEnough(E[1], L[3])
            && Vector3.Angle(L[3] - E[1], forward) > 90)
            return false;

        if (!ArePointsCloseEnough(E[2], R[0])
            && Vector3.Angle(R[0] - E[2], forward) > 90)
            return false;

        if (!ArePointsCloseEnough(E[3], R[3])
            && Vector3.Angle(R[3] - E[3], forward) > 90)
            return false;
        //
        //
        return true;
    }

    private Vector3[][] GetSidewalks(
        float i,
        Vector3[] L,
        Vector3[] R,
        Vector3[] E)
    {
        if (i % 2 == 0)
            return GetCurvedSidewalks(L: L, R: R, E: E);
        else
            return GetNonCurvedSidewalks(L: L, R: R, E: E);
    }

    private Vector3[][] GetNonCurvedSidewalks(
        Vector3[] L,
        Vector3[] R,
        Vector3[] E)
    {
        List<Vector3> rightSidewalk = new();
        List<Vector3> leftSidewalk = new();

        if (!ArePointsCloseEnough(E[0], L[0]))
            leftSidewalk.Add(E[0]);
        leftSidewalk.Add(L[0]);
        leftSidewalk.Add(L[3]);
        if (!ArePointsCloseEnough(E[1], L[3]))
            leftSidewalk.Add(E[1]);
        //
        if (!ArePointsCloseEnough(E[2], R[0]))
            rightSidewalk.Add(E[2]);
        rightSidewalk.Add(R[0]);
        rightSidewalk.Add(R[3]);
        if (!ArePointsCloseEnough(E[3], R[3]))
            rightSidewalk.Add(E[3]);

        return
            new Vector3[][] {
                rightSidewalk.ToArray(),
                leftSidewalk.ToArray() };
    }

    private Vector3[][] GetCurvedSidewalks(
        Vector3[] L,
        Vector3[] R,
        Vector3[] E)
    {
        List<Vector3> rightSidewalk = new();
        List<Vector3> leftSidewalk = new();

        if (!ArePointsCloseEnough(E[0], L[0]))
            leftSidewalk.Add(E[0]);
        leftSidewalk.Add(L[0]);
        leftSidewalk.AddRange(
            ZeroCurvedLine.FindBazierLinePoints(
                vertexCount: 8,
                controlPoints: new Vector3[] { L[1], L[2], L[3] }
            ).Item1);
        if (!ArePointsCloseEnough(E[1], L[3]))
            leftSidewalk.Add(E[1]);
        //
        //
        if (!ArePointsCloseEnough(E[2], R[0]))
            rightSidewalk.Add(E[2]);
        rightSidewalk.AddRange(
            ZeroCurvedLine.FindBazierLinePoints(
                vertexCount: 8,
                controlPoints: new Vector3[] { R[0], R[1], R[2] }
            ).Item1);
        rightSidewalk.Add(R[3]);
        if (!ArePointsCloseEnough(E[3], R[3]))
            rightSidewalk.Add(E[3]);

        //
        return
            new Vector3[][] {
                rightSidewalk.ToArray(),
                leftSidewalk.ToArray() };
    }

    private bool ArePointsCloseEnough(Vector3 p1, Vector3 p2)
    {
        return (p1 - p2).magnitude < 0.75;

    }

    public static bool GetRoadIntersectionsForPrimary(
       ZeroRoad primaryRoad,
       Dictionary<string, ZeroLaneIntersection[]> leftIntersectionsByRoadName,
       Dictionary<string, ZeroLaneIntersection[]> rightIntersectionsByRoadName)
    {
        List<ZeroRoadIntersection> intersections = new();

        List<string> allIntersectingRoads =
                    leftIntersectionsByRoadName.Keys
                    .Union(rightIntersectionsByRoadName.Keys)
                    .ToList();
        foreach (var intersectingRoadName in allIntersectingRoads)
        {
            ZeroRoad intersectingRoad =
                ZeroRoadBuilder.BuiltRoadsByName[intersectingRoadName];

            ZeroLaneIntersection[] leftIntersections, rightIntersections;
            if (leftIntersectionsByRoadName
                .ContainsKey(intersectingRoadName))
                leftIntersections =
                  leftIntersectionsByRoadName[intersectingRoadName];
            else
                leftIntersections = new ZeroLaneIntersection[0];

            if (rightIntersectionsByRoadName
                .ContainsKey(intersectingRoadName))
                rightIntersections =
                   rightIntersectionsByRoadName[intersectingRoadName];
            else
                rightIntersections = new ZeroLaneIntersection[0];

            intersections.AddRange(
                GetRoadIntersectionsForPair(
                    primaryRoad,
                    intersectingRoad,
                    leftIntersections,
                    rightIntersections));
        }

        if (intersections.Exists(e => !e.IsValid))
        {
            return false;
        }

        primaryRoad.Intersections = intersections.OrderBy(e => e.PrimaryRoadLengthSoFar).ToArray();
        if (intersections.Count() > 0)
        {
            if (GetBranchRoadsForPrimary(
                    primaryRoad,
                    out List<ZeroRoad> branchRoads))
                primaryRoad.IntersectionBranchRoads = branchRoads.ToArray();
            else
                return false;
        }
        return true;
    }

    public static ZeroRoadIntersection[] GetRoadIntersectionsForPair(
        ZeroRoad primaryRoad,
        ZeroRoad intersectingRoad,
        ZeroLaneIntersection[] allLeftIntersections,
        ZeroLaneIntersection[] allRightIntersections)
    {
        float primaryRoadHeight = primaryRoad.Height;
        float intersectingRoadHeight = intersectingRoad.Height;
        Vector3[] primaryControlPoints = primaryRoad.ControlPoints;

        List<ZeroRoadIntersection> roadIntersections = new();
        int leftIntersectionCount = allLeftIntersections.Length;
        int rightIntersectionCount = allRightIntersections.Length;
        float intersectionHeight =
            Mathf.Max(primaryRoadHeight, intersectingRoadHeight);

        string name =
            String.Format("{0}_{1}",
                primaryRoad.Name,
                intersectingRoad.Name);

        if (leftIntersectionCount == 4 && rightIntersectionCount == 4)
        {
            roadIntersections.Add(new ZeroRoadIntersection(
                name: name + "_0",
                height: intersectionHeight,
                roadIntersectionType: ROAD_INTERSESCTION_TYPE_X,
                primaryRoad: primaryRoad,
                intersectingRoad: intersectingRoad,
                laneIntersections: new ZeroLaneIntersection[]{
                    allLeftIntersections[0],
                    allLeftIntersections[1],
                    allRightIntersections[1],
                    allRightIntersections[0]
                }
            ));
            roadIntersections.Add(new ZeroRoadIntersection(
                name: name + "_1",
                height: intersectionHeight,
                roadIntersectionType: ROAD_INTERSESCTION_TYPE_X,
                primaryRoad: primaryRoad,
                intersectingRoad: intersectingRoad,
                laneIntersections: new ZeroLaneIntersection[]{
                    allLeftIntersections[2],
                    allLeftIntersections[3],
                    allRightIntersections[3],
                    allRightIntersections[2]
                }
            ));
        }
        else if (leftIntersectionCount == 2 && rightIntersectionCount == 2)
        {
            roadIntersections.Add(new ZeroRoadIntersection(
                name: name,
                height: intersectionHeight,
                roadIntersectionType: ROAD_INTERSESCTION_TYPE_X,
                primaryRoad: primaryRoad,
                intersectingRoad: intersectingRoad,
                laneIntersections: new ZeroLaneIntersection[]{
                    allLeftIntersections[0],
                    allLeftIntersections[1],
                    allRightIntersections[1],
                    allRightIntersections[0]
                }
            ));
        }
        else if (leftIntersectionCount == 2 && rightIntersectionCount == 0)
        {
            roadIntersections.Add(new ZeroRoadIntersection(
                name: name,
                height: intersectionHeight,
                roadIntersectionType: ROAD_INTERSESCTION_TYPE_UP,
                primaryRoad: primaryRoad,
                intersectingRoad: intersectingRoad,
                laneIntersections: new ZeroLaneIntersection[]{
                    allLeftIntersections[1],
                    allLeftIntersections[0]
                }
            ));
        }
        else if (leftIntersectionCount == 0 && rightIntersectionCount == 2)
        {
            roadIntersections.Add(new ZeroRoadIntersection(
                name: name,
                height: intersectionHeight,
                roadIntersectionType: ROAD_INTERSESCTION_TYPE_DOWN,
                primaryRoad: primaryRoad,
                intersectingRoad: intersectingRoad,
                laneIntersections: new ZeroLaneIntersection[]{
                    allRightIntersections[0],
                    allRightIntersections[1]
                }
            ));
        }
        else if (leftIntersectionCount == 1 && rightIntersectionCount == 1)
        {
            if (IsRoadIntersectionLeft(
                    controlPoints: primaryControlPoints,
                    leftIntersection: allLeftIntersections[0]))
                roadIntersections.Add(new ZeroRoadIntersection(
                    name: name,
                    height: intersectionHeight,
                    roadIntersectionType: ROAD_INTERSESCTION_TYPE_LEFT,
                    primaryRoad: primaryRoad,
                    intersectingRoad: intersectingRoad,
                    laneIntersections: new ZeroLaneIntersection[]{
                    allLeftIntersections[0],
                    allRightIntersections[0]
                }
            ));
            else
                roadIntersections.Add(new ZeroRoadIntersection(
                    name: name,
                    height: intersectionHeight,
                    roadIntersectionType: ROAD_INTERSESCTION_TYPE_RIGHT,
                    primaryRoad: primaryRoad,
                    intersectingRoad: intersectingRoad,
                    laneIntersections: new ZeroLaneIntersection[]{
                    allRightIntersections[0],
                    allLeftIntersections[0]
                    }
                ));
        }
        foreach (var intersection in roadIntersections)
            ZeroRoadBuilder.ActiveIntersections[intersection.Name] = intersection;

        return roadIntersections.ToArray();
    }

    private static bool IsRoadIntersectionLeft(
        Vector3[] controlPoints,
        ZeroLaneIntersection leftIntersection
    )
    {
        ZeroRoad intersectingRoad =
                leftIntersection.CollisonPoints[0].CollidingSegment
                .ParentLane
                .ParentRoad;

        int startCollidersCount =
            Physics.OverlapSphere(
                position: controlPoints[0],
                radius: 0.5f)
            .Where((e) =>
            {
                return e.gameObject.name.StartsWith(intersectingRoad.Name + "_");
            }).Count();

        int endCollidersCount =
            Physics.OverlapSphere(
                position: controlPoints.Last(),
                radius: 0.5f)
            .Where((e) =>
            {
                return e.gameObject.name.StartsWith(intersectingRoad.Name + "_");
            }).Count();

        if (startCollidersCount > 0)
            return false;
        else if (endCollidersCount > 0)
            return true;
        else
            throw new Exception("Unrecognised road intersection type");
    }

    private static bool GetBranchRoadsForPrimary(
        ZeroRoad primaryRoad,
        out List<ZeroRoad> branchRoads)
    {
        branchRoads = new();
        ZeroRoadIntersection[] intersections = primaryRoad.Intersections;
        Dictionary<string,
            List<ZeroRoadIntersection>> intersectionsByIntersectingRoad = new();

        List<ZeroRoad> allIntersectingRoads = new();
        int k = 0;
        foreach (var intersecion in intersections)
        {
            ZeroRoad intersectingRoad = intersecion.IntersectingRoad;
            string intersectingRoadName = intersectingRoad.Name;
            if (!intersectionsByIntersectingRoad
                .ContainsKey(intersectingRoadName))
            {
                intersectionsByIntersectingRoad[intersectingRoadName] = new();
                allIntersectingRoads.Add(intersectingRoad);
            }
            intersectionsByIntersectingRoad[intersectingRoadName]
            .Add(intersecion);
            Debug.LogFormat("index={0} intersection={1} distanceFromOrigin={2}", k++, intersecion.Name, intersecion.PrimaryRoadLengthSoFar);
        }

        List<string> visitedIntersectingRoads = new();
        if (intersections.Length == 0)
            return true;

        for (int i = 0; i < intersections.Length; i++)
        {
            List<int> branchTypes = new() { 1 };
            if (i == 0)
                branchTypes.Add(0);
            if (i == intersections.Length - 1)
                branchTypes.Add(2);

            ZeroRoadIntersection currIntersection = intersections[i];
            ZeroRoad currIntersectingRoad = currIntersection.IntersectingRoad;

            string currIntersectingRoadName = currIntersectingRoad.Name;
            List<Vector3> branchCenterVertices;

            if (intersectionsByIntersectingRoad
                .ContainsKey(currIntersectingRoadName)
                && intersectionsByIntersectingRoad[currIntersectingRoadName]
                    .Count() == 2)
            {
                if (!visitedIntersectingRoads
                    .Contains(currIntersectingRoadName))
                {

                    var intersectionsForThisIntersectingRoad =
                        intersectionsByIntersectingRoad[currIntersectingRoadName];

                    if (!GetOverlappingSection(
                            leftVertices:
                                intersectionsForThisIntersectingRoad
                                .Last()
                                .BranchDownVertices,
                            rightVertices:
                                intersectionsForThisIntersectingRoad
                                .First()
                                .BranchDownVertices,
                            sourceRoadLength: currIntersectingRoad.Length,
                            isSourceRoadCurved: currIntersectingRoad.IsCurved,
                            out branchCenterVertices))
                        return false;

                    branchRoads.Add(
                        new ZeroRoad(
                            isPrimaryRoad: false,
                            sourceRoad: currIntersectingRoad,
                            centerVertices: branchCenterVertices.ToArray()));
                    visitedIntersectingRoads.Add(currIntersectingRoadName);
                }
            }
            else
                branchTypes.Add(3);

            if (i > 0)
            {
                ZeroRoadIntersection prevIntersection = intersections[i - 1];
                RenderVertices(currIntersection.BranchLeftVertices, currIntersection.Name + "CurrLeft", Color.blue);
                RenderVertices(prevIntersection.BranchRightVertices, prevIntersection.Name + "PrevRight", Color.black);
                if (!GetOverlappingSection(
                        leftVertices: prevIntersection.BranchRightVertices,
                        rightVertices: currIntersection.BranchLeftVertices,
                        sourceRoadLength: primaryRoad.Length,
                        isSourceRoadCurved: primaryRoad.IsCurved,
                        out branchCenterVertices))
                {
                    Debug.LogFormat("currIntersection={0} is invalid", currIntersection.Name);
                    return false;
                }
                RenderVertices(branchCenterVertices.ToArray(), currIntersection.Name + "Overlap", Color.white);

                branchRoads.Add(
                    new ZeroRoad(
                        isPrimaryRoad: false,
                        sourceRoad: primaryRoad,
                        centerVertices: branchCenterVertices.ToArray()));
            }
            branchRoads.AddRange(
                GetBranchRoadsForIntersection(
                    intersection: currIntersection,
                    branchTypes: branchTypes.ToArray())
            );
        }
        Debug.LogFormat(
            "branchRoads={0}",
            branchRoads.Select(e => e.Name).ToCommaSeparatedString());
        return true;
    }

    private static ZeroRoad[] GetBranchRoadsForIntersection(
        ZeroRoadIntersection intersection,
        int[] branchTypes)
    {
        List<ZeroRoad> branchRoads = new();

        for (int i = 0; i < branchTypes.Length; i++)
        {
            if (branchTypes[i] == ROAD_INTERSESCTION_TYPE_LEFT
                && intersection.BranchLeftVertices != null)
                branchRoads.Add(
                    new ZeroRoad(
                        isPrimaryRoad: false,
                        sourceRoad: intersection.PrimaryRoad,
                        centerVertices: intersection.BranchLeftVertices));
            else if (branchTypes[i] == ROAD_INTERSESCTION_TYPE_UP
                && intersection.BranchUpVertices != null)
                branchRoads.Add(
                    new ZeroRoad(
                        isPrimaryRoad: false,
                        sourceRoad: intersection.IntersectingRoad,
                        centerVertices: intersection.BranchUpVertices));
            else if (branchTypes[i] == ROAD_INTERSESCTION_TYPE_RIGHT
                && intersection.BranchRightVertices != null)
                branchRoads.Add(
                    new ZeroRoad(
                        isPrimaryRoad: false,
                        sourceRoad: intersection.PrimaryRoad,
                        centerVertices: intersection.BranchRightVertices));
            else if (branchTypes[i] == ROAD_INTERSESCTION_TYPE_DOWN
                && intersection.BranchDownVertices != null)
                branchRoads.Add(
                    new ZeroRoad(
                        isPrimaryRoad: false,
                        sourceRoad: intersection.IntersectingRoad,
                        centerVertices: intersection.BranchDownVertices));
        }
        return branchRoads.ToArray();
    }

    private static bool GetOverlappingSection(
        Vector3[] leftVertices,
        Vector3[] rightVertices,
        float sourceRoadLength,
        bool isSourceRoadCurved,
        out List<Vector3> overlappingVertices)
    {
        overlappingVertices = new();
        Debug.LogFormat("left length={0} right length={1} road length={2}",
            ZeroRoad.GetLength(leftVertices),
            ZeroRoad.GetLength(rightVertices),
            sourceRoadLength);
        if (leftVertices == null || rightVertices == null)
            return false;

        if (ZeroRoad.GetLength(leftVertices)
            + ZeroRoad.GetLength(rightVertices)
            < sourceRoadLength)
            return false;

        if (isSourceRoadCurved)
        {
            Vector3 rightEnd = rightVertices.Last();
            bool overlapStarted = false;
            for (int i = 0; i < leftVertices.Length; i++)
            {
                if (leftVertices[i].Equals(rightVertices[^1])
                    || leftVertices[i].Equals(rightVertices[^2])
                    || ZeroCollisionMap.IsPointOnLineSegment(
                        point: leftVertices[i],
                        start: rightVertices[^1],
                        end: rightVertices[^2]))
                {
                    overlapStarted = true;
                    if (!leftVertices[i].Equals(rightVertices[^1]))
                        overlappingVertices.Add(rightVertices[^1]);
                }
                if (overlapStarted)
                    overlappingVertices.Add(leftVertices[i]);
            }
        }
        else
        {
            overlappingVertices.Add(rightVertices.Last());
            overlappingVertices.Add(leftVertices.Last());
        }
        return true;
    }

    public void RenderLaneIntersections(Color? color = null)
    {
        RenderVertices(LaneIntersectionPoints,
          Name + "_LI",
         color: color);
    }

    public void RenderSidewalks(Color? color = null)
    {
        RenderVertices(Sidewalks,
          Name + "_SW",
         color: color);
    }

    public void RenderCrosswalks(Color? color = null)
    {
        RenderVertices(CrossWalks,
          Name + "_CW",
         color: color);
    }

    public void RenderMainSquare(Color? color = null)
    {
        RenderVertices(MainSquare,
          Name + "_MS",
         color: color);
    }

    public void RenderRoadEdges(Color? color = null)
    {
        RenderVertices(RoadEdges,
          Name + "_RE",
         color: color);
    }

    public Dictionary<string, Vector3> LaneIntersectionsLogPairs()
    {
        return GetVertexLogPairs(LaneIntersectionPoints, Name + "_LI");
    }

    public Dictionary<string, Vector3> CrosswalksLogPairs()
    {
        return GetVertexLogPairs(CrossWalks, Name + "_CW");
    }

    public Dictionary<string, Vector3> SidewalksLogPairs()
    {
        return GetVertexLogPairs(Sidewalks, Name + "_SW");
    }

    public Dictionary<string, Vector3> MainSquareLogPairs()
    {
        return GetVertexLogPairs(MainSquare, Name + "_MS");
    }

    public Dictionary<string, Vector3> RoadEdgesLogPairs()
    {
        return GetVertexLogPairs(RoadEdges, Name + "_RE");
    }

    private static void RenderVertices(
        Vector3[][] vertices,
        string prefix,
        Color? color = null)
    {
        Color[] colors = new Color[]{
            Color.blue,
            Color.yellow,
            Color.red,
            Color.green,
            Color.white,
            Color.black,
            Color.gray,
            Color.magenta,
            Color.cyan
        };
        for (int i = 0; i < vertices.Length; i++)
            for (int j = 0; j < vertices[i].Length; j++)
                ZeroRenderer.RenderSphere(
                    position: vertices[i][j],
                    sphereName: String.Format("{0}_{1}_{2}", prefix, i, j),
                    color:
                        color ?? colors[
                                i > colors.Length ?
                                i - colors.Length : i]);
    }

    public static void RenderVertices(
        Vector3[] vertices,
        string prefix,
        Color? color = null)
    {
        if (vertices != null && vertices.Length > 0)
        {
            Color[] colors = new Color[]{
                Color.blue,
                Color.yellow,
                Color.red,
                Color.green,
                Color.white,
                Color.black,
                Color.gray,
                Color.magenta,
                Color.cyan
            };
            for (int i = 0; i < vertices.Length; i++)
                ZeroRenderer.RenderSphere(
                    position: vertices[i],
                    sphereName: String.Format("{0}_{1}", prefix, i),
                    color:
                        color ?? colors[
                                i > colors.Length ?
                                i - colors.Length : i]);
        }
    }

    private Dictionary<string, Vector3> GetVertexLogPairs(
        Vector3[][] vertices,
        string prefix)
    {
        Dictionary<string, Vector3> vertexStrings = new();
        for (int i = 0; i < vertices.Length; i++)
            for (int j = 0; j < vertices[i].Length; j++)
                vertexStrings[String.Format("{0}_{1}_{2}", prefix, i, j)] =
                vertices[i][j];
        return vertexStrings;
    }
}
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
    public List<(Vector3[], int)> BranchVerticesTypePairs;
    public ZeroLaneIntersection[] LaneIntersections;
    public Vector3[][] LaneIntersectionPoints;
    public ZeroRoad PrimaryRoad;
    public ZeroRoad IntersectingRoad;
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
        IsValid = false;
        PrimaryRoad = primaryRoad;
        IntersectingRoad = intersectingRoad;

        GetIntersectionModules();

        // Debug.LogFormat(
        //    "intersectingRoadName={0} right intersections={1}",
        //    leftIntersections,
        //    rightIntersections);

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

            GetBranchRoad(
               leftIntersectionPosition: GetLeftIntersectionPosition(intersectionSideIndex),
               edgeMidPoint: edgeMidpoint,
               L: L,
               R: R,
               leftIntersectionL0: _L[0],
               out IsValid);

            if (!IsValid)
                break;
            Vector3[] mainSquare = new Vector3[] { E[0], L[1], L[2], R[1], R[2], E[3] };
            Vector3[] crosswalk = new Vector3[] { L[3], L[2], R[1], R[0] };
            middleSquare.Add(L[2]);
            mainSquares.Add(mainSquare);
            crosswalks.Add(crosswalk);
            sidewalks.AddRange(GetSidewalks(i: intersectionSideIndex, L: L, R: R, E: E).Where(e => e.Length > 2));
            roadEdges.Add(E);
            edgeMidpoints.Add(edgeMidpoint);

            //If there are only two lane intersections (it's a T intersection), 
            //then there should be only one iteration of this loop i.e. one crosswalk, two sidewalks and one main s=square part
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
        ZeroLaneIntersection leftIntersection = LaneIntersections[intersectionSideIndex];
        ZeroLaneIntersection rightIntersection = LaneIntersections[(intersectionSideIndex + n - 1) % n];
        L = new Vector3[4];
        R = new Vector3[4];
        E = new Vector3[4];
        _L = new ZeroCollisionInfo[4];
        _R = new ZeroCollisionInfo[4];

        int leftIntersectionPosition = GetLeftIntersectionPosition(intersectionSideIndex);
        Debug.LogFormat("n={6} this.RoadIntersectionType={0} leftIntersectionPosition={1} leftIntersection=[{2}-{3}] rightIntersection=[{4}-{5}]",
            RoadIntersectionType,
            leftIntersectionPosition,
            leftIntersection.PrimaryLane.Name,
            leftIntersection.CollidingLane.Name,
            rightIntersection.PrimaryLane.Name,
            rightIntersection.CollidingLane.Name,
            n);
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
            // Debug.LogFormat("i={0} edgePointIndex={1} Valid={2} edgePoints={3}", i, 1, IsEdgeValid(L, R, _E), _E.Select(e => e.ToString()).ToCommaSeparatedString());
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
            // Debug.LogFormat("i={0} edgePointIndex={1} Valid={2} edgePoints={3}", i, 0, IsEdgeValid(L, R, _E), _E.Select(e => e.ToString()).ToCommaSeparatedString());
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
            // Debug.LogFormat("i={0} edgePointIndex={1} Valid={2} edgePoints={3}", i, 3, IsEdgeValid(L, R, _E), _E.Select(e => e.ToString()).ToCommaSeparatedString());
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
            // Debug.LogFormat("i={0} edgePointIndex={1} Valid={2} edgePoints={3}", i, 2, IsEdgeValid(L, R, _E), _E.Select(e => e.ToString()).ToCommaSeparatedString());
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


    private void GetBranchRoad(
        int leftIntersectionPosition,
        Vector3 edgeMidPoint,
        Vector3[] L,
        Vector3[] R,
        ZeroCollisionInfo leftIntersectionL0,
        out bool isValid)
    {
        Vector3 __edgeMidPoint = edgeMidPoint - 0.5f * Height * Vector3.up;
        Vector3[] __L = L.Select(e => e - 0.5f * Height * Vector3.up).ToArray();
        Vector3[] __R = R.Select(e => e - 0.5f * Height * Vector3.up).ToArray();
        Vector3[] branchVertices;
        if (leftIntersectionPosition == 0)
            GetBranchRoad(
                leftIntersectionPosition,
                edgeMidPoint: __edgeMidPoint,
                L: __L,
                R: __R,
                oldCenterVertices: PrimaryRoad.CenterVertices,
                isDirectionReversed: false,
                newCenterVertices: out branchVertices,
                isIntersectionValid: out isValid);

        else if (leftIntersectionPosition == 2)
            GetBranchRoad(
                leftIntersectionPosition,
                edgeMidPoint: __edgeMidPoint,
                L: __L,
                R: __R,
                oldCenterVertices: PrimaryRoad.CenterVertices,
                isDirectionReversed: true,
                newCenterVertices: out branchVertices,
                isIntersectionValid: out isValid);
        // else if (leftIntersectionPosition == 1 || leftIntersectionPosition == 3)
        else
        {
            if (leftIntersectionL0.CollidingSegment.ParentLane.IsLeftSidewalk)
                GetBranchRoad(
                    leftIntersectionPosition,
                    edgeMidPoint: __edgeMidPoint,
                    L: __L,
                    R: __R,
                    oldCenterVertices: IntersectingRoad.CenterVertices,
                    isDirectionReversed: false,
                    newCenterVertices: out branchVertices,
                    isIntersectionValid: out isValid);
            else
                GetBranchRoad(
                    leftIntersectionPosition,
                    edgeMidPoint: __edgeMidPoint,
                    L: __L,
                    R: __R,
                    oldCenterVertices: IntersectingRoad.CenterVertices,
                    isDirectionReversed: true,
                    newCenterVertices: out branchVertices,
                    isIntersectionValid: out isValid);
        }
        BranchVerticesTypePairs.Add((branchVertices, leftIntersectionPosition));
    }

    private void GetBranchRoad(
        int leftIntersectionPosition,
        Vector3 edgeMidPoint,
        Vector3[] L,
        Vector3[] R,
        Vector3[] oldCenterVertices,
        bool isDirectionReversed,
        out Vector3[] newCenterVertices,
        out bool isIntersectionValid)
    {
        List<Vector3> newCenterVerticesList = new();

        Vector3[] currentCenterVertices =
            isDirectionReversed ?
            oldCenterVertices.Reverse().ToArray()
            : oldCenterVertices;

        // ZeroRenderer.RenderSphere(edgeMidPoint, "Mid");
        // ZeroRenderer.RenderSphere(L[0], "L0");
        // ZeroRenderer.RenderSphere(R[3], "R3");
        float thresholdAngle = Vector3.Angle(L[0] - edgeMidPoint, R[3] - edgeMidPoint);
        for (int i = 0; i < currentCenterVertices.Count(); i++)
        {
            if (i > 0)
            {
                float AngleBetweenCurrToL0AndReverse =
                    Vector3.Angle(
                        currentCenterVertices[i - 1] - currentCenterVertices[i],
                        L[0] - currentCenterVertices[i]);
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
                    L[0] - currentCenterVertices[i],
                    R[3] - currentCenterVertices[i]);
            if (currentAngleWithEdgePoints >= thresholdAngle)
            {
                //Current vertex is right of the road edge 
                if (i == 0)
                    //error case=> And if start of the road is right of the road edge 
                    isIntersectionValid = false;
                else
                    //normal case=> the previous vertex was the new end vertex. 
                    break;
            }
            newCenterVerticesList.Add(currentCenterVertices[i]);
        }
        newCenterVerticesList.Add(edgeMidPoint);

        if (newCenterVerticesList.Count() > 1)
        {
            Vector3[] newControlPoints = new Vector3[] { newCenterVerticesList[0], newCenterVerticesList.Last() };

            for (int j = 0; j < newCenterVerticesList.Count(); j++)
            {
                ZeroRenderer.RenderSphere(newCenterVerticesList[j], "RI" + leftIntersectionPosition + "V" + j);
            }
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

        // Debug.LogFormat("leftSegment={0} IsRightSidewalk={1} leftIntersectionPosition={2}",
        //     leftSegment.Name,
        //     leftSegment.ParentLane.IsRightSidewalk,
        //     leftIntersectionPosition);

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
        // Debug.LogFormat("Vector3.Angle(L[0] - E[0], forward)={0}", Vector3.Angle(L[0] - E[0], forward));
        // Debug.LogFormat("Vector3.Angle(L[3] - E[1], forward)={0}", Vector3.Angle(L[3] - E[1], forward));
        // Debug.LogFormat("Vector3.Angle(R[0] - E[2], forward)={0}", Vector3.Angle(R[0] - E[2], forward));
        // Debug.LogFormat("Vector3.Angle(R[3] - E[3], forward)={0}", Vector3.Angle(R[3] - E[3], forward));

        if (!ArePointsCloseEnough(E[0], L[0]) && Vector3.Angle(L[0] - E[0], forward) > 90)
            return false;
        if (!ArePointsCloseEnough(E[1], L[3]) && Vector3.Angle(L[3] - E[1], forward) > 90)
            return false;
        if (!ArePointsCloseEnough(E[2], R[0]) && Vector3.Angle(R[0] - E[2], forward) > 90)
            return false;
        if (!ArePointsCloseEnough(E[3], R[3]) && Vector3.Angle(R[3] - E[3], forward) > 90)
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

        return new Vector3[][] { rightSidewalk.ToArray(), leftSidewalk.ToArray() };
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
        return new Vector3[][] { rightSidewalk.ToArray(), leftSidewalk.ToArray() };
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
            ZeroRoad intersectingRoad = ZeroRoadBuilder.BuiltRoadsByName[intersectingRoadName];

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
            return false;

        if (intersections.Count() > 0)
        {
            primaryRoad.Intersections = intersections.ToArray();
            if (GetBranchRoadVertices(
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
        float intersectionHeight = Mathf.Max(primaryRoadHeight, intersectingRoadHeight);

        string name = String.Format("{0}_{1}", primaryRoad.Name, intersectingRoad.Name);

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
            if (IsRoadIntersectionLeft(primaryControlPoints, allLeftIntersections[0]))
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

    private static bool GetBranchRoadVertices(
        ZeroRoad primaryRoad,
        out List<ZeroRoad> branchRoads)
    {
        branchRoads = new();
        ZeroRoadIntersection[] intersections = primaryRoad.Intersections;
        List<string> visitedIntersectingRoads = new();
        if (intersections.Length == 0)
            return true;
        else if (intersections.Length == 1)
            new ZeroRoad(
                sourceRoad: primaryRoad,
                centerVertices:
                    intersections.First()
                    .BranchVerticesTypePairs.First().Item1
                    .ToArray());
        else
        {
            for (int i = 1; i < intersections.Length; i++)
            {
                ZeroRoadIntersection currIntersection = intersections[i];
                ZeroRoadIntersection prevIntersection = intersections[i - 1];

                ZeroRoad intersectingRoad = currIntersection.IntersectingRoad;
                List<Vector3> branchRoadVertices;
                if (visitedIntersectingRoads.Contains(intersectingRoad.Name))
                {
                    if (!GetOverlappingSection(
                            leftVertices: currIntersection.BranchBottomVertices,
                            rightVertices: prevIntersection.BranchBottomVertices,
                            primaryRoadLength: primaryRoad.Length,
                            out branchRoadVertices)
                        )
                        return false;

                    branchRoads.Add(
                        new ZeroRoad(
                            sourceRoad: primaryRoad,
                            centerVertices: branchRoadVertices.ToArray())
                    );
                }
                else
                    visitedIntersectingRoads.Add(currIntersection.IntersectingRoad.Name);

                if (!GetOverlappingSection(
                        leftVertices: currIntersection.BranchLeftVertices,
                        rightVertices: prevIntersection.BranchRightVertices,
                        primaryRoadLength: primaryRoad.Length,
                            out branchRoadVertices))
                    return false;

                branchRoads.Add(
                    new ZeroRoad(
                        sourceRoad: primaryRoad,
                        centerVertices: branchRoadVertices.ToArray())
                );
            }
        }

        return true;
    }

    private static bool GetOverlappingSection(
        Vector3[] leftVertices,
        Vector3[] rightVertices,
        float primaryRoadLength,
        out List<Vector3> overlappingVertices)
    {
        overlappingVertices = new();
        Vector3 rightEnd = rightVertices.Last();
        if (leftVertices == null
            || rightVertices == null)
            return false;
        if (
            GetLength(leftVertices)
                + GetLength(rightVertices)
            <= primaryRoadLength)
            return false;
        bool overlapStarted = false;
        for (int i = 0; i < leftVertices.Length; i++)
        {
            overlappingVertices.Add(leftVertices[i]);
            if (leftVertices[i].Equals(rightEnd))
                overlapStarted = true;
            if (overlapStarted)
                overlappingVertices.Add(leftVertices[i]);
        }
        return true;
    }

    private static float GetLength(Vector3[] vertices)
    {
        float totalDistance = 0;
        for (int i = 1; i < vertices.Length; i++)
            totalDistance += (vertices[i] - vertices[i - 1]).magnitude;
        return totalDistance;
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

    private void RenderVertices(Vector3[][] vertices, string prefix, Color? color = null)
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
                    color: color ?? colors[i > colors.Length ? i - colors.Length : i]);
    }

    private Dictionary<string, Vector3> GetVertexLogPairs(Vector3[][] vertices, string prefix)
    {
        Dictionary<string, Vector3> vertexStrings = new();
        for (int i = 0; i < vertices.Length; i++)
            for (int j = 0; j < vertices[i].Length; j++)
                vertexStrings[String.Format("{0}_{1}_{2}", prefix, i, j)] =
                vertices[i][j];
        return vertexStrings;
    }
}
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
    public ZeroLaneIntersection[] LaneIntersections;
    public Vector3[][] LaneIntersectionPoints;
    public float Height;
    public int RoadIntersectionType;
    public static int ROAD_INTERSESCTION_TYPE_LEFT = 0;
    public static int ROAD_INTERSESCTION_TYPE_UP = 1;
    public static int ROAD_INTERSESCTION_TYPE_RIGHT = 2;
    public static int ROAD_INTERSESCTION_TYPE_DOWN = 3;
    public static int ROAD_INTERSESCTION_TYPE_X = 4;

    public ZeroRoadIntersection(
        string name,
        float height,
        int roadIntersectionType,
        ZeroLaneIntersection[] laneIntersections)
    {
        this.Name = name;
        this.LaneIntersections = laneIntersections;
        this.RoadIntersectionType = roadIntersectionType;
        this.LaneIntersectionPoints =
            this.LaneIntersections
            .Select(e =>
                e.IntersectionPoints.Select(c => c.CollisionPoint).ToArray()).ToArray();
        this.Height = height;

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
        List<Vector3> middleSquare = new();

        int n = this.LaneIntersections.Length;
        for (int i = 0; i < n; i++)
        {
            GetLRE(
                i: i,
                n: n,
                L: out Vector3[] L,
                R: out Vector3[] R,
                E: out Vector3[] E);



            //
            Vector3[] mainSquare = new Vector3[] { E[0], L[1], L[2], R[1], R[2], E[3] };
            Vector3[] crosswalk = new Vector3[] { L[3], L[2], R[1], R[0] };
            middleSquare.Add(L[2]);
            mainSquares.Add(mainSquare);
            crosswalks.Add(crosswalk);
            sidewalks.AddRange(GetSidewalks(i: i, L: L, R: R, E: E).Where(e => e.Length > 2));
            roadEdges.Add(E);

            //If there are only two lane intersections (it's a T intersection), 
            //then there should be only one iteration of this loop i.e. one crosswalk, two sidewalks and one main s=square part
            if (n == 2) i++;
        }
        if (n == 4)
            mainSquares.Add(middleSquare.ToArray());
        //
        this.Sidewalks = sidewalks.ToArray();
        this.CrossWalks = crosswalks.ToArray();
        this.MainSquare = mainSquares.ToArray();
        this.RoadEdges = roadEdges.ToArray();
    }

    private void GetLRE(
        int i,
        int n,
        out Vector3[] L,
        out Vector3[] R,
        out Vector3[] E)
    {
        ZeroLaneIntersection leftIntersection = this.LaneIntersections[i];
        ZeroLaneIntersection rightIntersection = this.LaneIntersections[(i + n - 1) % n];
        L = new Vector3[4];
        R = new Vector3[4];
        E = new Vector3[4];
        ZeroCollisionInfo[] _L = new ZeroCollisionInfo[4];
        ZeroCollisionInfo[] _R = new ZeroCollisionInfo[4];

        int leftIntersectionPosition;
        if (this.RoadIntersectionType == ROAD_INTERSESCTION_TYPE_X)
            leftIntersectionPosition = i;
        else if (this.RoadIntersectionType == ROAD_INTERSESCTION_TYPE_LEFT)
            leftIntersectionPosition = 0;
        else if (this.RoadIntersectionType == ROAD_INTERSESCTION_TYPE_UP)
            leftIntersectionPosition = 1;
        else if (this.RoadIntersectionType == ROAD_INTERSESCTION_TYPE_RIGHT)
            leftIntersectionPosition = 2;
        // else if (this.RoadIntersectionType == INTERSESCTION_TYPE_T_DOWN)
        else
            leftIntersectionPosition = 3;
        // Debug.LogFormat("n={5} this.RoadIntersectionType={0} leftIntersectionPosition={1} leftIntersection=[{2}-{3}] rightIntersection=[{3}-{4}]",
        //     this.RoadIntersectionType,
        //     leftIntersectionPosition,
        //     leftIntersection.PrimaryLane.Name,
        //     leftIntersection.IntersectingLane.Name,
        //     rightIntersection.PrimaryLane.Name,
        //     rightIntersection.IntersectingLane.Name,
        //     n);
        for (int j = 0; j < 4; j++)
        {
            _L[j] = leftIntersection.IntersectionPoints[(j + leftIntersectionPosition) % 4];
            _R[j] = rightIntersection.IntersectionPoints[(j + leftIntersectionPosition) % 4];
            L[j] = _L[j].CollisionPoint;
            R[j] = _R[j].CollisionPoint;
        }
        Vector3[] _E = new Vector3[4];
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

    public static ZeroRoadIntersection[] GetRoadIntersections(
        float primaryRoadHeight,
        float collidingRoadHeight,
        Vector3[] controlPoints,
        ZeroLaneIntersection[] allLeftIntersections,
        ZeroLaneIntersection[] allRightIntersections)
    {

        List<ZeroRoadIntersection> roadIntersections = new();
        int leftIntersectionCount = allLeftIntersections.Length;
        int rightIntersectionCount = allRightIntersections.Length;
        float intersectionHeight = Mathf.Max(primaryRoadHeight, collidingRoadHeight);
        ZeroCollisionInfo left0Collision = allLeftIntersections[0].IntersectionPoints[0];
        string primaryRoad = left0Collision.PrimarySegment.ParentLane.ParentRoad.Name;
        string collidingRoad = left0Collision.CollidingSegment.ParentLane.ParentRoad.Name;
        string name = String.Format("{0}_{1}", primaryRoad, collidingRoad);

        Debug.LogFormat("leftIntersectionCount={0} rightIntersectionCount={1}",
            leftIntersectionCount,
            rightIntersectionCount);

        if (leftIntersectionCount == 4 && rightIntersectionCount == 4)
        {
            roadIntersections.Add(new ZeroRoadIntersection(
                    name: name + "0",
                    height: intersectionHeight,
                    roadIntersectionType: ZeroRoadIntersection.ROAD_INTERSESCTION_TYPE_X,
                    laneIntersections: new ZeroLaneIntersection[]{
                        allLeftIntersections[0],
                        allLeftIntersections[1],
                        allRightIntersections[1],
                        allRightIntersections[0]
                    }
                ));
            roadIntersections.Add(new ZeroRoadIntersection(
                    name: name + "1",
                    height: intersectionHeight,
                    roadIntersectionType: ZeroRoadIntersection.ROAD_INTERSESCTION_TYPE_X,
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
                    roadIntersectionType: ZeroRoadIntersection.ROAD_INTERSESCTION_TYPE_X,
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
                    roadIntersectionType: ZeroRoadIntersection.ROAD_INTERSESCTION_TYPE_UP,
                    laneIntersections: new ZeroLaneIntersection[]{
                        allLeftIntersections[0],
                        allLeftIntersections[1]
                    }
                ));
        }
        else if (leftIntersectionCount == 0 && rightIntersectionCount == 2)
        {
            roadIntersections.Add(new ZeroRoadIntersection(
                    name: name,
                    height: intersectionHeight,
                    roadIntersectionType: ZeroRoadIntersection.ROAD_INTERSESCTION_TYPE_DOWN,
                    laneIntersections: new ZeroLaneIntersection[]{
                        allRightIntersections[1],
                        allRightIntersections[0]
                    }
                ));
        }
        else if (leftIntersectionCount == 1 && rightIntersectionCount == 1)
        {
            int intersectionType;
            if (IsRoadIntersectionLeft(controlPoints, allLeftIntersections, allRightIntersections))
                intersectionType = ZeroRoadIntersection.ROAD_INTERSESCTION_TYPE_LEFT;
            else
                intersectionType = ZeroRoadIntersection.ROAD_INTERSESCTION_TYPE_RIGHT;
            //
            roadIntersections.Add(new ZeroRoadIntersection(
                name: name,
                height: intersectionHeight,
                roadIntersectionType: intersectionType,
                laneIntersections: new ZeroLaneIntersection[]{
                allLeftIntersections[0],
                allRightIntersections[0]
                }
            ));
        }
        foreach (var intersection in roadIntersections)
            ZeroRoadBuilder.ActiveIntersections[intersection.Name] = intersection;

        return roadIntersections.ToArray();
    }

    private static bool IsRoadIntersectionLeft(
        Vector3[] controlPoints,
        ZeroLaneIntersection[] leftIntersections,
        ZeroLaneIntersection[] rightIntersections
    )
    {
        float collidingRoadWidth =
                leftIntersections[0].IntersectionPoints[0]
                .CollidingSegment.ParentLane
                .ParentRoad.WidthInclSidewalks;
        ZeroRoadSegment leftSegment = leftIntersections[0].IntersectionPoints[0].PrimarySegment;
        ZeroRoadSegment rightSegment = rightIntersections[0].IntersectionPoints[0].PrimarySegment;
        Vector3 leftStartPoint = leftIntersections[0].IntersectionPoints[0].CollisionPoint;
        Vector3 rightStartPoint = rightIntersections[0].IntersectionPoints[0].CollisionPoint;
        Vector3 leftEndPoint = leftStartPoint + leftSegment.Forward * collidingRoadWidth;
        Vector3 rightEndPoint = rightStartPoint + rightSegment.Forward * collidingRoadWidth;

        if (ZeroCollisionMap.IsPointInsideBounds(
            controlPoints[0],
            new Vector3[]{
                        leftStartPoint,
                        leftEndPoint,
                        rightEndPoint,
                        rightStartPoint}))
            return false;
        else if (ZeroCollisionMap.IsPointInsideBounds(
            controlPoints[^1],
            new Vector3[]{
                        leftStartPoint,
                        leftEndPoint,
                        rightEndPoint,
                        rightStartPoint}))
            return true;
        else
            throw new Exception("Unrecognised road intersection type");
    }


    public void RenderLaneIntersections(Color? color = null)
    {
        RenderVertices(this.LaneIntersectionPoints,
          this.Name + "LI",
         color: color);
    }

    public void RenderSidewalks(Color? color = null)
    {
        RenderVertices(this.Sidewalks,
          this.Name + "SW",
         color: color);
    }

    public void RenderCrosswalks(Color? color = null)
    {
        RenderVertices(this.CrossWalks,
          this.Name + "CW",
         color: color);
    }

    public void RenderMainSquare(Color? color = null)
    {
        RenderVertices(this.MainSquare,
          this.Name + "MS",
         color: color);
    }

    public void RenderRoadEdges(Color? color = null)
    {
        RenderVertices(this.RoadEdges,
          this.Name + "RE",
         color: color);
    }

    public Dictionary<string, Vector3> LaneIntersectionsLogPairs()
    {
        return GetVertexLogPairs(this.LaneIntersectionPoints, this.Name + "LI");
    }

    public Dictionary<string, Vector3> CrosswalksLogPairs()
    {
        return GetVertexLogPairs(this.CrossWalks, this.Name + "CW");
    }

    public Dictionary<string, Vector3> SidewalksLogPairs()
    {
        return GetVertexLogPairs(this.Sidewalks, this.Name + "SW");
    }

    public Dictionary<string, Vector3> MainSquareLogPairs()
    {
        return GetVertexLogPairs(this.MainSquare, this.Name + "MS");
    }

    public Dictionary<string, Vector3> RoadEdgesLogPairs()
    {
        return GetVertexLogPairs(this.RoadEdges, this.Name + "RE");
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
                    sphereName: prefix + i.ToString() + j.ToString(),
                    color: color ?? colors[i > colors.Length ? i - colors.Length : i]);
    }

    private Dictionary<string, Vector3> GetVertexLogPairs(Vector3[][] vertices, string prefix)
    {
        Dictionary<string, Vector3> vertexStrings = new();
        for (int i = 0; i < vertices.Length; i++)
            for (int j = 0; j < vertices[i].Length; j++)
                vertexStrings[String.Format("{0}{1}{2}", prefix, i, j)] =
                vertices[i][j];
        return vertexStrings;
    }
}
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
        float height,
        int roadIntersectionType,
        ZeroLaneIntersection[] laneIntersections)
    {
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

        int n = this.LaneIntersections.Length;
        for (int i = 0; i < n; i++)
        {
            GetLRE(
                i: i,
                n: n,
                L: out Vector3[] L,
                R: out Vector3[] R,
                E: out Vector3[] E);

            if (i % 2 == 0)
                mainSquares.Add(new Vector3[] { E[0], L[1], L[2], R[1], R[2], E[3] });
            crosswalks.Add(new Vector3[] { L[3], L[2], R[1], R[0] });
            sidewalks.AddRange(GetSidewalks(i: i, L: L, R: R, E: E));
            roadEdges.Add(E);

            //If there are only two lane intersections (it's a T intersection), 
            //then there should be only one iteration of this loop i.e. one crosswalk, two sidewalks and one main s=square part
            if (n == 2) i++;
        }
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
            Debug.LogFormat("i={0} edgePointIndex={1} Valid={2} edgePoints={3}", i, 1, IsEdgeValid(L, R, _E), _E.Select(e => e.ToString()).ToCommaSeparatedString());
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
            Debug.LogFormat("i={0} edgePointIndex={1} Valid={2} edgePoints={3}", i, 0, IsEdgeValid(L, R, _E), _E.Select(e => e.ToString()).ToCommaSeparatedString());
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
            Debug.LogFormat("i={0} edgePointIndex={1} Valid={2} edgePoints={3}", i, 3, IsEdgeValid(L, R, _E), _E.Select(e => e.ToString()).ToCommaSeparatedString());
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
            Debug.LogFormat("i={0} edgePointIndex={1} Valid={2} edgePoints={3}", i, 2, IsEdgeValid(L, R, _E), _E.Select(e => e.ToString()).ToCommaSeparatedString());
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
        Debug.LogFormat("laneWidth={0} roadWidthExclSidewalks={1}", laneWidth, roadWidthExclSidewalks);

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

        Debug.LogFormat("leftSegment={0} IsRightSidewalk={1} leftIntersectionPosition={2}",
            leftSegment.Name,
            leftSegment.ParentLane.IsRightSidewalk,
            leftIntersectionPosition);

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
        Debug.LogFormat("Vector3.Angle(L[0] - E[0], forward)={0}", Vector3.Angle(L[0] - E[0], forward));
        Debug.LogFormat("Vector3.Angle(L[3] - E[1], forward)={0}", Vector3.Angle(L[3] - E[1], forward));
        Debug.LogFormat("Vector3.Angle(R[0] - E[2], forward)={0}", Vector3.Angle(R[0] - E[2], forward));
        Debug.LogFormat("Vector3.Angle(R[3] - E[3], forward)={0}", Vector3.Angle(R[3] - E[3], forward));

        if (!E[0].Equals(L[0]) && Vector3.Angle(L[0] - E[0], forward) > 90)
            return false;
        if (!E[1].Equals(L[3]) && Vector3.Angle(L[3] - E[1], forward) > 90)
            return false;
        if (!E[2].Equals(R[0]) && Vector3.Angle(R[0] - E[2], forward) > 90)
            return false;
        if (!E[3].Equals(R[3]) && Vector3.Angle(R[3] - E[3], forward) > 90)
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

        if (!E[0].Equals(L[0]))
            leftSidewalk.Add(E[0]);
        leftSidewalk.Add(L[0]);
        leftSidewalk.Add(L[3]);
        if (!E[1].Equals(L[3]))
            leftSidewalk.Add(E[1]);
        //
        if (!E[2].Equals(R[0]))
            rightSidewalk.Add(E[2]);
        rightSidewalk.Add(R[0]);
        rightSidewalk.Add(R[3]);
        if (!E[3].Equals(R[3]))
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

        if (!E[0].Equals(L[0]))
            leftSidewalk.Add(E[0]);
        leftSidewalk.Add(L[0]);
        leftSidewalk.AddRange(
            ZeroCurvedLine.FindBazierLinePoints(
                vertexCount: 8,
                controlPoints: new Vector3[] { L[1], L[2], L[3] }
            ));
        if (!E[1].Equals(L[3]))
            leftSidewalk.Add(E[1]);
        //
        //
        if (!E[2].Equals(R[0]))
            rightSidewalk.Add(E[2]);
        rightSidewalk.AddRange(
            ZeroCurvedLine.FindBazierLinePoints(
                vertexCount: 8,
                controlPoints: new Vector3[] { R[0], R[1], R[2] }
            ));
        rightSidewalk.Add(R[3]);
        if (!E[3].Equals(R[3]))
            rightSidewalk.Add(E[3]);

        //
        return new Vector3[][] { rightSidewalk.ToArray(), leftSidewalk.ToArray() };
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

    public (string, string)[] LaneIntersectionsLogPairs()
    {
        return GetVertexLogPairs(this.LaneIntersectionPoints, this.Name + "LI");
    }

    public (string, string)[] CrosswalksLogPairs()
    {
        return GetVertexLogPairs(this.CrossWalks, this.Name + "CW");
    }

    public (string, string)[] SidewalksLogPairs()
    {
        return GetVertexLogPairs(this.Sidewalks, this.Name + "SW");
    }

    public (string, string)[] MainSquareLogPairs()
    {
        return GetVertexLogPairs(this.MainSquare, this.Name + "MS");
    }

    public (string, string)[] RoadEdgesLogPairs()
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

    private (string, string)[] GetVertexLogPairs(Vector3[][] vertices, string prefix)
    {
        List<(string, string)> vertexStrings = new();
        for (int i = 0; i < vertices.Length; i++)
            for (int j = 0; j < vertices[i].Length; j++)
                vertexStrings.Add(
                    (String.Format("{0}{1}{2}", prefix, i, j),
                    String.Format(vertices[i][j].ToString()))
                );
        return vertexStrings.ToArray();
    }
}
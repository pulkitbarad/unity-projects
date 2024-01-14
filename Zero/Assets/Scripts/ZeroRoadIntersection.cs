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
    public Vector3[][] SidewalkCorners;
    public Vector3[][] LaneIntersections;
    private readonly ZeroLaneIntersection _leftStartIntersection;
    private readonly ZeroLaneIntersection _rightStartIntersection;
    private readonly ZeroLaneIntersection _leftEndIntersection;
    private readonly ZeroLaneIntersection _rightEndIntersection;
    public float Height;
    public float IntersectingRoadWidth;
    public float PrimaryRoadWidth;

    public ZeroRoadIntersection(
        string name,
        float height,
        float primaryRoadWidth,
        float intersectingRoadWidth,
        ZeroLaneIntersection leftStartIntersection,
        ZeroLaneIntersection rightStartIntersection,
        ZeroLaneIntersection leftEndIntersection,
        ZeroLaneIntersection rightEndIntersection)
    {
        this.Name = name;
        this._leftStartIntersection = leftStartIntersection;
        this._rightStartIntersection = rightStartIntersection;
        this._leftEndIntersection = leftEndIntersection;
        this._rightEndIntersection = rightEndIntersection;
        this.PrimaryRoadWidth = primaryRoadWidth;
        this.IntersectingRoadWidth = intersectingRoadWidth;
        this.Height = height;

        GetGridBounds();
        GetSidewalkAndCrosswalks();
        GetSidewalkCorners();

        // Debug.LogFormat(
        //    "intersectingRoadName={0} right intersections={1}",
        //    leftIntersections,
        //    rightIntersections);

    }

    private void GetGridBounds()
    {
        GetLaneIntersectionGrid();
    }

    private void GetLaneIntersectionGrid()
    {
        this.LaneIntersections = new Vector3[4][];
        this.LaneIntersections[0] = new Vector3[4];
        this.LaneIntersections[1] = new Vector3[4];
        this.LaneIntersections[2] = new Vector3[4];
        this.LaneIntersections[3] = new Vector3[4];

        this.LaneIntersections[0][0] = this._leftStartIntersection.IntersectionPoints[0];
        this.LaneIntersections[0][1] = this._leftStartIntersection.IntersectionPoints[1];
        this.LaneIntersections[0][2] = this._leftStartIntersection.IntersectionPoints[2];
        this.LaneIntersections[0][3] = this._leftStartIntersection.IntersectionPoints[3];
        //
        this.LaneIntersections[1][0] = this._leftEndIntersection.IntersectionPoints[0];
        this.LaneIntersections[1][1] = this._leftEndIntersection.IntersectionPoints[1];
        this.LaneIntersections[1][2] = this._leftEndIntersection.IntersectionPoints[2];
        this.LaneIntersections[1][3] = this._leftEndIntersection.IntersectionPoints[3];
        //
        this.LaneIntersections[2][0] = this._rightEndIntersection.IntersectionPoints[0];
        this.LaneIntersections[2][1] = this._rightEndIntersection.IntersectionPoints[1];
        this.LaneIntersections[2][2] = this._rightEndIntersection.IntersectionPoints[2];
        this.LaneIntersections[2][3] = this._rightEndIntersection.IntersectionPoints[3];
        //
        this.LaneIntersections[3][0] = this._rightStartIntersection.IntersectionPoints[0];
        this.LaneIntersections[3][1] = this._rightStartIntersection.IntersectionPoints[1];
        this.LaneIntersections[3][2] = this._rightStartIntersection.IntersectionPoints[2];
        this.LaneIntersections[3][3] = this._rightStartIntersection.IntersectionPoints[3];
    }

    private void GetSidewalkAndCrosswalks()
    {
        this.Sidewalks = new Vector3[8][];
        this.CrossWalks = new Vector3[4][];
        Vector3[][] curvedSideWalks = new Vector3[4][];

        Vector3 leftToRight, rightToLeft, endToStart;
        float roadWidth;

        float CWL = ZeroRoadBuilder.RoadCrossWalkLength;
        float SWW = ZeroRoadBuilder.RoadLaneWidth;
        float CWLMinusSWW = CWL - SWW;
        Vector3 PE2S = (this.LaneIntersections[0][0] - this.LaneIntersections[0][1]).normalized;
        Vector3 IE2S = (this.LaneIntersections[0][0] - this.LaneIntersections[0][3]).normalized;
        Vector3 PL2R = Vector3.Cross(PE2S, Vector3.up);
        Vector3 IL2R = Vector3.Cross(IE2S, Vector3.up);

        var angleForwardWithRightToLeft = GetAngleForwardWithRightToLeft();

        int sidewalkIndex = 0;
        for (int intersectionIndex = 0; intersectionIndex < 4; intersectionIndex++)
        {
            Vector3[] side1Vertices = new Vector3[4];
            Vector3[] side2Vertices = new Vector3[4];
            this.CrossWalks[intersectionIndex] = new Vector3[4];
            curvedSideWalks[intersectionIndex] = new Vector3[4];

            //Point LSLS
            Vector3 leftEdgePoint =
                this.LaneIntersections
                    [intersectionIndex]
                    [intersectionIndex];
            //Point RSRE
            Vector3 rightEdgePoint =
                 this.LaneIntersections
                    [intersectionIndex == 0 ? 3 : intersectionIndex - 1]
                    [intersectionIndex == 0 ? 3 : intersectionIndex - 1];
            //Point LSRE
            Vector3 leftInsidePoint =
                this.LaneIntersections
                    [intersectionIndex]
                    [intersectionIndex > 1 ? intersectionIndex - 2 : intersectionIndex + 2];
            //Point RSLE
            Vector3 rightInsidePoint =
                 this.LaneIntersections
                    [intersectionIndex == 0 ? 3 : intersectionIndex - 1]
                    [intersectionIndex == 3 ? 0 : intersectionIndex + 1];
            if (intersectionIndex % 2 == 0)
            {
                endToStart = PE2S;
                leftToRight = PL2R;
                roadWidth = this.PrimaryRoadWidth;
            }
            else
            {
                endToStart = IE2S;
                leftToRight = IL2R;
                roadWidth = this.IntersectingRoadWidth;
            }

            if (intersectionIndex > 1)
            {
                endToStart = -endToStart;
                leftToRight = -leftToRight;
            }
            rightToLeft = -leftToRight;

            if ((intersectionIndex % 2 == 0 && angleForwardWithRightToLeft > 90)
                || (intersectionIndex % 2 > 0 && angleForwardWithRightToLeft <= 90))
            {
                side1Vertices[1] = leftEdgePoint;
                //
                side1Vertices[2] = side1Vertices[1] + leftToRight * SWW;
                side1Vertices[0] = side1Vertices[1] + endToStart * CWLMinusSWW;
                side1Vertices[3] = side1Vertices[0] + leftToRight * SWW;
                //
                //
                side2Vertices[0] = side1Vertices[3] + leftToRight * roadWidth;
                side2Vertices[2] = rightEdgePoint;
                //
                side2Vertices[1] = side2Vertices[2] + rightToLeft * SWW;
                side2Vertices[3] = side2Vertices[0] + leftToRight * SWW;
            }
            else
            {
                side2Vertices[2] = rightEdgePoint;
                //
                side2Vertices[1] = side2Vertices[2] + rightToLeft * SWW;
                side2Vertices[0] = side2Vertices[1] + endToStart * CWLMinusSWW;
                side2Vertices[3] = side2Vertices[0] + leftToRight * SWW;
                //
                //
                side1Vertices[3] = side2Vertices[0] + rightToLeft * roadWidth;
                side1Vertices[1] = leftEdgePoint;
                //
                side1Vertices[2] = side1Vertices[1] + leftToRight * SWW;
                side1Vertices[0] = side1Vertices[3] + rightToLeft * SWW;
            }
            //
            //
            this.CrossWalks[intersectionIndex][0] = this.LaneIntersections[intersectionIndex][(intersectionIndex + 3) % 4];
            this.CrossWalks[intersectionIndex][1] = this.LaneIntersections[intersectionIndex][(intersectionIndex + 2) % 4];
            this.CrossWalks[intersectionIndex][2] = this.LaneIntersections[(intersectionIndex + 3) % 4][(intersectionIndex + 1) % 4];
            this.CrossWalks[intersectionIndex][3] = this.LaneIntersections[(intersectionIndex + 3) % 4][intersectionIndex];
            //
            //
            this.Sidewalks[sidewalkIndex++] = side2Vertices;
            this.Sidewalks[sidewalkIndex++] = side1Vertices;
        }
    }

    private void GetSidewalkCorners()
    {
        this.SidewalkCorners = new Vector3[4][];

        int controlPointIndex = 0;
        int sidewalkIndex = 1;

        for (int sidewalkCornerIndex = 0;
            sidewalkCornerIndex < 4;
            sidewalkCornerIndex++)
        {
            Vector3[] currentSidewalk = this.Sidewalks[sidewalkIndex++];
            Vector3[] nextSidewalk = this.Sidewalks[sidewalkIndex == 8 ? 0 : sidewalkIndex++];

            List<Vector3> sidewalkCorner = new()
            {
                currentSidewalk[1]
            };

            if (sidewalkCornerIndex < 2)
                controlPointIndex = sidewalkCornerIndex + 2;
            else
                controlPointIndex = sidewalkCornerIndex - 2;

            var angleForwardWithRightToLeft = GetAngleForwardWithRightToLeft();
            int vertexCount = 4;
            if (
                (angleForwardWithRightToLeft > 90 && sidewalkCornerIndex % 2 == 0)
                || (angleForwardWithRightToLeft <= 90 && sidewalkCornerIndex % 2 > 0))
                vertexCount = 8;
            Vector3[] curvePoints = ZeroCurvedLine.FindBazierLinePoints(
                vertexCount: vertexCount,
                nextSidewalk[1],
                this.LaneIntersections[sidewalkCornerIndex][controlPointIndex],
                currentSidewalk[2]);

            sidewalkCorner.AddRange(curvePoints);
            this.SidewalkCorners[sidewalkCornerIndex] = sidewalkCorner.ToArray();
        }
    }

    private float GetAngleForwardWithRightToLeft()
    {
        return
            Vector3.Angle(
                this.LaneIntersections[0][0] - this.LaneIntersections[0][3],
                this.LaneIntersections[0][2] - this.LaneIntersections[0][3]);
    }

    private void GetMainSquare()
    {
    }

    public void RenderLaneIntersections(Color? color = null)
    {
        RenderVertices(this.LaneIntersections,
          this.Name + "LI",
         color: color);
    }

    public void RenderSidewalkCorners(Color? color = null)
    {
        RenderVertices(this.SidewalkCorners,
          this.Name + "SWCR",
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

    public (string, string)[] LaneIntersectionsLogPairs()
    {
        return GetVertexLogPairs(this.LaneIntersections, this.Name + "LI");
    }

    public (string, string)[] SidewalkCornersLogPairs()
    {
        return GetVertexLogPairs(this.SidewalkCorners, this.Name + "SWCR");
    }

    public (string, string)[] CrosswalksLogPairs()
    {
        return GetVertexLogPairs(this.CrossWalks, this.Name + "CW");
    }

    public (string, string)[] SidewalksLogPairs()
    {
        return GetVertexLogPairs(this.Sidewalks, this.Name + "SW");
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

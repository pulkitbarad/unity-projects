using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ZeroRoadIntersection
{

    public string Name;
    public ZeroPolygon[] NormalSidewalks;
    public ZeroPolygon[] CrossWalks;
    private Vector3[][] _cornerGrid;
    public ZeroPolygon3D RightStartCorner;
    public ZeroPolygon3D LeftEndCorner;
    public ZeroPolygon3D RightEndCorner;
    private ZeroCrossRoadGrid _grid = new();
    private readonly ZeroLaneIntersection _leftStartIntersection;
    private readonly ZeroLaneIntersection _rightStartIntersection;
    private readonly ZeroLaneIntersection _leftEndIntersection;
    private readonly ZeroLaneIntersection _rightEndIntersection;
    private float Height;
    private float IntersectingRoadWidth;
    private float PrimaryRoadWidth;

    private class ZeroCrossRoadGrid
    {
        public Vector3 PL2R;
        public Vector3 IL2R;
        public Vector3 PE2S;
        public Vector3 IE2S;
        public Vector3[][] CornerVertices;
    }


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
        GetCrosswalks();
        GetNormalSidewalksVertices();
        GetSidewalkCorners();
    }

    private void GetGridBounds()
    {
        GetLaneIntersectionGrid();
    }

    private void GetLaneIntersectionGrid()
    {
        this._cornerGrid = new Vector3[4][];
        this._cornerGrid[0] = new Vector3[4];

        this._cornerGrid[0][0] = this._leftStartIntersection.IntersectionPoints.LeftStart;
        this._cornerGrid[0][1] = this._leftStartIntersection.IntersectionPoints.LeftEnd;
        this._cornerGrid[0][2] = this._leftStartIntersection.IntersectionPoints.RightEnd;
        this._cornerGrid[0][3] = this._leftStartIntersection.IntersectionPoints.RightStart;
        //
        this._cornerGrid[1][0] = this._leftEndIntersection.IntersectionPoints.LeftStart;
        this._cornerGrid[1][1] = this._leftEndIntersection.IntersectionPoints.LeftEnd;
        this._cornerGrid[1][2] = this._leftEndIntersection.IntersectionPoints.RightEnd;
        this._cornerGrid[1][3] = this._leftEndIntersection.IntersectionPoints.RightStart;
        //
        this._cornerGrid[2][0] = this._rightEndIntersection.IntersectionPoints.LeftStart;
        this._cornerGrid[2][1] = this._rightEndIntersection.IntersectionPoints.LeftEnd;
        this._cornerGrid[2][2] = this._rightEndIntersection.IntersectionPoints.RightEnd;
        this._cornerGrid[2][3] = this._rightEndIntersection.IntersectionPoints.RightStart;
        //
        this._cornerGrid[3][0] = this._rightStartIntersection.IntersectionPoints.LeftStart;
        this._cornerGrid[3][1] = this._rightStartIntersection.IntersectionPoints.LeftEnd;
        this._cornerGrid[3][2] = this._rightStartIntersection.IntersectionPoints.RightEnd;
        this._cornerGrid[3][3] = this._rightStartIntersection.IntersectionPoints.RightStart;
    }

    private Vector3[][] GetNormalSidewalksVertices()
    {
        Vector3[][] normalSidewalks = new Vector3[8][];

        Vector3 left, endToStart;
        float roadWidth;

        float CWL = ZeroRoadBuilder.RoadCrossWalkLength;
        float SWW = ZeroRoadBuilder.RoadLaneWidth;
        Vector3 PE2S = (this._grid.CornerVertices[0][0] - this._grid.CornerVertices[0][1]).normalized;
        Vector3 IE2S = (this._grid.CornerVertices[0][0] - this._grid.CornerVertices[0][3]).normalized;
        Vector3 PL2R = Vector3.Cross(this._grid.PE2S, Vector3.up);
        Vector3 IL2R = Vector3.Cross(this._grid.IE2S, Vector3.up);

        var angleForwardAndRight =
            Vector3.Angle(
                this._grid.CornerVertices[0][0] - this._grid.CornerVertices[0][3],
                this._grid.CornerVertices[0][2] - this._grid.CornerVertices[0][3]);

        for (int i = 0; i < 4; i++)
        {
            Vector3[] leftVertices = new Vector3[4];
            Vector3[] rightVertices = new Vector3[4];
            Vector3 leftEdgePoint =
                this._cornerGrid[i][i];
            Vector3 rightEdgePoint =
                 this._cornerGrid[i == 0 ? 3 : i - 1][i == 0 ? 3 : i - 1];
            if (i % 2 == 0)
            {
                endToStart = PE2S;
                left = PL2R;
                roadWidth = this.PrimaryRoadWidth;
            }
            else
            {
                endToStart = IE2S;
                left = IL2R;
                roadWidth = this.IntersectingRoadWidth;
            }

            if (i > 1)
            {
                endToStart = -endToStart;
                left = -left;
            }

            Vector3 right = -left;

            if ((i % 2 == 0 && angleForwardAndRight > 90)
                || (i % 2 > 0 && angleForwardAndRight <= 90))
            {
                leftVertices[1] = leftEdgePoint;
                //
                leftVertices[2] = leftVertices[1] + left * SWW;
                leftVertices[0] = leftVertices[1] + endToStart * CWL;
                leftVertices[3] = leftVertices[0] + left * SWW;
                //
                //
                rightVertices[0] = leftVertices[3] + left * roadWidth;
                rightVertices[2] = rightEdgePoint;
                //
                rightVertices[1] = rightVertices[2] + right * SWW;
                rightVertices[3] = rightVertices[0] + left * SWW;
            }
            else
            {
                leftVertices[2] = leftEdgePoint;
                //
                leftVertices[1] = leftVertices[2] + right * SWW;
                leftVertices[0] = leftVertices[1] + endToStart * CWL;
                leftVertices[3] = leftVertices[0] + left * SWW;
                //
                //
                rightVertices[3] = leftVertices[0] + right * roadWidth;
                rightVertices[1] = rightEdgePoint;
                //
                rightVertices[2] = rightVertices[1] + left * SWW;
                rightVertices[0] = rightVertices[3] + right * SWW;
            }
            normalSidewalks[i] = rightVertices;
            normalSidewalks[i + 1] = leftVertices;
        }
        return normalSidewalks;
    }

    private void GetSidewalkCorners()
    {
    }

    private void GetCrosswalks()
    {
    }

    public void RenderCornerVertices(Color? color = null)
    {
        this.LeftStartCorner.RenderVertices(color ?? Color.white);
        this.RightStartCorner.RenderVertices(color ?? Color.black);
        this.LeftEndCorner.RenderVertices(color ?? Color.gray);
        this.RightEndCorner.RenderVertices(color ?? Color.cyan);
    }

    public void RenderCrosswalkVertices(Color? color = null)
    {
        this.BackCrosswalk.RenderVertices(Color.blue);
        this.FrontCrosswalk.RenderVertices(Color.yellow);
        this.LeftCrosswalk.RenderVertices(Color.red);
        this.RightCrosswalk.RenderVertices(Color.green);
    }
}

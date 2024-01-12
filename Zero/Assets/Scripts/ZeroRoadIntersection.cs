using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ZeroRoadIntersection
{

    public string Name;
    public Vector3[][] Sidewalks;
    public Vector3[][] CrossWalks;
    public Vector3[][] SidewalkCorners;
    private Vector3[][] _intersectionCornerGrid;
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
        GetSidewalkAndCrosswalks();
        GetSidewalkCorners();
    }

    private void GetGridBounds()
    {
        GetLaneIntersectionGrid();
    }

    private void GetLaneIntersectionGrid()
    {
        this._intersectionCornerGrid = new Vector3[4][];
        this._intersectionCornerGrid[0] = new Vector3[4];

        this._intersectionCornerGrid[0][0] = this._leftStartIntersection.IntersectionPoints.LeftStart;
        this._intersectionCornerGrid[0][1] = this._leftStartIntersection.IntersectionPoints.LeftEnd;
        this._intersectionCornerGrid[0][2] = this._leftStartIntersection.IntersectionPoints.RightEnd;
        this._intersectionCornerGrid[0][3] = this._leftStartIntersection.IntersectionPoints.RightStart;
        //
        this._intersectionCornerGrid[1][0] = this._leftEndIntersection.IntersectionPoints.LeftStart;
        this._intersectionCornerGrid[1][1] = this._leftEndIntersection.IntersectionPoints.LeftEnd;
        this._intersectionCornerGrid[1][2] = this._leftEndIntersection.IntersectionPoints.RightEnd;
        this._intersectionCornerGrid[1][3] = this._leftEndIntersection.IntersectionPoints.RightStart;
        //
        this._intersectionCornerGrid[2][0] = this._rightEndIntersection.IntersectionPoints.LeftStart;
        this._intersectionCornerGrid[2][1] = this._rightEndIntersection.IntersectionPoints.LeftEnd;
        this._intersectionCornerGrid[2][2] = this._rightEndIntersection.IntersectionPoints.RightEnd;
        this._intersectionCornerGrid[2][3] = this._rightEndIntersection.IntersectionPoints.RightStart;
        //
        this._intersectionCornerGrid[3][0] = this._rightStartIntersection.IntersectionPoints.LeftStart;
        this._intersectionCornerGrid[3][1] = this._rightStartIntersection.IntersectionPoints.LeftEnd;
        this._intersectionCornerGrid[3][2] = this._rightStartIntersection.IntersectionPoints.RightEnd;
        this._intersectionCornerGrid[3][3] = this._rightStartIntersection.IntersectionPoints.RightStart;
    }

    private void GetSidewalkAndCrosswalks()
    {
        this.Sidewalks = new Vector3[8][];
        this.CrossWalks = new Vector3[4][];
        Vector3[][] curvedSideWalks = new Vector3[4][];

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
            this.CrossWalks[i] = new Vector3[4];
            curvedSideWalks[i] = new Vector3[4];

            Vector3 leftEdgePoint =
                this._intersectionCornerGrid[i][i];
            Vector3 rightEdgePoint =
                 this._intersectionCornerGrid[i == 0 ? 3 : i - 1][i == 0 ? 3 : i - 1];
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
                //
                //
                this.CrossWalks[i][0] = leftVertices[3];
                this.CrossWalks[i][1] = leftVertices[2];
                this.CrossWalks[i][2] = this.CrossWalks[i][1] + right * roadWidth;
                this.CrossWalks[i][3] = this.CrossWalks[i][0] + right * roadWidth;
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
                //
                //
                this.CrossWalks[i][2] = rightVertices[1];
                this.CrossWalks[i][3] = rightVertices[0];
                this.CrossWalks[i][0] = this.CrossWalks[i][3] + left * roadWidth;
                this.CrossWalks[i][1] = this.CrossWalks[i][2] + left * roadWidth;
            }
            this.Sidewalks[i] = rightVertices;
            this.Sidewalks[i + 1] = leftVertices;
        }
    }

    private void GetSidewalkCorners()
    {

        this.SidewalkCorners = new Vector3[4][];

        int sidewalkCornerIndex = 0;
        int controlPointIndex = 0;

        for (int i = 0; i < 8; i++, sidewalkCornerIndex++)
        {
            Vector3[] currentSidewalk = this.Sidewalks[i++];
            Vector3[] nextSidewalk = this.Sidewalks[i];
            List<Vector3> sidewalkCorner = new()
            {
                currentSidewalk[1],
                nextSidewalk[1]
            };

            if (sidewalkCornerIndex < 2)
                controlPointIndex = sidewalkCornerIndex + 2;
            else if (sidewalkCornerIndex == 2)
                controlPointIndex = 0;
            else if (sidewalkCornerIndex == 3)
                controlPointIndex = 1;

            Vector3[] curvePoints = ZeroCurvedLine.FindBazierLinePoints(
                vertexCount: 6,
                nextSidewalk[1],
                this._intersectionCornerGrid[i][controlPointIndex],
                currentSidewalk[2]);

            sidewalkCorner.AddRange(curvePoints);
            sidewalkCorner.Add(currentSidewalk[2]);
            this.SidewalkCorners[sidewalkCornerIndex] = sidewalkCorner.ToArray();
        }
    }

    private void GetMainSquare()
    {
    }

    public void RenderCornerVertices(Color? color = null)
    {
        Color[] colors = new Color[]{
            Color.white,
            Color.black,
            Color.gray,
            Color.cyan
        };
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < this.SidewalkCorners[i].Length; j++)
                ZeroRenderer.RenderSphere(
                    position: this.SidewalkCorners[i][j],
                    sphereName: this.Name + "CR" + j++.ToString(),
                    color: color ?? colors[i]);
    }

    public void RenderSidewalkVertices(Color? color = null)
    {
        Color[] colors = new Color[]{
            Color.blue,
            Color.yellow,
            Color.red,
            Color.green,
            //
            Color.white,
            Color.black,
            Color.gray,
            Color.cyan
        };
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < this.Sidewalks[i].Length; j++)
                ZeroRenderer.RenderSphere(
                    position: this.CrossWalks[i][j],
                    sphereName: this.Name + "SW" + j++.ToString(),
                    color: color ?? colors[i]);
    }
    public void RenderCrosswalkVertices(Color? color = null)
    {
        Color[] colors = new Color[]{
            Color.blue,
            Color.yellow,
            Color.red,
            Color.green
        };
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < this.CrossWalks[i].Length; j++)
                ZeroRenderer.RenderSphere(
                    position: this.CrossWalks[i][j],
                    sphereName: this.Name + "CW" + j++.ToString(),
                    color: color ?? colors[i]);
    }
}

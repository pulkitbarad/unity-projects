using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroRoadIntersection
{

    public string Name;
    public ZeroParallelogram BackCrosswalk;
    public ZeroParallelogram FrontCrosswalk;
    public ZeroParallelogram LeftCrosswalk;
    public ZeroParallelogram RightCrosswalk;
    public ZeroPolygon3D LeftStartCorner;
    public ZeroPolygon3D RightStartCorner;
    public ZeroPolygon3D LeftEndCorner;
    public ZeroPolygon3D RightEndCorner;
    private ZeroCrossRoadGrid _grid = new();
    private readonly ZeroLaneIntersection _leftStartIntersection;
    private readonly ZeroLaneIntersection _rightStartIntersection;
    private readonly ZeroLaneIntersection _leftEndIntersection;
    private readonly ZeroLaneIntersection _rightEndIntersection;

    public class ZeroCrossRoadGrid
    {
        public Vector3 LSLS;
        public Vector3 LSLE;
        public Vector3 LSRS;
        public Vector3 LSRE;
        //
        public Vector3 LELS;
        public Vector3 LELE;
        public Vector3 LERS;
        public Vector3 LERE;
        //
        public Vector3 RSLS;
        public Vector3 RSLE;
        public Vector3 RSRS;
        public Vector3 RSRE;
        //
        public Vector3 RELS;
        public Vector3 RELE;
        public Vector3 RERS;
        public Vector3 RERE;

        //Midpoints for the sidewalk corner shapes
        //
        public Vector3 LSEM;
        public Vector3 LSRM;
        public Vector3 RSLM;
        public Vector3 RSEM;
        //
        public Vector3 RESM;
        public Vector3 RELM;
        public Vector3 LESM;
        public Vector3 LERM;

    }

    public ZeroRoadIntersection(
        string name,
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
        GetGridBounds();
        GetCrosswalks();
        GetSidewalkCorners();
    }

    private void GetGridBounds()
    {
        this._grid.LSLS = this._leftStartIntersection.IntersectionPoints.LeftStart;
        this._grid.LSLE = this._leftStartIntersection.IntersectionPoints.LeftEnd;
        this._grid.LSRS = this._leftStartIntersection.IntersectionPoints.RightStart;
        this._grid.LSRE = this._leftStartIntersection.IntersectionPoints.RightEnd;
        //
        this._grid.LELS = this._leftEndIntersection.IntersectionPoints.LeftStart;
        this._grid.LELE = this._leftEndIntersection.IntersectionPoints.LeftEnd;
        this._grid.LERS = this._leftEndIntersection.IntersectionPoints.RightStart;
        this._grid.LERE = this._leftEndIntersection.IntersectionPoints.RightEnd;
        //
        this._grid.RSLS = this._rightStartIntersection.IntersectionPoints.LeftStart;
        this._grid.RSLE = this._rightStartIntersection.IntersectionPoints.LeftEnd;
        this._grid.RSRS = this._rightStartIntersection.IntersectionPoints.RightStart;
        this._grid.RSRE = this._rightStartIntersection.IntersectionPoints.RightEnd;
        //
        this._grid.RELS = this._rightEndIntersection.IntersectionPoints.LeftStart;
        this._grid.RELE = this._rightEndIntersection.IntersectionPoints.LeftEnd;
        this._grid.RERS = this._rightEndIntersection.IntersectionPoints.RightStart;
        this._grid.RERE = this._rightEndIntersection.IntersectionPoints.RightEnd;
        //
        //
        this._grid.LSEM = this._grid.LSRE + 0.5f * (this._grid.LSLE - this._grid.LSRE);
        this._grid.LSRM = this._grid.LSRS + 0.5f * (this._grid.LSRE - this._grid.LSRS);
        //
        this._grid.LSEM = this._grid.LSRE + 0.5f * (this._grid.LSLE - this._grid.LSRE);
        this._grid.LSRM = this._grid.LSRS + 0.5f * (this._grid.LSRE - this._grid.LSRS);
        //
        this._grid.RSLM = this._grid.RSLS + 0.5f * (this._grid.RSLE - this._grid.RSLS);
        this._grid.RSEM = this._grid.RSRE + 0.5f * (this._grid.RSLE - this._grid.RSRE);
        //
        this._grid.RELM = this._grid.RELS + 0.5f * (this._grid.RELE - this._grid.RELS);
        this._grid.RESM = this._grid.RERS + 0.5f * (this._grid.RELS - this._grid.RERS);
        //
        this._grid.LESM = this._grid.LELS + 0.5f * (this._grid.LERS - this._grid.LELS);
        this._grid.LERM = this._grid.LERS + 0.5f * (this._grid.LERE - this._grid.LERS);
    }

    private void GetSidewalkCorners()
    {

        this.LeftStartCorner =
            new ZeroPolygon3D(this.Name + "LSC",
                new Vector3[] {
                    this._grid.LSRM,
                    this._grid.LSRS,
                    this._grid.LSLS,
                    this._grid.LSLE,
                    this._grid.LSEM
                },
                new int[] { 0, 1, 2, 0, 2, 4, 4, 2, 3 });
        this.RightStartCorner =
            new ZeroPolygon3D(this.Name + "RSC",
                new Vector3[] {
                    this._grid.RSLM,
                    this._grid.RSLS,
                    this._grid.RSRS,
                    this._grid.RSRE,
                    this._grid.RSEM
                },
                new int[] { 2, 1, 0, 2, 0, 4, 4, 3, 2 });
        this.RightEndCorner =
            new ZeroPolygon3D(this.Name + "REC",
                new Vector3[] {
                    this._grid.RESM,
                    this._grid.RERS,
                    this._grid.RERE,
                    this._grid.RELE,
                    this._grid.RELM
                },
                new int[] { 1, 0, 2, 2, 0, 4, 4, 3, 2 });
        this.LeftEndCorner =
            new ZeroPolygon3D(
                this.Name + "LEC",
                new Vector3[] {
                    this._grid.LERM,
                    this._grid.LERE,
                    this._grid.LELE,
                    this._grid.LELS,
                    this._grid.LESM
                },
                new int[] { 0, 2, 1, 0, 4, 2, 4, 3, 2 });
    }

    private void GetCrosswalks()
    {
        this.LeftCrosswalk =
            new ZeroParallelogram(
                name: this.Name + "LCW",
                leftStart: this._grid.LELS,
                rightStart: this._grid.LSLE,
                leftEnd: this._grid.LERS,
                rightEnd: this._grid.LSRE
            );
        this.RightCrosswalk =
            new ZeroParallelogram(
                name: this.Name + "RCW",
                leftStart: this._grid.RSRE,
                rightStart: this._grid.RERS,
                leftEnd: this._grid.RSLE,
                rightEnd: this._grid.RELS
            );
        this.FrontCrosswalk =
            new ZeroParallelogram(
                name: this.Name + "FCW",
                leftStart: this._grid.RELE,
                rightStart: this._grid.LERE,
                leftEnd: this._grid.RELS,
                rightEnd: this._grid.LERS
            );
        this.BackCrosswalk =
            new ZeroParallelogram(
                name: this.Name + "BCW",
                leftStart: this._grid.LSRS,
                rightStart: this._grid.RSLS,
                leftEnd: this._grid.LSRE,
                rightEnd: this._grid.RSLE
            );
    }
    public override string ToString()
    {
        return "ZeroRoadIntersection("
        + "\n Name:" + this.Name
        + "\n FrontCrosswalk:" + this.FrontCrosswalk.ToString()
        + "\n BackCrosswalk:" + this.BackCrosswalk.ToString()
        + "\n LeftCrosswalk:" + this.LeftCrosswalk.ToString()
        + "\n RightCrosswalk:" + this.RightCrosswalk.ToString()
        + "\n LeftStartCorner:" + this.LeftStartCorner.ToString()
        + "\n RightStartCorner:" + this.RightStartCorner.ToString()
        + "\n LeftEndCorner:" + this.LeftEndCorner.ToString()
        + "\n RightEndCorner:" + this.RightEndCorner.ToString()
        + ")";
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

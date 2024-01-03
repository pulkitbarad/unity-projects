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
    public ZeroTriangle LeftStartCorner;
    public ZeroTriangle RightStartCorner;
    public ZeroTriangle LeftEndCorner;
    public ZeroTriangle RightEndCorner;
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
    }

    private void GetSidewalkCorners()
    {
        this.LeftStartCorner = new ZeroTriangle(this.Name + "LSC", this._grid.LSRS, this._grid.LSLS, this._grid.LSLE);
        this.RightEndCorner = new ZeroTriangle(this.Name + "REC", this._grid.RELE, this._grid.RERE, this._grid.RERS);
        this.LeftEndCorner = new ZeroTriangle(this.Name + "LEC", this._grid.LELS, this._grid.LELE, this._grid.LERE);
        this.RightStartCorner = new ZeroTriangle(this.Name + "RSC", this._grid.RSRE, this._grid.RSRS, this._grid.RSLS);
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

    public void RenderVertices()
    {
        this.FrontCrosswalk.RenderVertices(Color.yellow);
        this.BackCrosswalk.RenderVertices(Color.blue);
        this.LeftCrosswalk.RenderVertices(Color.red);
        this.RightCrosswalk.RenderVertices(Color.green);
        this.LeftStartCorner.RenderVertices(Color.white);
        this.RightStartCorner.RenderVertices(Color.black);
        this.LeftEndCorner.RenderVertices(Color.gray);
        this.RightEndCorner.RenderVertices(Color.cyan);
    }
}

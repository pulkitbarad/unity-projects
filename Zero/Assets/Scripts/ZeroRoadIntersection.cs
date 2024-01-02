using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroRoadIntersection
{
    public ZeroParallelogram BackCrosswalk;
    public ZeroParallelogram FrontCrosswalk;
    public ZeroParallelogram LeftCrosswalk;
    public ZeroParallelogram RightCrosswalk;
    public ZeroTriangle LeftStartCorner;
    public ZeroTriangle RightStartCorner;
    public ZeroTriangle LeftEndCorner;
    public ZeroTriangle RightEndCorner;
    public float PrimaryLengthSoFar;
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
        float primaryLengthSoFar,
        ZeroLaneIntersection leftStartIntersection,
        ZeroLaneIntersection rightStartIntersection,
        ZeroLaneIntersection leftEndIntersection,
        ZeroLaneIntersection rightEndIntersection)
    {
        this.PrimaryLengthSoFar = primaryLengthSoFar;
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
        this.LeftStartCorner = new ZeroTriangle(this._grid.LSRS, this._grid.LSLS, this._grid.LSLE);
        this.LeftEndCorner = new ZeroTriangle(this._grid.LELS, this._grid.LELE, this._grid.LERE);
        this.RightEndCorner = new ZeroTriangle(this._grid.RELE, this._grid.RERE, this._grid.RERS);
        this.RightStartCorner = new ZeroTriangle(this._grid.RSRE, this._grid.RSRS, this._grid.RSLS);
    }

    private void GetCrosswalks()
    {
        this.LeftCrosswalk =
            new ZeroParallelogram(
                leftStart: this._grid.LELS,
                rightStart: this._grid.LSLE,
                leftEnd: this._grid.LERS,
                rightEnd: this._grid.LSRE
            );
        this.FrontCrosswalk =
            new ZeroParallelogram(
                leftStart: this._grid.RELE,
                rightStart: this._grid.LERE,
                leftEnd: this._grid.RELS,
                rightEnd: this._grid.LERS
            );
        this.RightCrosswalk =
            new ZeroParallelogram(
                leftStart: this._grid.RSRE,
                rightStart: this._grid.RERS,
                leftEnd: this._grid.RSLE,
                rightEnd: this._grid.RELS
            );
        this.BackCrosswalk =
            new ZeroParallelogram(
                leftStart: this._grid.LSRS,
                rightStart: this._grid.RSLS,
                leftEnd: this._grid.LSRE,
                rightEnd: this._grid.RSLE
            );
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroRoadIntersection
{

    public string Name;
    public ZeroPolygon BackCrosswalk;
    public ZeroPolygon FrontCrosswalk;
    public ZeroPolygon LeftCrosswalk;
    public ZeroPolygon RightCrosswalk;
    public ZeroPolygon3D LeftStartCorner;
    public ZeroPolygon3D RightStartCorner;
    public ZeroPolygon3D LeftEndCorner;
    public ZeroPolygon3D RightEndCorner;
    private ZeroCrossRoadGrid _grid = new();
    private readonly ZeroLaneIntersection _leftStartIntersection;
    private readonly ZeroLaneIntersection _rightStartIntersection;
    private readonly ZeroLaneIntersection _leftEndIntersection;
    private readonly ZeroLaneIntersection _rightEndIntersection;

    private class ZeroCrossRoadGrid
    {
        public Vector3 PL2R;
        public Vector3 PS2E;
        public Vector3 IL2R;
        public Vector3 IS2E;
        public Vector3 PR2L;
        public Vector3 PE2S;
        public Vector3 IR2L;
        public Vector3 IE2S;
        public float IRW;
        public float PRW;
        public float CWL;
        public float SWW;

        // LSLS = Left Start lane intersection-> Left Start point
        public Vector3 LSLS, LSLE, LSRS, LSRE;
        //
        public Vector3 LELS, LELE, LERS, LERE;
        //
        public Vector3 RSLS, RSLE, RSRS, RSRE;
        //
        public Vector3 RELS, RELE, RERS, RERE;

        // LLLS = Left crosswalk-> Left sidewalk-> Left Start point
        public Vector3 LLLS, LLLE, LLRS, LLRE;
        //
        public Vector3 LRLS, LRLE, LRRS, LRRE;
        //
        public Vector3 RLLS, RLLE, RLRS, RLRE;
        //
        public Vector3 RRLS, RRLE, RRRS, RRRE;
        //
        public Vector3 BLLS, BLLE, BLRS, BLRE;
        //
        public Vector3 BRLS, BRLE, BRRS, BRRE;
        //
        public Vector3 FLLS, FLLE, FLRS, FLRE;
        //
        public Vector3 FRLS, FRLE, FRRS, FRRE;
    }


    public ZeroRoadIntersection(
        string name,
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
        this._grid.PS2E = (this._grid.LSLE - this._grid.LSLS).normalized;
        this._grid.IS2E = (this._grid.LSRS - this._grid.LSLS).normalized;
        this._grid.PL2R = Vector3.Cross(this._grid.PS2E, Vector3.down);
        this._grid.IL2R = Vector3.Cross(this._grid.IS2E, Vector3.down);
        this._grid.PRW = primaryRoadWidth;
        this._grid.IRW = intersectingRoadWidth;
        this._grid.CWL = ZeroRoadBuilder.RoadCrossWalkLength;
        this._grid.SWW = ZeroRoadBuilder.RoadLaneWidth;

        GetGridBounds();
        GetCrosswalks();
        GetSidewalkCorners();
    }

    private void GetGridBounds()
    {
        GetLaneIntersectionGrid();
    }

    private void GetLaneIntersectionGrid()
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

    private void GetNormalSidewalkGrid()
    {
        this._grid.BLLE = this._grid.LSLS;
        this._grid.BLLS = this._grid.LSLS + this._grid.PE2S * this._grid.CWL;
        this._grid.BLRE = this._grid.BLLE + this._grid.PL2R * this._grid.SWW;
        this._grid.BLRS = this._grid.BLLS + this._grid.PL2R * this._grid.SWW;
        //
        this._grid.BRRE = this._grid.RSRS;
        this._grid.BRLS = this._grid.BLRS + this._grid.PL2R * this._grid.PRW;
        this._grid.BRRS = this._grid.BRLS + this._grid.PL2R * this._grid.SWW;
        this._grid.BRLE = this._grid.BRRE + this._grid.PR2L * this._grid.SWW;
        //
        //
        this._grid.LRRE = this._grid.LSLS;
        this._grid.LRRS = this._grid.LRRE + this._grid.IE2S * this._grid.CWL;
        this._grid.LRLE = this._grid.LRRE + this._grid.IR2L * this._grid.SWW;
        this._grid.LRLS = this._grid.LRRS + this._grid.IR2L * this._grid.SWW;
        //
        this._grid.LLLE = this._grid.LELE;
        this._grid.LLRS = this._grid.LRLS + this._grid.IR2L * this._grid.IRW;
        this._grid.LLRE = this._grid.LLLE + this._grid.IL2R * this._grid.SWW;
        this._grid.LLLS = this._grid.LLRS + this._grid.IR2L * this._grid.SWW;
        //
        //
        this._grid.FLLE = this._grid.RERE;
        this._grid.FLLS = this._grid.FLLE + this._grid.PS2E * this._grid.CWL;
        this._grid.FLRE = this._grid.FLLE + this._grid.PR2L * this._grid.SWW;
        this._grid.FLRS = this._grid.FLLS + this._grid.PR2L * this._grid.SWW;
        //
        this._grid.FRRE = this._grid.LELE;
        this._grid.FRLE = this._grid.FRRE + this._grid.PL2R * this._grid.SWW;
        this._grid.FRLS = this._grid.FLRS + this._grid.PR2L * this._grid.PRW;
        this._grid.FRRS = this._grid.FRLS + this._grid.PR2L * this._grid.SWW;
        //
        //
        this._grid.RRRE = this._grid.RERE;
        this._grid.RRRS = this._grid.RRRE + this._grid.IS2E * this._grid.CWL;
        this._grid.RRLE = this._grid.RRRE + this._grid.IL2R * this._grid.SWW;
        this._grid.RRLS = this._grid.RRRS + this._grid.IL2R * this._grid.SWW;
        //
        this._grid.RLLE = this._grid.RSRS;
        this._grid.RLRS = this._grid.RRLS + this._grid.IL2R * this._grid.IRW;
        this._grid.RLRE = this._grid.RLLE + this._grid.IR2L * this._grid.SWW;
        this._grid.RLLS = this._grid.RLRS + this._grid.IL2R * this._grid.SWW;

    }

    private void GetSidewalkCorners()
    {

        this.LeftStartCorner =
            new ZeroPolygon3D(
                name: this.Name + "LSC",
                height: _leftStartIntersection.PrimaryLane.Height,
                topVertices: new Vector3[] {
                    this._grid.LSRM,
                    this._grid.LSRS,
                    this._grid.LSLS,
                    this._grid.LSLE,
                    this._grid.LSEM
                });
        this.RightStartCorner =
            new ZeroPolygon3D(
                name: this.Name + "RSC",
                height: _leftStartIntersection.PrimaryLane.Height,
                topVertices: new Vector3[] {
                    this._grid.RSLM,
                    this._grid.RSLS,
                    this._grid.RSRS,
                    this._grid.RSRE,
                    this._grid.RSEM
                });
        this.RightEndCorner =
            new ZeroPolygon3D(
                name: this.Name + "REC",
                height: _leftStartIntersection.PrimaryLane.Height,
                topVertices: new Vector3[] {
                    this._grid.RESM,
                    this._grid.RERS,
                    this._grid.RERE,
                    this._grid.RELE,
                    this._grid.RELM
                });
        this.LeftEndCorner =
            new ZeroPolygon3D(
                this.Name + "LEC",
                 _leftStartIntersection.PrimaryLane.Height,
                new Vector3[] {
                    this._grid.LERM,
                    this._grid.LERE,
                    this._grid.LELE,
                    this._grid.LELS,
                    this._grid.LESM
                });
    }

    private void GetCrosswalks()
    {
        this.LeftCrosswalk =
            new ZeroPolygon(
                name: this.Name + "LCW",
                 this._grid.LELS,
                 this._grid.LERS,
                this._grid.LSRE,
                this._grid.LSLE
            );
        this.RightCrosswalk =
            new ZeroPolygon(
                name: this.Name + "RCW",
                this._grid.RSRE,
                this._grid.RSLE,
                this._grid.RELS,
                this._grid.RERS
            );
        this.FrontCrosswalk =
            new ZeroPolygon(
                name: this.Name + "FCW",
                this._grid.RELE,
                this._grid.RELS,
                this._grid.LERS,
                this._grid.LERE
            );
        this.BackCrosswalk =
            new ZeroPolygon(
                name: this.Name + "BCW",
                this._grid.LSRS,
                this._grid.LSRE,
                this._grid.RSLE,
                this._grid.RSLS
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

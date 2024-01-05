using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ZeroPolygon3D
{
    public string Name;
    public ZeroParallelogram TopPlane;
    public ZeroParallelogram BottomPlane;
    public ZeroParallelogram LeftPlane;
    public ZeroParallelogram RightPlane;
    public ZeroParallelogram BackPlane;
    public ZeroParallelogram FrontPlane;

    public ZeroPolygon3D(
        string name,
        float height,
        Vector3 up,
        ZeroParallelogram centerPlane)
    {
        this.Name = name;
        Vector3 halfUp = 0.5f * height * up;
        Vector3 halfDown = -0.5f * height * up;
        this.TopPlane =
            new(
                name: this.Name + "T",
                leftStart: centerPlane.LeftStart + halfUp,
                rightStart: centerPlane.RightStart + halfUp,
                leftEnd: centerPlane.LeftEnd + halfUp,
                rightEnd: centerPlane.RightEnd + halfUp);

        this.BottomPlane =
            new(
                name: this.Name + "B",
                leftStart: centerPlane.LeftStart + halfDown,
                rightStart: centerPlane.RightStart + halfDown,
                leftEnd: centerPlane.LeftEnd + halfDown,
                rightEnd: centerPlane.RightEnd + halfDown);

        AssignLeftPlane();
        AssignRightPlane();
        AssignFrontPlane();
        AssignBackPlane();
    }

    public ZeroPolygon3D(
        string name,
        ZeroParallelogram topPlane,
        ZeroParallelogram bottomPlane)
    {
        this.Name = name;
        this.TopPlane = topPlane;
        this.BottomPlane = bottomPlane;
        AssignLeftPlane();
        AssignRightPlane();
        AssignFrontPlane();
        AssignBackPlane();
    }

    public void AssignLeftPlane()
    {
        this.LeftPlane = new ZeroParallelogram(
            name: this.Name + "L",
            leftStart: this.TopPlane.LeftStart,
            rightStart: this.BottomPlane.LeftStart,
            leftEnd: this.TopPlane.LeftEnd,
            rightEnd: this.BottomPlane.LeftEnd
        );
    }

    public void AssignRightPlane()
    {
        this.RightPlane = new ZeroParallelogram(
            name: this.Name + "R",
            leftStart: this.TopPlane.RightStart,
            rightStart: this.BottomPlane.RightStart,
            leftEnd: this.TopPlane.RightEnd,
            rightEnd: this.BottomPlane.RightEnd
        );
    }

    public void AssignFrontPlane()
    {
        this.FrontPlane = new ZeroParallelogram(
            name: this.Name + "F",
            leftStart: this.TopPlane.RightEnd,
            rightStart: this.BottomPlane.RightEnd,
            leftEnd: this.TopPlane.LeftEnd,
            rightEnd: this.BottomPlane.LeftEnd
        );
    }

    public void AssignBackPlane()
    {
        this.BackPlane = new ZeroParallelogram(
            name: this.Name + "K",
            leftStart: this.TopPlane.LeftStart,
            rightStart: this.BottomPlane.LeftStart,
            leftEnd: this.TopPlane.RightStart,
            rightEnd: this.BottomPlane.RightStart
        );
    }
}

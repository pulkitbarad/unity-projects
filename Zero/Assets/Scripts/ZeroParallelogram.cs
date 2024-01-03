using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroParallelogram
{
    public string Name;
    public Vector3 LeftStart;
    public Vector3 LeftEnd;
    public Vector3 RightStart;
    public Vector3 RightEnd;
    public Vector3 Direction;

    public ZeroParallelogram(
        string name,
        Vector3 leftStart,
        Vector3 rightStart,
        Vector3 leftEnd,
        Vector3 rightEnd
        )
    {
        this.Name = name;
        this.LeftStart = leftStart;
        this.LeftEnd = leftEnd;
        this.RightStart = rightStart;
        this.RightEnd = rightEnd;
        this.Direction = (this.LeftEnd - this.LeftStart).normalized;
    }

    public Vector3[] GetVertices()
    {
        return new Vector3[]{
                this.LeftStart,
                this.RightStart,
                this.LeftEnd,
                this.RightEnd};
    }

    public void MoveVertices(Vector3 scalar)
    {

        this.LeftStart += scalar;
        this.LeftEnd += scalar;
        this.RightStart += scalar;
        this.RightEnd += scalar;
    }
    public override string ToString()
    {
        return "ZeroParallelogram("
        + " Name:" + this.Name
        + " LeftStart:" + this.LeftStart
        + ", RightStart:" + this.RightStart
        + ", LeftEnd:" + this.LeftEnd
        + ", RightEnd:" + this.RightEnd
        + ")";
    }
    public void RenderVertices(Color? color = null)
    {
        Color newColor = color.HasValue ? color.Value : Color.yellow;
        ZeroRenderer.RenderSphere(this.LeftStart, this.Name + "LeftStart", color: newColor);
        ZeroRenderer.RenderSphere(this.RightStart, this.Name + "RightStart", color: newColor);
        ZeroRenderer.RenderSphere(this.LeftEnd, this.Name + "LeftEnd", color: newColor);
        ZeroRenderer.RenderSphere(this.RightEnd, this.Name + "RightEnd", color: newColor);
    }
}

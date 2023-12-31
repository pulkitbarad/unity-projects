using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroParallelogram
{
    public Vector3 LeftStart;
    public Vector3 LeftEnd;
    public Vector3 RightStart;
    public Vector3 RightEnd;
    public Vector3 Direction;

    public ZeroParallelogram(
        Vector3 leftStart,
        Vector3 rightStart,
        Vector3 leftEnd,
        Vector3 rightEnd
        )
    {
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
}

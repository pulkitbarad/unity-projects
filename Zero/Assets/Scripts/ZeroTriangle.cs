using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroTriangle
{

    public string Name;
    public Vector3[] Vertices;

    public ZeroTriangle(string name, params Vector3[] vertices)
    {
        this.Name = name;
        this.Vertices = vertices;
    }

    public Vector3[] GetVertices()
    {
        return Vertices;
    }
    public override string ToString()
    {
        return "ZeroTriangle("
        + " Name:" + this.Name
        + " 0:" + this.Vertices[0]
        + ", 1:" + this.Vertices[1]
        + ", 2:" + this.Vertices[2]
        + ")";
    }
    public void RenderVertices(Color? color = null)
    {
        Color newColor = color.HasValue ? color.Value : Color.yellow;
        ZeroRenderer.RenderSphere(this.Vertices[0], this.Name + "0", color: newColor);
        ZeroRenderer.RenderSphere(this.Vertices[1], this.Name + "1", color: newColor);
        ZeroRenderer.RenderSphere(this.Vertices[2], this.Name + "2", color: newColor);
    }
}

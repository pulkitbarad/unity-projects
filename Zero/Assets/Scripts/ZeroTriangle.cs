using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroPolygon3D
{

    public string Name;
    public Vector3[] Vertices;
    public Vector3[] Triangles;

    public ZeroPolygon3D(string name, Vector3[] vertices, Vector3[] triangles)
    {
        this.Name = name;
        this.Vertices = vertices;
        this.Triangles = triangles;
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

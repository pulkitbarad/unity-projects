using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ZeroPolygon3D
{
    public string Name;
    public Vector3[] Vertices;
    public int[] Triangles;

    public ZeroPolygon3D(string name, Vector3[] vertices, int[] triangles)
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
        + "(" + this.Vertices.Select(e => e.ToString()).ToCommaSeparatedString()
        + ")";
    }

    public void RenderVertices(Color? color = null)
    {
        int i = 0;
        foreach (var point in this.Vertices)
            ZeroRenderer.RenderSphere(point, this.Name + (i++).ToString(), color: color ?? Color.yellow);
    }
}

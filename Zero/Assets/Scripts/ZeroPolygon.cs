using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

//
// Summary:
//     Convex Polygon with mesh vertices and triangles 
public class ZeroPolygon
{
    public string Name;
    //
    // Summary:
    //     The first clockwise bounding vertex
    public Vector3 LeftStart;
    //
    // Summary:
    //     The second clockwise bounding vertex
    public Vector3 LeftEnd;
    //
    // Summary:
    //     fourth clockwise bounding vertex
    public Vector3 RightEnd;
    //
    // Summary:
    //     The fourth clockwise bounding vertex
    public Vector3 RightStart;

    //
    // Summary:
    //     All clockwise bounding vertices in world position
    public Vector3[] Vertices;

    public int VertexCount;

    public ZeroPolygon(
        string name,
        params Vector3[] vertices
        )
    {
        this.Name = name;
        this.Vertices = vertices;
        this.VertexCount = vertices.Length;
        this.LeftStart = vertices[0];
        this.LeftEnd = vertices[1];
        this.RightEnd = vertices[2];
        this.RightStart = vertices[3];
    }

    public override string ToString()
    {
        return "ZeroPolygon("
        + " Name:" + this.Name
        + "(" + this.Vertices.Select(e => e.ToString()).ToCommaSeparatedString()
        + ")";
    }

    public void RenderVertices(Color? color = null)
    {
        int i = 0;
        foreach (var point in this.Vertices)
            ZeroRenderer.RenderSphere(point, this.Name + i++.ToString(), color: color ?? Color.yellow);
    }

    //
    // Summary:
    //     param gameObject:Gameobject with a position vector assigned
    //     param verticesWorld: all vertice of the polygon in world positions
    // 
    public static Vector3[] GetMeshVertices(GameObject gameObject, Vector3[] verticesWorld)
    {
        return verticesWorld
            .Select(
                (point) =>
                {
                    //round the vector's floating numbers to two decimals
                    Vector3 localPoint = gameObject.transform.InverseTransformPoint(point);
                    Vector3 newPoint =
                        new(
                            (float)Math.Round(localPoint.x, 2),
                            (float)Math.Round(localPoint.y, 2),
                            (float)Math.Round(localPoint.z, 2));
                    return newPoint;
                }
                ).ToArray();
    }

    public static int[] GetMeshTriangles(int[] vertexPositions)
    {
        int vertexCount = vertexPositions.Length;
        int numOfTriangleVertices = (vertexCount - 2) * 3;

        int[] triangles = new int[numOfTriangleVertices];
        for (int i = 0; i < numOfTriangleVertices; i++)
        {
            triangles[i] = vertexPositions[0];
            triangles[i] = vertexPositions[i + 1];
            triangles[i] = vertexPositions[i + 2];
        }
        return triangles;
    }
}

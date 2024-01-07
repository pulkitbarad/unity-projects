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
        params Vector3[] clockwiseVertices
        )
    {
        this.Name = name;
        this.Vertices = clockwiseVertices;
        this.VertexCount = clockwiseVertices.Length;
        this.LeftStart = clockwiseVertices[0];
        this.LeftEnd = clockwiseVertices[1];
        this.RightEnd = clockwiseVertices[2];
        this.RightStart = clockwiseVertices[3];
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
        GameObject parent = ZeroObjectManager.FindOrCreateGameObject(this.Name);
        foreach (Transform child in parent.transform)
            ZeroObjectManager.ReleaseObjectToPool(child.gameObject, ZeroObjectManager.OBJECT_TYPE_DEBUG_SPHERE);

        int i = 0;
        foreach (var point in this.Vertices)
            ZeroRenderer.RenderSphere(
                position: point,
                sphereName: this.Name + i++.ToString(),
                parentTransform: parent.transform,
                color: color ?? Color.yellow);
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
        int numOfTriangles = vertexPositions.Length - 2;
        int[] triangles = new int[numOfTriangles * 3];

        int triangleVertexIndex = 0;
        for (int triangleIndex = 0; triangleIndex < numOfTriangles; triangleIndex++)
        {
            triangles[triangleVertexIndex++] = vertexPositions[0];
            triangles[triangleVertexIndex++] = vertexPositions[triangleIndex + 1];
            triangles[triangleVertexIndex++] = vertexPositions[triangleIndex + 2];
        }
        return triangles;
    }
}

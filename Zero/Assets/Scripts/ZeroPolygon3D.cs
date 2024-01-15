using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

//
// Summary:
//     A 3D symmetric object connected by two polygons of equal vertex count.
public class ZeroPolygon3D
{
    public string Name;
    public Vector3[] TopPlane;
    public Vector3[] BottomPlane;
    //
    // Summary:
    //     The first side and left side if the polygon was a cube.
    public Vector3[] LeftPlane;
    //
    // Summary:
    //     The second side and left side if the polygon was a cube
    public Vector3[] FrontPlane;
    //
    // Summary:
    //     The third side and left side if the polygon was a cube
    public Vector3[] RightPlane;
    //
    // Summary:
    //     The fourth side and left side if the polygon was a cube
    public Vector3[] BackPlane;
    //
    // Summary:
    //     positions of bounds of each side in clockwise position in the vertices array
    private int[][] _sideVertexPositions;
    private int[] _topVertexPositions;
    private int[] _bottomVertexPositions;
    //
    // Summary:
    //     Union of vertices of the top polygon and the bottom polygon
    private Vector3[] _verticesWorld;
    public int VertexCount;

    public static int LeftSideIndex = 0;
    public static int FrontSideIndex = 1;
    public static int RightSideIndex = 2;
    public static int BackSideIndex = 3;

    public ZeroPolygon3D(
        string name,
        Vector3[] topVertices,
        Vector3[] bottomVertices)
    {
        this.TopPlane = topVertices;
        this.BottomPlane = bottomVertices;
        this.Name = name;
        Initialise();
    }

    public ZeroPolygon3D(
        string name,
        float height,
        Vector3 up,
        Vector3[] centerVertices)
    {
        this.Name = name;
        Vector3 halfUp = 0.5f * height * up;
        Vector3 halfDown = -0.5f * height * up;
        this.TopPlane = centerVertices.Select(e => halfUp + e).ToArray();
        this.BottomPlane = centerVertices.Select(e => halfDown + e).ToArray();
        Initialise();
    }

    private void Initialise()
    {
        InitialiseVerticesWorld();
        InitialiseVertexPositions();
        InitialiseOtherPlanes();

    }

    private void InitialiseVerticesWorld()
    {
        List<Vector3> verticesList = new();
        verticesList.AddRange(this.TopPlane);
        verticesList.AddRange(this.BottomPlane);
        this._verticesWorld = verticesList.ToArray();
    }

    private void InitialiseVertexPositions()
    {
        int topVertexCount = this._verticesWorld.Length / 2;
        this._sideVertexPositions = new int[topVertexCount][];
        for (int i = 0; i < topVertexCount; i++)
        {
            this._sideVertexPositions[i] = new int[4];
            this._sideVertexPositions[i][0] = i;
            this._sideVertexPositions[i][1] = i + topVertexCount;
            this._sideVertexPositions[i][2] = i == topVertexCount - 1 ? topVertexCount : i + topVertexCount + 1;
            this._sideVertexPositions[i][3] = i == topVertexCount - 1 ? 0 : i + 1;
        }
        this._topVertexPositions =
            Enumerable.Range(0, topVertexCount)
            .ToArray();
        this._bottomVertexPositions =
            Enumerable.Range(0, topVertexCount)
            .Select(i => i + topVertexCount)
            .ToArray();
    }

    private void InitialiseOtherPlanes()
    {
        this.LeftPlane = this._sideVertexPositions[LeftSideIndex].Select(i => this._verticesWorld[i]).ToArray();
        this.FrontPlane = this._sideVertexPositions[FrontSideIndex].Select(i => this._verticesWorld[i]).ToArray();
        this.RightPlane = this._sideVertexPositions[RightSideIndex].Select(i => this._verticesWorld[i]).ToArray();
        this.BackPlane = this._sideVertexPositions[BackSideIndex].Select(i => this._verticesWorld[i]).ToArray();
    }


    //
    // Summary:
    //     param gameObject:Gameobject with a position vector assigned
    //     param verticesWorld: all vertice of the polygon in world positions
    // 
    public Vector3[] GetMeshVertices(GameObject gameObject)
    {
        Debug.LogFormat(
            "world={0} local={1}",
            this._verticesWorld.Select(e => e.ToString()).ToCommaSeparatedString(),
            this._verticesWorld
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
                ).ToArray().Select(e => e.ToString()).ToCommaSeparatedString());
        return
            this._verticesWorld
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

    public int[] GetMeshTriangles(
        bool renderTopPlane,
        bool renderBottomPlane,
        int[] sidesToRender)
    {
        List<int> triangles = new();
        if (renderTopPlane)
            triangles.AddRange(
                GetMeshTrianglesFromVertexPositions(
                    vertexPositions:
                        this._topVertexPositions));
        if (renderBottomPlane)
            triangles.AddRange(
                GetMeshTrianglesFromVertexPositions(
                    vertexPositions:
                        this._bottomVertexPositions));

        foreach (int sideIndex in sidesToRender)
        {
            triangles.AddRange(
                GetMeshTrianglesFromVertexPositions(
                    vertexPositions: this._sideVertexPositions[sideIndex]));
        }
        return triangles.ToArray();
    }
    private static int[] GetMeshTrianglesFromVertexPositions(int[] vertexPositions)
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
    public void RenderVertices(Color? color = null)
    {
        int i = 0;
        foreach (var point in this._verticesWorld)
            ZeroRenderer.RenderSphere(point, this.Name + i++.ToString(), color: color ?? Color.yellow);
    }
}

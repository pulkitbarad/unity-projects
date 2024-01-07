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
    public ZeroPolygon TopPlane;
    public ZeroPolygon BottomPlane;
    //
    // Summary:
    //     The first side and left side if the polygon was a cube.
    public ZeroPolygon LeftPlane;
    //
    // Summary:
    //     The second side and left side if the polygon was a cube
    public ZeroPolygon FrontPlane;
    //
    // Summary:
    //     The third side and left side if the polygon was a cube
    public ZeroPolygon RightPlane;
    //
    // Summary:
    //     The fourth side and left side if the polygon was a cube
    public ZeroPolygon BackPlane;
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
        this.Name = name;
        Initialise(topVertices, bottomVertices);
    }

    public ZeroPolygon3D(
        string name,
        float height,
        Vector3[] topVertices)
    {
        Vector3 heightAndDirection = Vector3.Cross(topVertices[0], topVertices[1]).normalized * height;
        this.Name = name;
        Vector3[] bottomVertices = topVertices.Select(e => heightAndDirection + e).ToArray();
        Initialise(topVertices, bottomVertices);
    }

    private void Initialise(
        Vector3[] topVertices,
        Vector3[] bottomVertices)
    {
        InitialiseVerticesWorld(topVertices, bottomVertices);
        InitialiseVertexPositions();
        InitialiseStandardPlanes(topVertices, bottomVertices);

    }

    private void InitialiseVerticesWorld(
        Vector3[] topVertices,
        Vector3[] bottomVertices)
    {
        List<Vector3> verticesList = new();
        verticesList.AddRange(topVertices);
        verticesList.AddRange(bottomVertices);
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

    private void InitialiseStandardPlanes(
        Vector3[] topVertices,
        Vector3[] bottomVertices
    )
    {
        this.TopPlane = new ZeroPolygon(
            name: this.Name + "T",
            clockwiseVertices: topVertices
        );
        this.BottomPlane = new ZeroPolygon(
            name: this.Name + "B",
            clockwiseVertices: bottomVertices
        );
        this.LeftPlane = new ZeroPolygon(
            name: this.Name + "L",
            clockwiseVertices: this._sideVertexPositions[LeftSideIndex].Select(i => this._verticesWorld[i]).ToArray()
        );
        this.FrontPlane = new ZeroPolygon(
            name: this.Name + "F",
            clockwiseVertices: this._sideVertexPositions[FrontSideIndex].Select(i => this._verticesWorld[i]).ToArray()
        );
        this.RightPlane = new ZeroPolygon(
            name: this.Name + "R",
            clockwiseVertices: this._sideVertexPositions[RightSideIndex].Select(i => this._verticesWorld[i]).ToArray()
        );
        this.BackPlane = new ZeroPolygon(
            name: this.Name + "K",
            clockwiseVertices: this._sideVertexPositions[BackSideIndex].Select(i => this._verticesWorld[i]).ToArray()
        );

    }

    public Vector3[] GetMeshVertices(GameObject gameObject)
    {
        return ZeroPolygon.GetMeshVertices(gameObject, this._verticesWorld);
    }

    public int[] GetMeshTriangles(
        bool renderTopPlane,
        bool renderBottomPlane,
        int[] sidesToRender)
    {
        List<int> triangles = new();
        if (renderTopPlane)
            triangles.AddRange(
                ZeroPolygon.GetMeshTriangles(
                    vertexPositions:
                        this._topVertexPositions));
        if (renderBottomPlane)
            triangles.AddRange(
                ZeroPolygon.GetMeshTriangles(
                    vertexPositions:
                        this._bottomVertexPositions));

        foreach (int sideIndex in sidesToRender)
        {
            triangles.AddRange(
                ZeroPolygon.GetMeshTriangles(
                    vertexPositions: this._sideVertexPositions[sideIndex]));
        }
        return triangles.ToArray();
    }

    public void RenderVertices(Color? color = null)
    {
        int i = 0;
        foreach (var point in this._verticesWorld)
            ZeroRenderer.RenderSphere(point, this.Name + i++.ToString(), color: color ?? Color.yellow);
    }

}

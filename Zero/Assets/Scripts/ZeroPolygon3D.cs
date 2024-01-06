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
        AssignVerticesWorld(topVertices, bottomVertices);
        AssignVertexPositions();
        AssignStandardPlanes(topVertices, bottomVertices);
    }

    private void AssignVerticesWorld(
        Vector3[] topVertices,
        Vector3[] bottomVertices)
    {
        this._verticesWorld = new Vector3[topVertices.Length * 2];
        this._verticesWorld.AddRange(topVertices);
        this._verticesWorld.AddRange(bottomVertices);
    }

    private void AssignStandardPlanes(
        Vector3[] topVertices,
        Vector3[] bottomVertices
    )
    {
        this.TopPlane = new ZeroPolygon(
            name: this.Name + "T",
            vertices: topVertices
        );
        this.BottomPlane = new ZeroPolygon(
            name: this.Name + "B",
            vertices: bottomVertices
        );
        this.LeftPlane = new ZeroPolygon(
            name: this.Name + "L",
            vertices: this._sideVertexPositions[LeftSideIndex].Select(i => this._verticesWorld[i]).ToArray()
        );
        this.FrontPlane = new ZeroPolygon(
            name: this.Name + "F",
            vertices: this._sideVertexPositions[FrontSideIndex].Select(i => this._verticesWorld[i]).ToArray()
        );
        this.RightPlane = new ZeroPolygon(
            name: this.Name + "R",
            vertices: this._sideVertexPositions[RightSideIndex].Select(i => this._verticesWorld[i]).ToArray()
        );
        this.BackPlane = new ZeroPolygon(
            name: this.Name + "K",
            vertices: this._sideVertexPositions[BackSideIndex].Select(i => this._verticesWorld[i]).ToArray()
        );

    }

    private void AssignVertexPositions()
    {
        int vertexCount = this._verticesWorld.Length;
        int vertexCountHalf = this._verticesWorld.Length / 2;
        this._sideVertexPositions = new int[vertexCount][];
        for (int i = 0; i < vertexCount; i++)
        {
            this._sideVertexPositions[i][0] = i;
            this._sideVertexPositions[i][1] = i + vertexCount;
            this._sideVertexPositions[i][2] = i + vertexCount + 1;
            this._sideVertexPositions[i][2] = i + 1;
        }
        this._topVertexPositions =
            Enumerable.Range(0, vertexCountHalf - 1)
            .ToArray();
        this._bottomVertexPositions =
            Enumerable.Range(0, vertexCountHalf - 1)
            .Select(i => i + vertexCountHalf)
            .ToArray();
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
        int topPlaneVertexCount = this.TopPlane.VertexCount;
        int numberOfTriangles = 0;
        if (renderTopPlane)
            numberOfTriangles += topPlaneVertexCount - 2;
        if (renderBottomPlane)
            numberOfTriangles += topPlaneVertexCount - 2;
        //
        numberOfTriangles += (topPlaneVertexCount - 2) * sidesToRender.Length;

        int[] triangles = new int[numberOfTriangles * 3];
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
        return triangles;
    }
}

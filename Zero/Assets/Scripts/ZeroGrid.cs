using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroGrid
{
    private float _cellSize;
    private int _rowCount;
    private int _columnCount;
    private float _cellGap;
    private float _cellHeight;
    private Grid _grid;
    private GameObject[][] _gridNodes;
    private GameObject[] _gridLinesHorizontal;
    private GameObject[] _gridLinesVertical;

    private GameObject _gridParent = new(name: "GridParent");
    private GameObject _gridNodesParent = new(name: "GridNodesParent");
    private GameObject _gridLinesParent = new(name: "GridLinesParent");

    public ZeroGrid(
        Vector3 location,
        int rowCount,
        int columnCount,
        float cellGap,
        float cellHeight,
        float cellSize
    )
    {
        if (rowCount % 2 == 1)
            _rowCount = rowCount + 1;
        if (columnCount % 2 == 1)
            _columnCount = columnCount + 1;

        _rowCount = rowCount;
        _columnCount = columnCount;
        _cellGap = cellGap;
        _cellSize = cellSize;
        _cellHeight = cellHeight;
        // _grid = gameObject.AddComponent<Grid>();
        // _grid.cellGap = new Vector3(_cellSize, 0, _cellGap);
        // _grid.cellSize = new Vector3(_cellSize, 0, _cellSize);
        _gridLinesHorizontal = new GameObject[_rowCount];
        _gridLinesVertical = new GameObject[_columnCount];

        _gridLinesParent.transform.SetParent(_gridParent.transform);
        CreateNodes(location);

    }

    private void CreateNodes(Vector3 location)
    {

        float cellX = location.x + _cellSize / 2 + _cellGap;
        float cellZ = location.z + _cellSize / 2 + _cellGap;
        _gridNodes = new GameObject[_rowCount][];
        for (int rowIndex = 0; rowIndex < _rowCount - 1; rowIndex++)
        {
            cellZ += _cellSize + _cellGap;
            cellX = location.x + _cellSize / 2 + _cellGap;
            _gridNodes[rowIndex] = new GameObject[_rowCount];
            for (int columnIndex = 0; columnIndex < _columnCount - 1; columnIndex++)
            {
                cellX += _cellSize + _cellGap;
                GameObject newNode = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                newNode.name = "GridNode[" + rowIndex + "][" + columnIndex + "]";
                newNode.transform.localScale = new Vector3(1, 0.01f, 1);
                newNode.transform.position = new Vector3(cellX, 0.005f, cellZ);
                var renderer = newNode.GetComponent<Renderer>();
                Color c = new(0, 1, 0,0.40f);
                // {
                //     a = 0.38f
                // };
                renderer.material.color = c;
                newNode.transform.SetParent(_gridNodesParent.transform);
                _gridNodes[rowIndex][columnIndex] = newNode;
            }
        }
    }

    private void CreateGrid(Vector3 location)
    {

        float gridWidth = (_columnCount * (_cellSize + _cellGap)) + _cellGap;
        float gridDepth = (_rowCount * (_cellSize + _cellGap)) + _cellGap;

        float cellX = location.x + gridWidth / 2, cellZ = location.z + _cellGap / 2;
        for (int rowIndex = 0; rowIndex < _rowCount - 1; rowIndex++)
        {
            cellZ += _cellSize + _cellGap;
            GameObject newRow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newRow.name = "GridRow" + rowIndex;
            newRow.transform.localScale = new Vector3(gridWidth, 0.2f, _cellGap);
            newRow.transform.position = new Vector3(cellX, 0.1f, cellZ);
            newRow.transform.SetParent(_gridLinesParent.transform);
            _gridLinesHorizontal[rowIndex] = newRow;
        }

        cellZ = location.z + gridDepth / 2;
        cellX = location.x + _cellGap / 2;
        for (int columnIndex = 0; columnIndex < _columnCount - 1; columnIndex++)
        {
            cellX += _cellSize + _cellGap;
            GameObject newColumnm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newColumnm.name = "GridColumn" + columnIndex;
            newColumnm.transform.localScale = new Vector3(_cellGap, 0.05f, gridDepth);
            newColumnm.transform.position = new Vector3(cellX, 0.025f, cellZ);
            newColumnm.transform.SetParent(_gridLinesParent.transform);
            _gridLinesVertical[columnIndex] = newColumnm;
        }
    }
}

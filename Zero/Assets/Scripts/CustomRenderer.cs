using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CustomRenderer : MonoBehaviour
{

    private static readonly List<GameObject> _lineObjectPool = new();
    private static readonly int _lineObjectPoolCount;
    private static readonly List<string> _existingPoints = new();
    private static Material _baseLineMaterial;
    public static bool IsDebugEnabled = false;

    void Start()
    {
        _baseLineMaterial = Resources.Load("Material/LineMaterial", typeof(Material)) as Material;
        InstantiateLinePool();
    }

    public static void InstantiateLinePool()
    {
        for (int i = 0; i < _lineObjectPoolCount; i++)
        {
            GameObject temp = new();
            temp.SetActive(false);
            _lineObjectPool.Add(temp);
        }
    }

    public static void ReleaseLineObjectToPool(string name)
    {
        for (int i = 0; i < _lineObjectPoolCount; i++)
        {
            var lineObject = _lineObjectPool[i];

            if (lineObject.name.Equals(name) && lineObject.activeInHierarchy)
            {
                lineObject.SetActive(false);
            }
        }
    }

    public static GameObject GetLineObject(
        string name,
        Color color,
        float width = 2f)
    {
        GameObject lineObject = GameObject.Find(name);

        if (lineObject == null)
        {
            for (int i = 0; i < _lineObjectPoolCount; i++)
            {
                var newLineObject = _lineObjectPool[i];
                if (!newLineObject.activeInHierarchy)
                {
                    newLineObject.name = name;
                    newLineObject.SetActive(true);
                    lineObject = newLineObject;
                    break;
                }
            }
            if (lineObject == null)
            {
                lineObject = new GameObject(name);
            }
            LineRenderer primaryLineRenderer =
                lineObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
            Material newLineMaterial = new(_baseLineMaterial);
            primaryLineRenderer.sharedMaterial = new Material(newLineMaterial);
            // primaryLineRenderer.sharedMaterial.SetColor("_Color",color);
            primaryLineRenderer.startColor = color;
            primaryLineRenderer.endColor = color;
            primaryLineRenderer.startWidth = width;
            primaryLineRenderer.endWidth = width;
            primaryLineRenderer.positionCount = 3;
        }
        return lineObject;
    }

    public static GameObject RenderLine(
        string name,
        Color color,
        float width = 2f,
        float pointSize = 5f,
        Transform parentTransform = null,
        bool renderPoints = false,
        params Vector3[] linePoints)
    {
        GameObject lineObject =
            CommonController.FindGameObject(name, true) ?? GetLineObject(name, color, width: width);
        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
        lineRenderer.positionCount = linePoints.Length;
        Transform lineTransform = lineObject.transform;

        lineRenderer.SetPositions(linePoints);
        if (renderPoints)
        {
            for (int i = 0; i < linePoints.Length; i++)
            {
                RenderPoint(point: linePoints[i], name + "Point" + i, size: pointSize, parentTransform: lineTransform, color: color);
            }
        }
        if (parentTransform != null)
        {
            lineObject.transform.SetParent(parentTransform);
        }
        return lineObject;
    }

    public static GameObject RenderPoint(
        Vector3 point,
        string pointName,
        float size = 5f,
        Transform parentTransform = null,
        Color? color = null)
    {
        GameObject sphere;
        if (_existingPoints.FirstOrDefault(e => e.Contains(pointName)) == null)
        {
            sphere = RenderSphere(pointName, point, size, color);
            _existingPoints.Add(pointName);
        }
        else
        {
            sphere = GameObject.Find(pointName);
        }
        if (sphere != null)
        {
            sphere.transform.position = point;
            sphere.transform.SetParent(parentTransform);

        }
        return sphere;
    }
    public static GameObject RenderSphere(
        string sphereName,
        Vector3 position,
        float size = 5f,
        Color? color = null)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        if (sphereName.Length > 0)
            sphere.name = sphereName;
        sphere.transform.localScale = new Vector3(size, size, size);
        sphere.transform.position = position;
        var sphereRenderer = sphere.GetComponent<Renderer>();
        sphereRenderer.material.color = color ?? Color.yellow;
        return sphere;
    }

    public static GameObject RenderCylinder(
        string objectName,
        Vector3 position,
        float size = 5f,
        Color? color = null)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (objectName.Length > 0)
            cylinder.name = objectName;
        cylinder.transform.localScale = new Vector3(size, 1, size);
        cylinder.transform.position = position;
        var sphereRenderer = cylinder.GetComponent<Renderer>();
        sphereRenderer.material.color = color ?? Color.yellow;
        return cylinder;
    }

}

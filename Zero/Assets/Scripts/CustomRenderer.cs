using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomRenderer : MonoBehaviour
{

    private static readonly List<GameObject> _lineObjectPool = new();
    private static readonly int _lineObjectPoolCount;
    private static bool _isDebugEnabled = false;
    private static readonly List<string> _existingPoints = new();

    void Start()
    {
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
        float width = 10f)
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
            Material materialNeonLight = Resources.Load("LineMaterial") as Material;
            primaryLineRenderer.SetMaterials(new List<Material>() { materialNeonLight });
            primaryLineRenderer.material.SetColor("_Color", color);
            primaryLineRenderer.startWidth = width;
            primaryLineRenderer.endWidth = width;
            primaryLineRenderer.positionCount = 3;
        }
        return lineObject;
    }

    public static GameObject RenderLine(
        string name,
        Color color,
        float width = 10f,
        float pointSize = 20f,
        Transform parentTransform = null,
        params Vector3[] linePoints)
    {
        GameObject lineObject =
            CommonController.FindGameObject(name, true) ?? GetLineObject(name, color, width: width);
        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
        lineRenderer.positionCount = linePoints.Length;

        lineRenderer.SetPositions(linePoints);
        if (_isDebugEnabled)
        {
            for (int i = 0; i < linePoints.Length; i++)
            {
                RenderPoint(point: linePoints[i], size: pointSize, color: Color.yellow);
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
        float size = 20f,
        Color? color = null)
    {
        string sphereName = "Point_" + point[0] + "_" + point[1] + "_" + point[2];
        if (_existingPoints.FirstOrDefault(e => e.Contains(sphereName)) == null)
        {
            GameObject sphere = RenderSphere(sphereName, point, size, color);
            _existingPoints.Add(sphereName);
            return sphere;
        }
        else
        {
            return GameObject.Find(sphereName);
        }
    }
    public static GameObject RenderSphere(
        string sphereName,
        Vector3 position,
        float size = 20f,
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
        float size = 20f,
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

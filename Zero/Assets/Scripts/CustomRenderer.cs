using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomRenderer : MonoBehaviour
{

    private static readonly List<GameObject> _lineObjectPool = new();
    private static readonly int _lineObjectPoolCount;
    private static readonly List<string> _existingSpheres = new();
    public static GameObject DebuggingParent;
    private static Material _baseLineMaterial;
    public static bool IsDebugEnabled = false;

    void Start()
    {
        _baseLineMaterial = Resources.Load("Material/LineMaterial", typeof(Material)) as Material;
        InstantiateLinePool();
        DebuggingParent = new GameObject("DebuggingParent");
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
        UnityEngine.Color? color = null,
        float width = 0.5f,
        Transform parentTransform = null)
    {
        GameObject lineObject = CommonController.FindGameObject(name, true);

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
            primaryLineRenderer.startColor = color ?? UnityEngine.Color.yellow; ;
            primaryLineRenderer.endColor = color ?? UnityEngine.Color.yellow; ;
            primaryLineRenderer.startWidth = width;
            primaryLineRenderer.endWidth = width;
            primaryLineRenderer.positionCount = 3;
        }
        if (parentTransform != null)
        {
            lineObject.transform.SetParent(parentTransform);
        }
        else
        {
            lineObject.transform.SetParent(DebuggingParent.transform);
        }

        return lineObject;
    }

    public static GameObject RenderLine(
        string name,
        UnityEngine.Color? color = null,
        float width = 0.5f,
        float pointSize = 0.5f,
        Transform parentTransform = null,
        bool renderPoints = false,
        params Vector3[] linePoints)
    {
        GameObject lineObject =
            CommonController.FindGameObject(name, true) ?? GetLineObject(name, color, width: width, parentTransform);
        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
        lineRenderer.positionCount = linePoints.Length;
        Transform lineTransform = lineObject.transform;

        lineRenderer.SetPositions(linePoints);
        if (renderPoints)
        {
            for (int i = 0; i < linePoints.Length; i++)
            {
                RenderSphere(position: linePoints[i], name + "Point" + i, size: pointSize, parentTransform: lineTransform, color: color);
            }
        }
        return lineObject;
    }

    public static GameObject RenderSphere(
        Vector3 position,
        string sphereName = "",
        float size = 0.5f,
        Transform parentTransform = null,
        UnityEngine.Color? color = null)
    {
        string newSphereName = sphereName.Length > 0 ? sphereName : "Sphere" + _existingSpheres.Count;

        GameObject sphere = CommonController.FindGameObject(newSphereName, true);
        if (sphere == null)
        {
            sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _existingSpheres.Add(newSphereName);
        }
        sphere.name = newSphereName;
        sphere.transform.localScale = new Vector3(size, size, size);
        sphere.transform.position = position;
        var sphereRenderer = sphere.GetComponent<Renderer>();
        sphereRenderer.material.color = color ?? UnityEngine.Color.yellow;

        if (parentTransform != null)
            sphere.transform.SetParent(parentTransform);
        else
            sphere.transform.SetParent(DebuggingParent.transform);
        sphere.SetActive(true);

        Debug.Log(newSphereName + "=" + position);
        return sphere;
    }

    public static GameObject RenderCylinder(
        string objectName,
        Vector3 position,
        float size = 1f,
        UnityEngine.Color? color = null)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (objectName.Length > 0)
            cylinder.name = objectName;
        cylinder.transform.localScale = new Vector3(size, 0.5f, size);
        cylinder.transform.position = position;
        var sphereRenderer = cylinder.GetComponent<Renderer>();
        sphereRenderer.material.color = color ?? UnityEngine.Color.yellow;
        return cylinder;
    }

}

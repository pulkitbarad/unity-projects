using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using UnityEngine.EventSystems;

public class CommonController : MonoBehaviour
{

    private static List<string> _existingPoints = new();
    public static bool IsDebugEnabled = false;
    // public static bool IsSingleTouchConsumed = false;
    // public static bool IsDoubleTouchLocked = false;
    public static bool IsRoadBuildEnabled = false;

    // Start is called before the first frame update
    void Start()
    {

    }


    //Touchphase Ended is ignored
    public static bool IsTouchOverNonUI(bool suppressTouchEndEvent = true)
    {
        return
        Input.touchCount > 0
            && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)
            && (!suppressTouchEndEvent || Input.GetTouch(0).phase != TouchPhase.Ended);

    }
    // Update is called once per frame

    public static GameObject GetLineObject(string lineName, Color lineColor)
    {
        GameObject primaryLine = GameObject.Find(lineName);

        if (primaryLine == null)
        {
            primaryLine = new GameObject(lineName);
            LineRenderer primaryLineRenderer = primaryLine.AddComponent(typeof(LineRenderer)) as LineRenderer;
            Material materialNeonLight = Resources.Load("NearestSphere") as Material;
            primaryLineRenderer.SetMaterials(new List<Material>() { materialNeonLight });
            primaryLineRenderer.material.SetColor("_Color", lineColor);
            primaryLineRenderer.startWidth = 0.5f;
            primaryLineRenderer.endWidth = 0.5f;
            primaryLineRenderer.positionCount = 3;
        }

        return primaryLine;
    }
    public static void RenderLine(string lineName, Color lineColor, params Vector3[] linePoints)
    {

        GameObject lineObject = GameObject.Find(lineName);

        if (lineObject == null)
        {
            lineObject = GetLineObject(lineName, lineColor);
        }

        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
        lineRenderer.positionCount = linePoints.Length;

        lineRenderer.SetPositions(linePoints);
        if (IsDebugEnabled)
        {
            foreach (var point in linePoints)
                DrawPointSphere(point);
        }
    }
    public static void RenderLine(string lineName, Color lineColor, float lineWidth = 10, float pointSize = 20, params Vector2[] linePoints)
    {

        GameObject lineObject = GameObject.Find(lineName);

        if (lineObject == null)
        {
            lineObject = new GameObject(lineName);
            LineRenderer primaryLineRenderer = lineObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
            primaryLineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            primaryLineRenderer.material.SetColor("_Color", lineColor);
            primaryLineRenderer.startWidth = lineWidth;
            primaryLineRenderer.endWidth = lineWidth;
            primaryLineRenderer.positionCount = 3;
        }


        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
        List<Vector3> linePoints3D = new List<Vector3>();
        foreach (var point in linePoints)
            linePoints3D.Add(point);
        lineRenderer.positionCount = linePoints3D.Count;

        lineRenderer.SetPositions(linePoints3D.ToArray());
        if (IsDebugEnabled)
        {
            foreach (var point in linePoints3D)
                DrawPointSphere(point, pointSize);
        }
    }
    public static GameObject DrawPointSphere(Vector3 point, float size = 20, Color? color = null)
    {
        string sphereName = "Point_" + point[0];
        Debug.Log("sphereName=" + sphereName);
        if (_existingPoints.FirstOrDefault(e => e.Contains(sphereName)) == null)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = sphereName;
            sphere.transform.localScale = new Vector3(size, size, size);
            sphere.transform.position = point;
            var sphereRenderer = sphere.GetComponent<Renderer>();
            Debug.Log("color=" + color.Value);
            if (color.HasValue)
                sphereRenderer.material.color = color.Value;
            _existingPoints.Add(sphereName);
            return sphere;
        }
        else
        {
            return GameObject.Find(sphereName);
        }
    }
}

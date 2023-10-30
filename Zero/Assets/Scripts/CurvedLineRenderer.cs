using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class CurvedLineRenderer : MonoBehaviour
{

    public Transform StartFixedPoint;
    public Transform EndFixedPoint;
    public Transform StartPoint;
    public Transform PivotPoint1;
    public Transform PivotPoint2;
    public Transform EndPoint;
    public float VertexMultiplier = 10;
    public float RoadWidth = 3;
    public float ConnectionMode = 0;
    private List<string> _existingPoints = new List<string>();
    private bool _isDebugEnabled = false;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        List<Vector3> controlPoints = new List<Vector3>()
        {
            StartPoint.position,
            PivotPoint1.position,
            PivotPoint2.position,
            EndPoint.position
        };

        List<Vector3> curvePoints = new List<Vector3>();
        GameObject primaryLine = GetLineObject("PrimaryCurvedLine", new Color(0.1568f, 1f, 0.21f, 1f));

        if (ConnectionMode == 1)
            curvePoints = CreateBezierCurveFixedStart(primaryLine, controlPoints, StartPoint.position - StartFixedPoint.position, 100);
        else if (ConnectionMode == 2)
            curvePoints = CreateBezierCurveFixedEnd(primaryLine, controlPoints, EndPoint.position - EndFixedPoint.position, 100);
        else if (ConnectionMode == 3)
        {
            var directions = new List<Vector3>() { StartPoint.position - StartFixedPoint.position, EndPoint.position - EndFixedPoint.position };
            curvePoints = CreateBezierCurveFixedBoth(primaryLine, controlPoints, directions, 100);
        }
        else
            curvePoints = CreateBezierCurve(primaryLine, controlPoints, 100);

        // List<Vector3> curvePoints = CreateSmoothCurve(GetLineObject("PrimaryCurvedLine", new Color(0.1568f, 1f, 0.21f, 1f)), mainLinePoints, VertexMultiplier);

        List<Vector3> rightParallelPoints = new List<Vector3>();
        List<Vector3> leftParallelPoints = new List<Vector3>();
        List<Vector3> parallelPoints = new List<Vector3>();
        if (curvePoints.Count >= 3)
        {
            for (int i = 1; i < curvePoints.Count; i += 1)
            {
                parallelPoints = FindParallelPoints(curvePoints[i - 1], curvePoints[i], RoadWidth);
                rightParallelPoints.Add(parallelPoints[0]);
                leftParallelPoints.Add(parallelPoints[1]);

                if (i == curvePoints.Count - 1)
                {
                    parallelPoints = FindParallelPoints(curvePoints[i - 1], curvePoints[i], RoadWidth, true);
                    rightParallelPoints.Add(parallelPoints[0]);
                    leftParallelPoints.Add(parallelPoints[1]);

                }
            }
            RenderLine(GetLineObject("RightCurvedLine", new Color(0.156f, 1f, 0.972f, 1f)), rightParallelPoints);
            RenderLine(GetLineObject("LeftCurvedLine", new Color(0.96875f, 0.578f, 0.578f, 1f)), leftParallelPoints);
        }

    }

    List<Vector3> CreateBezierCurveFixedStart(GameObject line, List<Vector3> controlPoints, Vector3 fixedDirection, int vertexCount)
    {
        var controlPoint1Distance = Vector3.Distance(controlPoints[0], controlPoints[3]) / 2;
        controlPoints[1] = (fixedDirection + controlPoints[0]).normalized * controlPoint1Distance;

        return CreateBezierCurve(line, controlPoints, vertexCount);
    }

    List<Vector3> CreateBezierCurveFixedEnd(GameObject line, List<Vector3> controlPoints, Vector3 fixedDirection, int vertexCount)
    {
        var controlPoint1Distance = Vector3.Distance(controlPoints[0], controlPoints[3]) / 2;
        controlPoints[2] = (fixedDirection + controlPoints[3]).normalized * controlPoint1Distance;

        return CreateBezierCurve(line, controlPoints, vertexCount);
    }

    List<Vector3> CreateBezierCurveFixedBoth(GameObject line, List<Vector3> controlPoints, List<Vector3> fixedDirections, int vertexCount)
    {
        var controlPoint1Distance = Vector3.Distance(controlPoints[0], controlPoints[3]) / 2;
        controlPoints[1] = (fixedDirections[0] + controlPoints[0]).normalized * controlPoint1Distance;
        controlPoints[2] = (fixedDirections[1] + controlPoints[3]).normalized * controlPoint1Distance;

        return CreateBezierCurve(line, controlPoints, vertexCount);
    }
    List<Vector3> CreateBezierCurve(GameObject line, List<Vector3> controlPoints, int vertexCount)
    {

        List<Vector3> pathPoints = new List<Vector3>();
        var segments = controlPoints.Count / 3;

        for (int s = 0; s < controlPoints.Count - 3; s += 3)
        {
            Vector3 p0 = controlPoints[s];
            Vector3 p1 = controlPoints[s + 1];
            Vector3 p2 = controlPoints[s + 2];
            Vector3 p3 = controlPoints[s + 3];

            if (s == 0)
            {
                pathPoints.Add(BezierPathCalculation(p0, p1, p2, p3, 0.0f));
            }

            for (int p = 0; p < (vertexCount / segments); p++)
            {
                float t = (1.0f / (vertexCount / segments)) * p;
                Vector3 point = new Vector3();
                point = BezierPathCalculation(p0, p1, p2, p3, t);
                pathPoints.Add(point);
            }
        }
        RenderLine(line, pathPoints);
        return pathPoints;
    }

    Vector3 BezierPathCalculation(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float tt = t * t;
        float ttt = t * tt;
        float u = 1.0f - t;
        float uu = u * u;
        float uuu = u * uu;

        Vector3 B = new Vector3();
        B = uuu * p0;
        B += 3.0f * uu * t * p1;
        B += 3.0f * u * tt * p2;
        B += ttt * p3;

        return B;
    }

    List<Vector3> CreateSmoothCurve(GameObject line, List<Vector3> points, float vertexMultiplier)
    {
        Vector3 startPosition = points[0];
        Vector3 midPosition = points[1];
        Vector3 endPosition = points[2];

        List<Vector3> curvePoints = new List<Vector3>();

        var angle = Vector3.Angle(midPosition - startPosition, endPosition - midPosition);
        var vertexCount = 100 / angle;
        if (angle == 0)
        {
            angle = 1;
        }
        if (vertexMultiplier > 0)
        {
            vertexCount *= vertexMultiplier;
        }

        for (float interpolationRatio = 0; interpolationRatio <= 1; interpolationRatio += 1 / vertexCount)
        {
            var tangent1 = Vector3.Lerp(startPosition, midPosition, interpolationRatio);
            var tangent2 = Vector3.Lerp(midPosition, endPosition, interpolationRatio);
            var curve = Vector3.Lerp(tangent1, tangent2, interpolationRatio);
            curvePoints.Add(curve);
        }
        curvePoints.Add(endPosition);
        RenderLine(line, curvePoints);
        return curvePoints;
    }

    void RenderLine(GameObject line, List<Vector3> linePoints)
    {

        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray());
        if (_isDebugEnabled)
        {
            linePoints.ForEach(e => DrawPointSphere(e));
        }
    }

    List<Vector3> FindParallelPoints(Vector3 originPoint, Vector3 targetPoint, float parallelWidth, bool isReversed = false)
    {

        Vector3 newOriginPoint = originPoint;
        Vector3 newTargetPoint = targetPoint;
        if (isReversed)
        {
            newOriginPoint = targetPoint;
            newTargetPoint = originPoint;
        }

        Vector3 forwardVector = newTargetPoint - newOriginPoint;
        Vector3 upPoint = (new Vector3(newOriginPoint[0], newOriginPoint[1] + 3, newOriginPoint[2]));
        Vector3 upVector = upPoint - newOriginPoint;
        Vector3 rightVector = Vector3.Cross(forwardVector, upVector).normalized;
        var rightPoint = newOriginPoint + (rightVector * parallelWidth);
        var leftPoint = newOriginPoint - (rightVector * parallelWidth);

        if (isReversed)
        {
            var temp = rightPoint;
            rightPoint = leftPoint;
            leftPoint = temp;
        }
        // if(_isDebugEnabled){
        //     Debug.DrawLine(newOriginPoint, newOriginPoint+forwardVector, Color.red, Mathf.Infinity);
        //     Debug.DrawLine(newOriginPoint, upPoint, Color.green, Mathf.Infinity);
        //     Debug.DrawLine(newOriginPoint, rightPoint, Color.red, Mathf.Infinity);
        //     Debug.DrawLine(newOriginPoint, leftPoint, Color.yellow, Mathf.Infinity);
        // }

        return new List<Vector3>() { rightPoint, leftPoint };
    }

    GameObject GetLineObject(string lineName, Color lineColor)
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

    void DrawPointSphere(Vector3 point)
    {
        string sphereName = "Point_" + point[0];
        if (_existingPoints.FirstOrDefault(e => e.Contains(sphereName)) == null)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = sphereName;
            sphere.transform.position = point;
            _existingPoints.Add(sphereName);
        }
    }
}

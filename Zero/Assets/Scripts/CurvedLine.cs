using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CurvedLine : MonoBehaviour
{

    public static float RoadMaxChangeInAngle;
    public static int RoadMaxVertexCount;
    public static int RoadMinVertexCount;
    public static int RoadSegmentMinLength;

    public static List<Vector3> FindBazierLinePoints(params Vector3[] controlPoints)
    {
        int vertexCount = 2;
        if (controlPoints.Length > 2)
            vertexCount = RoadMinVertexCount;

        return FindBazierLinePoints(vertexCount, controlPoints);
    }

    private static List<Vector3> FindBazierLinePoints(
      int vertexCount,
      params Vector3[] controlPoints)
    {
        List<Vector3> bazierLinePoints = new();

        for (int p = 0; p < vertexCount; p++)
        {
            float t = 1.0f / vertexCount * p;
            Vector3 point = BezierPathCalculation(t, controlPoints);
            bazierLinePoints.Add(point);
            if (p > 1)
            {
                Vector3 currSegment = bazierLinePoints[p] - bazierLinePoints[p - 1];
                Vector3 prevSegment = bazierLinePoints[p - 1] - bazierLinePoints[p - 2];
                float currAngle = Vector3.Angle(currSegment, prevSegment);

                if (vertexCount < RoadMaxVertexCount
                    && currSegment.magnitude >= RoadSegmentMinLength
                    && currAngle > RoadMaxChangeInAngle)
                {
                    return FindBazierLinePoints(vertexCount + 1, controlPoints);
                }
            }
        }
        bazierLinePoints.Add(controlPoints[^1]);
        return bazierLinePoints;
    }

    private static Vector3 BezierPathCalculation(
        float t,
        params Vector3[] controlPoints
        )
    {
        float tt = t * t;
        float ttt = t * tt;
        float u = 1.0f - t;
        float uu = u * u;
        float uuu = u * uu;
        Vector3 p0 = controlPoints[0];
        Vector3 p1 = controlPoints[1];

        Vector3 B = new();
        if (controlPoints.Count() == 2)
        {
            B = u * p0;
            B += t * p1;
        }
        else
        {
            Vector3 p2 = controlPoints[2];
            if (controlPoints.Count() == 3)
            {
                B = uu * p0;
                B += 2.0f * u * t * p1;
                B += tt * p2;
            }
            else if (controlPoints.Count() == 4)
            {
                Vector3 p3 = controlPoints[3];
                B = uuu * p0;
                B += 3.0f * uu * t * p1;
                B += 3.0f * u * tt * p2;
                B += ttt * p3;
            }
        }
        return B;
    }
    public static void FindParallelLines(
        List<Vector3> curvePoints,
        int pathWidth,
        out List<Vector3> leftLinePoints,
        out List<Vector3> rightLinePoints)
    {
        leftLinePoints = new();
        rightLinePoints = new();
        if (curvePoints.Count >= 2)
        {
            for (int i = 0; i <= curvePoints.Count - 2; i++)
            {
                List<Vector3> parallelPoints = new();
                parallelPoints = FindPerpendicularPoints(
                   originPoint: curvePoints[i],
                   targetPoint: curvePoints[i + 1],
                   parallelWidth: pathWidth);

                rightLinePoints.Add(parallelPoints[0]);
                leftLinePoints.Add(parallelPoints[1]);

                if (i == curvePoints.Count - 2)
                {
                    parallelPoints = FindPerpendicularPoints(
                        originPoint: curvePoints[i + 1],
                        targetPoint: curvePoints[i],
                        parallelWidth: pathWidth);
                    rightLinePoints.Add(parallelPoints[1]);
                    leftLinePoints.Add(parallelPoints[0]);
                }
                // Debug.Log("left point=" + parallelPoints[1]);
            }
        }
    }

    private static List<Vector3> FindPerpendicularPoints(
        Vector3 originPoint,
        Vector3 targetPoint,
        float parallelWidth)
    {
        Vector3 forwardVector = targetPoint - originPoint;
        Vector3 upPoint = new(originPoint[0], originPoint[1] + 3, originPoint[2]);
        Vector3 upVector = upPoint - originPoint;
        Vector3 leftVector = Vector3.Cross(forwardVector, upVector).normalized;
        var leftPoint = originPoint + (leftVector * parallelWidth / 2);
        var rightPoint = originPoint - (leftVector * parallelWidth / 2);

        return new List<Vector3>() { rightPoint, leftPoint };
    }

    private static List<Vector3> CreateSmoothCurve(
        GameObject line,
        List<Vector3> points,
        float vertexMultiplier)
    {
        Vector3 startPosition = points[0];
        Vector3 midPosition = points[1];
        Vector3 endPosition = points[2];

        List<Vector3> curvePoints = new();

        var angle = Vector3.Angle(midPosition - startPosition, endPosition - midPosition);
        var vertexCount = 100 / angle;
        if (angle == 0)
        {
            //do something in future?
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
        CustomRenderer.RenderLine(
            line.name,
            Color.red,
            width: 10f,
            linePoints: curvePoints.ToArray());
        return curvePoints;
    }
}

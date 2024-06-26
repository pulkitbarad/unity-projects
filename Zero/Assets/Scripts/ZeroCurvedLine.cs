using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ZeroCurvedLine
{
    public static (Vector3[], float) FindBazierLinePoints(params Vector3[] controlPoints)
    {
        int vertexCount = 1;
        if (controlPoints.Length > 2)
            vertexCount = ZeroRoadBuilder.RoadMinVertexCount;

        return FindBazierLinePoints(vertexCount, controlPoints);
    }

    public static (Vector3[], float) FindBazierLinePoints(
      int vertexCount,
      params Vector3[] controlPoints)
    {
        float maxAngle = 0;
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

                if (vertexCount < ZeroRoadBuilder.RoadMaxVertexCount
                    && currSegment.magnitude >= ZeroRoadBuilder.RoadSegmentMinLength
                    && currAngle > ZeroRoadBuilder.RoadChangeAngleThreshold)
                {
                    return FindBazierLinePoints(vertexCount + 1, controlPoints);
                }
                else if (currAngle > maxAngle)
                    maxAngle = currAngle;
            }
        }
        bazierLinePoints.Add(controlPoints.Last());
        return (bazierLinePoints.ToArray(), maxAngle);
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

    // public static void FindParallelLines(
    //     List<Vector3> curvePoints,
    //     int pathWidth,
    //     out List<Vector3> leftLinePoints,
    //     out List<Vector3> rightLinePoints)
    // {
    //     leftLinePoints = new();
    //     rightLinePoints = new();
    //     if (curvePoints.Count >= 2)
    //     {
    //         for (int i = 0; i <= curvePoints.Count - 2; i++)
    //         {
    //             List<Vector3> parallelPoints = new();
    //             parallelPoints = FindPerpendicularPoints(
    //                originPoint: curvePoints[i],
    //                targetPoint: curvePoints[i + 1],
    //                parallelWidth: pathWidth);

    //             rightLinePoints.Add(parallelPoints[0]);
    //             leftLinePoints.Add(parallelPoints[1]);

    //             if (i == curvePoints.Count - 2)
    //             {
    //                 parallelPoints = FindPerpendicularPoints(
    //                     originPoint: curvePoints[i + 1],
    //                     targetPoint: curvePoints[i],
    //                     parallelWidth: pathWidth);
    //                 rightLinePoints.Add(parallelPoints[1]);
    //                 leftLinePoints.Add(parallelPoints[0]);
    //             }
    //             // Debug.Log("left point=" + parallelPoints[1]);
    //         }
    //     }
    // }

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

        return curvePoints;
    }
}

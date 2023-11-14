using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CommonController : MonoBehaviour
{

    private static List<string> _existingPoints = new();
    public static bool IsDebugEnabled = false;
    // public static bool IsSingleTouchConsumed = false;
    // public static bool IsDoubleTouchLocked = false;
    public static bool IsRoadBuildEnabled = false;
    private static List<GameObject> _lineObjectPool = new();
    private static int _lineObjectPoolCount = 100;

    // Start is called before the first frame update
    void Start()
    {
        TestRendrer.InstantiateLinePool();
    }


    //Touchphase Ended is ignored
    public static bool IsTouchOverNonUI(bool suppressTouchEndEvent = true)
    {
        return
        Input.touchCount > 0
            && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)
            && (!suppressTouchEndEvent || Input.GetTouch(0).phase != TouchPhase.Ended);

    }

    public static class TestRendrer
    {

        public static void InstantiateLinePool()
        {

            GameObject temp = new GameObject();
            for (int i = 0; i < _lineObjectPoolCount; i++)
            {
                temp.AddComponent<LineRenderer>();
                temp.SetActive(false);
                _lineObjectPool.Add(temp);
            }
        }

        private static GameObject GetLineFromPool(string name)
        {
            for (int i = 0; i < _lineObjectPoolCount; i++)
            {
                var lineObject = _lineObjectPool[i];
                if (!lineObject.activeInHierarchy)
                {
                    lineObject.name = name;
                    lineObject.SetActive(true);
                    return lineObject;
                }
            }
            return null;
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
                    }
                }
                if (lineObject == null)
                {
                    lineObject = new GameObject(name);
                }
            }
            LineRenderer primaryLineRenderer = lineObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
            Material materialNeonLight = Resources.Load("NearestSphere") as Material;
            primaryLineRenderer.SetMaterials(new List<Material>() { materialNeonLight });
            primaryLineRenderer.material.SetColor("_Color", color);
            primaryLineRenderer.startWidth = width;
            primaryLineRenderer.endWidth = width;
            primaryLineRenderer.positionCount = 3;

            return lineObject;
        }
        public static void RenderLine(
            string lineName,
            Color lineColor,
            float width= 10f,
            params Vector3[] linePoints)
        {

            GameObject lineObject = GameObject.Find(lineName);

            if (lineObject == null)
            {
                lineObject = GetLineObject(lineName, lineColor,width:width);
            }

            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
            lineRenderer.positionCount = linePoints.Length;

            lineRenderer.SetPositions(linePoints);
            if (IsDebugEnabled)
            {
                foreach (var point in linePoints)
                    RenderPointSphere(point);
            }
        }
        public static void RenderLine(
            string lineName,
            Color lineColor,
            float lineWidth = 10,
            float pointSize = 20,
            params Vector2[] linePoints)
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
                    RenderPointSphere(point, pointSize);
            }
        }
        public static GameObject RenderPointSphere(
            Vector3 point,
            float size = 20,
            Color? color = null)
        {
            string sphereName = "Point_" + point[0] + "_" + point[1] + "_" + point[2];
            if (_existingPoints.FirstOrDefault(e => e.Contains(sphereName)) == null)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = sphereName;
                sphere.transform.localScale = new Vector3(size, size, size);
                sphere.transform.position = point;
                var sphereRenderer = sphere.GetComponent<Renderer>();
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
    public static class CurvedLine
    {
        public static List<Vector3> FindBazierLinePoints(
            Vector3 startPoint,
            Vector3 endPoint,
            int vertexCount)
        {
            var startToEndDirection = endPoint - startPoint;
            var startToEndDistance = startToEndDirection.magnitude;
            Vector3 p0 = startPoint;
            Vector3 p1 = startToEndDirection.normalized * startToEndDistance * 0.33f;
            Vector3 p2 = startToEndDirection.normalized * startToEndDistance * 0.67f;
            Vector3 p3 = endPoint;

            List<Vector3> pathPoints = new List<Vector3>();

            for (int s = 0; s < 1; s += 3)
            {
                if (s == 0)
                {
                    pathPoints.Add(BezierPathCalculation(p0, p1, p2, p3, 0.0f));
                }

                for (int p = 0; p < (vertexCount); p++)
                {
                    float t = 1.0f / (vertexCount) * p;
                    Vector3 point = new();
                    point = BezierPathCalculation(p0, p1, p2, p3, t);
                    pathPoints.Add(point);
                }
            }
            return pathPoints;

        }

        public static List<List<Vector3>> FindParallelLines(
            List<Vector3> curvePoints,
            int pathWidth)
        {
            List<Vector3> leftLinePoints = new();
            List<Vector3> rightLinePoints = new();
            if (curvePoints.Count >= 3)
            {
                for (int i = 0; i < curvePoints.Count - 1; i++)
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
                            originPoint: curvePoints[i],
                            targetPoint: curvePoints[i - 1],
                            parallelWidth: pathWidth);
                        rightLinePoints.Add(parallelPoints[0]);
                        leftLinePoints.Add(parallelPoints[1]);
                    }
                }
                return new List<List<Vector3>>() { leftLinePoints, rightLinePoints };
            }
            return new List<List<Vector3>>() { };
        }


        private static Vector3 BezierPathCalculation(
            Vector3 p0,
            Vector3 p1,
            Vector3 p2,
            Vector3 p3,
            float t)
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
        private static List<Vector3> FindPerpendicularPoints(
            Vector3 originPoint,
            Vector3 targetPoint,
            float parallelWidth)
        {
            Vector3 forwardVector = targetPoint - originPoint;
            Vector3 upPoint = (new Vector3(originPoint[0], originPoint[1] + 3, originPoint[2]));
            Vector3 upVector = upPoint - originPoint;
            Vector3 rightVector = Vector3.Cross(forwardVector, upVector).normalized;
            var rightPoint = originPoint + (rightVector * parallelWidth);
            var leftPoint = originPoint - (rightVector * parallelWidth);

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
            TestRendrer.RenderLine(line.name, Color.red, width: 10f,curvePoints.ToArray());
            return curvePoints;
        }

    }
}

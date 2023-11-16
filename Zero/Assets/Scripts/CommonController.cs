using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class CommonController : MonoBehaviour
{

    private static List<string> _existingPoints = new();
    public static bool IsDebugEnabled = false;
    // public static bool IsSingleTouchConsumed = false;
    // public static bool IsDoubleTouchLocked = false;
    public static bool IsRoadMenuActive = false;
    private static List<GameObject> _lineObjectPool = new();
    private static int _lineObjectPoolCount = 100;
    public static float MainCameraMoveSpeed = 1f;
    public static float MainCameraSmoothing = 2f;
    public static float MainCameraZoomSpeed = 2f;
    public static float MainCameraRotationSpeed = 5f;
    public static float MainCameraTiltSpeed = 2f;
    public static float MainCameraPinchDistanceThreshold = 50f;
    public static float MainCameraRotateAngleThreshold = 10f;
    public static GameObject StartControlObject;
    public static GameObject StartObject;
    public static GameObject EndControlObject;
    public static GameObject EndObject;
    public static Camera MainCamera;
    public static GameObject MainCameraHolder;
    public static GameObject MainCameraRoot;
    public static GameObject ButtonRoad;

    // Start is called before the first frame update
    void Start()
    {
        TestRendrer.InstantiateLinePool();
        DeactivateRoadControlPoints();
    }

    public static bool IsTouchOverNonUI(bool suppressTouchEndEvent = true)
    {
        return
        Input.touchCount > 0
            && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)
            && (!suppressTouchEndEvent || Input.GetTouch(0).phase != TouchPhase.Ended);
    }

    public static void HandleRoadObjectsDrag()
    {
        CommonController.HandleGameObjectDrag(CommonController.StartObject);
        CommonController.HandleGameObjectDrag(CommonController.StartControlObject);
        CommonController.HandleGameObjectDrag(CommonController.EndControlObject);
        CommonController.HandleGameObjectDrag(CommonController.EndObject);
    }
    private static bool HandleGameObjectDrag(GameObject gameObject)
    {
        if (Input.touchCount > 0
                && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)
                && (Input.GetTouch(0).phase == TouchPhase.Moved
                    || Input.GetTouch(0).phase == TouchPhase.Stationary))
        {
            Touch touch0 = Input.GetTouch(0);
            Ray touchPointRay = MainCamera.ScreenPointToRay(touch0.position);
            gameObject.SetActive(true);
            if (
                Physics.Raycast(touchPointRay, out RaycastHit hit)
                    && hit.transform == gameObject.transform)
            {
                gameObject.transform.position = touch0.position;
                return true;
            }
        }
        return false;
    }


    public static void DeactivateRoadControlPoints()
    {
        StartObject.transform.position = Vector3.zero;
        StartControlObject.transform.position = Vector3.zero;
        EndControlObject.transform.position = Vector3.zero;
        EndObject.transform.position = Vector3.zero;

        StartObject.SetActive(false);
        StartControlObject.SetActive(false);
        EndControlObject.SetActive(false);
        EndObject.SetActive(false);
    }

    public static class TestRendrer
    {

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
                LineRenderer primaryLineRenderer = lineObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
                Material materialNeonLight = Resources.Load("NearestSphere") as Material;
                primaryLineRenderer.SetMaterials(new List<Material>() { materialNeonLight });
                primaryLineRenderer.material.SetColor("_Color", color);
                primaryLineRenderer.startWidth = width;
                primaryLineRenderer.endWidth = width;
                primaryLineRenderer.positionCount = 3;
            }

            return lineObject;
        }
        public static void RenderLine(
            string name,
            Color color,
            float width = 10f,
            float pointSize = 20f,
            params Vector3[] linePoints)
        {

            GameObject lineObject = GameObject.Find(name) ?? GetLineObject(name, color, width: width);
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
            lineRenderer.positionCount = linePoints.Length;

            lineRenderer.SetPositions(linePoints);
            if (IsDebugEnabled)
            {
                for (int i = 0; i < linePoints.Length; i++)
                {
                    RenderPointSphere(point: linePoints[i], size: pointSize, color: Color.yellow);
                }
            }
        }
        public static GameObject RenderPointSphere(
            Vector3 point,
            float size = 20f,
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
                sphereRenderer.material.color = color ?? Color.yellow;
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

        public static void FindBazierLinePoints(
            GameObject startObject,
            GameObject endObject,
            int vertexCount,
            GameObject startControlObject,
            GameObject endControlObject,
            out List<Vector3> bazierLinePoints)
        {
            Vector3 startPointPosition = startObject.transform.position;
            Vector3 endPointPosition = endObject.transform.position;
            var startToEndDirection = endPointPosition - startPointPosition;
            var startToEndDistance = startToEndDirection.magnitude;
            if (startControlObject.activeInHierarchy && startControlObject.transform.position == Vector3.zero)
            {
                startControlObject.transform.position = 0.33f * startToEndDistance * startToEndDirection.normalized;

            }
            if (endControlObject.activeInHierarchy && endControlObject.transform.position == Vector3.zero)
            {
                endControlObject.transform.position = 0.67f * startToEndDistance * startToEndDirection.normalized;

            }
            Vector3 p0 = startPointPosition;
            Vector3 p1 = startControlObject.transform.position;
            Vector3 p2 = endControlObject.transform.position;
            Vector3 p3 = endPointPosition;

            bazierLinePoints = new List<Vector3>();

            for (int s = 0; s < 1; s += 3)
            {
                if (s == 0)
                {
                    bazierLinePoints.Add(BezierPathCalculation(p0, p1, p2, p3, 0.0f));
                }

                for (int p = 0; p < (vertexCount); p++)
                {
                    float t = 1.0f / (vertexCount) * p;
                    Vector3 point = BezierPathCalculation(p0, p1, p2, p3, t);
                    bazierLinePoints.Add(point);
                }
            }
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
            TestRendrer.RenderLine(line.name, Color.red, width: 10f, linePoints: curvePoints.ToArray());
            return curvePoints;
        }

    }
}

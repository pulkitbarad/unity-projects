using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using System;
using UnityEngine.SocialPlatforms;
using UnityEngine.AI;

public class CommonController : MonoBehaviour
{

    private static readonly List<string> _existingPoints = new();
    public static string ObjectBeingDragged = "";
    private static bool IsDebugEnabled = false;
    public static bool IsRoadMenuActive = false;
    private static readonly Dictionary<string, Vector3> _InitialStaticLocalScale = new();

    public static GameObject CurrentRoadCenterLine;

    public static float CameraInitialHeight = 1f;
    public static Vector2 StartTouch0 = Vector2.zero;
    public static Vector2 StartTouch1 = Vector2.zero;
    private static readonly List<GameObject> _lineObjectPool = new();
    private static readonly int _lineObjectPoolCount;
    public static float MainCameraMoveSpeed;
    public static float MainCameraSmoothing;
    public static float MainCameraZoomSpeed;
    public static float MainCameraRotationSpeed;
    public static float MainCameraTiltSpeed;
    public static float MainCameraTiltAngleThreshold;
    private static GameObject _StaticObjectParent;
    private static GameObject _RoadControlObject;
    private static GameObject _RoadStartObject;
    private static GameObject _RoadEndObject;
    public static Camera MainCamera;
    public static GameObject MainCameraAnchor;
    public static GameObject MainCameraHolder;
    public static GameObject MainCameraRoot;



    // Start is called before the first frame update
    void Start()
    {
        TestRendrer.InstantiateLinePool();
        InitializeCamera();
        InitializeStaticObjects();
        DeactivateRoadControlPoints();
    }

    private static void InitializeCamera()
    {
        Transform rootTransform = MainCameraRoot.transform;
        rootTransform.position = new Vector3(-1033.7561f, 0, -77.6995697f);

        Transform anchorTransform = MainCameraAnchor.transform;
        anchorTransform.localPosition = Vector3.zero;
        anchorTransform.SetParent(rootTransform);

        Transform holderTransform = MainCameraHolder.transform;
        holderTransform.localPosition = new Vector3(0, 100, -100);
        holderTransform.eulerAngles = new Vector3(45, 0, 0);
        holderTransform.SetParent(anchorTransform);

        Transform cameraTransform = MainCamera.transform;
        cameraTransform.localPosition = Vector3.zero;
        cameraTransform.SetParent(holderTransform);

        CameraInitialHeight = holderTransform.localPosition.y;
    }


    public static void InitializeStaticObjects()
    {
        _StaticObjectParent = new GameObject("StaticObjects");
        _RoadStartObject = InitializeStaticObject(objectName: "RoadStart", size: 10, color: new Color(0.25f, 0.35f, 0.30f));
        _RoadControlObject = InitializeStaticObject(objectName: "RoadStartControl", size: 10, color: new Color(0, 1, 0.20f));
        _RoadEndObject = InitializeStaticObject(objectName: "RoadEnd", size: 10, color: new Color(0.70f, 0.45f, 0f));
    }

    public static GameObject InitializeStaticObject(string objectName, float size, Color? color)
    {
        GameObject gameObject = RenderCylinder(objectName: objectName, position: Vector3.zero, size: size, color: color);

        gameObject.transform.SetParent(_StaticObjectParent.transform);
        gameObject.SetActive(false);
        _InitialStaticLocalScale.Add(gameObject.name, gameObject.transform.localScale);
        return gameObject;
    }

    public static void DeactivateRoadControlPoints()
    {
        DeactivateRoadControlPoint(_RoadStartObject);
        DeactivateRoadControlPoint(_RoadControlObject);
        DeactivateRoadControlPoint(_RoadEndObject);
    }

    public static void DeactivateRoadControlPoint(GameObject gameObject)
    {
        gameObject.transform.position = Vector3.zero;
        gameObject.SetActive(false);
    }

    private static void ScaleStaticObjects()
    {
        ScaleStaticObject(_RoadStartObject);
        ScaleStaticObject(_RoadControlObject);
        ScaleStaticObject(_RoadEndObject);
    }

    private static void ScaleStaticObject(GameObject gameObject)
    {
        float magnitude = MainCameraHolder.transform.localPosition.y
                 / CameraInitialHeight;

        if (_InitialStaticLocalScale.ContainsKey(gameObject.name))
        {
            Vector3 initialScale = _InitialStaticLocalScale[gameObject.name];
            gameObject.transform.localScale =
            new Vector3(
               initialScale.x * magnitude,
               initialScale.y,
               initialScale.z * magnitude);
        }
    }

    public static void HandleRoadObjectsDrag(Vector2 touchPosition)
    {
        var areControlsActivated = false;

        if (IsRoadMenuActive && !EventSystem.current.IsPointerOverGameObject())
        {
            if (!_RoadStartObject.transform.position.Equals(Vector3.zero))
            {
                areControlsActivated =
                HandleGameObjectDrag(_RoadStartObject, touchPosition)
                || HandleGameObjectDrag(_RoadControlObject, touchPosition);

            }
            if (!_RoadEndObject.transform.position.Equals(Vector3.zero))
            {
                areControlsActivated = areControlsActivated
                || HandleGameObjectDrag(_RoadEndObject, touchPosition);
            }

            if (areControlsActivated)
            {
                RebuildRoad();
            }
        }
    }
    private static bool HandleGameObjectDrag(GameObject gameObject, Vector2 touchPosition)
    {
        if (!EventSystem.current.IsPointerOverGameObject() || ObjectBeingDragged.Length > 0)
        {
            Ray touchPointRay = MainCamera.ScreenPointToRay(touchPosition);
            gameObject.SetActive(true);
            if (
                (ObjectBeingDragged.Length > 0 && ObjectBeingDragged.Equals(gameObject.name))
                || (Physics.Raycast(touchPointRay, out RaycastHit hit)
                && hit.transform == gameObject.transform))
            {
                gameObject.transform.position = CameraMovement.GetTerrainHitPoint(touchPosition);
                ObjectBeingDragged = gameObject.name;
                return true;
            }
            Physics.Raycast(touchPointRay, out RaycastHit hit2);
        }
        return false;
    }
    public static void InitializeStartAndEndPositions()
    {
        Vector3 startObjectPosition, endObjectPosition;
        startObjectPosition = CameraMovement.GetTerrainHitPoint(GetScreenCenterPoint());
        _RoadStartObject.transform.position = endObjectPosition = startObjectPosition;
        endObjectPosition += 200f * MainCameraRoot.transform.right;
        _RoadEndObject.transform.position = endObjectPosition;
        _RoadStartObject.SetActive(true);
        _RoadEndObject.SetActive(true);
        _RoadControlObject.SetActive(true);
        IsRoadMenuActive = true;
        RebuildRoad();
    }

    public static void RebuildRoad()
    {

        CurvedLine.FindBazierLinePoints(
             roadStartObject: _RoadStartObject,
              roadEndObject: _RoadEndObject,
            vertexCount: 50,
              roadControlObject: _RoadControlObject,
            bazierLinePoints: out List<Vector3> bazierLinePoints);

        IsDebugEnabled = false;
        CurrentRoadCenterLine = TestRendrer.RenderLine(
            name: "Road_center",
            color: Color.yellow,
            // color: new Color(0f, 1f, 0.82f),
            width: 10,
            pointSize: 20,
            linePoints: bazierLinePoints.ToArray());

        IsDebugEnabled = false;
    }


    private static Vector2 GetScreenCenterPoint()
    {
        return new Vector2(Screen.width / 2, Screen.height / 2);
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
            params Vector3[] linePoints)
        {
            GameObject lineObject =
                GameObject.Find(name) ?? GetLineObject(name, color, width: width);
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
            lineRenderer.positionCount = linePoints.Length;

            lineRenderer.SetPositions(linePoints);
            if (IsDebugEnabled)
            {
                for (int i = 0; i < linePoints.Length; i++)
                {
                    RenderPoint(point: linePoints[i], size: pointSize, color: Color.yellow);
                }
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

    public static class CameraMovement
    {
        public static void MoveCamera(Vector2 direction)
        {
            Transform rootTransform = MainCameraRoot.transform;
            Vector3 right = rootTransform.right * direction.x;
            Vector3 forward = rootTransform.forward * direction.y;
            var input = (forward + right).normalized;

            float moveSpeed =
                MainCameraMoveSpeed
                * MainCameraHolder.transform.localPosition.y
                / CameraInitialHeight;

            Vector3 targetCameraPosition =
                rootTransform.position + (moveSpeed * input);

            rootTransform.position
                = Vector3.Lerp(
                    a: rootTransform.position,
                    b: targetCameraPosition,
                    t: Time.deltaTime * 100 * MainCameraSmoothing);
        }

        public static void TiltCamera(Vector2 currentTouch0, Vector2 currentTouch1)
        {
            var touch0Delta = currentTouch0 - StartTouch0;
            var touch1Delta = currentTouch1 - StartTouch1;

            var maxDeltaMagnitude = Math.Abs(Math.Max(touch0Delta.magnitude, touch1Delta.magnitude));
            if (maxDeltaMagnitude < 0)
                return;
            var delta0VerticalAngle = Vector2.Angle(touch0Delta, Vector2.up);
            var delta1VerticalAngle = Vector2.Angle(touch1Delta, Vector2.up);
            if (AreBothGesturesVertical(delta0VerticalAngle, delta1VerticalAngle))
            {

                if (delta0VerticalAngle > 90 || delta1VerticalAngle > 90)
                    TiltCamera(Math.Abs(maxDeltaMagnitude) / 100f);
                else
                    TiltCamera(Math.Abs(maxDeltaMagnitude) / -100f);
            }
        }

        public static void TiltCamera(float magnitude)
        {
            Transform rootTransform = MainCameraAnchor.transform;
            float currentHorizontalAngle, targetHorizontalAngle;
            currentHorizontalAngle = targetHorizontalAngle = rootTransform.eulerAngles.x;

            targetHorizontalAngle += magnitude * MainCameraTiltSpeed;
            // var targetEurlerAngle = Mathf.Lerp(currentHorizontalAngle, targetHorizontalAngle, Time.deltaTime * CommonController.MainCameraSmoothing);
            // rootTransform.rotation = Quaternion.AngleAxis(GetValidTiltAngle(targetEurlerAngle), rootTransform.right);
            rootTransform.eulerAngles =
            new Vector3(
                GetValidTiltAngle(targetHorizontalAngle),
                rootTransform.eulerAngles.y,
                rootTransform.eulerAngles.z
            );
        }

        private static float GetValidTiltAngle(float targetEurlerAngle)
        {
            var tempAngle = targetEurlerAngle;
            float holderLocalAngle = MainCameraHolder.transform.localEulerAngles.x;
            if (tempAngle > 180)
                tempAngle -= 360;

            if (tempAngle + holderLocalAngle > 80)
                tempAngle = 80 - holderLocalAngle;
            else if (tempAngle + holderLocalAngle < 25)
                tempAngle = 25 - holderLocalAngle;

            if (tempAngle < 0)
                tempAngle += 360;
            else if (tempAngle > 180)
                tempAngle -= 360;

            return tempAngle;
        }

        public static void RotateCamera(float magnitude)
        {
            RoatateObject(MainCameraRoot, magnitude, MainCameraRotationSpeed, MainCameraSmoothing);
            // RoatateObject(CommonController.MainCameraAnchor, magnitude, MainCameraRotationSpeed, MainCameraSmoothing);
        }

        private static void RoatateObject(GameObject gameObject, float magnitude, float rotationSpeed, float cameraSmoothing)
        {
            Transform objectTransform = gameObject.transform;
            float currentVerticalAngle, targetVerticalAngle;
            currentVerticalAngle = targetVerticalAngle = objectTransform.eulerAngles.y;
            targetVerticalAngle += magnitude * rotationSpeed;
            var targetEurlerAngle =
                Mathf.Lerp(
                    a: currentVerticalAngle,
                    b: targetVerticalAngle,
                    t: Time.deltaTime * cameraSmoothing);
            objectTransform.rotation = Quaternion.AngleAxis(targetEurlerAngle, objectTransform.up);
        }

        public static void ZoomCamera(Vector2 currentTouch0, Vector2 currentTouch1)
        {
            var touch0Delta = currentTouch0 - StartTouch0;
            var touch1Delta = currentTouch1 - StartTouch1;

            var maxDeltaMagnitude = Math.Abs(Math.Max(touch0Delta.magnitude, touch1Delta.magnitude));
            if (maxDeltaMagnitude < 0)
                return;
            var delta0VerticalAngle = Vector2.Angle(touch0Delta, Vector2.up);
            var delta1VerticalAngle = Vector2.Angle(touch1Delta, Vector2.up);
            if (!AreBothGesturesVertical(delta0VerticalAngle, delta1VerticalAngle))
            {
                if (IsTouchPinchingOut(currentTouch0, currentTouch1))
                    ZoomCamera(maxDeltaMagnitude);
                else
                    ZoomCamera(-1f * maxDeltaMagnitude);
            }
        }

        public static void ZoomCamera(float magnitude)
        {
            Vector3 cameraDirection =

                MainCameraAnchor
                .transform
                .InverseTransformDirection(

                    MainCameraHolder
                    .transform
                    .forward);

            Vector3 currentPosition = MainCameraHolder.transform.localPosition;
            Vector3 targetPosition;

            targetPosition = currentPosition + magnitude * cameraDirection;

            float tiltAngle = Vector3.Angle(cameraDirection, Vector3.down);
            if (targetPosition.y < 30)
                magnitude = (currentPosition.y - 30) / MathF.Cos(tiltAngle * MathF.PI / 180F);
            else if (targetPosition.y > 1000)
                magnitude = (1000 - currentPosition.y) / MathF.Cos(tiltAngle * MathF.PI / 180F);

            targetPosition = currentPosition + magnitude * cameraDirection;

            MainCameraHolder.transform.localPosition =
                Vector3.Lerp(
                    a: currentPosition,
                    b: targetPosition,
                    t: Time.deltaTime * MainCameraSmoothing);
            ScaleStaticObjects();
        }

        // public static void HandleTouchZoomAndTilt()
        // {
        //     if (Input.touchCount == 2)
        //     {
        //         var touch0 = Input.GetTouch(0);
        //         var touch1 = Input.GetTouch(1);

        //         var maxDeltaMagnitude =
        //             Math.Abs(
        //                 Math.Max(
        //                     touch0.deltaPosition.magnitude,
        //                     touch1.deltaPosition.magnitude));

        //         if (maxDeltaMagnitude <= 0)
        //             return;

        //         if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended)
        //         {
        //             _IsZoomInProgress = false;
        //             _IsTiltInProgress = false;
        //         }
        //         var touch0DeltaPosition = touch0.deltaPosition;
        //         var touch1DeltaPosition = touch1.deltaPosition;
        //         var delta0VerticalAngle = Vector2.Angle(touch0DeltaPosition, Vector2.up);
        //         var delta1VerticalAngle = Vector2.Angle(touch1DeltaPosition, Vector2.up);

        //         if (!_IsZoomInProgress
        //             && AreBothGesturesVertical(delta0VerticalAngle, delta1VerticalAngle))
        //         {
        //             //Lock the current movement for tilt only
        //             if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
        //                 _IsTiltInProgress = true;
        //             //Vertical tilt gesture
        //             if (delta0VerticalAngle > 90 || delta1VerticalAngle > 90)
        //                 CommonController.CameraMovement.TiltCamera(
        //                     isTiltup: false,
        //                     magnitude: maxDeltaMagnitude / 100 * CommonController.MainCameraTiltSpeedTouch);
        //             else
        //                 CommonController.CameraMovement.TiltCamera(
        //                     isTiltup: true,
        //                     magnitude: maxDeltaMagnitude / 100 * CommonController.MainCameraTiltSpeedTouch);
        //         }
        //         else if (!_IsTiltInProgress)
        //         {

        //             if (CommonController.CameraMovement.IsTouchPinchingOut(touch0, touch1))
        //                 //If Zoom-in, inverse the direction
        //                 CommonController.CameraMovement.ZoomCamera(
        //                     isZoomIn: true,
        //                     magnitude: maxDeltaMagnitude / 10 * CommonController.MainCameraZoomSpeedTouch);
        //             else
        //                 CommonController.CameraMovement.ZoomCamera(
        //                     isZoomIn: false,
        //                     magnitude: maxDeltaMagnitude / 10 * CommonController.MainCameraZoomSpeedTouch);
        //         }
        //     }
        // }

        public static void HandleMouseZoom()
        {
            Vector3 cameraDirection =

                MainCameraRoot
                .transform
                .InverseTransformDirection(

                    MainCameraHolder
                    .transform
                    .forward);

            float _input = Input.GetAxisRaw("Mouse ScrollWheel");
            var currentPosition = MainCameraHolder.transform.localPosition;
            Vector3 targetPosition =
                currentPosition
                + (cameraDirection
                    * _input * MainCameraZoomSpeed);
            // if (IsInBounds(nextTargetPosition)) _targetPosition = nextTargetPosition;
            MainCameraHolder.transform.localPosition
                = Vector3.Lerp(
                    MainCameraHolder.transform.localPosition,
                    targetPosition,
                    Time.deltaTime * MainCameraSmoothing);
        }
        public static bool AreBothGesturesVertical(float delta0VerticalAngle, float delta1VerticalAngle)
        {
            return (delta0VerticalAngle < MainCameraTiltAngleThreshold
                    && delta1VerticalAngle < MainCameraTiltAngleThreshold)
                || (delta0VerticalAngle > (180 - MainCameraTiltAngleThreshold)
                    && delta1VerticalAngle > (180 - MainCameraTiltAngleThreshold));
        }

        public static bool IsTouchPinchingOut(Vector2 touch0, Vector2 touch1)
        {
            return (StartTouch0 - StartTouch1).magnitude < (touch0 - touch1).magnitude;
        }
        public static Vector3 GetTerrainHitPoint(Vector2 origin)
        {
            Vector3 groundPosition = Vector3.zero;
            if (
                Physics.Raycast(ray: MainCamera.ScreenPointToRay(origin),
                    hitInfo: out RaycastHit _rayHit,
                    maxDistance: MainCamera.farClipPlane,
                    layerMask: LayerMask.GetMask("Ground")))
            {
                groundPosition = _rayHit.point;

            }
            return groundPosition;
        }
    }

    public static class CurvedLine
    {
        public static void FindBazierLinePoints(
            GameObject roadStartObject,
            GameObject roadEndObject,
            int vertexCount,
            GameObject roadControlObject,
            out List<Vector3> bazierLinePoints)
        {
            Vector3 startPointPosition = roadStartObject.transform.position;
            Vector3 endPointPosition = roadEndObject.transform.position;
            var startToEndDirection = endPointPosition - startPointPosition;
            var startToEndDistance = startToEndDirection.magnitude;


            if (roadControlObject.activeInHierarchy
                && roadControlObject.transform.position == Vector3.zero)
            {
                Vector3 midPointVector = 0.5f * startToEndDistance * startToEndDirection.normalized;
                roadControlObject.transform.position =
                    startPointPosition + Quaternion.AngleAxis(45, Vector3.up) * midPointVector;
            }
            Vector3 p0 = startPointPosition;
            Vector3 p1 = roadControlObject.transform.position;
            Vector3 p2 = endPointPosition;

            bazierLinePoints = new List<Vector3>();

            for (int p = 0; p < vertexCount; p++)
            {
                float t = 1.0f / vertexCount * p;
                Vector3 point = BezierPathCalculation(t, p0, p1, p2);
                bazierLinePoints.Add(point);
            }
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
            Vector3 p2 = controlPoints[2];

            Vector3 B = new();
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
            return B;
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

        private static List<Vector3> FindPerpendicularPoints(
            Vector3 originPoint,
            Vector3 targetPoint,
            float parallelWidth)
        {
            Vector3 forwardVector = targetPoint - originPoint;
            Vector3 upPoint = new Vector3(originPoint[0], originPoint[1] + 3, originPoint[2]);
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

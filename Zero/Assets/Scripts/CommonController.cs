using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using System;
using UnityEngine.SocialPlatforms;

public class CommonController : MonoBehaviour
{

    private static List<string> _existingPoints = new();
    private static string _ObjectBeingDragged = "";
    private static bool _IsDragInProgress = false;

    public static bool IsDebugEnabled = false;
    // public static bool IsSingleTouchConsumed = false;
    // public static bool IsDoubleTouchLocked = false;
    public static bool IsRoadMenuActive = false;
    public static float CameraInitialHeight = 1f;
    public static Vector2 StartTouch0 = Vector2.zero;
    public static Vector2 StartTouch1 = Vector2.zero;
    private static List<GameObject> _lineObjectPool = new();
    private static int _lineObjectPoolCount;
    public static float MainCameraMoveSpeed;
    public static float MainCameraSmoothing;
    public static float MainCameraZoomSpeed;
    public static float MainCameraRotationSpeed;
    public static float MainCameraTiltSpeed;
    public static float MainCameraTiltAngleThreshold;
    public static GameObject StartControlObject;
    public static GameObject StartObject;
    public static GameObject EndControlObject;
    public static GameObject EndObject;
    public static Camera MainCamera;
    public static GameObject MainCameraAnchor;
    public static GameObject MainCameraHolder;
    public static GameObject MainCameraRoot;


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

    public static bool HandleRoadObjectsDrag()
    {
        var areControlsActivated = false;
        if (Input.touchCount > 0)
        {

            if (CommonController.IsRoadMenuActive)
            {
                var touch0 = Input.GetTouch(0);
                if (CommonController.StartObject.transform.position.Equals(Vector3.zero)
                && CommonController.EndObject.transform.position.Equals(Vector3.zero)
                )
                {
                    Debug.Log("phase at initialization=" + touch0.phase);
                    InitializeStartAndEndPositions();
                    Debug.Log("Initialization complete");
                    areControlsActivated = true;
                }
                else
                {

                    if (!CommonController.StartObject.transform.position.Equals(Vector3.zero))
                    {
                        areControlsActivated =
                        CommonController.HandleGameObjectDrag(CommonController.StartObject)
                        || CommonController.HandleGameObjectDrag(CommonController.StartControlObject);
                    }
                    if (!CommonController.EndObject.transform.position.Equals(Vector3.zero))
                    {
                        areControlsActivated =
                        CommonController.HandleGameObjectDrag(CommonController.EndControlObject)
                        || CommonController.HandleGameObjectDrag(CommonController.EndObject);
                    }
                }

            }
        }
        return areControlsActivated;
    }
    private static bool HandleGameObjectDrag(GameObject gameObject)
    {

        if (Input.touchCount > 0)
        {
            var touch0 = Input.GetTouch(0);


            if (touch0.phase == TouchPhase.Ended)
            {
                _IsDragInProgress = false;
                _ObjectBeingDragged = "";

            }
            else if (touch0.phase == TouchPhase.Began)
            {
                _IsDragInProgress = true;
                _ObjectBeingDragged = "";

            }

            if (!EventSystem.current.IsPointerOverGameObject(touch0.fingerId)
                && touch0.phase == TouchPhase.Moved
                && touch0.deltaPosition.magnitude > 0)
            {
                Ray touchPointRay = MainCamera.ScreenPointToRay(touch0.position);
                gameObject.SetActive(true);
                if (
                    Physics.Raycast(touchPointRay, out RaycastHit hit)
                        && hit.transform.position == gameObject.transform.position)
                {
                    Debug.Log("hit.transform=" + hit.transform);
                    gameObject.transform.position = CameraMovement.GetTerrainHitPoint(touch0.position);
                    return true;
                }
            }
        }

        return false;
    }
    private static void InitializeStartAndEndPositions()
    {
        Vector3 startObjectPosition, endObjectPosition;
        startObjectPosition = CommonController.CameraMovement.GetTerrainHitPoint(Input.GetTouch(0).position);
        CommonController.StartObject.transform.position = endObjectPosition = startObjectPosition;
        endObjectPosition.z += 200f;
        CommonController.EndObject.transform.position = endObjectPosition;
        CommonController.StartObject.SetActive(true);
        CommonController.EndObject.SetActive(true);
    }

    public static void RebuildRoad()
    {

        // CommonController.CurvedLine.FindBazierLinePoints(
        //     startObject: CommonController.StartObject,
        //     endObject: CommonController.EndObject,
        //     vertexCount: 10,
        //     startControlObject: CommonController.StartControlObject,
        //     endControlObject: CommonController.EndControlObject,
        //     bazierLinePoints: out List<Vector3> bazierLinePoints);

        // CommonController.IsDebugEnabled = false;
        // CommonController.TestRendrer.RenderLine(
        //     name: "Road_center",
        //     color: Color.blue,
        //     width: 10,
        //     pointSize: 20,
        //     linePoints: bazierLinePoints.ToArray());

        // CommonController.IsDebugEnabled = false;
    }


    private static Vector2 GetScreenCenterPoint()
    {
        return new Vector2(Screen.width / 2, Screen.height / 2);
    }

    public static void InvokeOnTapHold(
        GameObject button,
        bool directionFlag,
        float magnitude,
        Action<bool, float> onButtonDown)
    {
        if (Input.touchCount > 0
                && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)
                //&& Input.GetTouch(0).phase == TouchPhase.Stationary
                )
        {
            PointerEventData eventData = new(EventSystem.current)
            {
                position = Input.GetTouch(0).position
            };
            List<RaycastResult> results = new();
            EventSystem.current.RaycastAll(eventData, results);
            if (results.First().gameObject.name.Equals(button.name))
            {
                onButtonDown(directionFlag, magnitude);
            }
        }
    }

    public static void InvokeOnTap(
        GameObject button,
        Action onButtonDown)
    {
        if (Input.touchCount > 0
                && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)
                //&& Input.GetTouch(0).phase == TouchPhase.Stationary
                )
        {
            PointerEventData eventData = new(EventSystem.current)
            {
                position = Input.GetTouch(0).position
            };
            List<RaycastResult> results = new();
            EventSystem.current.RaycastAll(eventData, results);
            if (results.First().gameObject.name.Equals(button.name))
            {
                onButtonDown();
            }
        }
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
                LineRenderer primaryLineRenderer =
                    lineObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
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

            GameObject lineObject =
                GameObject.Find(name) ?? GetLineObject(name, color, width: width);
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

    public static class CameraMovement
    {

        public static void MoveCamera(Vector2 direction)
        {
            Transform rootTransform = CommonController.MainCameraRoot.transform;
            Transform anchorTransform = CommonController.MainCameraAnchor.transform;
            Vector3 right = rootTransform.right * direction.x;
            Vector3 forward = rootTransform.forward * direction.y;
            var input = (forward + right).normalized;


            float moveSpeed =
                CommonController.MainCameraMoveSpeed
                * CommonController.MainCameraHolder.transform.localPosition.y
                / CameraInitialHeight;

            Vector3 targetCameraPosition =
                rootTransform.position + (moveSpeed * input);


            rootTransform.position
                = Vector3.Lerp(
                    a: rootTransform.position,
                    b: targetCameraPosition,
                    t: Time.deltaTime * 100 * CommonController.MainCameraSmoothing);
            // anchorTransform.position = rootTransform.position;
        }

        public static void TiltCamera(Vector2 currentTouch0, Vector2 currentTouch1)
        {
            var touch0Delta = currentTouch0 - CommonController.StartTouch0;
            var touch1Delta = currentTouch1 - CommonController.StartTouch1;

            var maxDeltaMagnitude = Math.Abs(Math.Max(touch0Delta.magnitude, touch1Delta.magnitude));
            if (maxDeltaMagnitude < 0)
                return;
            var delta0VerticalAngle = Vector2.Angle(touch0Delta, Vector2.up);
            var delta1VerticalAngle = Vector2.Angle(touch1Delta, Vector2.up);
            if (CommonController.CameraMovement.AreBothGesturesVertical(delta0VerticalAngle, delta1VerticalAngle))
            {

                if (delta0VerticalAngle > 90 || delta1VerticalAngle > 90)
                    TiltCamera(Math.Abs(maxDeltaMagnitude) / 100f);
                else
                    TiltCamera(Math.Abs(maxDeltaMagnitude) / -100f);
            }

        }

        public static void TiltCamera(float magnitude)
        {
            Transform rootTransform = CommonController.MainCameraAnchor.transform;
            float currentHorizontalAngle, targetHorizontalAngle;
            currentHorizontalAngle = targetHorizontalAngle = rootTransform.eulerAngles.x;

            targetHorizontalAngle += magnitude * CommonController.MainCameraTiltSpeed;
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
            float holderLocalAngle = CommonController.MainCameraHolder.transform.localEulerAngles.x;
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

            Debug.Log("targetEurlerAngle=" + targetEurlerAngle);
            Debug.Log("tempAngle=" + tempAngle);

            return tempAngle;
        }

        public static void RotateCamera(float magnitude)
        {
            RoatateObject(CommonController.MainCameraRoot, magnitude, MainCameraRotationSpeed, MainCameraSmoothing);
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
            var touch0Delta = currentTouch0 - CommonController.StartTouch0;
            var touch1Delta = currentTouch1 - CommonController.StartTouch1;

            var maxDeltaMagnitude = Math.Abs(Math.Max(touch0Delta.magnitude, touch1Delta.magnitude));
            if (maxDeltaMagnitude < 0)
                return;
            var delta0VerticalAngle = Vector2.Angle(touch0Delta, Vector2.up);
            var delta1VerticalAngle = Vector2.Angle(touch1Delta, Vector2.up);
            if (!CommonController.CameraMovement.AreBothGesturesVertical(delta0VerticalAngle, delta1VerticalAngle))
            {
                if (IsTouchPinchingOut(currentTouch0, currentTouch1))
                {
                    ZoomCamera(maxDeltaMagnitude);
                }
                else
                {
                    ZoomCamera(-1f * maxDeltaMagnitude);
                }
            }
        }

        public static void ZoomCamera(float magnitude)
        {
            Vector3 cameraDirection =
                CommonController
                .MainCameraAnchor
                .transform
                .InverseTransformDirection(
                    CommonController
                    .MainCameraHolder
                    .transform
                    .forward);

            Vector3 currentPosition = CommonController.MainCameraHolder.transform.localPosition;
            Vector3 targetPosition = currentPosition;
            Transform holder = CommonController.MainCamera.transform;


            targetPosition += magnitude * cameraDirection;

            if (targetPosition.y > 3 && targetPosition.y < 1000)
            {
                CommonController.MainCameraHolder.transform.localPosition =
                    Vector3.Lerp(
                        a: currentPosition,
                        b: targetPosition,
                        t: Time.deltaTime * CommonController.MainCameraSmoothing);
            }
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
                CommonController
                .MainCameraRoot
                .transform
                .InverseTransformDirection(
                    CommonController
                    .MainCameraHolder
                    .transform
                    .forward);

            float _input = Input.GetAxisRaw("Mouse ScrollWheel");
            var currentPosition = CommonController.MainCameraHolder.transform.localPosition;
            Vector3 targetPosition =
                currentPosition
                + (cameraDirection
                    * _input * CommonController.MainCameraZoomSpeed);
            // if (IsInBounds(nextTargetPosition)) _targetPosition = nextTargetPosition;
            CommonController.MainCameraHolder.transform.localPosition
                = Vector3.Lerp(
                    CommonController.MainCameraHolder.transform.localPosition,
                    targetPosition,
                    Time.deltaTime * CommonController.MainCameraSmoothing);
        }
        public static bool AreBothGesturesVertical(float delta0VerticalAngle, float delta1VerticalAngle)
        {
            return (delta0VerticalAngle < CommonController.MainCameraTiltAngleThreshold
                    && delta1VerticalAngle < CommonController.MainCameraTiltAngleThreshold)
                || (delta0VerticalAngle > (180 - CommonController.MainCameraTiltAngleThreshold)
                    && delta1VerticalAngle > (180 - CommonController.MainCameraTiltAngleThreshold));
        }

        public static bool IsTouchPinchingOut(Vector2 touch0, Vector2 touch1)
        {
            return (StartTouch0 - StartTouch1).magnitude < (touch0 - touch1).magnitude;
        }
        public static Vector3 GetTerrainHitPoint(Vector2 origin)
        {
            Vector3 groundPosition = Vector3.zero;

            if (
                Physics.Raycast(ray: CommonController.MainCamera.ScreenPointToRay(origin),
                    hitInfo: out RaycastHit _rayHit,
                    maxDistance: CommonController.MainCamera.farClipPlane,
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
            if (startControlObject.activeInHierarchy
                && startControlObject.transform.position == Vector3.zero)
            {
                startControlObject.transform.position =
                    0.33f * startToEndDistance * startToEndDirection.normalized;

            }
            if (endControlObject.activeInHierarchy
                && endControlObject.transform.position == Vector3.zero)
            {
                endControlObject.transform.position =
                    0.67f * startToEndDistance * startToEndDirection.normalized;

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

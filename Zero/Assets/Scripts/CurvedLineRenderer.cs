using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CurvedLineRenderer : MonoBehaviour
{

    public Transform StartPoint;
    public Transform PivotPoint;
    public Transform EndPoint;
    public float VertexCount = 6;
    public float RoadWidth = 3;
    private List<string> _existingPoints = new List<string>();
    private bool _isDebugEnabled = false;

    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {

        List<Vector3> mainLinePoints = new List<Vector3>(){StartPoint.position,PivotPoint.position,EndPoint.position}; 
        

        List<Vector3> curvePoints = CreateSmoothCurve(GetLineObject("PrimaryCurvedLine", new Color(0.1568f,1f,0.21f,1f)),mainLinePoints,VertexCount);

        // FindParallelPoints(curvePoints[0],curvePoints[1],RoadWidth);
        // FindParallelPoints(curvePoints[1],curvePoints[2],RoadWidth);
        // FindParallelPoints(curvePoints[2],curvePoints[3],RoadWidth);
        List<Vector3> rightParallelPoints = new List<Vector3>();
        List<Vector3> leftParallelPoints = new List<Vector3>();
        List<Vector3> parallelPoints = new List<Vector3>();
        if(curvePoints.Count>=3)
        {
            for(int i=1; i < curvePoints.Count; i+=1)
            {
                parallelPoints = FindParallelPoints(curvePoints[i-1],curvePoints[i],RoadWidth);
                rightParallelPoints.Add(parallelPoints[0]);
                leftParallelPoints.Add(parallelPoints[1]);

                if(i == curvePoints.Count-1){
                    parallelPoints = FindParallelPoints(curvePoints[i-1],curvePoints[i],RoadWidth,true);
                    rightParallelPoints.Add(parallelPoints[0]);
                    leftParallelPoints.Add(parallelPoints[1]);

                }
            }
            RenderLine(GetLineObject("RightCurvedLine", new Color(0.156f,1f,0.972f,1f)),rightParallelPoints);
            RenderLine(GetLineObject("LeftCurvedLine", new Color(0.96875f,0.578f,0.578f,1f)),leftParallelPoints);
        }
 
    }

    List<Vector3> CreateSmoothCurve(GameObject line, List<Vector3> points, float vertexCount)
    {


        Vector3 startPosition = points[0];
        Vector3 midPosition  = points[1];
        Vector3 endPosition  = points[2];

        List<Vector3> curvePoints = new List<Vector3>();

        // curvePoints.Add(startPosition);
        string lineName = line.name;
        for(float interpolationRatio = 0; interpolationRatio<=1;interpolationRatio+= 1/vertexCount)
        {
            var tangent1 = Vector3.Lerp(startPosition, midPosition, interpolationRatio);
            var tangent2 = Vector3.Lerp(midPosition, endPosition, interpolationRatio);
            var curve = Vector3.Lerp(tangent1,tangent2,interpolationRatio);
            curvePoints.Add(curve);
            // string sphereName = lineName+"_"+curve[0];
            // if(_existingPoints.FirstOrDefault(e=> e.Contains(sphereName)) ==null)
            // {
            //     GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //     sphere.name = sphereName;
            //     sphere.transform.position = curve;
            //     _existingPoints.Add(sphereName);
            // }
        }
        // curvePoints.Add(endPosition);
        RenderLine(line,curvePoints);
        return curvePoints;
    }

    void RenderLine(GameObject line, List<Vector3> linePoints){

        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray());
    }

    List<Vector3> FindParallelPoints(Vector3 originPoint, Vector3 targetPoint, float parallelWidth, bool isReversed=false){

        Vector3 newOriginPoint = originPoint;
        Vector3 newTargetPoint = targetPoint;
        if(isReversed){
            newOriginPoint = targetPoint;
            newTargetPoint = originPoint;
        }

        Vector3 forwardVector = newTargetPoint - newOriginPoint;
        Vector3 upPoint = (new Vector3(newOriginPoint[0],newOriginPoint[1]+ 3,newOriginPoint[2]));
        Vector3 upVector = upPoint - newOriginPoint;
        Vector3 rightVector = Vector3.Cross(forwardVector,upVector).normalized;
        var rightPoint = newOriginPoint + (rightVector * parallelWidth);
        var leftPoint = newOriginPoint - (rightVector * parallelWidth);

        if(isReversed){
            var temp = rightPoint;
            rightPoint = leftPoint;
            leftPoint = temp;
        }

        if(_isDebugEnabled){
            Debug.DrawLine(newOriginPoint, newOriginPoint+forwardVector, Color.red, Mathf.Infinity);
            Debug.DrawLine(newOriginPoint, upPoint, Color.green, Mathf.Infinity);
            Debug.DrawLine(newOriginPoint, rightPoint, Color.red, Mathf.Infinity);
            Debug.DrawLine(newOriginPoint, leftPoint, Color.yellow, Mathf.Infinity);
            DrawPointSphere(newOriginPoint);
            DrawPointSphere(rightPoint);
            DrawPointSphere(newTargetPoint);
            DrawPointSphere(leftPoint);
            DrawPointSphere(upPoint);
        }

        return new List<Vector3>(){rightPoint,leftPoint};
    }

    GameObject GetLineObject(string lineName, Color lineColor){
        GameObject primaryLine = GameObject.Find(lineName);
        
        if(primaryLine == null){
            primaryLine = new GameObject(lineName);
            LineRenderer primaryLineRenderer = primaryLine.AddComponent(typeof(LineRenderer)) as LineRenderer;
            Material materialNeonLight = Resources.Load("NearestSphere") as Material;
            primaryLineRenderer.SetMaterials(new List<Material>(){materialNeonLight});
            primaryLineRenderer.material.SetColor("_Color", lineColor);
            primaryLineRenderer.startWidth = 0.5f;
            primaryLineRenderer.endWidth = 0.5f;
            primaryLineRenderer.positionCount = 3;
        }

        return primaryLine;
    }

    void DrawPointSphere(Vector3 point){
        string sphereName = "Point_"+point[0];
        if(_existingPoints.FirstOrDefault(e=> e.Contains(sphereName)) ==null)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = sphereName;
            sphere.transform.position = point;
            _existingPoints.Add(sphereName);
        }
    }
}

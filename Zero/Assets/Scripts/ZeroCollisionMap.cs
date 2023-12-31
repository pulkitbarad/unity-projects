using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ZeroCollisionMap
{

    public static int COLLISION_ORIGIN_LEFT_START = 0;
    public static int COLLISION_ORIGIN_RIGHT_START = 1;
    public static int COLLISION_ORIGIN_LEFT_END = 2;
    public static int COLLISION_ORIGIN_RIGHT_END = 3;

    public ZeroRoadLane PrimaryLane;
    public string RoadName;
    public string LayerMaskName;
    public List<ZeroCollisionInfo> Collisions;
    public Dictionary<string, List<ZeroCollisionInfo>> CollisionsByCollidingLane = new();
    public List<ZeroCollisionInfo> LeftStartCollisions = new();
    public List<ZeroCollisionInfo> RightStartCollisions = new();
    public List<ZeroCollisionInfo> LeftEndCollisions = new();
    public List<ZeroCollisionInfo> RightEndCollisions = new();
    public bool IsValid = false;

    public ZeroCollisionMap(
        string roadName,
        ZeroRoadLane primaryLane,
        string layerMaskName)
    {
        this.RoadName = roadName;
        this.LayerMaskName = layerMaskName;
        this.PrimaryLane = primaryLane;
        for (int segmentIndex = 0; segmentIndex < primaryLane.Segments.Length; segmentIndex++)
        {
            ZeroRoadSegment segment = primaryLane.Segments[segmentIndex];
            List<Collider> partialOverlaps = GetPartialOverlaps(roadName, segment, layerMaskName);
            Debug.Log(segment.Name + " partial overlaps= " + partialOverlaps.Select(e => e.gameObject.name).ToCommaSeparatedString());

            if (partialOverlaps.Count > 0)
            {
                this.GetCollisions(
                    primarySegment: segment,
                    overalppingColliders: partialOverlaps);
            }
        }
    }

    public ZeroLaneIntersection[] GetIntersections()
    {
        GenerateCollisonLists();
        List<ZeroLaneIntersection> intersections = new();
        if (this.IsValid)
        {
            foreach (var entry in this.CollisionsByCollidingLane)
            {
                ZeroRoadLane intersectingLane = entry.Value[0].CollidingSegment.ParentLane;
                ZeroRoadLane primaryLane = entry.Value[0].PrimarySegment.ParentLane;
                for (int i = 0; i < this.LeftStartCollisions.Count; i++)
                {
                    Vector3 leftStart = this.LeftStartCollisions[i].CollisionPoint;
                    Vector3 rightStart = this.RightStartCollisions[i].CollisionPoint;
                    Vector3 leftEnd = this.LeftEndCollisions[i].CollisionPoint;
                    Vector3 rightEnd = this.RightEndCollisions[i].CollisionPoint;
                    ZeroRenderer.RenderSphere(leftStart);
                    ZeroRenderer.RenderSphere(leftEnd);
                    ZeroRenderer.RenderSphere(rightStart);
                    ZeroRenderer.RenderSphere(rightEnd);

                    intersections.Add(
                        new ZeroLaneIntersection(
                            intersectionPoints:
                                new ZeroParallelogram(
                                    leftStart: leftStart,
                                    rightStart: rightStart,
                                    leftEnd: leftEnd,
                                    rightEnd: rightEnd),
                            primaryLane: primaryLane,
                            intersectingLane: intersectingLane)
                    );
                }
            }
        }
        return intersections.ToArray();
    }

    public void GenerateCollisonLists()
    {
        ZeroCollisionInfo[] sortedCollisions =
            this.CollisionsByCollidingLane.Values
            .SelectMany(e => e)
            .OrderBy(e => e.PrimarySegment.Index)
            .ToArray();
        foreach (ZeroCollisionInfo collision in sortedCollisions)
        {
            if (collision.CollisionOriginType == ZeroCollisionMap.COLLISION_ORIGIN_LEFT_START)
                this.LeftStartCollisions.Add(collision);
            else if (collision.CollisionOriginType == ZeroCollisionMap.COLLISION_ORIGIN_RIGHT_START)
                this.RightStartCollisions.Add(collision);
            else if (collision.CollisionOriginType == ZeroCollisionMap.COLLISION_ORIGIN_LEFT_END)
                this.LeftEndCollisions.Add(collision);
            else if (collision.CollisionOriginType == ZeroCollisionMap.COLLISION_ORIGIN_RIGHT_END)
                this.RightEndCollisions.Add(collision);
        }
        Debug.Log("leftStartCollisionPoints count=" + LeftStartCollisions.Count());
        Debug.Log("rightStartCollisionPoints count=" + RightStartCollisions.Count());
        Debug.Log("leftEndCollisionPoints count=" + LeftEndCollisions.Count());
        Debug.Log("rightEndCollisionPoints count=" + RightEndCollisions.Count());
        if (this.LeftStartCollisions.Count() == this.LeftEndCollisions.Count()
            && this.LeftStartCollisions.Count() == this.RightStartCollisions.Count()
            && this.LeftStartCollisions.Count() == this.RightEndCollisions.Count())
        {
            this.IsValid = true;
        }
    }

    private List<Collider> GetPartialOverlaps(
        string roadName,
        ZeroRoadSegment segment,
        string layerMaskName)
    {
        ZeroParallelogram segmentTopPlane = segment.TopPlane;
        Collider[] overlaps = Physics.OverlapBox(
            center: segment.SegmentObject.transform.position,
            halfExtents: segment.SegmentObject.transform.localScale / 2,
            orientation: segment.SegmentObject.transform.rotation,
            layerMask: LayerMask.GetMask(layerMaskName));
        List<Collider> partialOverlaps = new();
        Debug.Log(segment.Name + " all overlaps= " + overlaps.Select(e => e.gameObject.name).ToCommaSeparatedString());

        foreach (Collider collider in overlaps)
        {
            string colliderGameObjectName = collider.gameObject.name;
            if (ZeroRoadBuilder.BuiltRoadSegments.ContainsKey(colliderGameObjectName)
                && !ZeroRoadBuilder.BuiltRoadSegments[colliderGameObjectName]
                    .ParentLane.ParentRoad.Name.Equals(roadName)
                && !IsColliderWithinbounds(collider, segmentTopPlane.GetVertices()))
            {
                partialOverlaps.Add(collider);
            }
        }
        return partialOverlaps;
    }

    private void GetCollisions(
        ZeroRoadSegment primarySegment,
        List<Collider> overalppingColliders)
    {
        foreach (Collider collider in overalppingColliders)
        {
            ZeroRoadSegment colliderSegment = ZeroRoadBuilder.BuiltRoadSegments[collider.gameObject.name];
            ZeroParallelogram primaryTopPlane = primarySegment.TopPlane;

            if (GetRayHitPointOnSegment(
                origin: primaryTopPlane.LeftStart,
                end: primaryTopPlane.LeftEnd,
                maxDistance: primarySegment.OldLength,
                collider: collider,
                hitPoint: out Vector3? hitPoint))
            {
                Vector3 collisionPoint = hitPoint.Value;
                this.AddCollision(
                    primarySegment: primarySegment,
                    collidingSegment: colliderSegment,
                    collisionPoint: collisionPoint,
                    collisionOriginType: ZeroCollisionMap.COLLISION_ORIGIN_LEFT_START,
                    distanceFromOrigin: (collisionPoint - primaryTopPlane.LeftStart).magnitude);
                ZeroRenderer.RenderSphere(collisionPoint);
            }

            if (GetRayHitPointOnSegment(
                origin: primaryTopPlane.RightStart,
                end: primaryTopPlane.RightEnd,
                maxDistance: primarySegment.OldLength,
                collider: collider,
                hitPoint: out Vector3? hitPoint2))
            {
                Vector3 collisionPoint = hitPoint2.Value;
                this.AddCollision(
                    primarySegment: primarySegment,
                    collidingSegment: colliderSegment,
                    collisionPoint: collisionPoint,
                    collisionOriginType: ZeroCollisionMap.COLLISION_ORIGIN_RIGHT_START,
                    distanceFromOrigin: (collisionPoint - primaryTopPlane.RightStart).magnitude);
                ZeroRenderer.RenderSphere(collisionPoint);
            }

            if (GetRayHitPointOnSegment(
                origin: primaryTopPlane.LeftEnd,
                end: primaryTopPlane.LeftStart,
                maxDistance: primarySegment.OldLength,
                collider: collider,
                hitPoint: out Vector3? hitPoint3))
            {
                Vector3 collisionPoint = hitPoint3.Value;
                this.AddCollision(
                    primarySegment: primarySegment,
                    collidingSegment: colliderSegment,
                    collisionPoint: collisionPoint,
                    collisionOriginType: ZeroCollisionMap.COLLISION_ORIGIN_LEFT_END,
                    distanceFromOrigin: (collisionPoint - primaryTopPlane.LeftEnd).magnitude);
                ZeroRenderer.RenderSphere(collisionPoint);
            }

            if (GetRayHitPointOnSegment(
                origin: primaryTopPlane.RightEnd,
                end: primaryTopPlane.RightStart,
                maxDistance: primarySegment.OldLength,
                collider: collider,
                hitPoint: out Vector3? hitPoint4))
            {
                Vector3 collisionPoint = hitPoint4.Value;
                this.AddCollision(
                    primarySegment: primarySegment,
                    collidingSegment: colliderSegment,
                    collisionPoint: collisionPoint,
                    collisionOriginType: ZeroCollisionMap.COLLISION_ORIGIN_RIGHT_END,
                    distanceFromOrigin: (collisionPoint - primaryTopPlane.RightEnd).magnitude);
                ZeroRenderer.RenderSphere(collisionPoint);
            }
        }
    }


    public void AddCollision(
        ZeroRoadSegment primarySegment,
        ZeroRoadSegment collidingSegment,
        Vector3 collisionPoint,
        int collisionOriginType,
        float distanceFromOrigin)
    {

        ZeroCollisionInfo collision = new(
             primarySegment: primarySegment,
             collidingSegment: collidingSegment,
             collisionPoint: collisionPoint,
             collisionOriginType: collisionOriginType,
             distanceFromOrigin: distanceFromOrigin);

        string collidingLaneName = collision.CollidingSegment.ParentLane.Name;

        if (!this.CollisionsByCollidingLane.ContainsKey(collidingLaneName))
            this.CollisionsByCollidingLane[collidingLaneName] = new();

        //Validate if the collision is too close to collision with another subsequent segment on the same colliding lane,
        // caused by the segment extensions in cureved roads
        int lookupIndex =
            this.CollisionsByCollidingLane
                [collidingLaneName]
                    .FindIndex(e =>
                        e.CollidingSegment.ParentLane.Name
                        .Equals(collidingLaneName)
                            && (e.CollisionPoint - collision.CollisionPoint).magnitude
                                    < collision.CollidingSegment.Width
                            && e.DistanceFromOrigin < collision.DistanceFromOrigin);
        if (lookupIndex != -1)
        {
            if (this.CollisionsByCollidingLane[collidingLaneName][lookupIndex]
                .DistanceFromOrigin > collision.DistanceFromOrigin)
            {
                this.CollisionsByCollidingLane[collidingLaneName][lookupIndex] = collision;
            }
        }
        else
        {
            this.CollisionsByCollidingLane[collidingLaneName].Add(collision);
        }
    }

    private static bool GetRayHitPointOnSegment(
        Vector3 origin,
        Vector3 end,
        float maxDistance,
        Collider collider,
        out Vector3? hitPoint)
    {
        ZeroRoadSegment colliderSegment = ZeroRoadBuilder.BuiltRoadSegments[collider.gameObject.name];
        ZeroParallelogram colliderTopPlane = colliderSegment.TopPlane;
        Vector3 direction = end - origin;
        if (collider.Raycast(
                ray: new Ray(origin, direction),
                hitInfo: out RaycastHit rayHitInfo,
                maxDistance: maxDistance)
            && (
                colliderSegment.PreviousSibling == null
                || !IsPointInsideBounds(
                    rayHitInfo.point,
                    colliderSegment.PreviousSibling.TopPlane.GetVertices()
                ))
            && (
                colliderSegment.NextSibling == null
                || !IsPointInsideBounds(
                    rayHitInfo.point,
                    colliderSegment.NextSibling.TopPlane.GetVertices()
                ))
            && !IsPointOnLineSegment(
                    rayHitInfo.point,
                    colliderTopPlane.LeftStart,
                    colliderTopPlane.RightStart)
            && !IsPointOnLineSegment(
                    rayHitInfo.point,
                    colliderTopPlane.LeftEnd,
                    colliderTopPlane.RightEnd))
        {
            hitPoint = rayHitInfo.point;
            return true;
        }
        hitPoint = null;
        return false;
    }

    private static bool IsColliderWithinbounds(Collider collider, Vector3[] bounds)
    {
        ZeroParallelogram colliderTopPlane = ZeroRoadBuilder.BuiltRoadSegments[collider.gameObject.name].TopPlane;
        return colliderTopPlane.GetVertices().Length > 0 && IsRectWithinBounds(colliderTopPlane, bounds);
    }

    private static bool IsRectWithinBounds(ZeroParallelogram rectangle, Vector3[] bounds)
    {
        for (int i = 0; i < 4; i++)
            if (!IsPointInsideBounds(rectangle.GetVertices()[i], bounds))
                return false;
        return true;
    }

    private static bool IsPointOnLineSegment(Vector3 point, Vector3 start, Vector3 end)
    {
        return Math.Round(Vector3.Cross(point - start, end - point).magnitude, 2) == 0;

    }
    private static bool IsPointInsideBounds(Vector3 point, Vector3[] bounds, float margin = 0)
    {
        float maxX = bounds[0].x;
        float maxY = bounds[0].y;
        float minX = bounds[0].x;
        float minY = bounds[0].y;

        for (int i = 1; i < 4; i++)
        {
            if (bounds[i].x > maxX) maxX = bounds[i].x;
            if (bounds[i].y > maxY) maxY = bounds[i].y;
            //
            if (bounds[i].x < minX) minX = bounds[i].x;
            if (bounds[i].y < minY) minY = bounds[i].y;
        }
        return point.x > minX && point.x < maxX && point.y > minY && point.y < maxY;
    }
}

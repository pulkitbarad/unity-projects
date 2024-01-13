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
            List<Collider> partialOverlaps = GetPartialOverlaps(segment);

            if (partialOverlaps.Count > 0)
            {
                this.GetCollisions(
                    primarySegment: segment,
                    overalppingColliders: partialOverlaps);
            }
        }
    }

    private List<Collider> GetPartialOverlaps(ZeroRoadSegment segment)
    {
        ZeroPolygon segmentTopPlane = segment.SegmentBounds.TopPlane;
        Collider[] overlaps = Physics.OverlapBox(
            center: segment.SegmentObject.transform.position,
            halfExtents: segment.SegmentObject.transform.localScale / 2,
            orientation: segment.SegmentObject.transform.rotation,
            layerMask: LayerMask.GetMask(this.LayerMaskName));
        List<Collider> partialOverlaps = new();
        if (overlaps.Length > 0)
        {
            foreach (Collider collider in overlaps)
            {
                string colliderGameObjectName = collider.gameObject.name;
                if (ZeroRoadBuilder.BuiltRoadSegmentsByName.ContainsKey(colliderGameObjectName)
                    && !ZeroRoadBuilder.BuiltRoadSegmentsByName[colliderGameObjectName]
                        .ParentLane.ParentRoad.Name.Equals(this.RoadName)
                    && !IsColliderWithinbounds(collider, segmentTopPlane.Vertices))
                {
                    partialOverlaps.Add(collider);
                }
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
            ZeroRoadSegment colliderSegment = ZeroRoadBuilder.BuiltRoadSegmentsByName[collider.gameObject.name];
            ZeroPolygon primaryTopPlane = primarySegment.SegmentBounds.TopPlane;

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

        //Validate if the collision is too close to a collision with another subsequent segment of the same colliding lane,
        // this is typically caused by the segment extension in cureved roads.
        int lookupIndex =
            this.CollisionsByCollidingLane
                [collidingLaneName]
                    .FindIndex(e =>
                        e.CollidingSegment.ParentLane.Name
                        .Equals(collidingLaneName)
                            && (e.CollisionPoint - collision.CollisionPoint).magnitude
                                    < collision.CollidingSegment.Width);
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

    public Dictionary<string, List<ZeroLaneIntersection>> GetLaneIntersectionsByRoadName()
    {
        Dictionary<string, List<ZeroLaneIntersection>> intersectionsByRoadName = new();

        foreach (var entry in this.CollisionsByCollidingLane)
        {
            string collidingLaneName = entry.Key;

            List<ZeroCollisionInfo> leftStartCollisions = new();
            List<ZeroCollisionInfo> rightStartCollisions = new();
            List<ZeroCollisionInfo> leftEndCollisions = new();
            List<ZeroCollisionInfo> rightEndCollisions = new();


            foreach (ZeroCollisionInfo collision in entry.Value)
            {
                if (collision.CollisionOriginType == ZeroCollisionMap.COLLISION_ORIGIN_LEFT_START)
                    leftStartCollisions.Add(collision);
                else if (collision.CollisionOriginType == ZeroCollisionMap.COLLISION_ORIGIN_RIGHT_START)
                    rightStartCollisions.Add(collision);
                else if (collision.CollisionOriginType == ZeroCollisionMap.COLLISION_ORIGIN_LEFT_END)
                    leftEndCollisions.Add(collision);
                else if (collision.CollisionOriginType == ZeroCollisionMap.COLLISION_ORIGIN_RIGHT_END)
                    rightEndCollisions.Add(collision);
            }
            //Each lane can intersect twice at maximum with another lane; 
            //Once for straight lane and twice for curved lanes,
            //because current implementation only supports bending a road in one direction during construction.
            //And each intersection should have the same number of left/right start/end collision points,
            // to make one (or two) valid parallelogram(s).
            if ((leftStartCollisions.Count() == 2
                    || leftStartCollisions.Count() == 1)
                && leftStartCollisions.Count() == leftEndCollisions.Count()
                && rightStartCollisions.Count() == rightEndCollisions.Count()
                && leftStartCollisions.Count() == rightStartCollisions.Count()
            )
            {
                leftStartCollisions =
                    leftStartCollisions
                    .OrderBy(e => e.PrimarySegment.RoadLengthSofar)
                    .ThenBy(e => e.DistanceFromOrigin)
                    .ToList();
                rightStartCollisions =
                    rightStartCollisions
                    .OrderBy(e => e.PrimarySegment.RoadLengthSofar)
                    .ThenBy(e => e.DistanceFromOrigin)
                    .ToList();

                leftEndCollisions =
                    leftEndCollisions
                    .OrderBy(e => e.PrimarySegment.RoadLengthSofar)
                    .ThenBy(e => -e.DistanceFromOrigin)
                    .ToList();
                rightEndCollisions =
                    rightEndCollisions
                    .OrderBy(e => e.PrimarySegment.RoadLengthSofar)
                    .ThenBy(e => -e.DistanceFromOrigin)
                    .ToList();

                string collidingRoadName = leftStartCollisions[0].CollidingSegment.ParentLane.ParentRoad.Name;

                if (!intersectionsByRoadName.ContainsKey(collidingRoadName))
                {
                    intersectionsByRoadName[collidingRoadName] = new();
                }
                string laneIntersectionName = this.PrimaryLane.Name + leftStartCollisions[0].CollidingSegment.ParentLane.Name + "I0";
                intersectionsByRoadName[collidingRoadName].Add(
                new ZeroLaneIntersection(
                    name: laneIntersectionName,
                    primaryDistance: leftStartCollisions[0].PrimarySegment.RoadLengthSofar,
                    intersectionPoints:
                        new ZeroPolygon(
                            name: laneIntersectionName + "P",
                             leftStartCollisions[0].CollisionPoint,
                             leftEndCollisions[0].CollisionPoint,
                             rightEndCollisions[0].CollisionPoint,
                             rightStartCollisions[0].CollisionPoint),
                    primaryLane: this.PrimaryLane,
                    intersectingLane: leftStartCollisions[0].CollidingSegment.ParentLane));
                // ZeroRenderer.RenderSphere(leftStartCollisions[0].CollisionPoint, sphereName: laneIntersectionName + "LS", color: Color.white);
                // ZeroRenderer.RenderSphere(rightStartCollisions[0].CollisionPoint, sphereName: laneIntersectionName + "RS", color: Color.black);
                // ZeroRenderer.RenderSphere(leftEndCollisions[0].CollisionPoint, sphereName: laneIntersectionName + "LE", color: Color.gray);
                // ZeroRenderer.RenderSphere(rightEndCollisions[0].CollisionPoint, sphereName: laneIntersectionName + "RE", color: Color.cyan);

                //If primary lane is intersecting twice with another lane,
                // intersection closest to the primary lane start will have 
                //left (and right) start points closer to their ray origin point used in collision detection   
                //Vice versa, left (and right) end points farther from their ray origin point used in collision detection   
                if (leftStartCollisions.Count() == 2)
                {
                    laneIntersectionName = this.PrimaryLane.Name + leftStartCollisions[0].CollidingSegment.ParentLane.Name + "I1";
                    intersectionsByRoadName[collidingRoadName].Add(
                    new ZeroLaneIntersection(
                        name: laneIntersectionName,
                        primaryDistance: leftStartCollisions[1].PrimarySegment.RoadLengthSofar,
                        intersectionPoints:
                            new ZeroPolygon(
                                name: laneIntersectionName + "P",
                             leftStartCollisions[1].CollisionPoint,
                             leftEndCollisions[1].CollisionPoint,
                             rightEndCollisions[1].CollisionPoint,
                             rightStartCollisions[1].CollisionPoint),
                        primaryLane: this.PrimaryLane,
                        intersectingLane: leftStartCollisions[1].CollidingSegment.ParentLane));
                    // ZeroRenderer.RenderSphere(leftStartCollisions[1].CollisionPoint, sphereName: laneIntersectionName + "LS2", color: Color.green);
                    // ZeroRenderer.RenderSphere(rightStartCollisions[1].CollisionPoint, sphereName: laneIntersectionName + "RS2", color: Color.red);
                    // ZeroRenderer.RenderSphere(leftEndCollisions[1].CollisionPoint, sphereName: laneIntersectionName + "LE2", color: Color.blue);
                    // ZeroRenderer.RenderSphere(rightEndCollisions[1].CollisionPoint, sphereName: laneIntersectionName + "RE2", color: Color.yellow);
                }
                this.IsValid = true;
            }
            else
                this.IsValid = false;
        }
        return intersectionsByRoadName;
    }

    private static bool GetRayHitPointOnSegment(
        Vector3 origin,
        Vector3 end,
        float maxDistance,
        Collider collider,
        out Vector3? hitPoint)
    {
        ZeroRoadSegment colliderSegment = ZeroRoadBuilder.BuiltRoadSegmentsByName[collider.gameObject.name];
        ZeroPolygon colliderTopPlane = colliderSegment.SegmentBounds.TopPlane;
        Vector3 direction = end - origin;

        if (collider.Raycast(
                ray: new Ray(origin, direction),
                hitInfo: out RaycastHit rayHitInfo,
                maxDistance: maxDistance)
            && (
                colliderSegment.PreviousSibling == null
                || !IsPointInsideBounds(
                    rayHitInfo.point,
                    colliderSegment.PreviousSibling.SegmentBounds.TopPlane.Vertices
                ))
            && (
                colliderSegment.NextSibling == null
                || !IsPointInsideBounds(
                    rayHitInfo.point,
                    colliderSegment.NextSibling.SegmentBounds.TopPlane.Vertices
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
        ZeroPolygon colliderTopPlane = ZeroRoadBuilder.BuiltRoadSegmentsByName[collider.gameObject.name].SegmentBounds.TopPlane;
        return colliderTopPlane.Vertices.Length > 0 && IsRectWithinBounds(colliderTopPlane, bounds);
    }

    private static bool IsRectWithinBounds(ZeroPolygon rectangle, Vector3[] bounds)
    {
        for (int i = 0; i < 4; i++)
            if (!IsPointInsideBounds(rectangle.Vertices[i], bounds))
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

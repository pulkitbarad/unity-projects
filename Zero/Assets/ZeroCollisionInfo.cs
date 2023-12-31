using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroCollisionInfo
{
        public string CollisionId;
        public ZeroRoadSegment PrimarySegment;
        public ZeroRoadSegment CollidingSegment;
        public Vector3 CollisionPoint;
        public float DistanceFromOrigin;
        public int CollisionOriginType;

        public ZeroCollisionInfo(
            ZeroRoadSegment primarySegment,
            ZeroRoadSegment collidingSegment,
            Vector3 collisionPoint,
            int collisionOriginType,
            float distanceFromOrigin
        )
        {
            this.PrimarySegment = primarySegment;
            this.CollidingSegment = collidingSegment;
            this.CollisionPoint = collisionPoint;
            this.CollisionOriginType = collisionOriginType;
            this.DistanceFromOrigin = distanceFromOrigin;
            this.CollisionId = primarySegment.Name + collidingSegment.Name + collisionOriginType;
        }
}

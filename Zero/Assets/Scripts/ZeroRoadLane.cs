using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroRoadLane
{
        public string Name;
        public int LaneIndex;
        public ZeroRoadSegment[] Segments;
        public GameObject LaneObject;
        public ZeroRoad ParentRoad;

        public ZeroRoadLane(
            string name,
            int laneIndex,
            ZeroRoad parentRoad,
            ZeroRoadSegment[] segments)
        {
            this.Name = name;
            this.LaneIndex = laneIndex;
            this.ParentRoad = parentRoad;
            this.Segments = segments;
            InitLaneObject(segments[segments.Length > 2 ? segments.Length / 2 - 1 : 0].Center);
            AssignParentToSegments();
        }

        public void AssignParentToSegments()
        {
            for (int i = 0; i < this.Segments.Length; i++)
            {
                this.Segments[i].ParentLane = this;
            }
        }

        public void InitLaneObject(Vector3 position)
        {
            GameObject laneObject =
                ZeroController.FindGameObject(this.Name, true)
                ?? new GameObject();

            laneObject.name = this.Name;
            laneObject.transform.position = position;
            laneObject.transform.SetParent(this.ParentRoad.RoadObject.transform);
            this.LaneObject = laneObject;
        }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ZeroObjectManager
{
    public static int OBJECT_TYPE_DEBUG_SPHERE = 0;
    public static int OBJECT_TYPE_ROAD_CUBE = 1;
    public static int OBJECT_TYPE_ROAD_POLYGON_3D = 2;
    public static ZeroObjectPool PoolDebugSphere;
    public static ZeroObjectPool PoolRoadCube;
    public static ZeroObjectPool PoolRoadPolygon3D;

    public static void Initialise()
    {
        PoolDebugSphere = new(
            objectType: OBJECT_TYPE_DEBUG_SPHERE,
            namePrefix: "DebugSphereObject",
            batchSize: 1000,
            maxSize: 5000
        );
        PoolRoadCube = new(
            objectType: OBJECT_TYPE_ROAD_CUBE,
            namePrefix: "RoadCubeObject",
            batchSize: 1000,
            maxSize: 5000
        );
        PoolRoadPolygon3D = new(
            objectType: OBJECT_TYPE_ROAD_POLYGON_3D,
            namePrefix: "RoadPolygon3DObjects",
            batchSize: 1000,
            maxSize: 5000
        );
    }

    public static GameObject GetObjectFromPool(string name, int objectType)
    {
        if (objectType == OBJECT_TYPE_DEBUG_SPHERE)
            return PoolDebugSphere.GetObject(name);
        else if (objectType == OBJECT_TYPE_ROAD_CUBE)
            return PoolRoadCube.GetObject(name);
        else
            return PoolRoadPolygon3D.GetObject(name);
    }

    public static bool ReleaseObjectToPool(GameObject gameObject, int objectType)
    {
        if (objectType == OBJECT_TYPE_DEBUG_SPHERE)
            return PoolDebugSphere.ReleaseObject(gameObject);
        else if (objectType == OBJECT_TYPE_ROAD_CUBE)
            return PoolRoadCube.ReleaseObject(gameObject);
        else
            return PoolRoadPolygon3D.ReleaseObject(gameObject);
    }

    public static GameObject FindOrCreateGameObject(string objectName)
    {
        GameObject gameObject = FindGameObject(objectName, true) ?? new GameObject(objectName);
        gameObject.SetActive(true);
        return gameObject;
    }

    private static GameObject FindGameObject(string objectName, bool findDisabled)
    {
        if (findDisabled)
        {
            foreach (GameObject resultObject in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
                if (!EditorUtility.IsPersistent(resultObject.transform.root.gameObject)
                     && !(resultObject.hideFlags == HideFlags.NotEditable || resultObject.hideFlags == HideFlags.HideAndDontSave))
                    if (resultObject.name == objectName)
                        return resultObject;
        }
        else
        {
            return GameObject.Find(objectName);
        }
        return null;
    }
}

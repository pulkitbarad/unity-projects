using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ZeroObjectManager
{
    public static class ObjectType
    {
        public static int DEBUG_SPHERE = 0;
        public static int ROAD_CUBE = 1;
        public static int ROAD_POLYGON_3D = 2;
        public static int STATIC_CYLINDER = 3;
    }

    private static class PoolPrefix
    {
        public static string DebugSphere = "DebugSphereObject";
        public static string RoadCube = "RoadCubeObject";
        public static string RoadPolygon3D = "RoadPolygon3DObject";
        public static string StaticCylinder = "StaticCylinderObject";
    }

    private static class PoolParent
    {
        public static GameObject DebugSphere = new("SphereObjects");
        public static GameObject RoadCube = new("RoadCubeObjects");
        public static GameObject RoadPolygon3D = new("RoadPolygon3DObjects");
        public static GameObject StaticCylinder = new("StaticCylinderObjects");
    }

    private static class PoolReference
    {
        public static GameObject[] DebugSphere;
        public static GameObject[] RoadCube;
        public static GameObject[] RoadPolygon3D;
        public static GameObject[] StaticCylinder;
    }

    public static class PoolBatchSize
    {
        public static int DebugSphere;
        public static int RoadCube;
        public static int RoadPolygon3D;
        public static int StaticCylinder;
    }

    public static class PoolMaxSize
    {
        public static int DebugSphere;
        public static int RoadCube;
        public static int RoadPolygon3D;
        public static int StaticCylinder;
    }

    public static void Initialise()
    {
        InitialiseConfig();
        InitialisePools();
    }

    private static void InitialiseConfig()
    {
        ZeroObjectManager.PoolBatchSize.DebugSphere = 3000;
        ZeroObjectManager.PoolBatchSize.RoadCube = 5000;
        ZeroObjectManager.PoolBatchSize.RoadPolygon3D = 1000;
        ZeroObjectManager.PoolBatchSize.StaticCylinder = 5;
    }

    private static void InitialisePools()
    {

        PoolReference.DebugSphere = new GameObject[PoolBatchSize.DebugSphere];
        PoolReference.RoadCube = new GameObject[PoolBatchSize.RoadCube];
        PoolReference.RoadPolygon3D = new GameObject[PoolBatchSize.RoadPolygon3D];
        PoolReference.StaticCylinder = new GameObject[PoolBatchSize.StaticCylinder];

        for (int i = 0;
            i < Math.Max(
                Math.Max(
                    PoolBatchSize.DebugSphere,
                    PoolBatchSize.RoadCube),
                PoolBatchSize.RoadPolygon3D);
            i++)
        {
            GameObject newObject;

            if (i < PoolBatchSize.DebugSphere)
            {
                newObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                newObject.name = PoolPrefix.DebugSphere + i;
                newObject.transform.SetParent(PoolParent.DebugSphere.transform);
                newObject.SetActive(false);
                PoolReference.DebugSphere[i] = newObject;

            }
            if (i < PoolBatchSize.RoadCube)
            {
                newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                newObject.name = PoolPrefix.RoadCube + i;
                newObject.transform.SetParent(PoolParent.RoadCube.transform);
                newObject.SetActive(false);
                PoolReference.RoadCube[i] = newObject;
            }
            if (i < PoolBatchSize.RoadPolygon3D)
            {
                newObject = new(PoolPrefix.RoadPolygon3D + i);
                newObject.transform.SetParent(PoolParent.RoadPolygon3D.transform);
                newObject.SetActive(false);
                PoolReference.RoadPolygon3D[i] = newObject;
            }
            if (i < PoolBatchSize.StaticCylinder)
            {
                newObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                newObject.name = PoolPrefix.StaticCylinder + i;
                newObject.transform.SetParent(PoolParent.StaticCylinder.transform);
                newObject.SetActive(false);
                PoolReference.StaticCylinder[i] = newObject;
            }
        }
    }

    public static GameObject GetNewObject(string name, int objectType)
    {
        if (objectType == ObjectType.DEBUG_SPHERE)
            return GetNewDebugObject(name);
        else if (objectType == ObjectType.ROAD_CUBE)
            return GetNewRoadCubeObject(name);
        else if (objectType == ObjectType.ROAD_POLYGON_3D)
            return GetNewRoadPolygon3DObject(name);
        else if (objectType == ObjectType.STATIC_CYLINDER)
            return GetNewStaticCylinderObject(name);

        //TO-DO: Log to the server about unsupported object classification
        GameObject unidentifiedObject = FindOrCreateGameObject(name, true);
        unidentifiedObject.SetActive(true);
        return unidentifiedObject;
    }

    public static bool ReleaseObject(string name, int objectType)
    {
        if (objectType == ObjectType.DEBUG_SPHERE)
        {
            var matches = PoolReference.DebugSphere.Where(e => e.name.Equals(name));
            if (matches.Count() > 0)
            {
                matches.First().SetActive(false);
                return true;
            }
        }
        else if (objectType == ObjectType.ROAD_CUBE)
        {
            var matches = PoolReference.RoadCube.Where(e => e.name.Equals(name));
            if (matches.Count() > 0)
            {
                matches.First().SetActive(false);
                return true;
            }
        }
        else if (objectType == ObjectType.ROAD_POLYGON_3D)
        {
            var matches = PoolReference.RoadPolygon3D.Where(e => e.name.Equals(name));
            if (matches.Count() > 0)
            {
                matches.First().SetActive(false);
                return true;
            }
        }
        else if (objectType == ObjectType.STATIC_CYLINDER)
        {
            var matches = PoolReference.StaticCylinder.Where(e => e.name.Equals(name));
            if (matches.Count() > 0)
            {
                matches.First().SetActive(false);
                return true;
            }
        }
        //TO-DO: Log to the server about object being released is not managed by any pool
        GameObject unidentifiedObject = FindGameObject(name, false);
        if (unidentifiedObject != null)
        {
            unidentifiedObject.SetActive(false);
            return true;
        }
        return false;
    }

    private static GameObject GetNewDebugObject(string name)
    {

        for (int i = 0; i < PoolReference.DebugSphere.Length; i++)
        {
            var newDebugObject = PoolReference.DebugSphere[i];
            if (!newDebugObject.activeInHierarchy)
            {
                if (name.Length > 0)
                    newDebugObject.name = name;
                newDebugObject.SetActive(true);
                return newDebugObject;
            }
        }

        if (PoolReference.DebugSphere.Length < PoolMaxSize.DebugSphere)
        {
            ExtendDebugObjectPool();
            return GetNewRoadCubeObject(name);
        }
        //TO-DO: Log to the server that the game has exceeded max road pool size.
        GameObject newObject = new(name);
        newObject.SetActive(true);
        return newObject;
    }

    private static GameObject GetNewRoadCubeObject(string name)
    {

        for (int i = 0; i < PoolReference.RoadCube.Length; i++)
        {
            var newRoadObject = PoolReference.RoadCube[i];
            if (!newRoadObject.activeInHierarchy)
            {
                if (name.Length > 0)
                    newRoadObject.name = name;
                newRoadObject.SetActive(true);
                return newRoadObject;
            }
        }

        if (PoolReference.RoadCube.Length < PoolMaxSize.RoadCube)
        {
            ExtendRoadCubeObjectPool();
            return GetNewRoadCubeObject(name);
        }
        //TO-DO: Log to the server that the game has exceeded max debug pool size.
        GameObject newObject = new(name);
        newObject.SetActive(true);
        return newObject;
    }

    private static GameObject GetNewRoadPolygon3DObject(string name)
    {

        for (int i = 0; i < PoolReference.RoadPolygon3D.Length; i++)
        {
            var newRoadObject = PoolReference.RoadPolygon3D[i];
            if (!newRoadObject.activeInHierarchy)
            {
                if (name.Length > 0)
                    newRoadObject.name = name;
                newRoadObject.SetActive(true);
                return newRoadObject;
            }
        }

        if (PoolReference.RoadPolygon3D.Length < PoolMaxSize.RoadPolygon3D)
        {
            ExtendRoadPolygon3DObjectPool();
            return GetNewRoadCubeObject(name);
        }
        //TO-DO: Log to the server that the game has exceeded max debug pool size.
        GameObject newObject = new(name);
        newObject.SetActive(true);
        return newObject;
    }

    private static GameObject GetNewStaticCylinderObject(string name)
    {
        for (int i = 0; i < PoolReference.StaticCylinder.Length; i++)
        {
            var newRoadObject = PoolReference.StaticCylinder[i];
            if (!newRoadObject.activeInHierarchy)
            {
                if (name.Length > 0)
                    newRoadObject.name = name;
                newRoadObject.SetActive(true);
                return newRoadObject;
            }
        }
        //Static objects are of fixed number and does not requrie extension. 
        //The purpose of the pool is to maintain object life cycle.
        GameObject newObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        newObject.SetActive(true);
        return newObject;
    }

    private static void ExtendDebugObjectPool()
    {
        Array.Resize(ref PoolReference.DebugSphere, PoolReference.DebugSphere.Length + PoolMaxSize.DebugSphere);
        for (int i = PoolReference.DebugSphere.Length; i < PoolReference.DebugSphere.Length + PoolMaxSize.DebugSphere; i++)
        {
            GameObject newObject = new(PoolPrefix.DebugSphere + i);
            newObject.transform.SetParent(PoolParent.DebugSphere.transform);
            newObject.SetActive(false);
            PoolReference.DebugSphere[i] = newObject;
        }
    }

    private static void ExtendRoadCubeObjectPool()
    {
        Array.Resize(ref PoolReference.RoadCube, PoolReference.RoadCube.Length + PoolMaxSize.RoadCube);
        for (int i = PoolReference.RoadCube.Length; i < PoolReference.RoadCube.Length + PoolMaxSize.RoadCube; i++)
        {
            GameObject newObject = new(PoolPrefix.RoadCube + i);
            newObject.transform.SetParent(PoolParent.RoadCube.transform);
            newObject.SetActive(false);
            PoolReference.RoadCube[i] = newObject;
        }
    }

    private static void ExtendRoadPolygon3DObjectPool()
    {
        Array.Resize(ref PoolReference.RoadPolygon3D, PoolReference.RoadPolygon3D.Length + PoolMaxSize.RoadPolygon3D);
        for (int i = PoolReference.RoadPolygon3D.Length; i < PoolReference.RoadPolygon3D.Length + PoolMaxSize.RoadPolygon3D; i++)
        {
            GameObject newObject = new(PoolPrefix.RoadPolygon3D + i);
            newObject.transform.SetParent(PoolParent.RoadPolygon3D.transform);
            newObject.SetActive(false);
            PoolReference.RoadPolygon3D[i] = newObject;
        }
    }

    private static GameObject FindOrCreateGameObject(string objectName, bool findDisabled)
    {
        GameObject gameObject = FindGameObject(objectName, findDisabled) ?? new GameObject(objectName);
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

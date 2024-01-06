using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroObjectPool
{
    private int _objectType;
    private string _namePrefix;
    private GameObject _parentObject;
    private Stack<GameObject> _stack;
    private int _objectsInUse = 0;
    private int _batchSize;
    private int _maxSize;

    public ZeroObjectPool(
        int objectType,
        string namePrefix,
        int batchSize,
        int maxSize
        )
    {
        this._objectType = objectType;
        this._namePrefix = namePrefix;
        this._batchSize = batchSize;
        this._maxSize = maxSize;
        this._parentObject = new GameObject(namePrefix);
        this._stack = new();
    }

    public void InitialiseStack()
    {
        for (int i = _objectsInUse; i < _objectsInUse + _batchSize; i++)
        {
            GameObject newObject = CreateNewObject();
            newObject.name = _namePrefix + i;
            newObject.transform.SetParent(_parentObject.transform);
            newObject.SetActive(false);
            _stack.Push(newObject);
        }
    }

    public bool ReleaseObject(GameObject gameObject)
    {
        gameObject.SetActive(false);
        _stack.Push(gameObject);
        _objectsInUse--;
        return true;
    }

    public GameObject GetObject(string name)
    {
        GameObject newObject;
        if (_stack.Count == 0)
        {
            if (_objectsInUse + _batchSize <= _maxSize)
            {
                InitialiseStack();
                newObject = _stack.Pop();
            }
            else
            {
                //TO-DO: Log to the server that the game has exceeded max debug pool size.
                newObject = CreateNewObject();
            }
        }
        else
            newObject = _stack.Pop();

        if (name.Length > 0)
            newObject.name = name;
        _objectsInUse++;
        newObject.SetActive(true);
        return newObject;
    }

    private GameObject CreateNewObject()
    {
        if (_objectType == ZeroObjectManager.OBJECT_TYPE_DEBUG_SPHERE)
            return GameObject.CreatePrimitive(PrimitiveType.Sphere);
        else if (_objectType == ZeroObjectManager.OBJECT_TYPE_ROAD_CUBE)
            return GameObject.CreatePrimitive(PrimitiveType.Cube);
        else
            return new();
    }
}

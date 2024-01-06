using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ZeroRenderer
{

    private static readonly List<GameObject> _sphereObjectPool = new();
    private static readonly int _sphereObjectPoolCount;
    private static readonly List<string> _existingSpheres = new();
    public static GameObject DebuggingParent;
    private static Material _baseLineMaterial;
    public static bool IsDebugEnabled = false;


    // public static GameObject GetSphereObject(
    //     string name,
    //     UnityEngine.Color? color = null,
    //     float width = 0.5f,
    //     Transform parentTransform = null)
    // {
    //     GameObject sphere = GetSphereFromObjectPool(name);

    //     if (sphere == null)
    //     {
    //         sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //         _existingSpheres.Add(newSphereName);
    //     }
    //     sphere.name = newSphereName;
    //     sphere.transform.localScale = new Vector3(size, size, size);
    //     sphere.transform.position = position;
    //     var sphereRenderer = sphere.GetComponent<Renderer>();
    //     sphereRenderer.material.color = color ?? UnityEngine.Color.yellow;

    //     if (parentTransform != null)
    //         sphere.transform.SetParent(parentTransform);
    //     else
    //         sphere.transform.SetParent(DebuggingParent.transform);
    //     sphere.SetActive(true);

    //     return sphere;

    // }

    public static GameObject RenderSphere(
        Vector3 position,
        string sphereName = "",
        float size = 0.5f,
        Transform parentTransform = null,
        UnityEngine.Color? color = null)
    {

        GameObject sphere = ZeroObjectManager.GetObjectFromPool(sphereName, ZeroObjectManager.OBJECT_TYPE_DEBUG_SPHERE);

        sphere.transform.localScale = new Vector3(size, size, size);
        sphere.transform.position = position;
        var sphereRenderer = sphere.GetComponent<Renderer>();
        sphereRenderer.material.color = color ?? UnityEngine.Color.yellow;

        if (parentTransform != null)
            sphere.transform.SetParent(parentTransform);
        else
            sphere.transform.SetParent(DebuggingParent.transform);

        return sphere;
    }
}

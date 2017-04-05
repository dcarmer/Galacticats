using System.Collections.Generic;
using UnityEngine;

public static class ObjectPool
{
    private static Dictionary<GameObject, List<GameObject>> Pool = new Dictionary<GameObject, List<GameObject>>();
    private static Dictionary<GameObject, Transform> Parents = new Dictionary<GameObject, Transform>();
    public static void Spawn(GameObject prefab, System.Action<GameObject> makeReady)
    {
        if(!Pool.ContainsKey(prefab))
        {
            Pool.Add(prefab, new List<GameObject>(1));
            Parents[prefab] = new GameObject(prefab.name + " Pool").transform;
        }
        GameObject inActive = Pool[prefab].Find((x) => !x.activeInHierarchy);
        if (inActive == null)
        {
            Pool[prefab].Add(inActive = Object.Instantiate(prefab));
            inActive.transform.parent = Parents[prefab];
        }
        inActive.SetActive(true);
        makeReady(inActive);
    }
    public static void Spawn<T>(T prefab, System.Action<T> makeReady) where T: Component
    {
        if (!Pool.ContainsKey(prefab.gameObject))
        {
            Pool.Add(prefab.gameObject, new List<GameObject>(1));
            Parents[prefab.gameObject] = new GameObject(prefab.name + " Pool").transform;
        }
        GameObject inActive = Pool[prefab.gameObject].Find((x) => !x.activeInHierarchy);
        if (inActive == null)
        {
            Pool[prefab.gameObject].Add(inActive = Object.Instantiate(prefab.gameObject));
            inActive.transform.parent = Parents[prefab.gameObject];
        }
        inActive.SetActive(true);
        makeReady(inActive.GetComponent<T>());
    }
}
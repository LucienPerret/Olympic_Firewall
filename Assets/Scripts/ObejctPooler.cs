using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ObejctPooler : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int poolSize = 10;

    public GameObject Prefab => prefab;

    private List<GameObject> _pool;

    private void Start()
    {
        // create pool
        _pool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewObject();
        }
        
    }

    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        _pool.Add(obj);
        return obj;

    }

    public GameObject GetPooledObject()
    {
        foreach (GameObject obj in _pool)
        {
            if (!obj.activeSelf) return obj;
        }
        return CreateNewObject();
    }
}

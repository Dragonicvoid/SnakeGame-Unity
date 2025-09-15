#nullable enable
using System.Collections.Generic;
using UnityEngine;

public class BasePooler : MonoBehaviour
{
    [SerializeField]
    GameObject? parent = null;

    [SerializeField]
    GameObject? food = null;

    [SerializeField]
    int initial = 20;

    List<GameObject> pool;

    void Awake()
    {
        if (!parent)
        {
            parent = gameObject;
        }
    }

    void Start()
    {
        for (int i = 0; i < initial; i++)
        {
            createNew();
        }
    }

    public GameObject? GetGameObj()
    {
        if (pool.Count <= 0)
        {
            createNew();
        }

        GameObject? obj = Util.Pop(pool);

        if (!obj) return null;

        return obj;
    }

    GameObject? createNew()
    {
        if (!food) return null;

        GameObject? obj = Instantiate(food);
        ReturnNode(obj);

        return obj;
    }

    public void ReturnNode(GameObject gameObj)
    {
        if (!gameObj) return;

        gameObj.SetActive(false);
        if (parent) gameObj.transform.SetParent(parent.transform);
        pool.Add(gameObj);
    }
}

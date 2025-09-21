#nullable enable
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [SerializeField]
    GameObject? parent = null;

    [SerializeField]
    BasePooler? pooler = null;

    void Awake()
    {
        if (!parent)
        {
            parent = gameObject;
        }
    }

    public void RemoveFood(GameObject gameObj)
    {
        pooler?.ReturnNode(gameObj);
    }

    public GameObject? Spawn(Vector2 pos)
    {
        GameObject? food = pooler?.GetGameObj();

        if (!food) return null;

        if (parent) food.transform.SetParent(parent.transform);
        food.transform.localPosition = pos;
        food.SetActive(true);

        return food;
    }
}

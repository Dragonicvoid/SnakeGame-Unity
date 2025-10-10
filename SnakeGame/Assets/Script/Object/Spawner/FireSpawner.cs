using UnityEngine;

public class FireSpawner : MonoBehaviour
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

    public void RemoveFire(Fire fire)
    {
        pooler?.ReturnNode(fire.gameObject);
    }

    public Fire? Spawn(Vector2 pos, bool isMainPlayer)
    {
        GameObject? fireObj = pooler?.GetGameObj();

        if (!fireObj) return null;

        if (parent) fireObj.transform.SetParent(parent.transform);
        fireObj.transform.localPosition = new Vector3(pos.x, pos.y, -0.5f);
        Fire? fire = fireObj.GetComponent<Fire>();

        fire.SetLayer(isMainPlayer ? LAYER.PHYSICS_PLAYER_BODIES : LAYER.PHYSICS_ENEMY_BODIES);
        fireObj.SetActive(true);


        return fire;
    }
}

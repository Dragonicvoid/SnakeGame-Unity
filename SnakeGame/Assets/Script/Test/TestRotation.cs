using UnityEngine;

public class TestRotation : MonoBehaviour
{
    [SerializeField]
    GameObject target;

    // Update is called once per frame
    void Update()
    {
        Vector3 rotTarget = target.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(rotTarget, Vector3.up);
    }
}


using UnityEngine;

public class CheckCameraProp : MonoBehaviour
{
    struct TweenCamera
    {
        public GameObject Cam;
        public Vector3 Target;
        public Vector3 Start;
        public Quaternion StartRot;
        public Quaternion TargetRot;
    }
    Camera? cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        startAnim();
    }

    void Update()
    {
    }

    void startAnim()
    {
        // BaseTween<TweenCamera> tweenData = new BaseTween<TweenCamera>(
        //     2,
        //     new TweenCamera
        //     {
        //         Cam = cam.gameObject,
        //         Target = new Vector3(0, 0, 0),
        //         Start = cam.transform.position,
        //     },
        //     (dist, data) =>
        //     {
        //         cam.transform.SetPositionAndRotation(data.Start, Quaternion.RotateTowards())
        //     },
        //     (dist, data) =>
        //     {

        //     },
        //     (dist, data) =>
        //     {

        //     }
        // );
        // IEnumerator<GameObject> anim = Tween.Create<GameObject>()
    }
}

#nullable enable
using UnityEngine;

public class HeadCollider : MonoBehaviour
{
  Collider2D? coll = null;
  Rigidbody2D? rgdBody = null;

  void Awake()
  {
    coll = GetComponent<Collider2D>();
    if (coll == null)
    {
      coll = gameObject.AddComponent<Collider2D>();
    }

    rgdBody = GetComponent<Rigidbody2D>();
    if (rgdBody == null)
    {
      rgdBody = gameObject.AddComponent<Rigidbody2D>();
    }
  }

  void OnCollisionEnter2D(Collision2D collision)
  {
    GameObject self = collision.gameObject;
    GameObject other = collision.otherCollider.gameObject;
    ContactPoint2D hitPos = collision.contacts[0];

    HeadCollideData data = new HeadCollideData
    {
      Self = self,
      Other = other,
      HitPos = hitPos.point,
    };
    CollisionEvent.Instance.HeadCollide(data);
  }
}

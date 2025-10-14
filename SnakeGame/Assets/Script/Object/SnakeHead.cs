using System.Collections.Generic;
using UnityEngine;

public class SnakeHead : MonoBehaviour
{
  // (0 - N) = Nearest - Farthest, 
  [SerializeField] List<Sprite> headSprites;
  [SerializeField] Sprite headDead;
  float minDistance = 50;
  float maxDistance = 150;
  SpriteRenderer? spriteRend;

  Vector2 front = Vector2.up;

  void Awake()
  {
    spriteRend = gameObject.GetComponent<SpriteRenderer>();

    if (spriteRend)
    {
      spriteRend.size = new Vector2(ARENA_DEFAULT_SIZE.SNAKE, ARENA_DEFAULT_SIZE.SNAKE);
    }
  }

  public void UpdateStatus(Vector2 dir, Vector2 pos)
  {
    updateAngle(dir);
    updatePos(pos);
  }

  void updateAngle(Vector2 dir)
  {
    float angle = Mathf.Atan2(dir.y, dir.x) - Mathf.Atan2(front.y, front.x);
    if (angle < 0) { angle += 2 * Mathf.PI; }

    gameObject.transform.eulerAngles = new Vector3(0, 0, angle * Mathf.Rad2Deg);
  }

  void updatePos(Vector2 pos)
  {
    gameObject.transform.localPosition = new Vector3(pos.x, pos.y, gameObject.transform.localPosition.z);
  }

  public void UpdateHeadSprite(float nearest)
  {
    if (nearest < 0)
    {
      spriteRend.sprite = headDead;
      return;
    }

    if (headSprites.Count <= 0) return;

    if (nearest < minDistance)
    {
      spriteRend.sprite = headSprites[0];
    }
    else if (nearest >= minDistance && nearest < maxDistance)
    {
      int startIdx = 1;
      int endIdx = headSprites.Count - 1;
      int delta = endIdx - startIdx;

      if (delta < 0)
      {
        spriteRend.sprite = headSprites[0];
      }
      else
      {
        float distPerSprite = delta <= 0 ? 1 : (1 / delta);
        int dist = delta * Mathf.FloorToInt((nearest - minDistance) / (maxDistance - minDistance) / distPerSprite);
        int idx = startIdx + dist;
        spriteRend.sprite = headSprites[idx];
      }
    }
    else
    {
      spriteRend.sprite = headSprites[headSprites.Count - 1];
    }
  }
}

using System;
using System.Collections;
using UnityEngine;

public class TutorialDoubleClickAnim : MonoBehaviour
{
  [SerializeField] SpriteRenderer? mouse;
  [SerializeField] Sprite[] sprites;
  Coroutine? currAnimCour;

  void OnEnable()
  {
    StartAnimating();
  }

  public void StopAnimating()
  {
    if (currAnimCour != null)
    {
      StopCoroutine(currAnimCour);
    }
  }

  void StartAnimating()
  {
    StopAnimating();
    BaseTween<int> tweenData = new BaseTween<int>(
      1f,
      2,
      (dist, phase) =>
      {
        int idx = Util.GetArrayIdxByDist(0f, sprites.Length);

        if (idx < 0 || !mouse) return;

        mouse.sprite = sprites[idx];
      },
      (dist, phase) =>
      {
        int idx = Util.GetArrayIdxByDist(dist, sprites.Length);

        if (idx < 0 || !mouse) return;

        mouse.sprite = sprites[idx];
      },
      (dist, phase) =>
      {
        int idx = Util.GetArrayIdxByDist(1f, sprites.Length);

        if (idx < 0 || !mouse) return;

        mouse.sprite = sprites[idx];
        StartCoroutine(delay(StartAnimating));
      }
    );
    IEnumerator tween = Tween.Create(tweenData);
    currAnimCour = StartCoroutine(tween);
  }

  IEnumerator delay(Action func)
  {
    yield return PersistentData.Instance.GetWaitSecond(0.5f);

    func();
  }
}

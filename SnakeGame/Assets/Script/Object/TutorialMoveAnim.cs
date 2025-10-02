#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TutorialMoveAnim : MonoBehaviour
{
  [SerializeField] TextMeshPro? text;
  [SerializeField] SpriteRenderer? mouse;
  [SerializeField] Sprite[] sprites;
  Coroutine? currAnimCour;
  Vector3 startPosStage1 = new Vector3(-200, -200, 0f);
  Vector3 endPosStage2 = new Vector3(100, 50, 0f);
  int animatorHash = 0;

  void Awake()
  {
    animatorHash = Animator.StringToHash("Base Layer.Finish State");
  }

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

  // FadeIn
  public void StartAnimating()
  {
    StopAnimating();
    Color prevCursorColor = mouse?.color ?? Color.white;
    Color prevTextColor = text?.color ?? Color.white;
    BaseTween<int> tweenData = new BaseTween<int>(
      0.5f,
      0,
      (dist, phase) =>
      {
        if (mouse)
        {
          mouse.transform.localPosition = startPosStage1;
          mouse.sprite = sprites[0];
          mouse.color = new Color(prevCursorColor.r, prevCursorColor.g, prevCursorColor.b, 0f);
        }
        if (text) text.color = new Color(prevTextColor.r, prevTextColor.g, prevTextColor.b, 0f);
      },
      (dist, phase) =>
      {
        if (mouse) mouse.color = new Color(prevCursorColor.r, prevCursorColor.g, prevCursorColor.b, dist);
        if (text) text.color = new Color(prevTextColor.r, prevTextColor.g, prevTextColor.b, dist);
      },
      (dist, phase) =>
      {
        if (mouse) mouse.color = new Color(prevCursorColor.r, prevCursorColor.g, prevCursorColor.b, 1f);
        if (text) text.color = new Color(prevTextColor.r, prevTextColor.g, prevTextColor.b, 1f);
        playAnimByPhase(phase);
      }
    );
    IEnumerator<object> tween = Tween.Create(tweenData);
    currAnimCour = StartCoroutine(tween);
  }

  // Start moving to center
  void startPhase1Anim()
  {
    Vector3 startPos = new Vector3(startPosStage1.x, startPosStage1.y);
    Vector3 endPos = Vector3.zero;
    StopAnimating();
    BaseTween<int> tweenData = new BaseTween<int>(
      1f,
      1,
      (dist, phase) =>
      {
        if (!mouse) return;

        mouse.sprite = sprites[0];
        mouse.transform.localPosition = new Vector3(startPos.x, startPos.y);
      },
      (dist, phase) =>
      {
        if (!mouse) return;

        Vector3 delta = new Vector3(endPos.x - startPos.x, endPos.y - startPos.y);
        delta *= Util.EaseOut(dist, 3);

        mouse.transform.localPosition = new Vector3(startPos.x + delta.x, startPos.y + delta.y);
      },
      (dist, phase) =>
      {
        if (!mouse) return;

        mouse.transform.localPosition = endPos;
        playAnimByPhase(phase);
      }
    );
    IEnumerator<object> tween = Tween.Create(tweenData);
    currAnimCour = StartCoroutine(tween);
  }

  // Start Animation Click
  void startPhase2Anim()
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
        playAnimByPhase(phase);

      }
    );
    IEnumerator<object> tween = Tween.Create(tweenData);
    currAnimCour = StartCoroutine(tween);
  }

  // Start Dragging
  void startPhase3Anim()
  {
    Vector3 startPos = Vector3.zero;
    Vector3 endPos = new Vector3(endPosStage2.x, endPosStage2.y);
    StopAnimating();
    BaseTween<int> tweenData = new BaseTween<int>(
      1f,
      3,
      (dist, phase) =>
      {
        if (!mouse) return;

        mouse.transform.position = new Vector3(startPos.x, startPos.y);
      },
      (dist, phase) =>
      {
        if (!mouse) return;

        Vector3 delta = new Vector3(endPos.x - startPos.x, endPos.y - startPos.y);
        delta *= Util.EaseOut(dist, 3);

        mouse.transform.position = new Vector3(startPos.x + delta.x, startPos.y + delta.y);
      },
      (dist, phase) =>
      {
        if (!mouse) return;

        mouse.transform.position = endPos;
        playAnimByPhase(phase);
      }
    );
    IEnumerator<object> tween = Tween.Create(tweenData);
    currAnimCour = StartCoroutine(tween);
  }

  IEnumerator<object> delay(Action func)
  {
    yield return PersistentData.Instance.GetWaitSecond(0.5f);

    func();
  }

  void playAnimByPhase(int phase)
  {
    switch (phase)
    {
      case 1:
        startPhase2Anim();
        break;
      case 2:
        startPhase3Anim();
        break;
      case 3:
        StopAnimating();
        currAnimCour = StartCoroutine(delay(startPhase1Anim));
        break;
      case 0:
      default:
        // Restart the animation on moving to center not fade in
        startPhase1Anim();
        break;
    }
  }

  void OnDisable()
  {
    if (mouse) mouse.color = new Color(mouse.color.r, mouse.color.g, mouse.color.b, 0f);
    if (text) text.color = new Color(text.color.r, text.color.g, text.color.b, 0f);
  }
}

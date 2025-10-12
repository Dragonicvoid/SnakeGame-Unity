using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TutorialLast : BaseTutorial
{
    [SerializeField] GameObject? tutorialLast;
    [SerializeField] TextMeshPro? text;
    Coroutine? currAnimCour;

    public void StopAnimating()
    {
        if (currAnimCour != null)
        {
            StopCoroutine(currAnimCour);
        }
    }

    // FadeIn
    void fadeIn()
    {
        StopAnimating();
        Color prevTextColor = text?.color ?? Color.white;
        BaseTween<int> tweenData = new BaseTween<int>(
          0.5f,
          0,
          (dist, phase) =>
          {
              if (text) text.color = new Color(prevTextColor.r, prevTextColor.g, prevTextColor.b, 0f);
          },
          (dist, phase) =>
          {
              if (text) text.color = new Color(prevTextColor.r, prevTextColor.g, prevTextColor.b, dist);
          },
          (dist, phase) =>
          {
              if (text) text.color = new Color(prevTextColor.r, prevTextColor.g, prevTextColor.b, 1f);
              StartCoroutine(delay(fadeOut));
          }
        );
        IEnumerator tween = Tween.Create(tweenData);
        currAnimCour = StartCoroutine(tween);
    }

    void fadeOut()
    {
        StopAnimating();
        Color prevTextColor = text?.color ?? Color.white;
        BaseTween<int> tweenData = new BaseTween<int>(
          0.5f,
          0,
          (dist, phase) =>
          {
              if (text) text.color = new Color(prevTextColor.r, prevTextColor.g, prevTextColor.b, 1f);
          },
          (dist, phase) =>
          {
              if (text) text.color = new Color(prevTextColor.r, prevTextColor.g, prevTextColor.b, 1.0f - dist);
          },
          (dist, phase) =>
          {
              if (text) text.color = new Color(prevTextColor.r, prevTextColor.g, prevTextColor.b, 0f);
              tutorialLast.SetActive(false);
              StartCoroutine(delay(Next, 1f));
          }
        );
        IEnumerator tween = Tween.Create(tweenData);
        currAnimCour = StartCoroutine(tween);
    }

    IEnumerator delay(Action func, float delay = 2.5f)
    {
        yield return PersistentData.Instance.GetWaitSecond(delay);

        func();
    }

    public override void Init()
    {
        if (!SaveManager.Instance.shouldDoTutorial)
        {
            Next();
            return;
        }
        tutorialLast.SetActive(true);
        if (text) text.color = new Color(text.color.r, text.color.g, text.color.b, 0f);
        StartCoroutine(delay(fadeIn, GENERAL_CONFIG.FIRE_ALIVE_TIME));
    }

    public override void OnChange()
    {
        StopAnimating();
        tutorialLast.SetActive(false);
        if (text) text.color = new Color(text.color.r, text.color.g, text.color.b, 0f);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class Difficulty : MonoBehaviour
{
  [SerializeField] List<RectTransform> diffObj = new List<RectTransform>();
  [SerializeField] RectTransform? content;

  DIFFICULTY _currDiff = DIFFICULTY.MEDIUM;
  DIFFICULTY currDiff
  {
    get
    {
      return _currDiff;
    }
    set
    {
      _currDiff = value;
    }
  }

  Coroutine? animCour;

  void Awake()
  {
    currDiff = PersistentData.Instance.Difficulty;

    RectTransform rect = diffObj[(int)currDiff];
    rect.anchoredPosition = new Vector2(0, 0);
    content.anchoredPosition = new Vector2(0, 0);
    rect.gameObject.SetActive(true);
  }

  public void onNextClick()
  {
    AudioManager.Instance.PlaySFX(ASSET_KEY.SFX_BUTTON_CLICK);
    int prevValue = (int)currDiff;
    int nextValue = (int)currDiff + 1;
    if (nextValue > 2) nextValue = (int)DIFFICULTY.EASY;

    PersistentData.Instance.Difficulty = (DIFFICULTY)nextValue;
    currDiff = (DIFFICULTY)nextValue;

    SaveManager.Instance.SaveData.LastDiffSelect = nextValue;
    SaveManager.Instance.Save();

    playAnimTo(prevValue, nextValue, false);
  }

  public void onPrevClick()
  {
    AudioManager.Instance.PlaySFX(ASSET_KEY.SFX_BUTTON_CLICK);
    int prevValue = (int)currDiff;
    int nextValue = (int)currDiff - 1;
    if (nextValue < 0) nextValue = (int)DIFFICULTY.HARD;

    PersistentData.Instance.Difficulty = (DIFFICULTY)nextValue;
    currDiff = (DIFFICULTY)nextValue;

    SaveManager.Instance.SaveData.LastDiffSelect = nextValue;
    SaveManager.Instance.Save();

    playAnimTo(prevValue, nextValue, true);
  }

  void playAnimTo(int start, int to, bool toLeft)
  {
    RectTransform? startNode = null;
    RectTransform? endNode = null;

    stopAnim();
    for (int i = 0; i < diffObj.Count; i++)
    {
      if (i == start)
      {
        startNode = diffObj[i];
        startNode.gameObject.SetActive(true);
        continue;
      }

      if (i == to)
      {
        endNode = diffObj[i];
        endNode.gameObject.SetActive(true);
        continue;
      }

      diffObj[i].gameObject.SetActive(false);
    }

    float direction = toLeft ? 1 : -1;
    BaseTween<object> tweenData = new BaseTween<object>(
      0.3f,
      null,
      (dist, _) =>
      {
        content.anchoredPosition = new Vector2(0, 0);
        startNode.anchoredPosition = new Vector2(0, 0);
        endNode.anchoredPosition = new Vector2(content.rect.width * -direction, 0);
      },
      (dist, _) =>
      {
        float currXPos = content.rect.width * direction * dist;
        content.anchoredPosition = new Vector2(currXPos, 0);
      },
      (dist, _) =>
      {
        content.anchoredPosition = new Vector2(0, 0);
        endNode.anchoredPosition = new Vector2(0, 0);
        startNode.gameObject.SetActive(false);
      }
    );
    IEnumerator<object> tween = Tween.Create(tweenData);
    animCour = StartCoroutine(tween);
  }

  void stopAnim()
  {
    if (animCour == null) return;

    StopCoroutine(animCour);
  }
}

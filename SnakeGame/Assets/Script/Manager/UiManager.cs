
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
  class UIAnimData
  {
    public Coroutine Cour;
    public Action OnFinish;
  }

  [SerializeField]
  RectTransform? uiCanvas = null;
  [SerializeField]
  RectTransform? startUI = null;
  [SerializeField]
  RectTransform? endUI = null;
  [SerializeField]
  RectTransform? creditUI = null;
  [SerializeField]
  Text? endLabel = null;
  [SerializeField]
  GameObject? movUI = null;
  [SerializeField]
  GameObject? movUIFront = null;
  [SerializeField]
  Background? background = null;

  private float movMaxLength = 50;

  UIAnimData? showCor;

  UIAnimData? hideCor;

  void Awake()
  {
    setListener();
  }

  public void StartGame()
  {
    ShowStartUI(false);
    background?.GoToGameplayPos();
  }

  public void EndGame()
  {
    UiEvent.Instance.onGameEndAnimFinish -= onGameEndAnimFinish;
    UiEvent.Instance.onGameEndAnimFinish += onGameEndAnimFinish;
    background?.GoToMainMenuPos();
  }

  public void ShowStartUI(bool val = true)
  {
    if (startUI == null) return;

    if (val)
    {
      showUIAnim(startUI);
    }
    else
    {
      hideUIAnim(startUI);
    }
  }

  public void ShowEndUI(GameOverData? data, bool val = true)
  {
    if (endLabel != null && data != null)
    {
      endLabel.text = data.IsWon == false ? "You Lose" : "You Won";
    }

    if (endUI == null) return;

    if (val)
    {
      showUIAnim(endUI);
    }
    else
    {
      hideUIAnim(endUI);
    }
  }

  public void ShowCreditUI(bool val = true)
  {
    if (creditUI == null) return;

    if (val)
    {
      showUIAnim(creditUI);
    }
    else
    {
      hideUIAnim(creditUI);
    }
  }

  void showUIAnim(RectTransform ui)
  {
    if (showCor != null)
    {
      StopCoroutine(showCor.Cour);
      showCor.OnFinish();
    }

    float startY = uiCanvas.rect.height;
    float targetY = 0;
    showCor = new UIAnimData
    {
      OnFinish = () => { },
    };
    BaseTween<UIAnimData> tweenData = new BaseTween<UIAnimData>(
      0.5f,
      showCor,
      (dst, _) =>
      {
        ui.gameObject.SetActive(true);
        ui.anchoredPosition = new Vector2(0, startY);
      },
      (dst, _) =>
      {
        float distTarget = Util.EaseOut(dst, 3) * (targetY - startY);
        ui.anchoredPosition = new Vector2(0, startY + distTarget);
      },
      (dst, data) =>
      {
        ui.anchoredPosition = new Vector2(0, targetY);
        data.OnFinish();
        showCor = null;
      }
    );
    IEnumerator tween = Tween.Create(tweenData);
    showCor.Cour = StartCoroutine(tween);
  }

  void hideUIAnim(RectTransform ui)
  {
    if (hideCor != null)
    {
      StopCoroutine(hideCor.Cour);
      hideCor.OnFinish();
    }

    float startY = ui.anchoredPosition.y;
    float targetY = -uiCanvas.rect.height;
    hideCor = new UIAnimData
    {
      OnFinish = () =>
      {
        ui.gameObject.SetActive(false);
      },
    };
    BaseTween<UIAnimData> tweenData = new BaseTween<UIAnimData>(
      0.5f,
      hideCor,
      (dst, _) =>
      {
        ui.anchoredPosition = new Vector2(0, startY);
      },
      (dst, _) =>
      {
        float distTarget = Util.EaseOut(dst, 3) * (targetY - startY);
        ui.anchoredPosition = new Vector2(0, startY + distTarget);
      },
      (dst, data) =>
      {
        ui.anchoredPosition = new Vector2(0, targetY);
        data.OnFinish();
        hideCor = null;
      }
    );
    IEnumerator tween = Tween.Create(tweenData);
    hideCor.Cour = StartCoroutine(tween);
  }

  public void onClickCredit(bool show)
  {
    AudioManager.Instance.PlaySFX(ASSET_KEY.SFX_BUTTON_CLICK);
    ShowStartUI(!show);
    ShowCreditUI(show);
  }

  private void setListener()
  {
    stopListener();
    GameplayMoveEvent.Instance.onGameUiStartTouch += onTouchStart;
    GameplayMoveEvent.Instance.onGameUiMoveTouch += onTouchMove;
    GameplayMoveEvent.Instance.onGameUiEndTouch += onTouchEnd;
  }

  void stopListener()
  {
    GameplayMoveEvent.Instance.onGameUiStartTouch -= onTouchStart;
    GameplayMoveEvent.Instance.onGameUiMoveTouch -= onTouchMove;
    GameplayMoveEvent.Instance.onGameUiEndTouch -= onTouchEnd;
  }

  private void onTouchStart(Vector2 pos)
  {
    showMovUI(true, pos);
  }

  private void onTouchMove(Vector2 pos)
  {
    setMovUIFrontDelta(pos);
  }

  private void onTouchEnd()
  {
    showMovUI(false, null);
  }

  private void onGameEndAnimFinish()
  {
    UiEvent.Instance.onGameEndAnimFinish -= onGameEndAnimFinish;
    ShowStartUI(true);
  }

  private void showMovUI(bool show, Vector2? pos)
  {
    if (movUI == null) return;

    movUI.SetActive(show);

    if (!show && movUIFront)
    {
      movUIFront.transform.localPosition = new Vector3(0, 0);
    }

    if (pos == null || !show) return;

    movUI.transform.position = new Vector2(pos.Value.x, pos.Value.y);
  }

  private void setMovUIFrontDelta(Vector2 pos)
  {
    if (movUIFront == null || movUI == null) return;

    Vector2 currPos = new Vector2(
      movUI.transform.position.x,
      movUI.transform.position.y
    );
    float dist = Vector2.Distance(currPos, pos);
    Vector2 dir = new Vector2(pos.x - currPos.x, pos.y - currPos.y);
    if (dist > movMaxLength)
    {
      Vector2 normVec = new Vector2(dir.x, dir.y);
      normVec.Normalize();
      normVec *= movMaxLength;
      movUIFront.transform.localPosition = new Vector3(normVec.x, normVec.y);
      movUI.transform.position = new Vector3(
        pos.x - normVec.x,
        pos.y - normVec.y,
        movUI.transform.position.z
      );
    }
    else
    {
      movUIFront.transform.localPosition = new Vector3(dir.x, dir.y);
    }

    GameplayMoveEvent.Instance.SnakeMoveCalculated(dir);
  }

  void OnDestroy()
  {
    stopListener();
  }
}

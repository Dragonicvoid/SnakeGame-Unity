#nullable enable
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UiManager : MonoBehaviour
{
  [SerializeField]
  GameObject? startUI = null;
  [SerializeField]
  GameObject? endUI = null;
  [SerializeField]
  Text? endLabel = null;
  [SerializeField]
  GameObject? movUI = null;
  [SerializeField]
  GameObject? movUIFront = null;
  [SerializeField]
  Background? background = null;

  private float movMaxLength = 50;

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
    UiEvent.Instance.onGameEndAnimFinish += onGameEndAnimFinish;
    background?.GoToMainMenuPos();
  }

  public void ShowStartUI(bool val = true)
  {
    if (startUI == null) return;

    startUI.SetActive(val);
  }

  public void ShowEndUI(GameOverData? data, bool val = true)
  {
    if (endUI == null) return;

    endUI.SetActive(val);

    if (endLabel == null || !val || data == null) return;

    endLabel.text = data.IsWon == false ? "You Lose" : "You Won";
  }

  private void setListener()
  {
    GameplayMoveEvent.Instance.onGameUiStartTouch += onTouchStart;
    GameplayMoveEvent.Instance.onGameUiMoveTouch += onTouchMove;
    GameplayMoveEvent.Instance.onGameUiEndTouch += onTouchEnd;
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
    GameplayMoveEvent.Instance.onGameUiStartTouch -= onTouchStart;
    GameplayMoveEvent.Instance.onGameUiMoveTouch -= onTouchMove;
    GameplayMoveEvent.Instance.onGameUiEndTouch -= onTouchEnd;
  }
}

#nullable enable
using System.Collections.Generic;
using UnityEngine;

public class TutorialMove : BaseTutorial
{
  [SerializeField]
  TutorialMoveAnim? moveTutorial;
  Coroutine showTutorial;
  public override void Init()
  {
    GameplayMoveEvent.Instance.onGameUiMoveTouch -= onTouchMove;
    GameplayMoveEvent.Instance.onGameUiMoveTouch += onTouchMove;
    showTutorial = StartCoroutine(showMoveTutorial());
  }

  IEnumerator<object> showMoveTutorial()
  {
    yield return PersistentData.Instance.GetWaitSecond(3f);

    moveTutorial?.gameObject.SetActive(true);
  }

  void onTouchMove(Vector2 _)
  {
    Next();
  }

  public override void OnChange()
  {
    StopCoroutine(showTutorial);
    moveTutorial?.gameObject.SetActive(false);
    GameplayMoveEvent.Instance.onGameUiMoveTouch -= onTouchMove;
  }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
  List<BaseTutorial> tutorials = new List<BaseTutorial>();

  int idx = 0;

  void Awake()
  {
    tutorials = new List<BaseTutorial>(gameObject.GetComponents<BaseTutorial>());
    setListener();
  }

  public void StartTutorial()
  {
    idx = 0;
    tutorials[idx].Init();
  }

  void OnEnable()
  {
    setListener();
  }

  void setListener()
  {
    TutorialEvent.Instance.onNextTutorial -= onNextTutorial;
    TutorialEvent.Instance.onNextTutorial += onNextTutorial;
  }

  void onNextTutorial()
  {
    if (idx >= tutorials.Count || idx < 0) return;

    tutorials[idx].OnChange();
    idx += 1;
    if (idx < tutorials.Count)
    {
      tutorials[idx].Init();
    }
    else
    {
      SaveManager.Instance.SaveData.TimeLastTutorial = Util.GetCurrWorldTime();
      SaveManager.Instance.Save();
      GameEvent.Instance.FinishTutorial();
    }
  }

  public void StopTutorial()
  {
    if (idx >= tutorials.Count) return;

    tutorials[idx].OnChange();
    idx = -1;
  }

  void OnDisable()
  {
    TutorialEvent.Instance.onNextTutorial -= onNextTutorial;
  }
}

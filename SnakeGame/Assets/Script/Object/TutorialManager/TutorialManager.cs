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
    TutorialEvent.Instance.onNextTutorial += onNextTutorial;
  }

  void onNextTutorial()
  {
    if (idx >= tutorials.Count) return;

    tutorials[idx].OnChange();
    idx += 1;
    if (idx < tutorials.Count)
    {
      tutorials[idx].Init();
    }
  }

  void OnDisable()
  {
    TutorialEvent.Instance.onNextTutorial -= onNextTutorial;
  }
}

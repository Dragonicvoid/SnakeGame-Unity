using System;
using UnityEngine;

public class TutorialEvent : MonoBehaviour
{
  public static TutorialEvent _instance;

  public static TutorialEvent Instance
  {
    get
    {
      if (_instance == null)
      {
        _instance = new TutorialEvent();
      }
      return _instance;
    }
  }

  void Awake()
  {
    _instance = this;
  }

  public event Action? onNextTutorial;
  public void NextTutorial()
  {
    if (onNextTutorial != null)
      onNextTutorial();
  }
}

using System;
using UnityEngine;

public class TutorialEvent : MonoBehaviour
{
  public static TutorialEvent Instance;

  void Awake()
  {
    Instance = this;
  }

  public event Action? onNextTutorial;
  public void NextTutorial()
  {
    if (onNextTutorial != null)
      onNextTutorial();
  }
}

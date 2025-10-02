using UnityEngine;

public class BaseTutorial : MonoBehaviour
{
  public virtual void Init()
  {

  }

  public virtual void Next()
  {
    TutorialEvent.Instance.NextTutorial();
  }

  public virtual void OnChange()
  {

  }
}

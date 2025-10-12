using UnityEngine;

public class TutorialFiring : BaseTutorial
{
  [SerializeField] GameObject? tutorialEatToFire;
  [SerializeField] GameObject? tutorialFiring;
  [SerializeField] FoodManager? foodManager;
  float remainingFood = 0;
  float currFoodIdx = 0;
  Vector2[] foodPos = new Vector2[4]
  {
    new Vector2(-200, 200),
    new Vector2(200, 200),
    new Vector2(200, -200),
    new Vector2(-200, -200)
  };

  public override void Init()
  {
    if (!SaveManager.Instance.shouldDoTutorial)
    {
      Next();
      return;
    }
    PersistentData.Instance.isPaused = true;
    showEatToFireTutorial();
  }

  void showEatToFireTutorial()
  {
    tutorialEatToFire.SetActive(true);
  }

  public void onEatForFireTutorialConfirm()
  {
    GameEvent.Instance.onMainPlayerEat += onMainPlayerEat;
    currFoodIdx = 0;
    remainingFood = GENERAL_CONFIG.FOOD_TO_FIRE - 1;
    tutorialEatToFire.SetActive(false);

    spawnFood();

    PersistentData.Instance.isPaused = false;
  }

  void spawnFood()
  {
    remainingFood--;

    if (remainingFood <= 0 || foodPos.Length == 0)
    {
      foodManager.SpawnFood(new Vector2(0, 0));
    }
    else
    {
      int currIdx = Mathf.FloorToInt(currFoodIdx % foodPos.Length);
      Vector2 pos = foodPos[currIdx];
      foodManager.SpawnFood(pos);
    }

    currFoodIdx++;
  }

  void onMainPlayerEat(float dist)
  {
    if (dist >= 1)
    {
      GameEvent.Instance.onMainPlayerEat -= onMainPlayerEat;
      showFireTutorial();
    }
    else
    {
      spawnFood();
    }
  }

  void showFireTutorial()
  {
    GameEvent.Instance.onMainPlayerFire += onMainPlayerFire;
    PersistentData.Instance.isPaused = true;
    tutorialFiring?.SetActive(true);
  }

  void onMainPlayerFire(float _)
  {
    GameEvent.Instance.onMainPlayerFire -= onMainPlayerFire;
    PersistentData.Instance.isPaused = false;
    tutorialFiring?.SetActive(false);
    Next();
  }

  public override void OnChange()
  {
    GameEvent.Instance.onMainPlayerEat -= onMainPlayerEat;
    GameEvent.Instance.onMainPlayerFire -= onMainPlayerFire;
  }
}


using System.Collections.Generic;
using UnityEngine;

public class TutorialEatFood : BaseTutorial
{
    [SerializeField]
    TutorialEatAnim? eatAnim;

    [SerializeField]
    FoodManager? foodManager;

    Coroutine showTutorial;
    public override void Init()
    {
        if (!SaveManager.Instance.shouldDoTutorial)
        {
            Next();
            return;
        }
        GameEvent.Instance.onPlayerSizeIncrease -= onPlayerSizeIncrease;
        GameEvent.Instance.onPlayerSizeIncrease += onPlayerSizeIncrease;
        showTutorial = StartCoroutine(showEatTutorial());
    }

    IEnumerator<object> showEatTutorial()
    {
        yield return null;
        yield return PersistentData.Instance.GetWaitSecond(1f);

        FoodConfig? food = foodManager?.SpawnFood(new Vector2(0, 0), false);

        if (food == null)
        {
            foodManager?.StartSpawningFood();
            Next();
            yield break;
        }

        eatAnim?.gameObject.SetActive(true);
        eatAnim?.SetFoodToEat(food);
    }

    void onPlayerSizeIncrease(SnakeConfig snake)
    {
        Next();
    }

    public override void OnChange()
    {
        GameEvent.Instance.onPlayerSizeIncrease -= onPlayerSizeIncrease;
        if (showTutorial != null)
        {
            StopCoroutine(showTutorial);
        }
        eatAnim?.gameObject.SetActive(false);
    }
}

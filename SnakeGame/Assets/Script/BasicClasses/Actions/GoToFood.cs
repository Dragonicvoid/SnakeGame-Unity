#nullable enable
using UnityEngine;

public class GoToFood : BaseAction
{
    int minBodySize = 9;

    public override void Run(SnakeConfig player, SnakeActionData data)
    {
        this.Player = player;
        CurrData = data;

        FoodConfig? foodTarget = data.DetectedFood;

        if (foodTarget == null) return;

        Vector2 foodAngle = ProcessBotMovementByFood(player, foodTarget);
        this.UpdateDirection(foodAngle);
    }

    public override float UpdateScore(PlannerFactor factor)
    {
        FoodConfig? food = factor.DetectedFood;
        float foodScore = food != null ? ACTION_SCORE.FOUND_FOOD_NEARBY : 0;

        float bodyFactor = Mathf.Max(
          factor.Player.State.Body.Count - minBodySize,
          0
        );
        float bodyFactScore = bodyFactor * ACTION_SCORE.SMALL_BODY;

        Score = bodyFactScore + foodScore;
        return Score;
    }
}

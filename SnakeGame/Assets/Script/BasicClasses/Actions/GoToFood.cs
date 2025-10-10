using UnityEngine;

public class GoToFood : BaseAction
{
    int minBodySize = 20;

    public override void Run(SnakeConfig player, SnakeActionData data)
    {
        Player = player;
        CurrData = data;

        FoodConfig? foodTarget = data.DetectedFood;

        if (foodTarget == null) return;

        Vector2 foodAngle = ProcessBotMovementByFood(player, foodTarget);

        IPlayerManager? playerManager = CurrData.Manager.PlayerManager;

        if (playerManager == null) return;

        playerManager.UpdateDirection(Player, foodAngle);
    }

    public override float UpdateScore(PlannerFactor factor)
    {
        FoodConfig? food = factor.DetectedFood;
        float foodScore = food != null ? ACTION_SCORE.FOUND_FOOD_NEARBY : 0;

        float bodyFactor = factor.Player.State.Body.Count - minBodySize;
        float bodyFactScore = bodyFactor * -1 * ACTION_SCORE.SMALL_BODY;

        Score = bodyFactScore + foodScore;
        return Score;
    }
}

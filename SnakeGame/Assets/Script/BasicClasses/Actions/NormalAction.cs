using System.Collections.Generic;
using UnityEngine;

public class NormalAction : BaseAction
{
    public override void Run(SnakeConfig player, SnakeActionData data)
    {
        this.Player = player;
        CurrData = data;

        List<float> detectedObs = new List<float>(data.DetectedWall);
        detectedObs.AddRange(data.DetectedPlayer);
        Vector2? dodgeAngle = ProcessBotMovementByFatalObs(player, detectedObs);

        if (dodgeAngle != null)
        {
            UpdateDirection(dodgeAngle.Value);
        }
    }

    public override float UpdateScore(PlannerFactor factor)
    {
        float score = ACTION_SCORE.NORMAL_ACTION;
        if (factor.DetectedWall.Count > 0)
        {
            score += ACTION_SCORE.OBSTACLE_DETECTED;
        }

        if (factor.DetectedPlayer.Count > 0)
        {
            score += ACTION_SCORE.OBSTACLE_DETECTED;
        }

        return score;
    }
}

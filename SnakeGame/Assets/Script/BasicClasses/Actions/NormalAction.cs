using System.Collections.Generic;
using UnityEngine;

public class NormalAction : BaseAction
{
    public override void Run(SnakeConfig player, SnakeActionData data)
    {
        Player = player;
        CurrData = data;

        List<float> detectedObs = new List<float>(data.DetectedWall);
        detectedObs.AddRange(data.DetectedPlayer);
        detectedObs.AddRange(data.DetectedFire);
        Vector2? dodgeAngle = ProcessBotMovementByFatalObs(player, detectedObs);

        IPlayerManager playerManager = CurrData.Manager.PlayerManager;

        if (dodgeAngle != null && playerManager != null)
        {
            playerManager.UpdateDirection(Player, dodgeAngle.Value);
        }
    }

    public override float UpdateScore(PlannerFactor factor)
    {
        Score = ACTION_SCORE.NORMAL_ACTION;
        if (factor.DetectedWall.Count > 0)
        {
            Score += ACTION_SCORE.OBSTACLE_DETECTED;
        }

        if (factor.DetectedPlayer.Count > 0)
        {
            Score += ACTION_SCORE.OBSTACLE_DETECTED;
        }

        if (factor.DetectedFire.Count > 0)
        {
            Score += ACTION_SCORE.OBSTACLE_DETECTED;
        }

        return Score;
    }
}

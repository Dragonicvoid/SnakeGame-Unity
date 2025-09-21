using System.Collections.Generic;
using UnityEngine;

public class BotPlanner : MonoBehaviour
{
    public IBaseAction Plan(List<IBaseAction> actions, PlannerFactor factor)
    {
        CustomPrioQ<IBaseAction> queue = new CustomPrioQ<IBaseAction>((a, b) =>
        {
            return a.Score > b.Score;
        }, new List<IBaseAction>());

        foreach (IBaseAction act in actions)
        {
            act.UpdateScore(factor);
            queue.Enqueue(new List<IBaseAction> { act });
        }

        IBaseAction result = queue.Dequeue();

        if (result == null) return null;

        return result;
    }
}

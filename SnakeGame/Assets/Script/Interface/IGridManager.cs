using System.Collections.Generic;
using UnityEngine;

public interface IGridManager
{
    public List<GridConfig> GridList { get; set; }
    public void Setup();
    public void AddSpike(SpikeConfig spike);
    public void AddFood(FoodConfig foodInstance);
    public void UpdateFood(FoodConfig food);
    public void RemoveFood(FoodConfig food);
    public void RemoveBodyOnGrid(Vector2 pos, string playerID);

    public void AddBodyOnGrid(Vector2 pos, string playerID);
}

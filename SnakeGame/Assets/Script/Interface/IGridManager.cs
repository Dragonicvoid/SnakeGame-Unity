using System.Collections.Generic;

public interface IGridManager
{
    public List<GridConfig> GridList { get; set; }
    public void Setup();
    public void AddSpike(SpikeConfig spike);
    public void AddFood(FoodConfig foodInstance);
    public void UpdateFood(FoodConfig food);
    public void RemoveFood(FoodConfig food);
    public void RemoveBodyOnGrid(Coordinate coord, string playerID);

    public void AddBodyOnGrid(Coordinate coord, string playerID);
}

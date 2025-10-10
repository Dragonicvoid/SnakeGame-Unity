using System.Collections.Generic;
using UnityEngine;

public interface IFoodManager
{
    public List<FoodConfig> FoodList { get; set; }
    public FoodConfig? SpawnFood(Vector2 pos, bool animated = true);
    public void StartSpawningFood();
    public void StopSpawningFood();
    public void ProcessEatenFood(SnakeConfig player, FoodConfig food);
    public void RemoveAllFood();
    public FoodConfig GetFoodByObj(GameObject obj);
    public FoodConfig GetFoodById(string id);
}

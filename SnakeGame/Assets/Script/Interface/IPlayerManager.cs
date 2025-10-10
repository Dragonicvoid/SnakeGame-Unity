
using System.Collections.Generic;
using UnityEngine;


public interface IPlayerManager
{
  public List<SnakeConfig> PlayerList { get; set; }

  public void CreatePlayer(Vector2 pos, Vector2 moveDir, bool isBot = false);
  public void RemoveAllPlayers();

  public List<float> FindNearestPlayerTowardPoint(
    SnakeConfig currentPlayer,
    float radius
    );

  public Vector2 GetPlayerDirection(string id);

  public void HandleMovement(
    string playerId,
  MovementOpts option
);
  public Vector2? TurnRadiusModification(
      SnakeConfig player,
      Vector2 newMovement,
      float turnRadius,
      float remaining,
      Vector2? coorDir
    );
  public void UpdateDirection(SnakeConfig player, Vector2 botNewDir);
  public void UpdateCoordinate(float delta = 0.016f);
  public void UpdateFire(float delta = 0.016f);
  public SnakeConfig? GetMainPlayer();
  public SnakeConfig? GetEnemy();
  public SnakeConfig? GetPlayerById(string id);
  public SnakeConfig? GetPlayerByBody(GameObject node);
  public SnakeConfig? GetPlayerByFoodGrabber(GameObject node);
}

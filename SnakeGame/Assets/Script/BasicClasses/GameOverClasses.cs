public class GameOverData
{
  public float Time { get; set; }
  public DIFFICULTY Diff { get; set; }
  public bool IsWon { get; set; }
  public SnakeConfig? Player { get; set; }
  public SnakeConfig? Enemy { get; set; }

  public GameOverData(
    float Time,
    DIFFICULTY Diff,
    bool IsWon,
    SnakeConfig? Player,
    SnakeConfig? Enemy
  )
  {
    this.Time = Time;
    this.Diff = Diff;
    this.IsWon = IsWon;

    if (Player != null)
    {
      this.Player = Player;
    }

    if (Enemy != null)
    {
      this.Enemy = Enemy;
    }
  }
}
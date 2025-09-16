#nullable enable
using System.Collections.Generic;
using UnityEngine;

public interface ISnakeRenderable
{
  public Material Mat { get; set; }
  public List<SnakeBody> SnakeBodies { get; set; }
  public SNAKE_TYPE SnakeType { get; set; }
  public SkinDetail? SkinData { get; set; }
  public void SetMatByType();
  public void SetSnakeSkin();
  public void SetSnakeBody(List<SnakeBody> bodies);
}

using System.Collections.Generic;
using UnityEngine;

public interface ISnakeRenderable
{
  // protected assembler: IAssembler | null = null;
  public Camera Cam { get; set; }
  public Sprite RenderSprite { get; set; }
  public uint Pixelated { get; set; }
  public List<SnakeBody> SnakesBody { get; set; }
  public SnakeType SnakeType { get; set; }
  public SkinDetail SkinData { get; set; }
  public void SetMatByType();
  public void SetSnakeSkin();
  public void SetSnakeBody(List<SnakeBody> bodies);
}

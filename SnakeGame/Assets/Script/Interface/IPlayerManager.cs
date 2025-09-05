using System.Collections.Generic;
using UnityEngine;


public interface IPlayerManager
{
    public List<SnakeConfig> PlayerList { get; set; }

    public void CreatePlayer(Vector2 pos, Vector2 moveDir, bool? isBot = false);
    public void RemoveAllPlayers();

    public List<float> FindNearestPlayerTowardPoint(
      SnakeConfig currentPlayer,
      float radius
      );

    public Vector2 GetPlayerDirection(string id);

    public void HandleMovement(
      string playerId,
    MovementOpts option
  )

  private onTouchMove(delta: Vec2)
    {
        this.handleMovement(this.PLAYER_ID, {
        direction: delta,
    });
    }

    private onSizeIncrease(snake: SnakeConfig)
    {
        const length = snake.state.body.length;
        const lastBody = snake.state.body[length - 1];
        const pos = lastBody.obj?.position;
        const newBody = this.createBody(
          snake.isBot,
          false,
          new Vec2(pos?.x, pos?.y),



        );

        if (!newBody) return;

        snake.state.body.push(newBody);

        let anim = this.eatAnim.get(snake.id);
        const obj = { val: 0 };
    const tw = tween(obj).to(
      0.2,
          { val: 1 },
      {
onUpdate: () =>
{
    let mat: Material | undefined = undefined;
    if (snake.isBot)
    {
        mat = this.enemyDisplay?.customMaterial ?? undefined;
    }
    else
    {
        mat = this.playerDisplay?.customMaterial ?? undefined;
    }
    const ratio = 1 - Math.abs(2 * obj.val - 1);
    mat?.setProperty("eatRatio", ratio);
},
        onComplete: () =>
        {
            let mat: Material | undefined = undefined;
            if (snake.isBot)
            {
                mat = this.enemyDisplay?.customMaterial ?? undefined;
            }
            else
            {
                mat = this.playerDisplay?.customMaterial ?? undefined;
            }
            mat?.setProperty("eatRatio", 0);
        },
      },
    );

if (anim) anim.stop();

anim = tw;
anim.start();
  }

  private getFoodGrabberPosition(head: SnakeBody) {
    const norm = new Vec2(0, 0);
Vec2.normalize(norm, head.velocity);
const x = norm.x * head.radius + head.position.x;
const y = norm.y * head.radius + head.position.y;

return { x: x, y: y }
;
  }

  public getMainPlayer()
{
    return this.getPlayerById(this.PLAYER_ID);
}

public getEnemy()
{
    return this.getPlayerById(this.ENEMY_ID);
}

public getPlayerById(id: string) {
    return this.playerList.find((item) => item.id === id);
  }

  public getPlayerByBody(node: Node) {
    return this.playerList.find((item) =>
      item.state.body.find((body) => body.obj === node),
    );
  }

  public getPlayerByFoodGrabber(node: Node) {
    return this.playerList.find((item) => item.state.foodGrabber.obj === node);
  }
}

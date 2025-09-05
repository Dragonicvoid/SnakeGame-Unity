using System.Collections.Generic;
using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    public GridManager GridManager = null;

    public ObstacleManager ObsManager = null;

    public AIDebugger AiDebugger = null;

    public List<List<TileMapData>> MapData = new List<List<TileMapData>>();

    public List<Vector2> SpawnPos = new List<Vector2>();

    public Vector2 CenterPos = new Vector2();

    void Awake()
    {
        InitializedMap();
    }

    public InitializedMap()
    {
        int mapIdx = PersistentData.Instance.SelectedMap;
        const map = configMaps[mapIdx];
        SpawnPos.Clear();
        CenterPos = convertCoorToArenaPos(
          Math.floor(map.row / 2),
          Math.floor(map.col / 2),


        );
        this.mapData = [[]];
        this.gridManager?.setup();
        this.obsManager?.clearObstacle();
        this.obsManager?.initializeObstacleMap();

        for (let y = map.col - 1; y >= 0; y--)
        {
            this.mapData[y] = new Array(map.row);
            let str = "";
            for (let x = 0; x < map.row; x++)
            {
                const posX =
                  x * ARENA_DEFAULT_OBJECT_SIZE.TILE - ARENA_DEFAULT_VALUE.WIDTH / 2;
                const posY =
                  y * ARENA_DEFAULT_OBJECT_SIZE.TILE - ARENA_DEFAULT_VALUE.HEIGHT / 2;
                const gridIdx = getGridIdxByArenaPos({
                x: posX,
          y: posY,
        });
        this.mapData[y][x] = {
        x: posX,
          y: posY,
          gridIdx: gridIdx,
          type: ARENA_OBJECT_TYPE.NONE,
          playerIDList: [],
        }
        ;
        const idx = y * map.row + x;
        this.handleTileByType(map.maps[idx], { x: x, y: y });
        str += map.maps[idx] + ",";
    }
}

this.aiDebugger?.setMapToDebug(this.mapData);
  }
}

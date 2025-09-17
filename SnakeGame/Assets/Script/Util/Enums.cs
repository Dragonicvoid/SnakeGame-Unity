public enum DIFFICULTY
{
    EASY,
    MEDIUM,
    HARD,
}

public enum ARENA_OBJECT_TYPE
{
    NONE = 1 << 0,
    FOOD = 1 << 1,
    WALL = 1 << 2,
    SPIKE = 1 << 3,
    SNAKE = 1 << 4,
    SPAWN_POINT = 1 << 5,
}

public enum BOT_ACTION
{
    NORMAL,
    CHASE_PLAYER,
    EAT,
}

public enum SNAKE_TYPE
{
    NORMAL,
    FIRE,
    WATER,
}

public enum LAYER
{
    DEFAULT = 0,
    TRANSPARENT_FX = 1,
    IGNORE_RAYCAST = 2,
    // Skip
    WATER = 4,
    UI = 5,

    PHYSICS_FOOD = 14,
    PHYSICS_PLAYER = 15,
    PHYSICS_FOOD_GRABBER = 16,
    PHYSICS_OBSTACLE = 17,
    PHYSICS_ENEMY = 18,
    PHYSICS_PLAYER_BODIES = 19,
    PHYSICS_ENEMY_BODIES = 20,
}
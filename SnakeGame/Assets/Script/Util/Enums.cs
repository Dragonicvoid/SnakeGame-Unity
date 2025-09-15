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
    TRANSPARENT_FX = 1 << 0,
    IGNORE_RAYCAST = 1 << 1,
    // Skip
    WATER = 1 << 3,
    UI = 1 << 4,

    PHYSICS_FOOD = 1 << 13,
    PHYSICS_PLAYER = 1 << 14,
    PHYSICS_FOOD_GRABBER = 1 << 15,
    PHYSICS_OBSTACLE = 1 << 16,
    PHYSICS_ENEMY = 1 << 17,
    PHYSICS_PLAYER_BODIES = 1 << 18,
    PHYSICS_ENEMY_BODIES = 1 << 19,
}
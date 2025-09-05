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
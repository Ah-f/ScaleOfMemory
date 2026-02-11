using UnityEngine;

public static class BattleConstants
{
    public const int INITIAL_MANA = 3;
    public const int MAX_MANA = 10;
    public const int DRAW_COUNT = 4;
    public const int STARTING_DECK_SIZE = 10;
    public const int MAX_DECK_SIZE = 15;
    public const int MAX_HAND_SIZE = 7;
    public const float TURN_TIMER = 30f;
    public const float QUICK_PLAY_WINDOW = 3f;
    public const float QUICK_PLAY_BONUS = 1.2f;
    public const int PLAYER_MAX_HP = 80;

    // Colors
    public static readonly Color Background = new Color32(15, 15, 26, 255);
    public static readonly Color Panel = new Color32(26, 26, 46, 255);
    public static readonly Color Highlight = new Color32(244, 162, 97, 255);
    public static readonly Color Danger = new Color32(230, 57, 70, 255);
    public static readonly Color Safe = new Color32(45, 106, 79, 255);
    public static readonly Color Magic = new Color32(74, 144, 217, 255);

    public static Color GetElementColor(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire: return new Color32(255, 80, 40, 255);
            case ElementType.Ice: return new Color32(100, 200, 255, 255);
            case ElementType.Lightning: return new Color32(255, 230, 50, 255);
            case ElementType.Water: return new Color32(60, 140, 220, 255);
            case ElementType.Nature: return new Color32(80, 200, 80, 255);
            case ElementType.Light: return new Color32(255, 240, 180, 255);
            case ElementType.Dark: return new Color32(120, 60, 180, 255);
            default: return new Color32(180, 180, 180, 255);
        }
    }
}

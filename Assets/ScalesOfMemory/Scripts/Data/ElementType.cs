public enum ElementType
{
    None,
    Fire,
    Ice,
    Lightning,
    Water,
    Nature,
    Light,
    Dark
}

public enum CardType
{
    Attack,
    Defend,
    Skill
}

public enum EnemyTrait
{
    None,
    Flying,
    Splitting,
    Charging,
    Summoning
}

public enum IntentType
{
    Attack,
    Defend,
    Buff,
    Debuff,
    Summon,
    Special
}

public enum StatusEffectType
{
    Burn,
    Freeze,
    Poison,
    Weakness,
    Strength
}

public enum BattlePhase
{
    PlayerTurn,
    EnemyTurn,
    Transition
}

public enum BattleState
{
    Setup,
    PlayerTurn,
    EnemyTurn,
    Victory,
    Defeat
}

[System.Serializable]
public class EncounterSetup
{
    public EnemyData[] enemies;
}

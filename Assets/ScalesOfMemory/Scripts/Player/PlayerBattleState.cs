using System.Collections.Generic;

public class PlayerBattleState
{
    public int CurrentHP { get; private set; }
    public int MaxHP { get; private set; }
    public int CurrentBlock { get; private set; }
    public List<StatusEffectInstance> StatusEffects { get; private set; }

    public PlayerBattleState(int maxHP)
    {
        MaxHP = maxHP;
        CurrentHP = maxHP;
        CurrentBlock = 0;
        StatusEffects = new List<StatusEffectInstance>();
    }

    public void TakeDamage(int damage)
    {
        int remaining = damage;
        if (CurrentBlock > 0)
        {
            if (CurrentBlock >= remaining)
            {
                CurrentBlock -= remaining;
                remaining = 0;
            }
            else
            {
                remaining -= CurrentBlock;
                CurrentBlock = 0;
            }
            GameEvents.BlockChanged(CurrentBlock);
        }

        if (remaining > 0)
        {
            CurrentHP = System.Math.Max(0, CurrentHP - remaining);
            GameEvents.PlayerDamaged(remaining);
        }
        GameEvents.PlayerHPChanged(CurrentHP, MaxHP);
    }

    public void AddBlock(int amount)
    {
        CurrentBlock += amount;
        GameEvents.BlockChanged(CurrentBlock);
    }

    public void Heal(int amount)
    {
        CurrentHP = System.Math.Min(MaxHP, CurrentHP + amount);
        GameEvents.PlayerHPChanged(CurrentHP, MaxHP);
    }

    public void ResetBlockAtTurnStart()
    {
        CurrentBlock = 0;
        GameEvents.BlockChanged(0);
    }

    public bool IsDead => CurrentHP <= 0;
}

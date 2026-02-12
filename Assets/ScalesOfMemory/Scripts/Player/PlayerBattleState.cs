using System.Collections.Generic;

public class PlayerBattleState
{
    public int CurrentHP { get; private set; }
    public int MaxHP { get; private set; }
    public int CurrentBlock { get; private set; }
    public int CurrentScales { get; private set; }
    public int MaxScales { get; private set; }
    public ElementType ScalesElement { get; private set; }
    public List<StatusEffectInstance> StatusEffects { get; private set; }

    public PlayerBattleState(int maxHP, int maxScales = BattleConstants.PLAYER_BASE_SCALES,
        ElementType scalesElement = ElementType.Fire)
    {
        MaxHP = maxHP;
        CurrentHP = maxHP;
        CurrentBlock = 0;
        MaxScales = maxScales;
        CurrentScales = maxScales;
        ScalesElement = scalesElement;
        StatusEffects = new List<StatusEffectInstance>();
    }

    // Backward-compatible: no element (status effects, etc.)
    public void TakeDamage(int damage)
    {
        TakeDamageInternal(damage, ElementType.None);
    }

    // With element type (enemy attacks)
    public void TakeDamage(int damage, ElementType attackElement)
    {
        TakeDamageInternal(damage, attackElement);
    }

    private void TakeDamageInternal(int damage, ElementType attackElement)
    {
        int remaining = damage;

        // Step 1: Block absorbs first
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

        // Step 2: Scales absorb next (with element resistance)
        if (remaining > 0 && CurrentScales > 0)
        {
            bool sameElement = attackElement != ElementType.None && attackElement == ScalesElement;

            // Same element = scales are 2x effective (damage to scales halved)
            int effectiveDmg = sameElement ? System.Math.Max(1, remaining / 2) : remaining;

            if (CurrentScales >= effectiveDmg)
            {
                CurrentScales -= effectiveDmg;
                remaining = 0;
            }
            else
            {
                // Scales depleted — reverse-calculate how much raw damage was absorbed
                int absorbed = sameElement ? CurrentScales * 2 : CurrentScales;
                remaining = System.Math.Max(0, remaining - absorbed);
                CurrentScales = 0;
            }
            GameEvents.ScalesChanged(CurrentScales, MaxScales);
        }

        // Step 3: HP takes remaining
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

    public void RestoreScalesToMax()
    {
        CurrentScales = MaxScales;
        GameEvents.ScalesChanged(CurrentScales, MaxScales);
    }

    public void RecoverScales(int amount)
    {
        CurrentScales = System.Math.Min(MaxScales, CurrentScales + amount);
        GameEvents.ScalesChanged(CurrentScales, MaxScales);
    }

    public bool IsDead => CurrentHP <= 0;
}

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
            int blocked = System.Math.Min(CurrentBlock, remaining);
            CurrentBlock -= blocked;
            remaining -= blocked;
            GameEvents.BlockChanged(CurrentBlock);
            if (blocked > 0)
                GameEvents.PlayerBlockAbsorbed(blocked);
        }

        // Step 2: Scales absorb next (with element resistance)
        if (remaining > 0 && CurrentScales > 0)
        {
            bool sameElement = attackElement != ElementType.None && attackElement == ScalesElement;
            int effectiveDmg = sameElement ? System.Math.Max(1, remaining / 2) : remaining;
            int scalesBefore = CurrentScales;

            if (CurrentScales >= effectiveDmg)
            {
                CurrentScales -= effectiveDmg;
                remaining = 0;
            }
            else
            {
                int absorbed = sameElement ? CurrentScales * 2 : CurrentScales;
                remaining = System.Math.Max(0, remaining - absorbed);
                CurrentScales = 0;
            }

            int scalesLost = scalesBefore - CurrentScales;
            if (scalesLost > 0)
                GameEvents.PlayerScalesAbsorbed(scalesLost);
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

    public void SetHP(int hp)
    {
        CurrentHP = System.Math.Max(0, System.Math.Min(hp, MaxHP));
        GameEvents.PlayerHPChanged(CurrentHP, MaxHP);
    }

    public void SetScales(int current, int max)
    {
        MaxScales = max;
        CurrentScales = System.Math.Max(0, System.Math.Min(current, max));
        GameEvents.ScalesChanged(CurrentScales, MaxScales);
    }

    public bool IsDead => CurrentHP <= 0;
}

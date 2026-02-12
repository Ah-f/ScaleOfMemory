using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInstance
{
    public EnemyData Data { get; private set; }
    public int CurrentHP { get; private set; }
    public int MaxHP { get; private set; }
    public int CurrentBlock { get; set; }
    public List<StatusEffectInstance> StatusEffects { get; private set; }
    public GameObject ViewObject { get; set; }
    public Intent CurrentIntent { get; set; }
    public bool IsDead => CurrentHP <= 0;

    public event Action<int, int> OnHPChanged;
    public event Action OnDefeated;

    public EnemyInstance(EnemyData data)
    {
        Data = data;
        MaxHP = data.maxHP;
        CurrentHP = data.maxHP;
        CurrentBlock = 0;
        StatusEffects = new List<StatusEffectInstance>();
    }

    public void TakeDamage(int damage)
    {
        if (Data.trait == EnemyTrait.Flying && UnityEngine.Random.value < 0.5f)
        {
            GameEvents.EnemyDodged(this);
            return;
        }

        int remaining = damage;
        if (CurrentBlock > 0)
        {
            int blocked = Mathf.Min(CurrentBlock, remaining);
            CurrentBlock -= blocked;
            remaining -= blocked;
            GameEvents.EnemyBlocked(this, blocked);
        }

        CurrentHP = Mathf.Max(0, CurrentHP - remaining);
        OnHPChanged?.Invoke(CurrentHP, MaxHP);

        if (CurrentHP <= 0)
        {
            OnDefeated?.Invoke();
            GameEvents.EnemyDefeated(this);
        }
    }

    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        OnHPChanged?.Invoke(CurrentHP, MaxHP);
    }

    public void AddBlock(int amount)
    {
        CurrentBlock += amount;
    }

    public void AddStatusEffect(StatusEffectInstance effect)
    {
        StatusEffects.Add(effect);
        GameEvents.StatusEffectChanged(effect, true);
    }
}

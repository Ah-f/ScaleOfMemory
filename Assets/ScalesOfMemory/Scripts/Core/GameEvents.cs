using System;
using System.Collections.Generic;

public static class GameEvents
{
    public static event Action<CardInstance> OnCardPlayed;
    public static event Action<EnemyInstance, int> OnEnemyDamaged;
    public static event Action<EnemyInstance> OnEnemyDefeated;
    public static event Action<int> OnPlayerDamaged;
    public static event Action<int, int> OnPlayerHPChanged;
    public static event Action<int> OnManaChanged;
    public static event Action<int> OnBlockChanged;
    public static event Action<int, int> OnScalesChanged; // current, max
    public static event Action<BattleState> OnPhaseChanged;
    public static event Action<bool> OnBattleEnd;
    public static event Action<int> OnTurnChanged;
    public static event Action OnHandChanged;
    public static event Action<StatusEffectInstance, bool> OnStatusEffectChanged; // bool=isEnemy
    public static event Action<int, int> OnEncounterChanged; // current, total
    public static event Action OnRunStateChanged;
    public static event Action<EnemyInstance> OnEnemyDodged;
    public static event Action<EnemyInstance, int> OnEnemyBlocked; // enemy, blocked amount
    public static event Action<int> OnPlayerBlockAbsorbed;   // amount blocked
    public static event Action<int> OnPlayerScalesAbsorbed;  // amount absorbed
    public static event Action<EnemyInstance, int> OnEnemyAttacking; // enemy, damage (before hit)
    public static event Action<CardData> OnCardRewardSelected;

    public static void CardPlayed(CardInstance card) => OnCardPlayed?.Invoke(card);
    public static void EnemyDamaged(EnemyInstance enemy, int dmg) => OnEnemyDamaged?.Invoke(enemy, dmg);
    public static void EnemyDefeated(EnemyInstance enemy) => OnEnemyDefeated?.Invoke(enemy);
    public static void PlayerDamaged(int dmg) => OnPlayerDamaged?.Invoke(dmg);
    public static void PlayerHPChanged(int current, int max) => OnPlayerHPChanged?.Invoke(current, max);
    public static void ManaChanged(int current) => OnManaChanged?.Invoke(current);
    public static void BlockChanged(int current) => OnBlockChanged?.Invoke(current);
    public static void ScalesChanged(int current, int max) => OnScalesChanged?.Invoke(current, max);
    public static void PhaseChanged(BattleState state) => OnPhaseChanged?.Invoke(state);
    public static void BattleEnd(bool playerWon) => OnBattleEnd?.Invoke(playerWon);
    public static void TurnChanged(int turn) => OnTurnChanged?.Invoke(turn);
    public static void HandChanged() => OnHandChanged?.Invoke();
    public static void StatusEffectChanged(StatusEffectInstance effect, bool isEnemy) => OnStatusEffectChanged?.Invoke(effect, isEnemy);
    public static void EncounterChanged(int current, int total) => OnEncounterChanged?.Invoke(current, total);
    public static void RunStateChanged() => OnRunStateChanged?.Invoke();
    public static void EnemyDodged(EnemyInstance enemy) => OnEnemyDodged?.Invoke(enemy);
    public static void EnemyBlocked(EnemyInstance enemy, int amount) => OnEnemyBlocked?.Invoke(enemy, amount);
    public static void PlayerBlockAbsorbed(int amount) => OnPlayerBlockAbsorbed?.Invoke(amount);
    public static void PlayerScalesAbsorbed(int amount) => OnPlayerScalesAbsorbed?.Invoke(amount);
    public static void EnemyAttacking(EnemyInstance enemy, int damage) => OnEnemyAttacking?.Invoke(enemy, damage);
    public static void CardRewardSelected(CardData card) => OnCardRewardSelected?.Invoke(card);

    public static void Clear()
    {
        OnCardPlayed = null;
        OnEnemyDamaged = null;
        OnEnemyDefeated = null;
        OnPlayerDamaged = null;
        OnPlayerHPChanged = null;
        OnManaChanged = null;
        OnBlockChanged = null;
        OnScalesChanged = null;
        OnPhaseChanged = null;
        OnBattleEnd = null;
        OnTurnChanged = null;
        OnHandChanged = null;
        OnStatusEffectChanged = null;
        OnEncounterChanged = null;
        OnRunStateChanged = null;
        OnEnemyDodged = null;
        OnEnemyBlocked = null;
        OnPlayerBlockAbsorbed = null;
        OnPlayerScalesAbsorbed = null;
        OnEnemyAttacking = null;
        OnCardRewardSelected = null;
    }
}

using System;
using UnityEngine;

public class TurnManager
{
    public BattleState CurrentState { get; private set; }
    public int TurnNumber { get; private set; }
    public float TurnTimer { get; private set; }
    public float TimeSinceLastPlay { get; private set; }
    public bool IsQuickPlay => TimeSinceLastPlay <= BattleConstants.QUICK_PLAY_WINDOW;

    public event Action<BattleState> OnStateChanged;
    public event Action OnTurnTimerExpired;

    bool _timerActive;

    public TurnManager()
    {
        CurrentState = BattleState.Setup;
        TurnNumber = 0;
    }

    public void StartBattle()
    {
        TurnNumber = 0;
        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        TurnNumber++;
        TurnTimer = BattleConstants.TURN_TIMER;
        TimeSinceLastPlay = BattleConstants.QUICK_PLAY_WINDOW + 1f;
        _timerActive = true;
        ChangeState(BattleState.PlayerTurn);
        GameEvents.TurnChanged(TurnNumber);
    }

    public void EndPlayerTurn()
    {
        _timerActive = false;
        ChangeState(BattleState.EnemyTurn);
    }

    public void EndEnemyTurn()
    {
        StartPlayerTurn();
    }

    public void OnCardPlayed()
    {
        TimeSinceLastPlay = 0f;
    }

    public void Update(float deltaTime)
    {
        if (!_timerActive) return;

        TimeSinceLastPlay += deltaTime;

        TurnTimer -= deltaTime;
        if (TurnTimer <= 0f)
        {
            TurnTimer = 0f;
            _timerActive = false;
            OnTurnTimerExpired?.Invoke();
        }
    }

    public void ChangeState(BattleState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Encounters")]
    [SerializeField] EncounterSetup[] encounters;
    [SerializeField] Transform[] enemySlots;

    [Header("Starter Deck")]
    [SerializeField] CardData[] starterDeck;

    [Header("Status Effects")]
    [SerializeField] StatusEffectData burnData;
    [SerializeField] StatusEffectData freezeData;
    [SerializeField] StatusEffectData poisonData;

    public PlayerBattleState Player { get; private set; }
    public ManaSystem Mana { get; private set; }
    public DeckManager Deck { get; private set; }
    public TurnManager Turns { get; private set; }
    public List<EnemyInstance> Enemies { get; private set; }

    public static BattleManager Instance { get; private set; }

    int _currentEncounter = 0;
    public int CurrentEncounter => _currentEncounter;
    public int TotalEncounters => encounters != null ? encounters.Length : 0;
    public bool IsLastEncounter => _currentEncounter >= TotalEncounters - 1;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitFirstBattle();
    }

    void Update()
    {
        if (Turns != null)
            Turns.Update(Time.deltaTime);
    }

    // Called once at start — initializes player, deck, turns
    void InitFirstBattle()
    {
        _currentEncounter = 0;

        // Player
        Player = new PlayerBattleState(BattleConstants.PLAYER_MAX_HP);
        Mana = new ManaSystem(BattleConstants.INITIAL_MANA);

        // Deck
        Deck = new DeckManager();
        if (starterDeck != null)
        {
            foreach (var card in starterDeck)
                Deck.AddCardToDeck(new CardInstance(card));
        }
        Deck.ShuffleDraw();

        // Turn Manager
        Turns = new TurnManager();
        Turns.OnTurnTimerExpired += OnTurnTimerExpired;
        Turns.OnStateChanged += OnBattleStateChanged;

        // Player sprite
        SpawnPlayerSprite();

        // Start first encounter
        StartEncounter();
    }

    // Called each encounter — spawns enemies, creates UI, starts turn
    void StartEncounter()
    {
        _battleEnded = false;
        Enemies = new List<EnemyInstance>();
        SpawnEnemies();

        // UI
        if (BattleUIManager.Instance != null)
            BattleUIManager.Instance.CreateEnemyViews(Enemies);

        GameEvents.EncounterChanged(_currentEncounter + 1, TotalEncounters);

        // Start
        Turns.StartBattle();
        BeginPlayerTurn();
    }

    // Called by UI when "Next Battle" button is clicked
    public void NextEncounter()
    {
        _currentEncounter++;
        if (_currentEncounter >= TotalEncounters)
            return;

        // Stop any running coroutines from previous battle
        StopAllCoroutines();

        // Clean up old enemies
        CleanupCurrentBattle();

        // Reset deck: gather all cards back and reshuffle
        Deck.DiscardHand();
        Deck.ShuffleDiscardIntoDraw();

        // Refill mana
        Mana.RefillMana();

        // Reset block
        Player.ResetBlockAtTurnStart();

        // Restore scales to max
        Player.RestoreScalesToMax();

        // Clear player status effects
        Player.StatusEffects.Clear();

        // Start next encounter
        StartEncounter();
    }

    void CleanupCurrentBattle()
    {
        if (Enemies != null)
        {
            foreach (var enemy in Enemies)
            {
                if (enemy.ViewObject != null)
                    Destroy(enemy.ViewObject);
            }
            Enemies.Clear();
        }
    }

    GameObject _playerSpriteObj;

    void SpawnPlayerSprite()
    {
        _playerSpriteObj = new GameObject("PlayerSprite");
        _playerSpriteObj.transform.position = new Vector3(-0.3f, 1.2f, 0f);

        var spriteObj = new GameObject("Sprite");
        spriteObj.transform.SetParent(_playerSpriteObj.transform, false);
        var sr = spriteObj.AddComponent<SpriteRenderer>();
        sr.sprite = PixelArtGenerator.GeneratePlayerSprite();
        sr.sortingOrder = 3;
        spriteObj.transform.localScale = Vector3.one * 0.5f;
        spriteObj.AddComponent<Billboard>();

        // Idle bob
        var anim = _playerSpriteObj.AddComponent<EnemyIdleAnimation>();
        anim.basePosition = _playerSpriteObj.transform.position;
    }

    void SpawnEnemies()
    {
        if (encounters == null || _currentEncounter >= encounters.Length) return;
        var encounterData = encounters[_currentEncounter].enemies;
        if (encounterData == null) return;

        for (int i = 0; i < encounterData.Length; i++)
        {
            var data = encounterData[i];
            var enemy = new EnemyInstance(data);

            // Pokemon style: enemies top-right, adjust spacing by count
            int count = encounterData.Length;
            float spacing = count <= 2 ? 1f : 0.7f;
            float startX = count <= 2 ? 0.8f : 0.6f;
            Vector3 pos = new Vector3(startX + i * spacing, 2.8f - i * 0.15f, 0f);

            enemy.ViewObject = EnemySpawner.SpawnEnemy(data, pos);
            enemy.OnDefeated += () => OnEnemyDefeated(enemy);

            Enemies.Add(enemy);
        }

        // Decide initial intents
        foreach (var enemy in Enemies)
        {
            enemy.CurrentIntent = EnemyAI.DecideNextIntent(enemy, Turns.TurnNumber);
        }
    }

    void BeginPlayerTurn()
    {
        // Reset block
        Player.ResetBlockAtTurnStart();

        // Process status effects
        int statusDmg = StatusEffectProcessor.ProcessTurnStart(Player.StatusEffects);
        if (statusDmg > 0)
        {
            Player.TakeDamage(statusDmg);
            if (Player.IsDead) { EndBattle(false); return; }
        }

        // Refill mana
        Mana.RefillMana();

        // Draw cards
        Deck.DrawCards(BattleConstants.DRAW_COUNT);
    }

    public bool TryPlayCard(CardInstance card, EnemyInstance target)
    {
        if (Turns.CurrentState != BattleState.PlayerTurn) return false;
        if (!Mana.CanSpend(card.CurrentManaCost)) return false;

        // Spend mana
        Mana.SpendMana(card.CurrentManaCost);

        // Quick play bonus
        bool quickPlay = Turns.IsQuickPlay;

        // Execute effect
        CardEffect.Execute(card, Player, Mana, target, Enemies, quickPlay);

        // Handle draw cards
        if (card.Data.drawCount > 0)
            Deck.DrawCards(card.Data.drawCount);

        // Track timing
        Turns.OnCardPlayed();

        // Remove from hand
        Deck.PlayCard(card);
        GameEvents.CardPlayed(card);

        // Check battle end
        CheckBattleEnd();

        return true;
    }

    public void EndPlayerTurn()
    {
        if (Turns.CurrentState != BattleState.PlayerTurn) return;

        // Discard hand
        Deck.DiscardHand();

        Turns.EndPlayerTurn();
        StartCoroutine(ExecuteEnemyTurn());
    }

    IEnumerator ExecuteEnemyTurn()
    {
        yield return new WaitForSeconds(0.5f);

        for (int i = Enemies.Count - 1; i >= 0; i--)
        {
            var enemy = Enemies[i];
            if (enemy.IsDead) continue;

            // Reset block
            enemy.CurrentBlock = 0;

            // Process enemy status effects
            int statusDmg = StatusEffectProcessor.ProcessTurnStart(enemy.StatusEffects);
            if (statusDmg > 0)
            {
                enemy.TakeDamage(statusDmg);
                if (enemy.IsDead)
                {
                    // Check if battle ended (e.g. last enemy died from status)
                    if (Turns.CurrentState == BattleState.Victory)
                        yield break;
                    continue;
                }
            }

            // Check freeze
            if (StatusEffectProcessor.HasActionPreventing(enemy.StatusEffects))
            {
                yield return new WaitForSeconds(0.3f);
                continue;
            }

            // Execute intent
            EnemyAI.ExecuteIntent(enemy, Player);
            yield return new WaitForSeconds(0.5f);

            if (Player.IsDead)
            {
                EndBattle(false);
                yield break;
            }

            // Decide next intent
            enemy.CurrentIntent = EnemyAI.DecideNextIntent(enemy, Turns.TurnNumber + 1);
        }

        // Check if battle already ended before starting next turn
        if (Turns.CurrentState == BattleState.Victory || Turns.CurrentState == BattleState.Defeat)
            yield break;

        // Next player turn
        Turns.EndEnemyTurn();
        BeginPlayerTurn();
    }

    void OnTurnTimerExpired()
    {
        EndPlayerTurn();
    }

    void OnEnemyDefeated(EnemyInstance enemy)
    {
        // Handle split (slime)
        if (enemy.Data.trait == EnemyTrait.Splitting && enemy.Data.splitInto != null)
        {
            for (int i = 0; i < enemy.Data.splitCount; i++)
            {
                var splitEnemy = new EnemyInstance(enemy.Data.splitInto);
                Vector3 basePos = enemy.ViewObject != null
                    ? enemy.ViewObject.transform.position
                    : Vector3.zero;
                Vector3 offset = new Vector3((i - 0.5f) * 1.5f, 0f, 0f);
                splitEnemy.ViewObject = EnemySpawner.SpawnEnemy(enemy.Data.splitInto, basePos + offset);
                splitEnemy.OnDefeated += () => OnEnemyDefeated(splitEnemy);
                splitEnemy.CurrentIntent = EnemyAI.DecideNextIntent(splitEnemy, Turns.TurnNumber);
                Enemies.Add(splitEnemy);

                // Add HP bar UI for split enemy
                if (BattleUIManager.Instance != null)
                    BattleUIManager.Instance.AddEnemyView(splitEnemy);
            }
        }

        // Destroy view
        if (enemy.ViewObject != null)
            Destroy(enemy.ViewObject);

        CheckBattleEnd();
    }

    void CheckBattleEnd()
    {
        // Check all enemies dead
        bool allDead = true;
        foreach (var e in Enemies)
        {
            if (!e.IsDead) { allDead = false; break; }
        }

        if (allDead)
            EndBattle(true);
    }

    bool _battleEnded;

    void EndBattle(bool victory)
    {
        if (_battleEnded) return; // prevent double-call
        _battleEnded = true;
        StopAllCoroutines();
        Turns.ChangeState(victory ? BattleState.Victory : BattleState.Defeat);
        GameEvents.BattleEnd(victory);
    }

    void OnBattleStateChanged(BattleState state)
    {
        GameEvents.PhaseChanged(state);
    }

    void OnDestroy()
    {
        if (Turns != null)
        {
            Turns.OnTurnTimerExpired -= OnTurnTimerExpired;
            Turns.OnStateChanged -= OnBattleStateChanged;
        }
        GameEvents.Clear();
        Instance = null;
    }
}

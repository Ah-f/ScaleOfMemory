using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] RunConfig runConfig;

    public RunState CurrentRun { get; private set; }
    public RunConfig Config => runConfig;

    Dictionary<string, CardData> _cardLookup;
    Dictionary<string, EnemyData> _enemyLookup;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildLookups();
    }

    void BuildLookups()
    {
        _cardLookup = new Dictionary<string, CardData>();
        if (runConfig.allCards != null)
        {
            foreach (var card in runConfig.allCards)
                if (card != null) _cardLookup[card.name] = card;
        }
        if (runConfig.starterDeck != null)
        {
            foreach (var card in runConfig.starterDeck)
                if (card != null && !_cardLookup.ContainsKey(card.name))
                    _cardLookup[card.name] = card;
        }

        _enemyLookup = new Dictionary<string, EnemyData>();
        AddEnemyPool(runConfig.normalEnemies);
        AddEnemyPool(runConfig.eliteEnemies);
        AddEnemyPool(runConfig.bossEnemies);
    }

    void AddEnemyPool(EnemyData[] pool)
    {
        if (pool == null) return;
        foreach (var e in pool)
            if (e != null && !_enemyLookup.ContainsKey(e.name))
                _enemyLookup[e.name] = e;
    }

    public void StartNewRun()
    {
        int seed = Random.Range(0, int.MaxValue);
        CurrentRun = new RunState();
        CurrentRun.seed = seed;
        CurrentRun.maxHP = runConfig.startHP;
        CurrentRun.currentHP = runConfig.startHP;
        CurrentRun.maxScales = runConfig.startScales;
        CurrentRun.currentScales = runConfig.startScales;
        CurrentRun.gold = runConfig.startGold;
        CurrentRun.currentPhase = RunPhase.Phase1;
        CurrentRun.currentNodeId = -1;

        CurrentRun.deckCardNames = new List<string>();
        if (runConfig.starterDeck != null)
        {
            foreach (var card in runConfig.starterDeck)
                if (card != null) CurrentRun.deckCardNames.Add(card.name);
        }

        CurrentRun.mapNodes = MapGenerator.Generate(seed, runConfig);
        LoadScene("MapScene");
    }

    public void EnterNode(MapNode node)
    {
        CurrentRun.currentNodeId = node.id;
        node.visited = true;
        CurrentRun.pendingBattleNode = node;

        switch (node.roomType)
        {
            case RoomType.Battle:
            case RoomType.Elite:
            case RoomType.Boss:
                LoadScene("BattleScene");
                break;
            case RoomType.Rest:
                int healAmount = Mathf.Max(1, CurrentRun.maxHP / 4);
                CurrentRun.currentHP = Mathf.Min(CurrentRun.maxHP, CurrentRun.currentHP + healAmount);
                node.cleared = true;
                GameEvents.RunStateChanged();
                break;
            case RoomType.Shop:
                node.cleared = true;
                GameEvents.RunStateChanged();
                break;
            case RoomType.Event:
                ShowRandomEvent(node);
                break;
        }
    }

    public void OnBattleVictory()
    {
        var node = CurrentRun.mapNodes.Find(n => n.id == CurrentRun.currentNodeId);
        if (node != null) node.cleared = true;

        // Gold reward is now handled by BattleUIManager before the card reward screen

        // Restore scales for next encounter
        CurrentRun.currentScales = CurrentRun.maxScales;

        if (node != null && node.roomType == RoomType.Boss)
        {
            int phaseIndex = (int)CurrentRun.currentPhase;
            if (phaseIndex < 2)
            {
                CurrentRun.currentPhase = (RunPhase)(phaseIndex + 1);
                CurrentRun.mapNodes = MapGenerator.Generate(
                    CurrentRun.seed + phaseIndex + 1, runConfig);
                CurrentRun.currentNodeId = -1;
            }
            else
            {
                // All phases complete - victory!
                LoadScene("MainMenuScene");
                return;
            }
        }

        LoadScene("MapScene");
    }

    public void OnBattleDefeat()
    {
        CurrentRun = null;
        LoadScene("MainMenuScene");
    }

    public bool IsNodeAccessible(MapNode node)
    {
        if (CurrentRun.currentNodeId == -1)
            return node.row == 0;

        var current = CurrentRun.mapNodes.Find(n => n.id == CurrentRun.currentNodeId);
        return current != null && current.connections.Contains(node.id);
    }

    public CardData ResolveCard(string cardName)
    {
        if (_cardLookup != null && _cardLookup.TryGetValue(cardName, out var data))
            return data;
        return null;
    }

    public EnemyData ResolveEnemy(string enemyName)
    {
        if (_enemyLookup != null && _enemyLookup.TryGetValue(enemyName, out var data))
            return data;
        return null;
    }

    public void AddCardToDeck(string cardName)
    {
        if (CurrentRun != null)
            CurrentRun.deckCardNames.Add(cardName);
    }

    public int GetGoldReward(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.Battle: return Random.Range(10, 21);
            case RoomType.Elite: return Random.Range(25, 41);
            case RoomType.Boss: return Random.Range(50, 81);
            default: return 0;
        }
    }

    void ShowRandomEvent(MapNode node)
    {
        var eventData = EventData.GetRandomEvent();
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            // Fallback: just clear the node
            node.cleared = true;
            GameEvents.RunStateChanged();
            return;
        }

        var eventUI = canvas.gameObject.AddComponent<EventUI>();
        eventUI.Show(canvas.transform, eventData, () =>
        {
            node.cleared = true;
            GameEvents.RunStateChanged();
        });
    }

    void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}

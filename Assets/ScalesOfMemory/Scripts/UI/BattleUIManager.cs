using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance { get; private set; }

    Canvas _canvas;
    CanvasScaler _scaler;

    // Top Info
    TextMeshProUGUI _turnText;
    TextMeshProUGUI _timerText;
    TextMeshProUGUI _encounterText;

    // Enemy HP (top-left, Pokemon style)
    List<EnemyView> _enemyViews = new List<EnemyView>();

    // Player Info (bottom-right, Pokemon style)
    Image _playerHPFill;
    TextMeshProUGUI _playerHPText;
    TextMeshProUGUI _manaText;
    TextMeshProUGUI _blockText;
    RectTransform _playerHPBar;

    // Card Hand
    HandView _handView;

    // Action Buttons
    Button _endTurnBtn;

    // Battle Result
    GameObject _resultPanel;
    TextMeshProUGUI _resultText;
    TextMeshProUGUI _resultSubText;
    Button _resultBtn;
    TextMeshProUGUI _resultBtnLabel;
    bool _hasNextEncounter;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BuildUI();
        SubscribeEvents();
    }

    void Update()
    {
        if (BattleManager.Instance != null && BattleManager.Instance.Turns != null)
        {
            float t = BattleManager.Instance.Turns.TurnTimer;
            if (_timerText != null)
            {
                _timerText.text = Mathf.CeilToInt(t) + "s";
                _timerText.color = t <= 5f ? BattleConstants.Danger : Color.white;
            }
        }
    }

    void BuildUI()
    {
        // Canvas
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 10;

        _scaler = gameObject.AddComponent<CanvasScaler>();
        _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        _scaler.referenceResolution = new Vector2(1080, 1920);
        _scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        // EventSystem
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        BuildTopBar();
        BuildPlayerInfoPanel();
        BuildHandArea();
        BuildActionBar();
        BuildResultPanel();
    }

    void BuildTopBar()
    {
        var panel = CreatePanel("TopBar", transform,
            new Vector2(0f, 0.95f), new Vector2(1f, 1f));
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);

        // Encounter info (left)
        _encounterText = CreateText("Encounter", panel.transform, "Battle 1/3", 22,
            new Vector2(0f, 0f), new Vector2(0.35f, 1f));
        _encounterText.color = BattleConstants.Highlight;

        _turnText = CreateText("Turn", panel.transform, "Turn 1", 26,
            new Vector2(0.35f, 0f), new Vector2(0.65f, 1f));
        _turnText.alignment = TextAlignmentOptions.Center;

        _timerText = CreateText("Timer", panel.transform, "30s", 30,
            new Vector2(0.65f, 0f), new Vector2(1f, 1f));
        _timerText.alignment = TextAlignmentOptions.Right;
    }

    // Enemy HP bars - top LEFT (Pokemon style)
    public void CreateEnemyViews(List<EnemyInstance> enemies)
    {
        foreach (var view in _enemyViews)
        {
            if (view != null && view.gameObject != null)
                Destroy(view.gameObject);
        }
        _enemyViews.Clear();

        for (int i = 0; i < enemies.Count; i++)
        {
            AddEnemyView(enemies[i]);
        }
    }

    public void AddEnemyView(EnemyInstance enemy)
    {
        int i = _enemyViews.Count;
        float yBase = 0.88f - i * 0.08f;

        var panel = CreatePanel($"Enemy_{i}", transform,
            new Vector2(0.02f, yBase), new Vector2(0.52f, yBase + 0.07f));
        panel.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.15f, 0.85f);

        var view = panel.AddComponent<EnemyView>();
        view.Init(enemy, panel.GetComponent<RectTransform>());
        _enemyViews.Add(view);
    }

    // Player info - bottom RIGHT (Pokemon style)
    void BuildPlayerInfoPanel()
    {
        var panel = CreatePanel("PlayerPanel", transform,
            new Vector2(0.45f, 0.28f), new Vector2(0.98f, 0.40f));
        panel.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.15f, 0.85f);
        var rt = panel.GetComponent<RectTransform>();

        // Player name
        CreateText("PlayerName", rt, "Player", 22,
            new Vector2(0.05f, 0.65f), new Vector2(0.5f, 1f));

        // HP Bar background
        var hpBg = CreatePanel("HPBarBg", rt,
            new Vector2(0.05f, 0.35f), new Vector2(0.95f, 0.6f));
        hpBg.GetComponent<Image>().color = new Color32(40, 40, 40, 255);

        // HP label
        var hpLabel = CreateText("HPLabel", hpBg.GetComponent<RectTransform>(), "HP", 14,
            new Vector2(-0.12f, 0f), new Vector2(0.02f, 1f));
        hpLabel.color = BattleConstants.Highlight;
        hpLabel.alignment = TextAlignmentOptions.Right;

        // HP Bar fill
        var hpFill = CreatePanel("HPBarFill", hpBg.GetComponent<RectTransform>(),
            new Vector2(0f, 0f), new Vector2(1f, 1f));
        _playerHPFill = hpFill.GetComponent<Image>();
        _playerHPFill.color = BattleConstants.Safe;
        _playerHPBar = hpFill.GetComponent<RectTransform>();

        // HP text
        _playerHPText = CreateText("HPText", rt, "80/80", 20,
            new Vector2(0.5f, 0.65f), new Vector2(0.95f, 1f));
        _playerHPText.alignment = TextAlignmentOptions.Right;

        // Mana + Block row
        _manaText = CreateText("ManaText", rt, "Mana: 3", 22,
            new Vector2(0.05f, 0f), new Vector2(0.5f, 0.35f));
        _manaText.color = BattleConstants.Magic;

        _blockText = CreateText("BlockText", rt, "", 22,
            new Vector2(0.5f, 0f), new Vector2(0.95f, 0.35f));
        _blockText.alignment = TextAlignmentOptions.Right;
        _blockText.color = BattleConstants.Highlight;
    }

    void BuildHandArea()
    {
        var handPanel = CreatePanel("HandPanel", transform,
            new Vector2(0f, 0.05f), new Vector2(1f, 0.28f));
        Destroy(handPanel.GetComponent<Image>());

        _handView = handPanel.AddComponent<HandView>();
    }

    void BuildActionBar()
    {
        var panel = CreatePanel("ActionBar", transform,
            new Vector2(0f, 0f), new Vector2(1f, 0.05f));
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f);
        var rt = panel.GetComponent<RectTransform>();

        // End Turn button
        _endTurnBtn = CreateButton("EndTurnBtn", rt, "END TURN",
            new Vector2(0.6f, 0.1f), new Vector2(0.98f, 0.9f),
            BattleConstants.Highlight);
        _endTurnBtn.onClick.AddListener(() =>
        {
            if (BattleManager.Instance != null)
                BattleManager.Instance.EndPlayerTurn();
        });

        // Deck info
        CreateText("DeckInfo", rt, "Deck", 18,
            new Vector2(0.02f, 0.1f), new Vector2(0.2f, 0.9f));

        // Timer text in action bar
        _timerText = CreateText("TimerBar", rt, "", 18,
            new Vector2(0.2f, 0.1f), new Vector2(0.55f, 0.9f));
        _timerText.alignment = TextAlignmentOptions.Center;
    }

    void BuildResultPanel()
    {
        _resultPanel = CreatePanel("ResultPanel", transform,
            new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.7f));
        _resultPanel.GetComponent<Image>().color = new Color32(20, 20, 40, 240);

        var rt = _resultPanel.GetComponent<RectTransform>();

        _resultText = CreateText("ResultText", rt, "", 48,
            new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.9f));
        _resultText.alignment = TextAlignmentOptions.Center;

        _resultSubText = CreateText("ResultSubText", rt, "", 28,
            new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.55f));
        _resultSubText.alignment = TextAlignmentOptions.Center;
        _resultSubText.color = new Color(0.7f, 0.7f, 0.8f);

        _resultBtn = CreateButton("ResultBtn", rt, "NEXT",
            new Vector2(0.25f, 0.08f), new Vector2(0.75f, 0.3f),
            BattleConstants.Highlight);

        // Get the label for dynamic text updates
        _resultBtnLabel = _resultBtn.GetComponentInChildren<TextMeshProUGUI>();

        _resultBtn.onClick.AddListener(OnResultButtonClicked);

        _resultPanel.SetActive(false);
    }

    void OnResultButtonClicked()
    {
        if (BattleManager.Instance == null) return;

        _resultPanel.SetActive(false);

        // If there's a next encounter, go to it
        if (_hasNextEncounter)
        {
            _hasNextEncounter = false;
            BattleManager.Instance.NextEncounter();
        }
    }

    void SubscribeEvents()
    {
        GameEvents.OnPlayerHPChanged += OnPlayerHPChanged;
        GameEvents.OnManaChanged += OnManaChanged;
        GameEvents.OnBlockChanged += OnBlockChanged;
        GameEvents.OnTurnChanged += OnTurnChanged;
        GameEvents.OnBattleEnd += OnBattleEnd;
        GameEvents.OnPhaseChanged += OnPhaseChanged;
        GameEvents.OnHandChanged += OnHandChanged;
        GameEvents.OnEncounterChanged += OnEncounterChanged;
    }

    void OnPlayerHPChanged(int current, int max)
    {
        float ratio = (float)current / max;
        if (_playerHPBar != null)
            _playerHPBar.anchorMax = new Vector2(ratio, 1f);
        if (_playerHPFill != null)
            _playerHPFill.color = Color.Lerp(BattleConstants.Danger, BattleConstants.Safe, ratio);
        if (_playerHPText != null)
            _playerHPText.text = $"{current}/{max}";
    }

    void OnManaChanged(int current)
    {
        if (_manaText != null)
            _manaText.text = $"Mana: {current}";
    }

    void OnBlockChanged(int current)
    {
        if (_blockText != null)
            _blockText.text = current > 0 ? $"Block: {current}" : "";
    }

    void OnTurnChanged(int turn)
    {
        if (_turnText != null)
            _turnText.text = $"Turn {turn}";
    }

    void OnEncounterChanged(int current, int total)
    {
        if (_encounterText != null)
            _encounterText.text = $"Battle {current}/{total}";
    }

    void OnPhaseChanged(BattleState state)
    {
        if (_endTurnBtn != null)
            _endTurnBtn.interactable = (state == BattleState.PlayerTurn);
    }

    void OnBattleEnd(bool victory)
    {
        if (_resultPanel == null) return;
        _resultPanel.SetActive(true);

        _hasNextEncounter = false;

        if (victory)
        {
            var bm = BattleManager.Instance;
            if (bm != null && !bm.IsLastEncounter)
            {
                // Mid-run victory
                _hasNextEncounter = true;
                int current = bm.CurrentEncounter + 1;
                int total = bm.TotalEncounters;
                _resultText.text = $"Battle {current}/{total} Clear!";
                _resultText.color = BattleConstants.Highlight;
                _resultSubText.text = $"Next: Battle {current + 1}";
                if (_resultBtnLabel != null)
                    _resultBtnLabel.text = "NEXT";
            }
            else
            {
                // Final victory
                _resultText.text = "ALL CLEAR!";
                _resultText.color = BattleConstants.Highlight;
                _resultSubText.text = "All encounters defeated!";
                if (_resultBtnLabel != null)
                    _resultBtnLabel.text = "OK";
            }
        }
        else
        {
            _resultText.text = "DEFEAT";
            _resultText.color = BattleConstants.Danger;
            _resultSubText.text = "";
            if (_resultBtnLabel != null)
                _resultBtnLabel.text = "OK";
        }
    }

    void OnHandChanged()
    {
        if (_handView != null && BattleManager.Instance != null)
            _handView.UpdateHand(BattleManager.Instance.Deck.Hand);
    }

    // === UI Helpers ===
    GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }

    TextMeshProUGUI CreateText(string name, Transform parent, string text, int fontSize,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(8, 0);
        rt.offsetMax = new Vector2(-8, 0);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        return tmp;
    }

    Button CreateButton(string name, Transform parent, string label,
        Vector2 anchorMin, Vector2 anchorMax, Color bgColor)
    {
        var go = CreatePanel(name, parent, anchorMin, anchorMax);
        go.GetComponent<Image>().color = bgColor;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = go.GetComponent<Image>();

        var textGo = new GameObject("Label", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.Center;

        return btn;
    }

    void OnDestroy()
    {
        GameEvents.OnPlayerHPChanged -= OnPlayerHPChanged;
        GameEvents.OnManaChanged -= OnManaChanged;
        GameEvents.OnBlockChanged -= OnBlockChanged;
        GameEvents.OnTurnChanged -= OnTurnChanged;
        GameEvents.OnBattleEnd -= OnBattleEnd;
        GameEvents.OnPhaseChanged -= OnPhaseChanged;
        GameEvents.OnHandChanged -= OnHandChanged;
        GameEvents.OnEncounterChanged -= OnEncounterChanged;
        Instance = null;
    }
}

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

    // Player Info — Combined Defense Bar
    RectTransform _hpFillRT;
    RectTransform _scalesFillRT;
    RectTransform _blockFillRT;
    Image _hpFillImg;
    Image _scalesFillImg;
    Image _blockFillImg;
    TextMeshProUGUI _defenseText; // "HP:60 SC:15 BK:5"
    TextMeshProUGUI _manaText;

    // Player status effect icons
    RectTransform _playerStatusRow;
    List<GameObject> _playerStatusIcons = new List<GameObject>();
    int _lastPlayerStatusHash;

    // Cached values for combined bar
    int _cachedHP, _cachedMaxHP;
    int _cachedScales, _cachedMaxScales;
    int _cachedBlock;

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

        // Initialize cached values
        _cachedMaxHP = BattleConstants.PLAYER_MAX_HP;
        _cachedHP = _cachedMaxHP;
        _cachedMaxScales = BattleConstants.PLAYER_BASE_SCALES;
        _cachedScales = _cachedMaxScales;
        _cachedBlock = 0;
        UpdateCombinedBar();
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

        UpdatePlayerStatusIcons();
    }

    void UpdatePlayerStatusIcons()
    {
        if (BattleManager.Instance == null || BattleManager.Instance.Player == null) return;
        if (_playerStatusRow == null) return;

        var effects = BattleManager.Instance.Player.StatusEffects;
        int hash = ComputeStatusHash(effects);
        if (hash == _lastPlayerStatusHash) return;
        _lastPlayerStatusHash = hash;

        // Clear old
        foreach (var icon in _playerStatusIcons)
        {
            if (icon != null) Destroy(icon);
        }
        _playerStatusIcons.Clear();

        // Build new
        int count = 0;
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i].IsExpired) continue;
            count++;
        }

        int idx = 0;
        for (int i = 0; i < effects.Count; i++)
        {
            var effect = effects[i];
            if (effect.IsExpired) continue;

            float iconWidth = 1f / Mathf.Max(count, 5);
            float x = idx * iconWidth;

            var iconGo = CreateStatusBadge(_playerStatusRow, effect, x, x + iconWidth * 0.9f);
            _playerStatusIcons.Add(iconGo);
            idx++;
        }
    }

    GameObject CreateStatusBadge(RectTransform parent, StatusEffectInstance effect, float xMin, float xMax)
    {
        var badge = new GameObject("StatusBadge", typeof(RectTransform), typeof(Image));
        badge.transform.SetParent(parent, false);
        var brt = badge.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(xMin, 0f);
        brt.anchorMax = new Vector2(xMax, 1f);
        brt.offsetMin = new Vector2(1, 0);
        brt.offsetMax = new Vector2(-1, 0);
        badge.GetComponent<Image>().color = effect.Data.iconColor;

        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(badge.transform, false);
        var trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        string initial = GetEffectInitial(effect.Data.type);
        string stackText = effect.Potency > 1 ? $"{initial}{effect.Potency}" : initial;
        tmp.text = $"{stackText} <size=70%>{effect.RemainingDuration}t</size>";
        tmp.fontSize = 14;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableWordWrapping = false;

        return badge;
    }

    static string GetEffectInitial(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.Burn: return "B";
            case StatusEffectType.Freeze: return "F";
            case StatusEffectType.Poison: return "P";
            case StatusEffectType.Weakness: return "W";
            case StatusEffectType.Strength: return "S";
            default: return "?";
        }
    }

    static int ComputeStatusHash(List<StatusEffectInstance> effects)
    {
        int hash = effects.Count;
        for (int i = 0; i < effects.Count; i++)
        {
            hash = hash * 31 + (int)effects[i].Data.type;
            hash = hash * 31 + effects[i].RemainingDuration;
            hash = hash * 31 + effects[i].Potency;
        }
        return hash;
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
    // Combined defense bar: [HP green | Scales gold | Block orange | empty]
    // + status icons row + mana
    void BuildPlayerInfoPanel()
    {
        var panel = CreatePanel("PlayerPanel", transform,
            new Vector2(0.45f, 0.24f), new Vector2(0.98f, 0.42f));
        panel.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.15f, 0.85f);
        var rt = panel.GetComponent<RectTransform>();

        // Top row: Player name + defense stats text
        CreateText("PlayerName", rt, "Player", 20,
            new Vector2(0.05f, 0.78f), new Vector2(0.35f, 1f));

        _defenseText = CreateText("DefenseText", rt, "", 16,
            new Vector2(0.35f, 0.78f), new Vector2(0.95f, 1f));
        _defenseText.alignment = TextAlignmentOptions.Right;

        // Combined bar background
        var barBg = CreatePanel("DefenseBarBg", rt,
            new Vector2(0.05f, 0.52f), new Vector2(0.95f, 0.76f));
        barBg.GetComponent<Image>().color = new Color32(30, 30, 30, 255);
        var barBgRT = barBg.GetComponent<RectTransform>();

        // HP fill (leftmost, green)
        var hpFill = CreatePanel("HPFill", barBgRT,
            new Vector2(0f, 0f), new Vector2(0.8f, 1f));
        _hpFillImg = hpFill.GetComponent<Image>();
        _hpFillImg.color = BattleConstants.Safe;
        _hpFillRT = hpFill.GetComponent<RectTransform>();

        // Scales fill (after HP, gold)
        var scalesFill = CreatePanel("ScalesFill", barBgRT,
            new Vector2(0.8f, 0f), new Vector2(1f, 1f));
        _scalesFillImg = scalesFill.GetComponent<Image>();
        _scalesFillImg.color = BattleConstants.Scales;
        _scalesFillRT = scalesFill.GetComponent<RectTransform>();

        // Block fill (after Scales, orange)
        var blockFill = CreatePanel("BlockFill", barBgRT,
            new Vector2(0f, 0f), new Vector2(0f, 1f));
        _blockFillImg = blockFill.GetComponent<Image>();
        _blockFillImg.color = BattleConstants.Highlight;
        _blockFillRT = blockFill.GetComponent<RectTransform>();

        // Status icons row (between bar and mana)
        var statusRowGo = new GameObject("PlayerStatusRow", typeof(RectTransform));
        statusRowGo.transform.SetParent(rt, false);
        _playerStatusRow = statusRowGo.GetComponent<RectTransform>();
        _playerStatusRow.anchorMin = new Vector2(0.05f, 0.30f);
        _playerStatusRow.anchorMax = new Vector2(0.95f, 0.50f);
        _playerStatusRow.offsetMin = Vector2.zero;
        _playerStatusRow.offsetMax = Vector2.zero;

        // Bottom row: Mana
        _manaText = CreateText("ManaText", rt, "Mana: 3", 20,
            new Vector2(0.05f, 0f), new Vector2(0.95f, 0.28f));
        _manaText.color = BattleConstants.Magic;
    }

    // Update the combined HP/Scales/Block bar
    void UpdateCombinedBar()
    {
        // Total max = MaxHP + MaxScales (Block has no fixed max, so it extends the bar)
        float totalMax = _cachedMaxHP + _cachedMaxScales;
        if (totalMax <= 0) return;

        float hpRatio = _cachedHP / totalMax;
        float scalesRatio = _cachedScales / totalMax;
        float blockRatio = _cachedBlock / totalMax;

        // HP: 0 to hpRatio
        if (_hpFillRT != null)
        {
            _hpFillRT.anchorMin = new Vector2(0f, 0f);
            _hpFillRT.anchorMax = new Vector2(hpRatio, 1f);
        }
        if (_hpFillImg != null)
        {
            float hpPct = _cachedMaxHP > 0 ? (float)_cachedHP / _cachedMaxHP : 0f;
            _hpFillImg.color = Color.Lerp(BattleConstants.Danger, BattleConstants.Safe, hpPct);
        }

        // Scales: hpRatio to hpRatio+scalesRatio
        if (_scalesFillRT != null)
        {
            _scalesFillRT.anchorMin = new Vector2(hpRatio, 0f);
            _scalesFillRT.anchorMax = new Vector2(hpRatio + scalesRatio, 1f);
        }
        if (_scalesFillImg != null)
        {
            float scPct = _cachedMaxScales > 0 ? (float)_cachedScales / _cachedMaxScales : 0f;
            Color c = BattleConstants.Scales;
            if (scPct < 0.3f) c = Color.Lerp(BattleConstants.Danger, BattleConstants.Scales, scPct / 0.3f);
            _scalesFillImg.color = c;
        }

        // Block: hpRatio+scalesRatio to hpRatio+scalesRatio+blockRatio
        if (_blockFillRT != null)
        {
            float blockStart = hpRatio + scalesRatio;
            float blockEnd = Mathf.Min(blockStart + blockRatio, 1f); // clamp to bar width
            _blockFillRT.anchorMin = new Vector2(blockStart, 0f);
            _blockFillRT.anchorMax = new Vector2(blockEnd, 1f);
        }

        // Defense text
        if (_defenseText != null)
        {
            string text = $"<color=#{ColorToHex(BattleConstants.Safe)}>HP:{_cachedHP}</color>";
            text += $" <color=#{ColorToHex(BattleConstants.Scales)}>SC:{_cachedScales}</color>";
            if (_cachedBlock > 0)
                text += $" <color=#{ColorToHex(BattleConstants.Highlight)}>BK:{_cachedBlock}</color>";
            _defenseText.text = text;
            _defenseText.richText = true;
        }
    }

    static string ColorToHex(Color c)
    {
        return ColorUtility.ToHtmlStringRGB(c);
    }

    void BuildHandArea()
    {
        var handPanel = CreatePanel("HandPanel", transform,
            new Vector2(0f, 0.05f), new Vector2(1f, 0.24f));
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
        GameEvents.OnScalesChanged += OnScalesChanged;
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
        _cachedHP = current;
        _cachedMaxHP = max;
        UpdateCombinedBar();
    }

    void OnScalesChanged(int current, int max)
    {
        _cachedScales = current;
        _cachedMaxScales = max;
        UpdateCombinedBar();
    }

    void OnBlockChanged(int current)
    {
        _cachedBlock = current;
        UpdateCombinedBar();
    }

    void OnManaChanged(int current)
    {
        if (_manaText != null)
            _manaText.text = $"Mana: {current}";
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
        GameEvents.OnScalesChanged -= OnScalesChanged;
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

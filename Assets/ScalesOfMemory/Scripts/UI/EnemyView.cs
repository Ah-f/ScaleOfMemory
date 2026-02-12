using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EnemyView : MonoBehaviour
{
    EnemyInstance _enemy;
    RectTransform _root;

    Image _hpFill;
    RectTransform _hpFillRect;
    TextMeshProUGUI _nameHPText;
    TextMeshProUGUI _intentText;
    Button _targetBtn;

    // Status effect icons
    RectTransform _statusRow;
    List<GameObject> _statusIcons = new List<GameObject>();
    int _lastStatusHash;

    public EnemyInstance Enemy => _enemy;

    public void Init(EnemyInstance enemy, RectTransform root)
    {
        _enemy = enemy;
        _root = root;

        // Name + HP (top row, merged)
        _nameHPText = CreateText("NameHP", root, "", 18,
            new Vector2(0.05f, 0.75f), new Vector2(0.95f, 1f));
        _nameHPText.alignment = TextAlignmentOptions.Center;
        UpdateNameHP();

        // HP bar bg
        var hpBg = CreatePanel("HPBg", root,
            new Vector2(0.1f, 0.58f), new Vector2(0.9f, 0.73f));
        hpBg.GetComponent<Image>().color = new Color32(40, 40, 40, 255);

        // HP fill
        var hpFillGo = CreatePanel("HPFill", hpBg.GetComponent<RectTransform>(),
            new Vector2(0f, 0f), new Vector2(1f, 1f));
        _hpFill = hpFillGo.GetComponent<Image>();
        _hpFill.color = BattleConstants.Danger;
        _hpFillRect = hpFillGo.GetComponent<RectTransform>();

        // Status icons row (between HP bar and intent)
        var statusRowGo = new GameObject("StatusRow", typeof(RectTransform));
        statusRowGo.transform.SetParent(root, false);
        _statusRow = statusRowGo.GetComponent<RectTransform>();
        _statusRow.anchorMin = new Vector2(0.05f, 0.38f);
        _statusRow.anchorMax = new Vector2(0.95f, 0.56f);
        _statusRow.offsetMin = Vector2.zero;
        _statusRow.offsetMax = Vector2.zero;

        // Intent
        _intentText = CreateText("Intent", root, "", 18,
            new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.36f));
        _intentText.alignment = TextAlignmentOptions.Center;
        _intentText.color = Color.yellow;

        // Target button (invisible, covers whole area)
        _targetBtn = gameObject.AddComponent<Button>();
        _targetBtn.targetGraphic = root.GetComponent<Image>();
        _targetBtn.onClick.AddListener(OnClicked);

        // Subscribe
        enemy.OnHPChanged += OnHPChanged;
        enemy.OnDefeated += OnDefeated;

        UpdateIntent();
    }

    void Update()
    {
        UpdateIntent();
        UpdateStatusIcons();
    }

    void UpdateNameHP()
    {
        if (_enemy == null || _nameHPText == null) return;
        string nameColor = ColorUtility.ToHtmlStringRGB(BattleConstants.Highlight);
        _nameHPText.richText = true;
        _nameHPText.text = $"<color=#{nameColor}>{_enemy.Data.enemyName}</color>  {_enemy.CurrentHP}/{_enemy.MaxHP}";
    }

    void UpdateIntent()
    {
        if (_enemy == null || _intentText == null) return;
        var intent = _enemy.CurrentIntent;
        string icon = GetIntentIcon(intent.Type);
        _intentText.text = $"{icon} {intent.Description} ({intent.Value})";
    }

    string GetIntentIcon(IntentType type)
    {
        switch (type)
        {
            case IntentType.Attack: return "Atk";
            case IntentType.Defend: return "Def";
            case IntentType.Buff: return "Buf";
            case IntentType.Summon: return "Sum";
            default: return "?";
        }
    }

    void UpdateStatusIcons()
    {
        if (_enemy == null || _statusRow == null) return;

        // Simple hash to avoid rebuilding every frame
        int hash = ComputeStatusHash(_enemy.StatusEffects);
        if (hash == _lastStatusHash) return;
        _lastStatusHash = hash;

        // Clear old icons
        foreach (var icon in _statusIcons)
        {
            if (icon != null) Destroy(icon);
        }
        _statusIcons.Clear();

        // Build new icons
        var effects = _enemy.StatusEffects;
        for (int i = 0; i < effects.Count; i++)
        {
            var effect = effects[i];
            if (effect.IsExpired) continue;

            float iconWidth = 1f / Mathf.Max(effects.Count, 4); // max 4 icons wide
            float x = i * iconWidth;

            var iconGo = CreateStatusIcon(_statusRow, effect, x, x + iconWidth * 0.9f);
            _statusIcons.Add(iconGo);
        }
    }

    GameObject CreateStatusIcon(RectTransform parent, StatusEffectInstance effect, float xMin, float xMax)
    {
        // Badge background
        var badge = new GameObject("StatusBadge", typeof(RectTransform), typeof(Image));
        badge.transform.SetParent(parent, false);
        var brt = badge.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(xMin, 0f);
        brt.anchorMax = new Vector2(xMax, 1f);
        brt.offsetMin = new Vector2(1, 0);
        brt.offsetMax = new Vector2(-1, 0);

        var img = badge.GetComponent<Image>();
        img.color = effect.Data.iconColor;

        // Text: initial + duration
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
        tmp.text = $"{stackText}\n<size=70%>{effect.RemainingDuration}t</size>";
        tmp.fontSize = 12;
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

    void OnHPChanged(int current, int max)
    {
        float ratio = (float)current / max;
        if (_hpFillRect != null)
            _hpFillRect.anchorMax = new Vector2(ratio, 1f);
        UpdateNameHP();
    }

    void OnDefeated()
    {
        gameObject.SetActive(false);
    }

    void OnClicked()
    {
        if (_enemy == null || _enemy.IsDead) return;
        if (HandView.Instance != null)
            HandView.Instance.SetTarget(_enemy);
    }

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
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        return tmp;
    }

    void OnDestroy()
    {
        if (_enemy != null)
        {
            _enemy.OnHPChanged -= OnHPChanged;
            _enemy.OnDefeated -= OnDefeated;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyView : MonoBehaviour
{
    EnemyInstance _enemy;
    RectTransform _root;

    Image _hpFill;
    RectTransform _hpFillRect;
    TextMeshProUGUI _hpText;
    TextMeshProUGUI _nameText;
    TextMeshProUGUI _intentText;
    Button _targetBtn;

    public EnemyInstance Enemy => _enemy;

    public void Init(EnemyInstance enemy, RectTransform root)
    {
        _enemy = enemy;
        _root = root;

        // Enemy name
        _nameText = CreateText("Name", root, enemy.Data.enemyName, 22,
            new Vector2(0.05f, 0.75f), new Vector2(0.95f, 1f));
        _nameText.alignment = TextAlignmentOptions.Center;
        _nameText.color = BattleConstants.Highlight;

        // HP bar bg
        var hpBg = CreatePanel("HPBg", root,
            new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.72f));
        hpBg.GetComponent<Image>().color = new Color32(40, 40, 40, 255);

        // HP fill
        var hpFillGo = CreatePanel("HPFill", hpBg.GetComponent<RectTransform>(),
            new Vector2(0f, 0f), new Vector2(1f, 1f));
        _hpFill = hpFillGo.GetComponent<Image>();
        _hpFill.color = BattleConstants.Danger;
        _hpFillRect = hpFillGo.GetComponent<RectTransform>();

        // HP text
        _hpText = CreateText("HPText", root, $"{enemy.CurrentHP}/{enemy.MaxHP}", 18,
            new Vector2(0.1f, 0.38f), new Vector2(0.9f, 0.55f));
        _hpText.alignment = TextAlignmentOptions.Center;

        // Intent
        _intentText = CreateText("Intent", root, "", 20,
            new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.35f));
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

    void OnHPChanged(int current, int max)
    {
        float ratio = (float)current / max;
        if (_hpFillRect != null)
            _hpFillRect.anchorMax = new Vector2(ratio, 1f);
        if (_hpText != null)
            _hpText.text = $"{current}/{max}";
    }

    void OnDefeated()
    {
        gameObject.SetActive(false);
    }

    void OnClicked()
    {
        if (_enemy == null || _enemy.IsDead) return;
        // Notify hand view to target this enemy
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

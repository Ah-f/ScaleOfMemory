using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardView : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    CardInstance _card;
    RectTransform _rt;
    int _handIndex;

    // Drag state
    Vector2 _restPos;
    bool _isDragging;

    // Hover animation
    bool _isHovered;
    float _hoverT; // 0=normal, 1=fully hovered
    const float HOVER_SPEED = 10f;
    const float HOVER_SCALE = 1.3f;
    const float HOVER_LIFT = 80f;

    // Drag threshold
    const float PLAY_THRESHOLD = 150f;

    // Targeting
    EnemyInstance _currentTarget;
    GameObject _targetIndicator;

    // Visuals
    Image _bgImage;
    Image _elementStrip;
    TextMeshProUGUI _nameText;
    TextMeshProUGUI _costText;
    TextMeshProUGUI _descText;
    TextMeshProUGUI _typeText;

    public CardInstance Card => _card;

    public void Init(CardInstance card, int index)
    {
        _card = card;
        _handIndex = index;
        _rt = GetComponent<RectTransform>();
        _restPos = _rt.anchoredPosition;

        BuildCardVisual();
        UpdateVisual();
    }

    void BuildCardVisual()
    {
        // Card background
        _bgImage = GetComponent<Image>();
        if (_bgImage == null) _bgImage = gameObject.AddComponent<Image>();
        _bgImage.color = BattleConstants.Panel;

        // Element color strip (top)
        var stripGo = new GameObject("ElementStrip", typeof(RectTransform), typeof(Image));
        stripGo.transform.SetParent(transform, false);
        var srt = stripGo.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0, 0.9f);
        srt.anchorMax = Vector2.one;
        srt.offsetMin = Vector2.zero;
        srt.offsetMax = Vector2.zero;
        _elementStrip = stripGo.GetComponent<Image>();

        // Mana cost (top-left)
        var costGo = new GameObject("Cost", typeof(RectTransform));
        costGo.transform.SetParent(transform, false);
        var crt = costGo.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0, 0.82f);
        crt.anchorMax = new Vector2(0.25f, 0.95f);
        crt.offsetMin = Vector2.zero;
        crt.offsetMax = Vector2.zero;
        _costText = costGo.AddComponent<TextMeshProUGUI>();
        _costText.fontSize = 26;
        _costText.alignment = TextAlignmentOptions.Center;
        _costText.color = BattleConstants.Magic;
        _costText.fontStyle = FontStyles.Bold;

        // Card name
        var nameGo = new GameObject("Name", typeof(RectTransform));
        nameGo.transform.SetParent(transform, false);
        var nrt = nameGo.GetComponent<RectTransform>();
        nrt.anchorMin = new Vector2(0.05f, 0.65f);
        nrt.anchorMax = new Vector2(0.95f, 0.82f);
        nrt.offsetMin = Vector2.zero;
        nrt.offsetMax = Vector2.zero;
        _nameText = nameGo.AddComponent<TextMeshProUGUI>();
        _nameText.fontSize = 18;
        _nameText.alignment = TextAlignmentOptions.Center;
        _nameText.fontStyle = FontStyles.Bold;

        // Type
        var typeGo = new GameObject("Type", typeof(RectTransform));
        typeGo.transform.SetParent(transform, false);
        var trt = typeGo.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.05f, 0.52f);
        trt.anchorMax = new Vector2(0.95f, 0.65f);
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        _typeText = typeGo.AddComponent<TextMeshProUGUI>();
        _typeText.fontSize = 14;
        _typeText.alignment = TextAlignmentOptions.Center;
        _typeText.color = new Color(0.6f, 0.6f, 0.7f);

        // Description
        var descGo = new GameObject("Desc", typeof(RectTransform));
        descGo.transform.SetParent(transform, false);
        var drt = descGo.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0.08f, 0.05f);
        drt.anchorMax = new Vector2(0.92f, 0.52f);
        drt.offsetMin = Vector2.zero;
        drt.offsetMax = Vector2.zero;
        _descText = descGo.AddComponent<TextMeshProUGUI>();
        _descText.fontSize = 15;
        _descText.alignment = TextAlignmentOptions.Center;
        _descText.enableWordWrapping = true;
    }

    public void UpdateVisual()
    {
        if (_card == null) return;

        var data = _card.Data;
        _elementStrip.color = data.CardColor;
        _costText.text = _card.CurrentManaCost.ToString();
        _nameText.text = data.cardName;
        _typeText.text = data.cardType.ToString();

        string desc = "";
        if (data.baseDamage > 0) desc += $"DMG {_card.CurrentDamage}\n";
        if (data.baseBlock > 0) desc += $"DEF {_card.CurrentBlock}\n";
        if (data.healAmount > 0) desc += $"Heal {data.healAmount}\n";
        if (data.drawCount > 0) desc += $"Draw {data.drawCount}\n";
        if (data.appliedEffect != null) desc += $"{data.appliedEffect.effectName}({data.effectDuration}t)";
        _descText.text = desc.TrimEnd();

        bool canPlay = BattleManager.Instance != null
            && _card.CanPlay(BattleManager.Instance.Mana.CurrentMana)
            && BattleManager.Instance.Turns.CurrentState == BattleState.PlayerTurn;
        _bgImage.color = canPlay ? BattleConstants.Panel : new Color(0.15f, 0.15f, 0.2f, 0.6f);
    }

    void Update()
    {
        if (_isDragging) return;

        // Smooth hover animation
        float target = _isHovered ? 1f : 0f;
        _hoverT = Mathf.MoveTowards(_hoverT, target, Time.deltaTime * HOVER_SPEED);

        // Scale
        float s = Mathf.Lerp(1f, HOVER_SCALE, _hoverT);
        _rt.localScale = Vector3.one * s;

        // Lift up
        float lift = Mathf.Lerp(0f, HOVER_LIFT, _hoverT);
        _rt.anchoredPosition = _restPos + Vector2.up * lift;
    }

    // === HOVER ===
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isDragging) return;
        _isHovered = true;
        if (HandView.Instance != null)
            HandView.Instance.OnCardHovered(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        if (!_isDragging && HandView.Instance != null)
            HandView.Instance.OnCardUnhovered(this);
    }

    // === DRAG ===
    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        _isHovered = false;
        _rt.localScale = Vector3.one * HOVER_SCALE;
        transform.SetAsLastSibling();
        _currentTarget = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _rt.anchoredPosition += eventData.delta / GetCanvasScale();

        // Update target highlight while dragging attack cards
        if (_card != null && _card.Data.needsTarget)
        {
            UpdateTargetHighlight(eventData.position);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _isDragging = false;

        // Clear highlight
        ClearTargetHighlight();

        float dragDist = _rt.anchoredPosition.y - _restPos.y;

        if (dragDist > PLAY_THRESHOLD)
        {
            TryPlayCard(eventData.position);
        }

        // Return to rest
        _rt.anchoredPosition = _restPos;
        _rt.localScale = Vector3.one;
        _hoverT = 0f;

        if (HandView.Instance != null)
            HandView.Instance.OnCardUnhovered(this);
    }

    // === TARGETING ===
    void UpdateTargetHighlight(Vector2 screenPos)
    {
        var newTarget = FindClosestEnemyToScreenPos(screenPos);
        if (newTarget == _currentTarget) return;

        // Clear old highlight
        ClearTargetHighlight();

        _currentTarget = newTarget;

        // Highlight new target's 3D sprite
        if (_currentTarget != null && _currentTarget.ViewObject != null)
        {
            // Create or reuse indicator ring around enemy
            if (_targetIndicator == null)
            {
                _targetIndicator = new GameObject("TargetIndicator");
                var sr = _targetIndicator.AddComponent<SpriteRenderer>();
                sr.sprite = CreateCircleSprite();
                sr.color = new Color(1f, 0.3f, 0.3f, 0.6f);
                sr.sortingOrder = 10;
            }

            _targetIndicator.SetActive(true);
            _targetIndicator.transform.position =
                _currentTarget.ViewObject.transform.position + Vector3.down * 0.3f;
            float scale = _currentTarget.Data.bodyScale * 0.6f;
            _targetIndicator.transform.localScale = new Vector3(scale, scale * 0.3f, 1f);
        }
    }

    void ClearTargetHighlight()
    {
        if (_targetIndicator != null)
            _targetIndicator.SetActive(false);
        _currentTarget = null;
    }

    static Sprite _circleSprite;
    static Sprite CreateCircleSprite()
    {
        if (_circleSprite != null) return _circleSprite;

        int size = 32;
        var tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;
        float center = size / 2f;
        float radius = size / 2f - 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = dist < radius ? 1f - (dist / radius) * 0.5f : 0f;
                tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        tex.Apply();
        _circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
        return _circleSprite;
    }

    void TryPlayCard(Vector2 screenPos)
    {
        if (BattleManager.Instance == null) return;

        EnemyInstance target = null;
        if (_card.Data.needsTarget)
        {
            target = FindClosestEnemyToScreenPos(screenPos);
        }

        BattleManager.Instance.TryPlayCard(_card, target);
    }

    EnemyInstance FindClosestEnemyToScreenPos(Vector2 screenPos)
    {
        if (BattleManager.Instance == null || BattleManager.Instance.Enemies == null)
            return null;

        EnemyInstance closest = null;
        float closestDist = float.MaxValue;
        Camera cam = Camera.main;
        if (cam == null) return null;

        foreach (var enemy in BattleManager.Instance.Enemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            if (enemy.ViewObject == null) continue;

            Vector3 worldPos = enemy.ViewObject.transform.position;
            Vector2 enemyScreenPos = cam.WorldToScreenPoint(worldPos);
            float dist = Vector2.Distance(screenPos, enemyScreenPos);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = enemy;
            }
        }

        return closest;
    }

    float GetCanvasScale()
    {
        var canvas = GetComponentInParent<Canvas>();
        return canvas != null ? canvas.scaleFactor : 1f;
    }

    void OnDestroy()
    {
        if (_targetIndicator != null)
            Destroy(_targetIndicator);
    }
}

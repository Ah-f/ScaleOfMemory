using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    GameObject _panel;
    Action _onComplete;
    TextMeshProUGUI _goldText;

    // Main shop view
    List<CardData> _shopCards;
    List<GameObject> _cardObjects = new List<GameObject>();
    List<bool> _sold = new List<bool>();
    GameObject _removeBtn;

    // Remove card view
    GameObject _removePanel;

    const float CARD_WIDTH = 220f;
    const float CARD_HEIGHT = 320f;
    const float CARD_SPACING = 15f;
    const int REMOVE_COST = 50;

    public void Show(Transform canvasTransform, Action onComplete)
    {
        _onComplete = onComplete;
        _shopCards = PickShopCards(3);
        _sold.Clear();
        for (int i = 0; i < _shopCards.Count; i++) _sold.Add(false);
        BuildPanel(canvasTransform);
    }

    int GetCardPrice(CardData card)
    {
        switch (card.manaCost)
        {
            case 0: return 40;
            case 1: return 30;
            case 2: return 50;
            case 3: return 75;
            default: return 50;
        }
    }

    List<CardData> PickShopCards(int count)
    {
        var result = new List<CardData>();
        if (GameManager.Instance == null || GameManager.Instance.Config == null) return result;
        var allCards = GameManager.Instance.Config.allCards;
        if (allCards == null || allCards.Length == 0) return result;

        var used = new HashSet<int>();
        int attempts = 0;
        while (result.Count < count && attempts < 50)
        {
            int idx = UnityEngine.Random.Range(0, allCards.Length);
            if (!used.Contains(idx) && allCards[idx] != null)
            {
                used.Add(idx);
                result.Add(allCards[idx]);
            }
            attempts++;
        }
        return result;
    }

    void BuildPanel(Transform parent)
    {
        _panel = new GameObject("ShopPanel", typeof(RectTransform), typeof(Image));
        _panel.transform.SetParent(parent, false);
        var rt = _panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        _panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.92f);

        var panelRT = rt;

        // Title
        var title = CreateText("Title", panelRT, "SHOP", 32,
            new Vector2(0.1f, 0.88f), new Vector2(0.9f, 0.96f));
        title.color = BattleConstants.Highlight;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;

        // Gold display
        int gold = GameManager.Instance != null && GameManager.Instance.CurrentRun != null
            ? GameManager.Instance.CurrentRun.gold : 0;
        _goldText = CreateText("Gold", panelRT, $"Gold: {gold}", 22,
            new Vector2(0.2f, 0.82f), new Vector2(0.8f, 0.89f));
        _goldText.color = BattleConstants.Scales;
        _goldText.alignment = TextAlignmentOptions.Center;

        // Cards for sale
        _cardObjects.Clear();
        for (int i = 0; i < _shopCards.Count; i++)
        {
            var cardGo = CreateShopCard(panelRT, _shopCards[i], i, _shopCards.Count);
            _cardObjects.Add(cardGo);
        }

        // Remove a Card button
        int deckSize = GetDeckSize();
        string removeLabel = $"Remove a Card - {REMOVE_COST}g";
        _removeBtn = CreateServiceButton("RemoveBtn", panelRT, removeLabel,
            new Vector2(0.1f, 0.12f), new Vector2(0.9f, 0.19f),
            new Color(0.25f, 0.08f, 0.08f));
        _removeBtn.GetComponent<Button>().onClick.AddListener(OnRemoveCardClicked);
        UpdateRemoveButton();

        // Leave button
        var leaveBtn = CreateServiceButton("LeaveBtn", panelRT, "LEAVE",
            new Vector2(0.25f, 0.03f), new Vector2(0.75f, 0.10f),
            new Color(0.15f, 0.15f, 0.2f));
        leaveBtn.GetComponent<Button>().onClick.AddListener(OnLeave);
    }

    GameObject CreateShopCard(RectTransform parent, CardData data, int index, int total)
    {
        var go = new GameObject($"ShopCard_{index}", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        float totalWidth = total * CARD_WIDTH + (total - 1) * CARD_SPACING;
        float startX = -totalWidth / 2f + CARD_WIDTH / 2f;
        float xPos = startX + index * (CARD_WIDTH + CARD_SPACING);

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(CARD_WIDTH, CARD_HEIGHT);
        rt.anchoredPosition = new Vector2(xPos, 60f);

        var bgImg = go.GetComponent<Image>();
        bgImg.color = BattleConstants.Panel;

        // Element strip
        var strip = new GameObject("Strip", typeof(RectTransform), typeof(Image));
        strip.transform.SetParent(go.transform, false);
        var srt = strip.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0, 0.9f);
        srt.anchorMax = Vector2.one;
        srt.offsetMin = Vector2.zero;
        srt.offsetMax = Vector2.zero;
        strip.GetComponent<Image>().color = data.CardColor;

        // Mana cost
        var cost = CreateText("Cost", go.GetComponent<RectTransform>(), data.manaCost.ToString(), 24,
            new Vector2(0f, 0.82f), new Vector2(0.25f, 0.95f));
        cost.color = BattleConstants.Magic;
        cost.fontStyle = FontStyles.Bold;
        cost.alignment = TextAlignmentOptions.Center;

        // Card name
        var cardName = CreateText("Name", go.GetComponent<RectTransform>(), data.cardName, 18,
            new Vector2(0.05f, 0.68f), new Vector2(0.95f, 0.82f));
        cardName.fontStyle = FontStyles.Bold;
        cardName.alignment = TextAlignmentOptions.Center;

        // Card type
        var type = CreateText("Type", go.GetComponent<RectTransform>(), data.cardType.ToString(), 14,
            new Vector2(0.05f, 0.60f), new Vector2(0.95f, 0.68f));
        type.color = new Color(0.6f, 0.6f, 0.7f);
        type.alignment = TextAlignmentOptions.Center;

        // Stats
        string stats = "";
        if (data.baseDamage > 0) stats += $"DMG {data.baseDamage}\n";
        if (data.baseBlock > 0) stats += $"DEF {data.baseBlock}\n";
        if (data.healAmount > 0) stats += $"Heal {data.healAmount}\n";
        if (data.drawCount > 0) stats += $"Draw {data.drawCount}\n";
        if (data.appliedEffect != null)
            stats += $"{data.appliedEffect.effectName}({data.effectDuration}t)";

        var desc = CreateText("Desc", go.GetComponent<RectTransform>(), stats.TrimEnd(), 14,
            new Vector2(0.08f, 0.22f), new Vector2(0.92f, 0.60f));
        desc.alignment = TextAlignmentOptions.Center;
        desc.enableWordWrapping = true;

        // Element name
        var elemText = CreateText("Element", go.GetComponent<RectTransform>(),
            data.element.ToString(), 12,
            new Vector2(0.05f, 0.12f), new Vector2(0.95f, 0.22f));
        elemText.color = data.CardColor;
        elemText.alignment = TextAlignmentOptions.Center;

        // Price tag
        int price = GetCardPrice(data);
        var priceText = CreateText("Price", go.GetComponent<RectTransform>(), $"{price}g", 20,
            new Vector2(0.1f, 0.01f), new Vector2(0.9f, 0.12f));
        priceText.color = BattleConstants.Scales;
        priceText.fontStyle = FontStyles.Bold;
        priceText.alignment = TextAlignmentOptions.Center;

        // Buy button
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = bgImg;
        int cardIndex = index;
        CardData cardData = data;
        btn.onClick.AddListener(() => OnBuyCard(cardIndex, cardData));

        UpdateCardBuyable(go, data, false);

        return go;
    }

    void OnBuyCard(int index, CardData data)
    {
        if (_sold[index]) return;
        int price = GetCardPrice(data);
        int gold = GetGold();
        if (gold < price) return;
        if (GetDeckSize() >= BattleConstants.MAX_DECK_SIZE) return;

        // Purchase
        GameManager.Instance.CurrentRun.gold -= price;
        GameManager.Instance.AddCardToDeck(data.name);
        _sold[index] = true;

        // Mark as sold
        var cardGo = _cardObjects[index];
        cardGo.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f);
        cardGo.GetComponent<Button>().interactable = false;

        // Add SOLD overlay
        var soldText = CreateText("Sold", cardGo.GetComponent<RectTransform>(), "SOLD", 28,
            new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.6f));
        soldText.color = new Color(0.5f, 0.5f, 0.5f);
        soldText.fontStyle = FontStyles.Bold;
        soldText.alignment = TextAlignmentOptions.Center;

        RefreshGoldAndButtons();
    }

    void OnRemoveCardClicked()
    {
        int gold = GetGold();
        if (gold < REMOVE_COST) return;
        if (GetDeckSize() <= 1) return;

        ShowRemoveCardView();
    }

    void ShowRemoveCardView()
    {
        _removePanel = new GameObject("RemovePanel", typeof(RectTransform), typeof(Image));
        _removePanel.transform.SetParent(_panel.transform, false);
        var rt = _removePanel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        _removePanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.95f);

        var panelRT = rt;

        // Title
        var title = CreateText("RemoveTitle", panelRT, "REMOVE A CARD", 28,
            new Vector2(0.1f, 0.88f), new Vector2(0.9f, 0.96f));
        title.color = BattleConstants.Danger;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;

        // Subtitle
        var sub = CreateText("RemoveSub", panelRT, $"Cost: {REMOVE_COST}g", 18,
            new Vector2(0.2f, 0.83f), new Vector2(0.8f, 0.89f));
        sub.color = BattleConstants.Scales;
        sub.alignment = TextAlignmentOptions.Center;

        // ScrollRect for deck cards
        var scrollGo = new GameObject("Scroll", typeof(RectTransform));
        scrollGo.transform.SetParent(panelRT, false);
        var scrollRT = scrollGo.GetComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0.03f, 0.12f);
        scrollRT.anchorMax = new Vector2(0.97f, 0.82f);
        scrollRT.offsetMin = Vector2.zero;
        scrollRT.offsetMax = Vector2.zero;

        var scrollRect = scrollGo.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        // Viewport
        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollGo.transform, false);
        var vpRT = viewport.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = Vector2.zero;
        vpRT.offsetMax = Vector2.zero;
        viewport.GetComponent<Image>().color = new Color(1, 1, 1, 0.01f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;
        scrollRect.viewport = vpRT;

        // Content
        var content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        scrollRect.content = contentRT;

        // Build deck card grid
        float removeCardW = 180f;
        float removeCardH = 260f;
        float gap = 10f;
        int cols = 3;

        var run = GameManager.Instance.CurrentRun;
        var deckNames = run.deckCardNames;
        int rows = Mathf.CeilToInt((float)deckNames.Count / cols);
        float totalH = rows * (removeCardH + gap) + gap;
        contentRT.sizeDelta = new Vector2(0, totalH);

        for (int i = 0; i < deckNames.Count; i++)
        {
            var cardData = GameManager.Instance.ResolveCard(deckNames[i]);
            if (cardData == null) continue;

            int col = i % cols;
            int row = i / cols;

            var cardGo = new GameObject($"DeckCard_{i}", typeof(RectTransform), typeof(Image));
            cardGo.transform.SetParent(content.transform, false);
            var crt = cardGo.GetComponent<RectTransform>();

            float totalGridW = cols * removeCardW + (cols - 1) * gap;
            float xStart = -totalGridW / 2f + removeCardW / 2f;
            float x = xStart + col * (removeCardW + gap);
            float y = -(gap + removeCardH / 2f + row * (removeCardH + gap));

            crt.anchorMin = new Vector2(0.5f, 1);
            crt.anchorMax = new Vector2(0.5f, 1);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(removeCardW, removeCardH);
            crt.anchoredPosition = new Vector2(x, y);

            cardGo.GetComponent<Image>().color = BattleConstants.Panel;

            // Element strip
            var strip = new GameObject("Strip", typeof(RectTransform), typeof(Image));
            strip.transform.SetParent(cardGo.transform, false);
            var srt = strip.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0, 0.9f);
            srt.anchorMax = Vector2.one;
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;
            strip.GetComponent<Image>().color = cardData.CardColor;

            // Card name
            var nameText = CreateText("Name", cardGo.GetComponent<RectTransform>(), cardData.cardName, 16,
                new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.85f));
            nameText.fontStyle = FontStyles.Bold;
            nameText.alignment = TextAlignmentOptions.Center;

            // Mana + Type
            var infoText = CreateText("Info", cardGo.GetComponent<RectTransform>(),
                $"{cardData.manaCost} Mana · {cardData.cardType}", 12,
                new Vector2(0.05f, 0.40f), new Vector2(0.95f, 0.55f));
            infoText.color = new Color(0.6f, 0.6f, 0.7f);
            infoText.alignment = TextAlignmentOptions.Center;

            // Stats summary
            string stats = "";
            if (cardData.baseDamage > 0) stats += $"DMG {cardData.baseDamage} ";
            if (cardData.baseBlock > 0) stats += $"DEF {cardData.baseBlock} ";
            if (cardData.healAmount > 0) stats += $"Heal {cardData.healAmount} ";
            if (cardData.drawCount > 0) stats += $"Draw {cardData.drawCount} ";
            if (!string.IsNullOrEmpty(stats))
            {
                var statText = CreateText("Stats", cardGo.GetComponent<RectTransform>(), stats.TrimEnd(), 12,
                    new Vector2(0.05f, 0.20f), new Vector2(0.95f, 0.40f));
                statText.alignment = TextAlignmentOptions.Center;
            }

            // Element
            var elem = CreateText("Elem", cardGo.GetComponent<RectTransform>(), cardData.element.ToString(), 11,
                new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.20f));
            elem.color = cardData.CardColor;
            elem.alignment = TextAlignmentOptions.Center;

            // Click to remove
            var cardBtn = cardGo.AddComponent<Button>();
            cardBtn.targetGraphic = cardGo.GetComponent<Image>();
            string capturedName = deckNames[i];
            cardBtn.onClick.AddListener(() => OnRemoveCard(capturedName));
        }

        // Back button
        var backBtn = CreateServiceButton("BackBtn", panelRT, "BACK",
            new Vector2(0.25f, 0.03f), new Vector2(0.75f, 0.10f),
            new Color(0.15f, 0.15f, 0.2f));
        backBtn.GetComponent<Button>().onClick.AddListener(OnBackFromRemove);
    }

    void OnRemoveCard(string cardName)
    {
        int gold = GetGold();
        if (gold < REMOVE_COST) return;

        GameManager.Instance.CurrentRun.gold -= REMOVE_COST;
        GameManager.Instance.RemoveCardFromDeck(cardName);

        // Close remove panel and refresh
        if (_removePanel != null) Destroy(_removePanel);
        _removePanel = null;
        RefreshGoldAndButtons();
    }

    void OnBackFromRemove()
    {
        if (_removePanel != null) Destroy(_removePanel);
        _removePanel = null;
    }

    void OnLeave()
    {
        Cleanup();
        _onComplete?.Invoke();
    }

    void RefreshGoldAndButtons()
    {
        int gold = GetGold();
        if (_goldText != null)
            _goldText.text = $"Gold: {gold}";

        for (int i = 0; i < _cardObjects.Count; i++)
        {
            if (!_sold[i])
                UpdateCardBuyable(_cardObjects[i], _shopCards[i], false);
        }
        UpdateRemoveButton();
    }

    void UpdateCardBuyable(GameObject cardGo, CardData data, bool sold)
    {
        int gold = GetGold();
        int price = GetCardPrice(data);
        bool canBuy = gold >= price && GetDeckSize() < BattleConstants.MAX_DECK_SIZE && !sold;

        var btn = cardGo.GetComponent<Button>();
        if (btn != null) btn.interactable = canBuy;

        var img = cardGo.GetComponent<Image>();
        if (img != null && !sold)
            img.color = canBuy ? BattleConstants.Panel : new Color(0.12f, 0.12f, 0.18f);
    }

    void UpdateRemoveButton()
    {
        if (_removeBtn == null) return;
        int gold = GetGold();
        bool canRemove = gold >= REMOVE_COST && GetDeckSize() > 1;
        _removeBtn.GetComponent<Button>().interactable = canRemove;
        _removeBtn.GetComponent<Image>().color = canRemove
            ? new Color(0.25f, 0.08f, 0.08f)
            : new Color(0.12f, 0.12f, 0.15f);
    }

    int GetGold()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentRun != null)
            return GameManager.Instance.CurrentRun.gold;
        return 0;
    }

    int GetDeckSize()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentRun != null)
            return GameManager.Instance.CurrentRun.deckCardNames.Count;
        return 0;
    }

    void Cleanup()
    {
        _cardObjects.Clear();
        _sold.Clear();
        if (_removePanel != null) Destroy(_removePanel);
        if (_panel != null) Destroy(_panel);
    }

    // === UI Helpers ===
    TextMeshProUGUI CreateText(string name, Transform parent, string text, int fontSize,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(4, 0);
        rt.offsetMax = new Vector2(-4, 0);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        return tmp;
    }

    GameObject CreateServiceButton(string name, Transform parent, string label,
        Vector2 anchorMin, Vector2 anchorMax, Color bgColor)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
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
        tmp.fontSize = 20;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        return go;
    }
}

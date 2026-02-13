using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class DeckViewUI : MonoBehaviour
{
    GameObject _panel;
    List<GameObject> _cardObjects = new List<GameObject>();
    Action _onClose;

    const float CARD_WIDTH = 220f;
    const float CARD_HEIGHT = 320f;
    const float CARD_SPACING = 10f;
    const int COLUMNS = 3;

    public void Show(Transform canvasTransform, Action onClose)
    {
        _onClose = onClose;
        BuildPanel(canvasTransform);
    }

    void BuildPanel(Transform parent)
    {
        // Full-screen overlay
        _panel = new GameObject("DeckViewPanel", typeof(RectTransform), typeof(Image));
        _panel.transform.SetParent(parent, false);
        var rt = _panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        _panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.9f);

        var panelRT = rt;

        // Load and sort cards
        var cards = LoadDeckCards();
        cards.Sort((a, b) =>
        {
            int elem = a.element.CompareTo(b.element);
            if (elem != 0) return elem;
            return a.manaCost.CompareTo(b.manaCost);
        });

        // Title
        var title = CreateText("Title", panelRT, $"YOUR DECK ({cards.Count})", 32,
            new Vector2(0.05f, 0.91f), new Vector2(0.95f, 0.97f));
        title.color = BattleConstants.Highlight;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;

        // Element stats bar
        string statsText = BuildElementStats(cards);
        var stats = CreateText("Stats", panelRT, statsText, 18,
            new Vector2(0.05f, 0.86f), new Vector2(0.95f, 0.91f));
        stats.alignment = TextAlignmentOptions.Center;
        stats.richText = true;

        // Scroll area
        var scrollGo = new GameObject("ScrollArea", typeof(RectTransform), typeof(Image));
        scrollGo.transform.SetParent(panelRT, false);
        var scrollRT = scrollGo.GetComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0.05f, 0.1f);
        scrollRT.anchorMax = new Vector2(0.95f, 0.85f);
        scrollRT.offsetMin = Vector2.zero;
        scrollRT.offsetMax = Vector2.zero;
        scrollGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);

        var mask = scrollGo.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        var scrollRect = scrollGo.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.viewport = scrollRT;

        // Content container
        var contentGo = new GameObject("Content", typeof(RectTransform));
        contentGo.transform.SetParent(scrollGo.transform, false);
        var contentRT = contentGo.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot = new Vector2(0.5f, 1f);

        int rows = Mathf.CeilToInt((float)cards.Count / COLUMNS);
        float contentHeight = rows * (CARD_HEIGHT + CARD_SPACING) + CARD_SPACING;
        contentRT.sizeDelta = new Vector2(0, contentHeight);

        scrollRect.content = contentRT;

        // Create card grid
        float totalGridWidth = COLUMNS * CARD_WIDTH + (COLUMNS - 1) * CARD_SPACING;

        for (int i = 0; i < cards.Count; i++)
        {
            int col = i % COLUMNS;
            int row = i / COLUMNS;

            float startX = -totalGridWidth / 2f + CARD_WIDTH / 2f;
            float xPos = startX + col * (CARD_WIDTH + CARD_SPACING);
            float yPos = -(CARD_SPACING + CARD_HEIGHT / 2f + row * (CARD_HEIGHT + CARD_SPACING));

            var cardGo = CreateDeckCard(contentRT, cards[i], xPos, yPos);
            _cardObjects.Add(cardGo);
        }

        // Close button
        var closeBtn = CreateButton("CloseBtn", panelRT, "CLOSE",
            new Vector2(0.3f, 0.02f), new Vector2(0.7f, 0.08f),
            new Color(0.15f, 0.15f, 0.2f));
        closeBtn.onClick.AddListener(OnClose);
    }

    List<CardData> LoadDeckCards()
    {
        var result = new List<CardData>();
        if (GameManager.Instance == null || GameManager.Instance.CurrentRun == null)
            return result;

        foreach (var cardName in GameManager.Instance.CurrentRun.deckCardNames)
        {
            var data = GameManager.Instance.ResolveCard(cardName);
            if (data != null)
                result.Add(data);
        }
        return result;
    }

    string BuildElementStats(List<CardData> cards)
    {
        var counts = new Dictionary<ElementType, int>();
        foreach (var card in cards)
        {
            if (!counts.ContainsKey(card.element))
                counts[card.element] = 0;
            counts[card.element]++;
        }

        string text = "";
        foreach (var kvp in counts)
        {
            Color c = BattleConstants.GetElementColor(kvp.Key);
            string hex = ColorUtility.ToHtmlStringRGB(c);
            string initial = GetElementInitial(kvp.Key);
            text += $"<color=#{hex}>{initial}:{kvp.Value}</color>  ";
        }
        return text.TrimEnd();
    }

    string GetElementInitial(ElementType elem)
    {
        switch (elem)
        {
            case ElementType.Fire: return "F";
            case ElementType.Ice: return "I";
            case ElementType.Lightning: return "L";
            case ElementType.Water: return "W";
            case ElementType.Nature: return "N";
            case ElementType.Light: return "Lt";
            case ElementType.Dark: return "D";
            default: return "?";
        }
    }

    GameObject CreateDeckCard(RectTransform parent, CardData data, float xPos, float yPos)
    {
        var go = new GameObject($"DeckCard", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(CARD_WIDTH, CARD_HEIGHT);
        rt.anchoredPosition = new Vector2(xPos, yPos);

        go.GetComponent<Image>().color = BattleConstants.Panel;

        // Element strip (top)
        var strip = new GameObject("Strip", typeof(RectTransform), typeof(Image));
        strip.transform.SetParent(go.transform, false);
        var srt = strip.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0, 0.9f);
        srt.anchorMax = Vector2.one;
        srt.offsetMin = Vector2.zero;
        srt.offsetMax = Vector2.zero;
        strip.GetComponent<Image>().color = data.CardColor;

        // Mana cost (top-left)
        var cost = CreateText("Cost", go.GetComponent<RectTransform>(), data.manaCost.ToString(), 24,
            new Vector2(0f, 0.82f), new Vector2(0.25f, 0.95f));
        cost.color = BattleConstants.Magic;
        cost.fontStyle = FontStyles.Bold;
        cost.alignment = TextAlignmentOptions.Center;

        // Card name
        var cardName = CreateText("Name", go.GetComponent<RectTransform>(), data.cardName, 17,
            new Vector2(0.05f, 0.68f), new Vector2(0.95f, 0.82f));
        cardName.fontStyle = FontStyles.Bold;
        cardName.alignment = TextAlignmentOptions.Center;

        // Card type
        var type = CreateText("Type", go.GetComponent<RectTransform>(), data.cardType.ToString(), 14,
            new Vector2(0.05f, 0.58f), new Vector2(0.95f, 0.68f));
        type.color = new Color(0.6f, 0.6f, 0.7f);
        type.alignment = TextAlignmentOptions.Center;

        // Stats
        string statsStr = "";
        if (data.baseDamage > 0) statsStr += $"DMG {data.baseDamage}\n";
        if (data.baseBlock > 0) statsStr += $"DEF {data.baseBlock}\n";
        if (data.healAmount > 0) statsStr += $"Heal {data.healAmount}\n";
        if (data.drawCount > 0) statsStr += $"Draw {data.drawCount}\n";
        if (data.appliedEffect != null)
            statsStr += $"{data.appliedEffect.effectName}({data.effectDuration}t)";

        var desc = CreateText("Desc", go.GetComponent<RectTransform>(), statsStr.TrimEnd(), 14,
            new Vector2(0.08f, 0.15f), new Vector2(0.92f, 0.58f));
        desc.alignment = TextAlignmentOptions.Center;
        desc.enableWordWrapping = true;

        // Element name at bottom
        var elemText = CreateText("Element", go.GetComponent<RectTransform>(),
            data.element.ToString(), 12,
            new Vector2(0.05f, 0.03f), new Vector2(0.95f, 0.15f));
        elemText.color = data.CardColor;
        elemText.alignment = TextAlignmentOptions.Center;

        return go;
    }

    void OnClose()
    {
        Cleanup();
        _onClose?.Invoke();
    }

    void Cleanup()
    {
        _cardObjects.Clear();
        if (_panel != null)
            Destroy(_panel);
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

    Button CreateButton(string name, Transform parent, string label,
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
        tmp.fontSize = 22;
        tmp.color = new Color(0.6f, 0.6f, 0.7f);
        tmp.alignment = TextAlignmentOptions.Center;

        return btn;
    }
}

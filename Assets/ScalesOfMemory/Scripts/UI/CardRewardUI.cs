using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class CardRewardUI : MonoBehaviour
{
    GameObject _panel;
    List<GameObject> _cardObjects = new List<GameObject>();
    CardData _selectedCard;
    int _selectedIndex = -1;
    Action _onComplete;
    int _goldReward;

    const float CARD_WIDTH = 260f;
    const float CARD_HEIGHT = 380f;
    const float CARD_SPACING = 20f;

    public void Show(Transform canvasTransform, int goldReward, Action onComplete)
    {
        _onComplete = onComplete;
        _goldReward = goldReward;
        _selectedCard = null;
        _selectedIndex = -1;

        BuildPanel(canvasTransform);
    }

    void BuildPanel(Transform parent)
    {
        // Full-screen overlay
        _panel = new GameObject("CardRewardPanel", typeof(RectTransform), typeof(Image));
        _panel.transform.SetParent(parent, false);
        var rt = _panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        _panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);

        var panelRT = rt;

        // Title
        var title = CreateText("Title", panelRT, "CHOOSE A CARD", 36,
            new Vector2(0.1f, 0.85f), new Vector2(0.9f, 0.95f));
        title.color = BattleConstants.Highlight;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;

        // Gold reward text
        if (_goldReward > 0)
        {
            var goldText = CreateText("Gold", panelRT, $"+{_goldReward} Gold!", 22,
                new Vector2(0.2f, 0.79f), new Vector2(0.8f, 0.86f));
            goldText.color = BattleConstants.Scales;
            goldText.alignment = TextAlignmentOptions.Center;
        }

        // Pick 3 random cards
        var cards = PickRewardCards(3);

        // Create card displays
        float totalWidth = cards.Count * CARD_WIDTH + (cards.Count - 1) * CARD_SPACING;

        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            var cardGo = CreateRewardCard(panelRT, card, i, cards.Count);
            _cardObjects.Add(cardGo);
        }

        // Skip button
        var skipBtn = CreateButton("SkipBtn", panelRT, "SKIP",
            new Vector2(0.3f, 0.05f), new Vector2(0.7f, 0.12f),
            new Color(0.15f, 0.15f, 0.2f));
        skipBtn.onClick.AddListener(OnSkip);
    }

    List<CardData> PickRewardCards(int count)
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

    GameObject CreateRewardCard(RectTransform parent, CardData data, int index, int total)
    {
        var go = new GameObject($"RewardCard_{index}", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        // Center cards horizontally
        float totalWidth = total * CARD_WIDTH + (total - 1) * CARD_SPACING;
        float startX = -totalWidth / 2f + CARD_WIDTH / 2f;
        float xPos = startX + index * (CARD_WIDTH + CARD_SPACING);

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(CARD_WIDTH, CARD_HEIGHT);
        rt.anchoredPosition = new Vector2(xPos, 20f);

        var bgImg = go.GetComponent<Image>();
        bgImg.color = BattleConstants.Panel;

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
        var cost = CreateText("Cost", go.GetComponent<RectTransform>(), data.manaCost.ToString(), 28,
            new Vector2(0f, 0.82f), new Vector2(0.25f, 0.95f));
        cost.color = BattleConstants.Magic;
        cost.fontStyle = FontStyles.Bold;
        cost.alignment = TextAlignmentOptions.Center;

        // Card name
        var name = CreateText("Name", go.GetComponent<RectTransform>(), data.cardName, 20,
            new Vector2(0.05f, 0.68f), new Vector2(0.95f, 0.82f));
        name.fontStyle = FontStyles.Bold;
        name.alignment = TextAlignmentOptions.Center;

        // Card type
        var type = CreateText("Type", go.GetComponent<RectTransform>(), data.cardType.ToString(), 16,
            new Vector2(0.05f, 0.58f), new Vector2(0.95f, 0.68f));
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

        var desc = CreateText("Desc", go.GetComponent<RectTransform>(), stats.TrimEnd(), 16,
            new Vector2(0.08f, 0.15f), new Vector2(0.92f, 0.58f));
        desc.alignment = TextAlignmentOptions.Center;
        desc.enableWordWrapping = true;

        // Element name at bottom
        var elemText = CreateText("Element", go.GetComponent<RectTransform>(),
            data.element.ToString(), 14,
            new Vector2(0.05f, 0.03f), new Vector2(0.95f, 0.15f));
        elemText.color = data.CardColor;
        elemText.alignment = TextAlignmentOptions.Center;

        // Button for selection
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = bgImg;
        int cardIndex = index;
        CardData cardData = data;
        btn.onClick.AddListener(() => OnCardClicked(cardIndex, cardData, go));

        return go;
    }

    void OnCardClicked(int index, CardData data, GameObject cardGo)
    {
        if (_selectedIndex == index)
        {
            // Double-click: confirm selection
            ConfirmSelection(data);
            return;
        }

        // First click: highlight
        _selectedIndex = index;
        _selectedCard = data;

        // Reset all cards
        for (int i = 0; i < _cardObjects.Count; i++)
        {
            var rt = _cardObjects[i].GetComponent<RectTransform>();
            var img = _cardObjects[i].GetComponent<Image>();

            float totalWidth = _cardObjects.Count * CARD_WIDTH + (_cardObjects.Count - 1) * CARD_SPACING;
            float startX = -totalWidth / 2f + CARD_WIDTH / 2f;
            float xPos = startX + i * (CARD_WIDTH + CARD_SPACING);

            if (i == index)
            {
                // Selected: lift up + highlight border
                rt.anchoredPosition = new Vector2(xPos, 50f);
                img.color = new Color(
                    BattleConstants.Highlight.r * 0.3f + BattleConstants.Panel.r * 0.7f,
                    BattleConstants.Highlight.g * 0.3f + BattleConstants.Panel.g * 0.7f,
                    BattleConstants.Highlight.b * 0.3f + BattleConstants.Panel.b * 0.7f);
            }
            else
            {
                // Unselected: dim
                rt.anchoredPosition = new Vector2(xPos, 20f);
                img.color = new Color(BattleConstants.Panel.r * 0.5f,
                    BattleConstants.Panel.g * 0.5f, BattleConstants.Panel.b * 0.5f);
            }
        }
    }

    void ConfirmSelection(CardData card)
    {
        // Add card to deck
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCardToDeck(card.name);
            GameEvents.CardRewardSelected(card);
        }

        Cleanup();
        _onComplete?.Invoke();
    }

    void OnSkip()
    {
        Cleanup();
        _onComplete?.Invoke();
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

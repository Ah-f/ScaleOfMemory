using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class EventUI : MonoBehaviour
{
    GameObject _panel;
    EventData _eventData;
    Action _onComplete;

    // Phase 1 elements (choice)
    List<GameObject> _choiceButtons = new List<GameObject>();
    GameObject _descriptionGo;

    // Phase 2 elements (result)
    GameObject _resultTextGo;
    GameObject _effectTextGo;
    GameObject _continueBtn;

    public void Show(Transform canvasTransform, EventData eventData, Action onComplete)
    {
        _eventData = eventData;
        _onComplete = onComplete;
        BuildPanel(canvasTransform);
    }

    void BuildPanel(Transform parent)
    {
        // Full-screen overlay
        _panel = new GameObject("EventPanel", typeof(RectTransform), typeof(Image));
        _panel.transform.SetParent(parent, false);
        var rt = _panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        _panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.9f);

        var panelRT = rt;

        // Title
        var title = CreateText("Title", panelRT, _eventData.title, 32,
            new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.93f));
        title.color = BattleConstants.Highlight;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;

        // Description
        _descriptionGo = CreateText("Description", panelRT, _eventData.description, 20,
            new Vector2(0.08f, 0.55f), new Vector2(0.92f, 0.80f)).gameObject;
        var descTmp = _descriptionGo.GetComponent<TextMeshProUGUI>();
        descTmp.alignment = TextAlignmentOptions.Center;
        descTmp.enableWordWrapping = true;

        // Choice buttons
        float btnHeight = 0.08f;
        float btnGap = 0.02f;
        float startY = 0.45f - (_eventData.choices.Count - 1) * (btnHeight + btnGap) / 2f;

        for (int i = 0; i < _eventData.choices.Count; i++)
        {
            var choice = _eventData.choices[i];
            float yMin = startY - i * (btnHeight + btnGap) - btnHeight;
            float yMax = yMin + btnHeight;

            var btnGo = CreateChoiceButton(panelRT, choice, yMin, yMax);
            _choiceButtons.Add(btnGo);

            // Effect preview text below button
            string preview = BuildEffectPreview(choice);
            if (!string.IsNullOrEmpty(preview))
            {
                var previewTmp = CreateText($"Preview_{i}", panelRT, preview, 14,
                    new Vector2(0.1f, yMin - 0.03f), new Vector2(0.9f, yMin));
                previewTmp.alignment = TextAlignmentOptions.Center;
                previewTmp.richText = true;
                _choiceButtons.Add(previewTmp.gameObject);
            }
        }
    }

    GameObject CreateChoiceButton(RectTransform parent, EventChoice choice, float yMin, float yMax)
    {
        Color btnColor = GetChoiceColor(choice);

        var go = new GameObject("ChoiceBtn", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.1f, yMin);
        rt.anchorMax = new Vector2(0.9f, yMax);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = btnColor;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = go.GetComponent<Image>();

        var textGo = new GameObject("Label", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(10, 0);
        trt.offsetMax = new Vector2(-10, 0);

        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = choice.choiceText;
        tmp.fontSize = 22;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        EventChoice capturedChoice = choice;
        btn.onClick.AddListener(() => OnChoiceSelected(capturedChoice));

        return go;
    }

    Color GetChoiceColor(EventChoice choice)
    {
        // Positive = green-ish, Negative = red-ish, Mixed/Neutral = blue-ish
        bool hasPositive = choice.goldDelta > 0 || choice.hpDelta > 0 ||
                          choice.maxHpDelta > 0 || choice.maxScalesDelta > 0 ||
                          !string.IsNullOrEmpty(choice.cardReward);
        bool hasNegative = choice.goldDelta < 0 || choice.hpDelta < 0 || choice.maxHpDelta < 0;

        if (hasPositive && hasNegative)
            return new Color(0.15f, 0.15f, 0.3f); // Mixed: dark blue
        if (hasPositive)
            return new Color(0.1f, 0.2f, 0.15f); // Positive: dark green
        if (hasNegative)
            return new Color(0.2f, 0.1f, 0.1f); // Negative: dark red
        return new Color(0.15f, 0.15f, 0.2f); // Neutral: dark gray
    }

    string BuildEffectPreview(EventChoice choice)
    {
        var parts = new List<string>();

        if (choice.goldDelta > 0)
            parts.Add($"<color=#{ColorUtility.ToHtmlStringRGB(BattleConstants.Scales)}>+{choice.goldDelta} Gold</color>");
        else if (choice.goldDelta < 0)
            parts.Add($"<color=#{ColorUtility.ToHtmlStringRGB(BattleConstants.Danger)}>{choice.goldDelta} Gold</color>");

        if (choice.hpDelta > 0)
            parts.Add($"<color=#{ColorUtility.ToHtmlStringRGB(BattleConstants.Safe)}>+{choice.hpDelta} HP</color>");
        else if (choice.hpDelta < 0)
            parts.Add($"<color=#{ColorUtility.ToHtmlStringRGB(BattleConstants.Danger)}>{choice.hpDelta} HP</color>");

        if (choice.maxHpDelta > 0)
            parts.Add($"<color=#{ColorUtility.ToHtmlStringRGB(BattleConstants.Safe)}>+{choice.maxHpDelta} Max HP</color>");
        else if (choice.maxHpDelta < 0)
            parts.Add($"<color=#{ColorUtility.ToHtmlStringRGB(BattleConstants.Danger)}>{choice.maxHpDelta} Max HP</color>");

        if (choice.maxScalesDelta > 0)
            parts.Add($"<color=#{ColorUtility.ToHtmlStringRGB(BattleConstants.Scales)}>+{choice.maxScalesDelta} Max Scales</color>");

        if (!string.IsNullOrEmpty(choice.cardReward))
            parts.Add($"<color=#{ColorUtility.ToHtmlStringRGB(BattleConstants.Magic)}>+1 Card</color>");

        return string.Join("  ", parts);
    }

    void OnChoiceSelected(EventChoice choice)
    {
        // Handle special case: Mysterious Chest 50/50
        if (choice.resultText == null)
        {
            if (UnityEngine.Random.value > 0.5f)
            {
                choice = new EventChoice(choice.choiceText,
                    "Jackpot! The chest is full of gold coins!",
                    goldDelta: 25);
            }
            else
            {
                choice = new EventChoice(choice.choiceText,
                    "A trap! Poison darts shoot out from the chest!",
                    hpDelta: -15);
            }
        }

        // Handle "random" card reward
        if (choice.cardReward == "random")
        {
            string actualCard = PickRandomCardName();
            choice = new EventChoice(choice.choiceText, choice.resultText,
                choice.goldDelta, choice.hpDelta, choice.maxHpDelta,
                choice.maxScalesDelta, actualCard);
        }

        // Apply effects
        ApplyOutcome(choice);

        // Switch to result phase
        ShowResult(choice);
    }

    string PickRandomCardName()
    {
        if (GameManager.Instance == null || GameManager.Instance.Config == null)
            return null;
        var allCards = GameManager.Instance.Config.allCards;
        if (allCards == null || allCards.Length == 0) return null;
        var card = allCards[UnityEngine.Random.Range(0, allCards.Length)];
        return card != null ? card.name : null;
    }

    void ApplyOutcome(EventChoice choice)
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentRun == null) return;
        var run = GameManager.Instance.CurrentRun;

        run.gold = Mathf.Max(0, run.gold + choice.goldDelta);
        run.maxHP += choice.maxHpDelta;
        if (run.maxHP < 1) run.maxHP = 1;
        run.currentHP = Mathf.Clamp(run.currentHP + choice.hpDelta, 1, run.maxHP);
        run.maxScales += choice.maxScalesDelta;
        if (run.maxScales < 0) run.maxScales = 0;

        if (!string.IsNullOrEmpty(choice.cardReward))
            GameManager.Instance.AddCardToDeck(choice.cardReward);
    }

    void ShowResult(EventChoice choice)
    {
        // Remove choice buttons and description
        foreach (var choiceBtn in _choiceButtons)
            if (choiceBtn != null) Destroy(choiceBtn);
        _choiceButtons.Clear();
        if (_descriptionGo != null) Destroy(_descriptionGo);

        var panelRT = _panel.GetComponent<RectTransform>();

        // Result text
        var resultTmp = CreateText("ResultText", panelRT, choice.resultText, 22,
            new Vector2(0.08f, 0.50f), new Vector2(0.92f, 0.75f));
        resultTmp.alignment = TextAlignmentOptions.Center;
        resultTmp.enableWordWrapping = true;
        _resultTextGo = resultTmp.gameObject;

        // Effect summary
        string effectText = BuildEffectPreview(choice);
        if (!string.IsNullOrEmpty(effectText))
        {
            var effectTmp = CreateText("EffectText", panelRT, effectText, 26,
                new Vector2(0.1f, 0.38f), new Vector2(0.9f, 0.50f));
            effectTmp.alignment = TextAlignmentOptions.Center;
            effectTmp.richText = true;
            _effectTextGo = effectTmp.gameObject;
        }

        // Continue button
        var btn = CreateButton("ContinueBtn", panelRT, "CONTINUE",
            new Vector2(0.25f, 0.15f), new Vector2(0.75f, 0.25f),
            BattleConstants.Highlight);
        btn.onClick.AddListener(OnContinue);
        _continueBtn = btn.gameObject;
    }

    void OnContinue()
    {
        Cleanup();
        _onComplete?.Invoke();
    }

    void Cleanup()
    {
        _choiceButtons.Clear();
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
        tmp.fontSize = 24;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        return btn;
    }
}

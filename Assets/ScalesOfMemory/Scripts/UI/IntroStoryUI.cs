using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class IntroStoryUI : MonoBehaviour
{
    GameObject _panel;
    TextMeshProUGUI _storyText;
    TextMeshProUGUI _promptText;
    Action _onComplete;

    int _currentPage;
    List<string> _pages;
    bool _isTyping;
    string _fullText;
    Coroutine _typeCoroutine;

    const float TYPE_SPEED = 0.03f;

    public void Show(Transform canvasTransform, Action onComplete)
    {
        _onComplete = onComplete;
        _currentPage = 0;
        _pages = BuildStoryPages();
        BuildPanel(canvasTransform);
        ShowPage(0);
    }

    List<string> BuildStoryPages()
    {
        return new List<string>
        {
            "Long ago, a great dragon guarded these lands.\n\n" +
            "Its scales held the memories of the world\u2014\n" +
            "every joy, every sorrow, every secret\n" +
            "woven into shimmering armor.",

            "But the six heroes feared the dragon's power.\n\n" +
            "They struck it down and scattered its scales\n" +
            "across cursed temples, forgotten pools,\n" +
            "and the darkest corners of the realm.",

            "Now the memories fade.\n" +
            "The world forgets what it once was.\n\n" +
            "You are the last of the dragon's kin.\n" +
            "Gather the scales. Restore what was lost.",

            "Three trials await.\n" +
            "Three guardians stand in your path.\n\n" +
            "Collect cards. Build your deck.\n" +
            "Remember what the world has forgotten."
        };
    }

    void BuildPanel(Transform parent)
    {
        _panel = new GameObject("IntroPanel", typeof(RectTransform), typeof(Image));
        _panel.transform.SetParent(parent, false);
        var rt = _panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        _panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.95f);

        // Make entire panel clickable
        var btn = _panel.AddComponent<Button>();
        btn.targetGraphic = _panel.GetComponent<Image>();
        btn.onClick.AddListener(OnTap);

        var panelRT = rt;

        // Story text (center)
        var textGo = new GameObject("StoryText", typeof(RectTransform));
        textGo.transform.SetParent(panelRT, false);
        var trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.1f, 0.3f);
        trt.anchorMax = new Vector2(0.9f, 0.8f);
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        _storyText = textGo.AddComponent<TextMeshProUGUI>();
        _storyText.text = "";
        _storyText.fontSize = 26;
        _storyText.color = new Color(0.85f, 0.85f, 0.9f);
        _storyText.alignment = TextAlignmentOptions.Center;
        _storyText.enableWordWrapping = true;
        _storyText.lineSpacing = 8f;

        // "Tap to continue" prompt
        var promptGo = new GameObject("Prompt", typeof(RectTransform));
        promptGo.transform.SetParent(panelRT, false);
        var prt = promptGo.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.2f, 0.08f);
        prt.anchorMax = new Vector2(0.8f, 0.14f);
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;

        _promptText = promptGo.AddComponent<TextMeshProUGUI>();
        _promptText.text = "";
        _promptText.fontSize = 18;
        _promptText.color = new Color(0.5f, 0.5f, 0.6f);
        _promptText.alignment = TextAlignmentOptions.Center;

        // Page indicator dots
        var dotsGo = new GameObject("Dots", typeof(RectTransform));
        dotsGo.transform.SetParent(panelRT, false);
        var drt = dotsGo.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0.3f, 0.18f);
        drt.anchorMax = new Vector2(0.7f, 0.23f);
        drt.offsetMin = Vector2.zero;
        drt.offsetMax = Vector2.zero;
    }

    void ShowPage(int pageIndex)
    {
        _currentPage = pageIndex;
        _fullText = _pages[pageIndex];
        _promptText.text = "";

        if (_typeCoroutine != null)
            StopCoroutine(_typeCoroutine);
        _typeCoroutine = StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        _isTyping = true;
        _storyText.text = "";

        for (int i = 0; i < _fullText.Length; i++)
        {
            _storyText.text = _fullText.Substring(0, i + 1);
            yield return new WaitForSeconds(TYPE_SPEED);
        }

        _isTyping = false;
        ShowPrompt();
    }

    void ShowPrompt()
    {
        if (_currentPage < _pages.Count - 1)
            _promptText.text = "tap to continue...";
        else
            _promptText.text = "tap to begin your journey...";

        StartCoroutine(PulsePrompt());
    }

    IEnumerator PulsePrompt()
    {
        float time = 0f;
        while (_promptText != null)
        {
            time += Time.deltaTime * 2f;
            float alpha = 0.4f + Mathf.Sin(time) * 0.2f;
            _promptText.color = new Color(0.5f, 0.5f, 0.6f, alpha);
            yield return null;
        }
    }

    void OnTap()
    {
        if (_isTyping)
        {
            // Skip typing animation
            if (_typeCoroutine != null)
                StopCoroutine(_typeCoroutine);
            _storyText.text = _fullText;
            _isTyping = false;
            ShowPrompt();
            return;
        }

        if (_currentPage < _pages.Count - 1)
        {
            ShowPage(_currentPage + 1);
        }
        else
        {
            // Story complete
            Cleanup();
            _onComplete?.Invoke();
        }
    }

    void Cleanup()
    {
        if (_panel != null)
            Destroy(_panel);
    }
}

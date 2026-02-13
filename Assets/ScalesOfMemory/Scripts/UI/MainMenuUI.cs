using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    void Start()
    {
        BuildUI();
    }

    void BuildUI()
    {
        // Canvas
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Background
        var bg = CreatePanel("Background", transform, Vector2.zero, Vector2.one);
        bg.GetComponent<Image>().color = BattleConstants.Background;

        // Title
        var title = CreateText("Title", transform, "SCALES OF\nMEMORY", 64,
            new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.85f));
        title.alignment = TextAlignmentOptions.Center;
        title.color = BattleConstants.Highlight;
        title.fontStyle = FontStyles.Bold;

        // Subtitle
        var subtitle = CreateText("Subtitle", transform,
            "~ A Roguelike Deck-Building Adventure ~", 22,
            new Vector2(0.1f, 0.50f), new Vector2(0.9f, 0.56f));
        subtitle.alignment = TextAlignmentOptions.Center;
        subtitle.color = BattleConstants.Scales;

        // New Run button
        var newRunBtn = CreateButton("NewRunBtn", transform, "NEW RUN",
            new Vector2(0.25f, 0.32f), new Vector2(0.75f, 0.40f),
            BattleConstants.Highlight);
        newRunBtn.onClick.AddListener(OnNewRun);

        // Settings button (placeholder)
        CreateButton("SettingsBtn", transform, "SETTINGS",
            new Vector2(0.25f, 0.22f), new Vector2(0.75f, 0.30f),
            new Color32(26, 26, 46, 255));

        // Version
        var version = CreateText("Version", transform, "Prototype v0.2", 16,
            new Vector2(0.3f, 0.02f), new Vector2(0.7f, 0.06f));
        version.alignment = TextAlignmentOptions.Center;
        version.color = new Color(0.4f, 0.4f, 0.5f);
    }

    void OnNewRun()
    {
        var introUI = gameObject.AddComponent<IntroStoryUI>();
        introUI.Show(transform, () =>
        {
            if (GameManager.Instance != null)
                GameManager.Instance.StartNewRun();
        });
    }

    // === UI Helpers (same pattern as BattleUIManager) ===
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
        tmp.fontSize = 28;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        return btn;
    }
}

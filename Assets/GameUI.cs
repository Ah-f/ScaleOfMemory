using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    private RectTransform _hpBarFillRect;
    private Image _hpBarFillImg;
    private Text _timerText;
    private GameObject _gameOverPanel;
    private Text _gameOverTimeText;

    void Start()
    {
        CreateUI();
    }

    void CreateUI()
    {
        // Canvas
        GameObject canvasGO = new GameObject("GameCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // HP Bar Background
        GameObject hpBg = CreateUIImage(canvasGO.transform, "HPBarBG",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0.5f, 1f),
            new Color(0.2f, 0.2f, 0.2f, 0.8f));
        RectTransform hpBgRect = hpBg.GetComponent<RectTransform>();
        hpBgRect.anchoredPosition = new Vector2(20f, -20f);
        hpBgRect.sizeDelta = new Vector2(400f, 30f);
        hpBgRect.pivot = new Vector2(0f, 1f);

        // HP Bar Fill — anchorMax.x로 길이 조절
        GameObject hpFill = CreateUIImage(hpBg.transform, "HPBarFill",
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f),
            Color.green);
        _hpBarFillRect = hpFill.GetComponent<RectTransform>();
        _hpBarFillImg = hpFill.GetComponent<Image>();
        _hpBarFillRect.offsetMin = new Vector2(2f, 2f);
        _hpBarFillRect.offsetMax = new Vector2(-2f, -2f);

        // HP Label
        GameObject hpLabel = CreateUIText(canvasGO.transform, "HPLabel", "HP",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        RectTransform hpLabelRect = hpLabel.GetComponent<RectTransform>();
        hpLabelRect.anchoredPosition = new Vector2(20f, -55f);
        hpLabelRect.sizeDelta = new Vector2(100f, 25f);
        hpLabel.GetComponent<Text>().fontSize = 18;

        // Timer Text
        GameObject timer = CreateUIText(canvasGO.transform, "Timer", "0.0s",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        RectTransform timerRect = timer.GetComponent<RectTransform>();
        timerRect.anchoredPosition = new Vector2(0f, -20f);
        timerRect.sizeDelta = new Vector2(200f, 40f);
        _timerText = timer.GetComponent<Text>();
        _timerText.fontSize = 28;
        _timerText.alignment = TextAnchor.MiddleCenter;

        // Game Over Panel
        _gameOverPanel = CreateUIImage(canvasGO.transform, "GameOverPanel",
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
            new Color(0f, 0f, 0f, 0.7f));
        _gameOverPanel.SetActive(false);

        // Game Over Title
        GameObject goTitle = CreateUIText(_gameOverPanel.transform, "GameOverTitle", "GAME OVER",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        RectTransform goTitleRect = goTitle.GetComponent<RectTransform>();
        goTitleRect.anchoredPosition = new Vector2(0f, 60f);
        goTitleRect.sizeDelta = new Vector2(500f, 60f);
        Text goTitleText = goTitle.GetComponent<Text>();
        goTitleText.fontSize = 48;
        goTitleText.alignment = TextAnchor.MiddleCenter;
        goTitleText.color = Color.red;

        // Survival Time on Game Over
        GameObject goTime = CreateUIText(_gameOverPanel.transform, "GameOverTime", "",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        RectTransform goTimeRect = goTime.GetComponent<RectTransform>();
        goTimeRect.anchoredPosition = new Vector2(0f, 0f);
        goTimeRect.sizeDelta = new Vector2(400f, 40f);
        _gameOverTimeText = goTime.GetComponent<Text>();
        _gameOverTimeText.fontSize = 28;
        _gameOverTimeText.alignment = TextAnchor.MiddleCenter;

        // Restart Button
        GameObject btnGO = CreateUIImage(_gameOverPanel.transform, "RestartBtn",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Color(0.2f, 0.6f, 0.2f, 1f));
        RectTransform btnRect = btnGO.GetComponent<RectTransform>();
        btnRect.anchoredPosition = new Vector2(0f, -60f);
        btnRect.sizeDelta = new Vector2(200f, 50f);

        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnGO.GetComponent<Image>();
        btn.onClick.AddListener(() =>
        {
            var gm = FindObjectOfType<GameManager>();
            if (gm != null) gm.Restart();
        });

        GameObject btnText = CreateUIText(btnGO.transform, "BtnText", "RESTART",
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f));
        btnText.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        btnText.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        Text bt = btnText.GetComponent<Text>();
        bt.fontSize = 24;
        bt.alignment = TextAnchor.MiddleCenter;
        bt.color = Color.white;

        // EventSystem
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    public void UpdateUI(float hpRatio, float time, bool gameOver)
    {
        if (_hpBarFillRect != null)
        {
            _hpBarFillRect.anchorMax = new Vector2(hpRatio, 1f);
            _hpBarFillImg.color = Color.Lerp(Color.red, Color.green, hpRatio);
        }

        if (_timerText != null)
            _timerText.text = time.ToString("F1") + "s";

        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(gameOver);
            if (gameOver && _gameOverTimeText != null)
                _gameOverTimeText.text = "Survived: " + time.ToString("F1") + "s";
        }
    }

    GameObject CreateUIImage(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        Image img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    GameObject CreateUIText(Transform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        Text t = go.AddComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.color = Color.white;
        return go;
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MapUI : MonoBehaviour
{
    RectTransform _mapContainer;
    ScrollRect _scrollRect;

    List<GameObject> _nodeObjects = new List<GameObject>();
    List<GameObject> _lineObjects = new List<GameObject>();
    GameObject _playerMarker;

    TextMeshProUGUI _hpText;
    TextMeshProUGUI _goldText;
    TextMeshProUGUI _phaseText;

    void Start()
    {
        BuildUI();
        PopulateMap();
        GameEvents.OnRunStateChanged += RefreshMap;
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

        // Top info bar
        var topBar = CreatePanel("TopBar", transform,
            new Vector2(0f, 0.93f), new Vector2(1f, 1f));
        topBar.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f);
        var topRT = topBar.GetComponent<RectTransform>();

        _hpText = CreateText("HP", topRT, "", 22,
            new Vector2(0.02f, 0f), new Vector2(0.35f, 1f));
        _hpText.color = BattleConstants.Safe;
        _hpText.alignment = TextAlignmentOptions.MidlineLeft;

        _goldText = CreateText("Gold", topRT, "", 22,
            new Vector2(0.35f, 0f), new Vector2(0.65f, 1f));
        _goldText.color = BattleConstants.Scales;
        _goldText.alignment = TextAlignmentOptions.Center;

        _phaseText = CreateText("Phase", topRT, "", 22,
            new Vector2(0.65f, 0f), new Vector2(0.98f, 1f));
        _phaseText.alignment = TextAlignmentOptions.MidlineRight;
        _phaseText.color = BattleConstants.Highlight;

        // Scroll area
        var scrollGo = new GameObject("ScrollArea", typeof(RectTransform), typeof(Image));
        scrollGo.transform.SetParent(transform, false);
        var scrollRT = scrollGo.GetComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0.05f, 0.02f);
        scrollRT.anchorMax = new Vector2(0.95f, 0.92f);
        scrollRT.offsetMin = Vector2.zero;
        scrollRT.offsetMax = Vector2.zero;
        scrollGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);

        var mask = scrollGo.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        _scrollRect = scrollGo.AddComponent<ScrollRect>();
        _scrollRect.horizontal = false;
        _scrollRect.vertical = true;
        _scrollRect.viewport = scrollRT;

        // Content container
        var contentGo = new GameObject("MapContent", typeof(RectTransform));
        contentGo.transform.SetParent(scrollGo.transform, false);
        _mapContainer = contentGo.GetComponent<RectTransform>();
        _mapContainer.anchorMin = new Vector2(0f, 0f);
        _mapContainer.anchorMax = new Vector2(1f, 0f);
        _mapContainer.pivot = new Vector2(0.5f, 0f);

        _scrollRect.content = _mapContainer;
    }

    void PopulateMap()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentRun == null) return;
        var run = GameManager.Instance.CurrentRun;

        // Update info bar
        _hpText.text = $"HP: {run.currentHP}/{run.maxHP}";
        _goldText.text = $"Gold: {run.gold}";
        _phaseText.text = $"Phase {(int)run.currentPhase + 1}";

        // Force layout update to get real container dimensions
        Canvas.ForceUpdateCanvases();

        // Find max row
        int maxRow = 0;
        foreach (var node in run.mapNodes)
            if (node.row > maxRow) maxRow = node.row;

        float viewportHeight = _scrollRect.GetComponent<RectTransform>().rect.height;
        int totalSlots = maxRow + 2; // rows + top/bottom padding
        float rowHeight = viewportHeight > 0
            ? Mathf.Max(viewportHeight / totalSlots, 100f)
            : 150f;
        float contentHeight = Mathf.Max(totalSlots * rowHeight, viewportHeight);

        _mapContainer.sizeDelta = new Vector2(0, contentHeight);

        // Clear existing
        foreach (var obj in _nodeObjects) if (obj != null) Destroy(obj);
        foreach (var obj in _lineObjects) if (obj != null) Destroy(obj);
        _nodeObjects.Clear();
        _lineObjects.Clear();

        // Get reliable container width
        float containerWidth = _mapContainer.rect.width;
        if (containerWidth <= 0)
            containerWidth = _scrollRect.GetComponent<RectTransform>().rect.width;
        if (containerWidth <= 0)
            containerWidth = 400f; // safe fallback

        // Node positions lookup
        var nodePositions = new Dictionary<int, Vector2>();

        // Padding so nodes don't touch edges
        float padding = 40f;
        float usableWidth = containerWidth - padding * 2f;

        // Create nodes
        foreach (var node in run.mapNodes)
        {
            float x = padding + node.xPosition * usableWidth;
            float y = node.row * rowHeight + rowHeight * 0.5f;

            var pos = new Vector2(x, y);
            nodePositions[node.id] = pos;

            var nodeGo = CreateMapNode(node, pos);
            _nodeObjects.Add(nodeGo);
        }

        // Draw connection lines
        foreach (var node in run.mapNodes)
        {
            if (!nodePositions.ContainsKey(node.id)) continue;
            Vector2 fromPos = nodePositions[node.id];

            foreach (int targetId in node.connections)
            {
                if (!nodePositions.ContainsKey(targetId)) continue;
                Vector2 toPos = nodePositions[targetId];

                var lineGo = CreateLine(fromPos, toPos, node.cleared);
                _lineObjects.Add(lineGo);
            }
        }

        // Player position marker
        if (_playerMarker != null) Destroy(_playerMarker);
        Vector2 markerPos;
        if (run.currentNodeId >= 0 && nodePositions.ContainsKey(run.currentNodeId))
        {
            markerPos = nodePositions[run.currentNodeId];
        }
        else
        {
            // Start of run: place marker below row 0
            float centerX = padding + 0.5f * usableWidth;
            markerPos = new Vector2(centerX, -rowHeight * 0.3f);
        }
        _playerMarker = CreatePlayerMarker(markerPos);
        _nodeObjects.Add(_playerMarker);

        // Scroll to current position
        if (contentHeight > 0)
        {
            float targetRow = run.currentNodeId >= 0
                ? run.mapNodes.Find(n => n.id == run.currentNodeId).row
                : 0;
            _scrollRect.verticalNormalizedPosition = Mathf.Clamp01(targetRow / (maxRow + 1));
        }
    }

    GameObject CreateMapNode(MapNode node, Vector2 pos)
    {
        float nodeSize = 80f;
        var go = new GameObject($"Node_{node.id}", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(_mapContainer, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(nodeSize, nodeSize);
        rt.anchoredPosition = pos;

        var img = go.GetComponent<Image>();
        Color baseColor = GetRoomColor(node.roomType);

        // Label
        var labelGo = new GameObject("Label", typeof(RectTransform));
        labelGo.transform.SetParent(go.transform, false);
        var lrt = labelGo.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
        var tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text = GetRoomIcon(node.roomType);
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;

        // Room type name below
        var nameGo = new GameObject("RoomName", typeof(RectTransform));
        nameGo.transform.SetParent(go.transform, false);
        var nrt = nameGo.GetComponent<RectTransform>();
        nrt.anchorMin = new Vector2(-0.5f, -0.5f);
        nrt.anchorMax = new Vector2(1.5f, 0f);
        nrt.offsetMin = Vector2.zero;
        nrt.offsetMax = Vector2.zero;
        var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
        nameTmp.text = GetRoomName(node.roomType);
        nameTmp.fontSize = 14;
        nameTmp.alignment = TextAlignmentOptions.Center;
        nameTmp.color = new Color(0.6f, 0.6f, 0.7f);

        // State-based coloring
        bool isAccessible = GameManager.Instance.IsNodeAccessible(node);
        bool isCurrent = GameManager.Instance.CurrentRun.currentNodeId == node.id;

        if (isCurrent)
            img.color = BattleConstants.Highlight;
        else if (node.cleared)
            img.color = new Color(baseColor.r * 0.3f, baseColor.g * 0.3f, baseColor.b * 0.3f);
        else if (!isAccessible)
            img.color = new Color(0.15f, 0.15f, 0.2f);
        else
            img.color = baseColor;

        // Button
        if (isAccessible && !node.cleared)
        {
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => OnNodeClicked(node));
        }

        return go;
    }

    void OnNodeClicked(MapNode node)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.EnterNode(node);
    }

    GameObject CreateLine(Vector2 from, Vector2 to, bool cleared)
    {
        var go = new GameObject("Line", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(_mapContainer, false);
        go.transform.SetAsFirstSibling();

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0f);

        Vector2 diff = to - from;
        float length = diff.magnitude;
        float angle = Mathf.Atan2(diff.x, diff.y) * Mathf.Rad2Deg;

        rt.sizeDelta = new Vector2(3f, length);
        rt.anchoredPosition = from;
        rt.localRotation = Quaternion.Euler(0, 0, -angle);

        var img = go.GetComponent<Image>();
        img.color = cleared ? new Color(0.2f, 0.2f, 0.3f) : new Color(0.4f, 0.4f, 0.5f);

        return go;
    }

    Color GetRoomColor(RoomType type)
    {
        switch (type)
        {
            case RoomType.Battle: return new Color32(40, 40, 60, 255);
            case RoomType.Elite: return new Color32(180, 60, 60, 255);
            case RoomType.Rest: return BattleConstants.Safe;
            case RoomType.Shop: return BattleConstants.Scales;
            case RoomType.Event: return BattleConstants.Magic;
            case RoomType.Boss: return BattleConstants.Danger;
            default: return Color.gray;
        }
    }

    string GetRoomIcon(RoomType type)
    {
        switch (type)
        {
            case RoomType.Battle: return "B";
            case RoomType.Elite: return "E";
            case RoomType.Rest: return "R";
            case RoomType.Shop: return "$";
            case RoomType.Event: return "?";
            case RoomType.Boss: return "X";
            default: return "?";
        }
    }

    string GetRoomName(RoomType type)
    {
        switch (type)
        {
            case RoomType.Battle: return "Battle";
            case RoomType.Elite: return "Elite";
            case RoomType.Rest: return "Rest";
            case RoomType.Shop: return "Shop";
            case RoomType.Event: return "Event";
            case RoomType.Boss: return "BOSS";
            default: return "";
        }
    }

    GameObject CreatePlayerMarker(Vector2 pos)
    {
        // Outer ring (pulsing)
        var ring = new GameObject("PlayerMarker", typeof(RectTransform), typeof(Image));
        ring.transform.SetParent(_mapContainer, false);
        var rrt = ring.GetComponent<RectTransform>();
        rrt.anchorMin = Vector2.zero;
        rrt.anchorMax = Vector2.zero;
        rrt.pivot = new Vector2(0.5f, 0.5f);
        rrt.sizeDelta = new Vector2(100f, 100f);
        rrt.anchoredPosition = pos;
        var ringImg = ring.GetComponent<Image>();
        ringImg.color = new Color(BattleConstants.Highlight.r, BattleConstants.Highlight.g,
            BattleConstants.Highlight.b, 0.4f);

        // Pulse animation
        ring.AddComponent<MarkerPulse>();

        // Arrow label above
        var arrow = new GameObject("Arrow", typeof(RectTransform));
        arrow.transform.SetParent(ring.transform, false);
        var art = arrow.GetComponent<RectTransform>();
        art.anchorMin = new Vector2(0f, 1f);
        art.anchorMax = new Vector2(1f, 1f);
        art.pivot = new Vector2(0.5f, 0f);
        art.sizeDelta = new Vector2(0, 30f);
        art.anchoredPosition = new Vector2(0, 5f);
        var atmp = arrow.AddComponent<TextMeshProUGUI>();
        atmp.text = "HERE";
        atmp.fontSize = 16;
        atmp.alignment = TextAlignmentOptions.Center;
        atmp.color = BattleConstants.Highlight;
        atmp.fontStyle = FontStyles.Bold;

        return ring;
    }

    void RefreshMap()
    {
        PopulateMap();
    }

    void OnDestroy()
    {
        GameEvents.OnRunStateChanged -= RefreshMap;
    }

    // === UI Helpers ===
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
        rt.offsetMin = new Vector2(8, 0);
        rt.offsetMax = new Vector2(-8, 0);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        return tmp;
    }
}

public class MarkerPulse : MonoBehaviour
{
    float _time;

    void Update()
    {
        _time += Time.deltaTime * 2f;
        float scale = 1f + Mathf.Sin(_time) * 0.15f;
        transform.localScale = new Vector3(scale, scale, 1f);

        var img = GetComponent<UnityEngine.UI.Image>();
        if (img != null)
        {
            float alpha = 0.3f + Mathf.Sin(_time) * 0.15f;
            var c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }
}

using UnityEngine;
using TMPro;
using System.Collections;

public class DamagePopup : MonoBehaviour
{
    TextMeshProUGUI _text;
    float _duration = 1f;
    float _elapsed;
    Vector3 _startPos;

    public static void Create(Transform parent, Vector3 worldPos, int amount, Color color)
    {
        var go = new GameObject("DmgPopup", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(200, 60);

        // Convert world pos to screen pos
        var cam = Camera.main;
        if (cam != null)
        {
            Vector2 screenPos = cam.WorldToScreenPoint(worldPos);
            rt.position = screenPos;
        }

        var popup = go.AddComponent<DamagePopup>();
        popup.Init(amount, color);
    }

    public static void CreateAtUI(Transform parent, Vector2 anchoredPos, int amount, Color color)
    {
        var go = new GameObject("DmgPopup", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(200, 60);
        rt.anchoredPosition = anchoredPos;

        var popup = go.AddComponent<DamagePopup>();
        popup.Init(amount, color);
    }

    void Init(int amount, Color color)
    {
        InitText(amount.ToString(), 36, color);
    }

    void InitText(string msg, int fontSize, Color color)
    {
        _text = gameObject.AddComponent<TextMeshProUGUI>();
        _text.text = msg;
        _text.fontSize = fontSize;
        _text.color = color;
        _text.alignment = TextAlignmentOptions.Center;
        _text.fontStyle = FontStyles.Bold;
        _text.raycastTarget = false;

        _startPos = transform.localPosition;
        _elapsed = 0f;
    }

    public static void CreateText(Transform parent, Vector3 worldPos, string message, Color color)
    {
        var go = new GameObject("TextPopup", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(200, 60);

        var cam = Camera.main;
        if (cam != null)
        {
            Vector2 screenPos = cam.WorldToScreenPoint(worldPos);
            rt.position = screenPos;
        }

        var popup = go.AddComponent<DamagePopup>();
        popup.InitText(message, 30, color);
    }

    void Update()
    {
        _elapsed += Time.deltaTime;
        float t = _elapsed / _duration;

        // Float up
        transform.localPosition = _startPos + Vector3.up * (t * 80f);

        // Fade out
        if (_text != null)
        {
            var c = _text.color;
            c.a = 1f - t;
            _text.color = c;
        }

        if (t >= 1f)
            Destroy(gameObject);
    }
}

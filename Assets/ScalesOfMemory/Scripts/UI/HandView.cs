using System.Collections.Generic;
using UnityEngine;

public class HandView : MonoBehaviour
{
    public static HandView Instance { get; private set; }

    List<CardView> _cardViews = new List<CardView>();
    RectTransform _rt;

    public EnemyInstance CurrentTarget { get; private set; }

    public const float CARD_WIDTH = 230f;
    public const float CARD_HEIGHT = 330f;
    const float OVERLAP_SPACING = 100f;

    void Awake()
    {
        Instance = this;
        _rt = GetComponent<RectTransform>();
    }

    public void SetTarget(EnemyInstance enemy)
    {
        CurrentTarget = enemy;
    }

    public void UpdateHand(List<CardInstance> hand)
    {
        // Clear old views
        foreach (var view in _cardViews)
        {
            if (view != null && view.gameObject != null)
                Destroy(view.gameObject);
        }
        _cardViews.Clear();

        if (hand == null || hand.Count == 0) return;

        // Fixed card size, overlapping layout
        float spacing = OVERLAP_SPACING;
        float totalWidth = CARD_WIDTH + (hand.Count - 1) * spacing;

        // Center the hand
        float startX = -totalWidth / 2f + CARD_WIDTH / 2f;

        for (int i = 0; i < hand.Count; i++)
        {
            var cardGo = new GameObject($"Card_{i}",
                typeof(RectTransform), typeof(CanvasRenderer));
            cardGo.transform.SetParent(_rt, false);

            var crt = cardGo.GetComponent<RectTransform>();
            crt.sizeDelta = new Vector2(CARD_WIDTH, CARD_HEIGHT);
            crt.anchorMin = new Vector2(0.5f, 0f);
            crt.anchorMax = new Vector2(0.5f, 0f);
            crt.pivot = new Vector2(0.5f, 0f);
            crt.anchoredPosition = new Vector2(startX + i * spacing, 0f);

            var view = cardGo.AddComponent<CardView>();
            view.Init(hand[i], i);
            _cardViews.Add(view);
        }
    }

    // Called by CardView when hovered - bring to front
    public void OnCardHovered(CardView card)
    {
        if (card != null)
            card.transform.SetAsLastSibling();
    }

    // Called by CardView when unhovered - restore order
    public void OnCardUnhovered(CardView card)
    {
        RestoreSiblingOrder();
    }

    void RestoreSiblingOrder()
    {
        for (int i = 0; i < _cardViews.Count; i++)
        {
            if (_cardViews[i] != null)
                _cardViews[i].transform.SetSiblingIndex(i);
        }
    }

    public void RefreshVisuals()
    {
        foreach (var view in _cardViews)
        {
            if (view != null)
                view.UpdateVisual();
        }
    }

    void OnDestroy()
    {
        Instance = null;
    }
}

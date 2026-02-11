using System.Collections.Generic;
using UnityEngine;

public class DeckManager
{
    public List<CardInstance> DrawPile { get; private set; }
    public List<CardInstance> Hand { get; private set; }
    public List<CardInstance> DiscardPile { get; private set; }

    public int DrawPileCount => DrawPile.Count;
    public int DiscardPileCount => DiscardPile.Count;

    public DeckManager()
    {
        DrawPile = new List<CardInstance>();
        Hand = new List<CardInstance>();
        DiscardPile = new List<CardInstance>();
    }

    public void InitializeDeck(List<CardData> startingDeck)
    {
        DrawPile.Clear();
        Hand.Clear();
        DiscardPile.Clear();

        foreach (var cardData in startingDeck)
            DrawPile.Add(new CardInstance(cardData));

        Shuffle(DrawPile);
    }

    public void AddCardToDeck(CardInstance card)
    {
        DrawPile.Add(card);
    }

    public void ShuffleDraw()
    {
        Shuffle(DrawPile);
    }

    public List<CardInstance> DrawCards(int count)
    {
        var drawn = new List<CardInstance>();
        for (int i = 0; i < count; i++)
        {
            if (DrawPile.Count == 0)
            {
                if (DiscardPile.Count == 0) break;
                ShuffleDiscardIntoDraw();
            }
            if (DrawPile.Count == 0) break;

            var card = DrawPile[0];
            DrawPile.RemoveAt(0);
            Hand.Add(card);
            drawn.Add(card);
        }
        GameEvents.HandChanged();
        return drawn;
    }

    public void PlayCard(CardInstance card)
    {
        Hand.Remove(card);
        DiscardPile.Add(card);
        GameEvents.HandChanged();
    }

    public void DiscardHand()
    {
        DiscardPile.AddRange(Hand);
        Hand.Clear();
        GameEvents.HandChanged();
    }

    public void ShuffleDiscardIntoDraw()
    {
        DrawPile.AddRange(DiscardPile);
        DiscardPile.Clear();
        Shuffle(DrawPile);
    }

    void Shuffle(List<CardInstance> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}

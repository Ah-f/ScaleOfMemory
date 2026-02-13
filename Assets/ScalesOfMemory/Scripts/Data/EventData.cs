using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EventChoice
{
    public string choiceText;
    public string resultText;
    public int goldDelta;
    public int hpDelta;
    public int maxHpDelta;
    public int maxScalesDelta;
    public string cardReward; // card name or null

    public EventChoice(string choiceText, string resultText,
        int goldDelta = 0, int hpDelta = 0, int maxHpDelta = 0,
        int maxScalesDelta = 0, string cardReward = null)
    {
        this.choiceText = choiceText;
        this.resultText = resultText;
        this.goldDelta = goldDelta;
        this.hpDelta = hpDelta;
        this.maxHpDelta = maxHpDelta;
        this.maxScalesDelta = maxScalesDelta;
        this.cardReward = cardReward;
    }
}

[System.Serializable]
public class EventData
{
    public string title;
    public string description;
    public List<EventChoice> choices;

    public EventData(string title, string description, List<EventChoice> choices)
    {
        this.title = title;
        this.description = description;
        this.choices = choices;
    }

    static List<EventData> _allEvents;

    public static EventData GetRandomEvent()
    {
        if (_allEvents == null)
            _allEvents = BuildEventPool();
        return _allEvents[Random.Range(0, _allEvents.Count)];
    }

    static List<EventData> BuildEventPool()
    {
        return new List<EventData>
        {
            new EventData(
                "Wandering Merchant",
                "A cloaked figure approaches with a cart of glowing potions.\n\"Care to buy a healing draught?\"",
                new List<EventChoice>
                {
                    new EventChoice("Buy a potion", "The warm liquid restores your vitality.",
                        goldDelta: -15, hpDelta: 20),
                    new EventChoice("Decline", "You nod politely and continue on your way.")
                }
            ),

            new EventData(
                "Ancient Shrine",
                "You discover a glowing shrine covered in dragon scale carvings.\nAncient power radiates from within.",
                new List<EventChoice>
                {
                    new EventChoice("Pray at the shrine", "A warm light fills your body. You feel stronger than before.",
                        maxHpDelta: 10),
                    new EventChoice("Smash it for gems", "The shrine shatters, revealing hidden gold inside.",
                        goldDelta: 30)
                }
            ),

            new EventData(
                "Wounded Traveler",
                "A wounded traveler lies by the roadside, clutching a strange card.\n\"Help me... and this is yours.\"",
                new List<EventChoice>
                {
                    new EventChoice("Share your vitality", "The traveler hands you a mysterious card as thanks.",
                        hpDelta: -10, cardReward: "random"),
                    new EventChoice("Walk away", "You leave the traveler behind. The road is harsh.")
                }
            ),

            new EventData(
                "Mysterious Chest",
                "An ornate chest sits in the middle of the path.\nIt could be treasure... or a trap.",
                new List<EventChoice>
                {
                    new EventChoice("Open it", null), // special: 50/50 handled in EventUI
                    new EventChoice("Leave it alone", "Better safe than sorry. You continue forward.")
                }
            ),

            new EventData(
                "Dragon Scale Pool",
                "A pool of shimmering water lined with dragon scales.\nThe water pulses with ancient energy.",
                new List<EventChoice>
                {
                    new EventChoice("Bathe in the pool", "Scales grow on your skin, hardening your defenses.",
                        maxScalesDelta: 2),
                    new EventChoice("Drink the water", "The water is refreshing and restorative.",
                        hpDelta: 15)
                }
            ),

            new EventData(
                "Cursed Tome",
                "A book bound in dark leather lies open on a pedestal.\nForbidden knowledge whispers from its pages.",
                new List<EventChoice>
                {
                    new EventChoice("Read the tome", "Dark power flows into you. It burns, but you gain new abilities.",
                        hpDelta: -10, cardReward: "random"),
                    new EventChoice("Burn it", "The flames reveal gold coins hidden in the binding.",
                        goldDelta: 15)
                }
            ),

            new EventData(
                "Fairy Ring",
                "Glowing mushrooms form a perfect circle in a forest clearing.\nA tiny voice whispers: \"Make a wish...\"",
                new List<EventChoice>
                {
                    new EventChoice("Wish for riches", "Gold appears, but you feel slightly weakened.",
                        goldDelta: 30, maxHpDelta: -5),
                    new EventChoice("Wish for health", "A gentle warmth heals your wounds.",
                        hpDelta: 10)
                }
            ),

            new EventData(
                "Abandoned Camp",
                "You find an abandoned campsite with scattered belongings.\nSomeone left in a hurry.",
                new List<EventChoice>
                {
                    new EventChoice("Search the supplies", "You find some leftover gold coins.",
                        goldDelta: 20),
                    new EventChoice("Rest by the fire", "The warmth of the embers soothes your wounds.",
                        hpDelta: 25)
                }
            )
        };
    }
}

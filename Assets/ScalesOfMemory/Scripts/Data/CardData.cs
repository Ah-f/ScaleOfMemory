using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "ScalesOfMemory/CardData")]
public class CardData : ScriptableObject
{
    public string cardName;
    [TextArea(2, 4)]
    public string description;
    public int manaCost;
    public ElementType element;
    public CardType cardType;
    public int baseDamage;
    public int baseBlock;
    public int healAmount;
    public int drawCount;
    public bool needsTarget = true;

    [Header("Status Effect")]
    public StatusEffectData appliedEffect;
    public int effectDuration;
    public int effectPotency;

    public Color CardColor => BattleConstants.GetElementColor(element);
}

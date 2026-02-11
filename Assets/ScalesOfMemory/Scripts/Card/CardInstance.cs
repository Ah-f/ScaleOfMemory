public class CardInstance
{
    public CardData Data { get; private set; }
    public int CurrentManaCost { get; set; }
    public int CurrentDamage { get; set; }
    public int CurrentBlock { get; set; }

    public CardInstance(CardData data)
    {
        Data = data;
        CurrentManaCost = data.manaCost;
        CurrentDamage = data.baseDamage;
        CurrentBlock = data.baseBlock;
    }

    public bool CanPlay(int currentMana)
    {
        return currentMana >= CurrentManaCost;
    }
}

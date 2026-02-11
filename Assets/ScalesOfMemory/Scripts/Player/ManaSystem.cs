public class ManaSystem
{
    public int CurrentMana { get; private set; }
    public int MaxMana { get; private set; }
    public int ManaPerTurn { get; private set; }

    public ManaSystem(int manaPerTurn = 3, int maxMana = 10)
    {
        ManaPerTurn = manaPerTurn;
        MaxMana = maxMana;
        CurrentMana = manaPerTurn;
    }

    public bool CanSpend(int amount) => CurrentMana >= amount;

    public bool SpendMana(int amount)
    {
        if (CurrentMana < amount) return false;
        CurrentMana -= amount;
        GameEvents.ManaChanged(CurrentMana);
        return true;
    }

    public void RefillMana()
    {
        CurrentMana = System.Math.Min(CurrentMana + ManaPerTurn, MaxMana);
        GameEvents.ManaChanged(CurrentMana);
    }

    public void AddMana(int amount)
    {
        CurrentMana = System.Math.Min(CurrentMana + amount, MaxMana);
        GameEvents.ManaChanged(CurrentMana);
    }
}

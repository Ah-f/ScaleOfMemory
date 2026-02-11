[System.Serializable]
public struct Intent
{
    public IntentType Type;
    public int Value;
    public string Description;

    public Intent(IntentType type, int value, string description)
    {
        Type = type;
        Value = value;
        Description = description;
    }
}

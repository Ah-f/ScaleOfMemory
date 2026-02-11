public class StatusEffectInstance
{
    public StatusEffectData Data { get; private set; }
    public int RemainingDuration { get; private set; }
    public int Potency { get; private set; }
    public bool IsExpired => RemainingDuration <= 0;

    public StatusEffectInstance(StatusEffectData data, int duration, int potency)
    {
        Data = data;
        RemainingDuration = duration;
        Potency = potency;
    }

    public void TickDown()
    {
        RemainingDuration--;
    }
}

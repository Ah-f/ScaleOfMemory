using System.Collections.Generic;

public static class StatusEffectProcessor
{
    public static int ProcessTurnStart(List<StatusEffectInstance> effects)
    {
        int totalDamage = 0;
        foreach (var effect in effects)
        {
            if (effect.IsExpired) continue;

            if (effect.Data.damagePerTurn > 0)
                totalDamage += effect.Data.damagePerTurn + effect.Potency;

            effect.TickDown();
        }
        RemoveExpired(effects);
        return totalDamage;
    }

    public static bool HasActionPreventing(List<StatusEffectInstance> effects)
    {
        foreach (var effect in effects)
        {
            if (!effect.IsExpired && effect.Data.preventsAction)
                return true;
        }
        return false;
    }

    public static void RemoveExpired(List<StatusEffectInstance> effects)
    {
        effects.RemoveAll(e => e.IsExpired);
    }
}

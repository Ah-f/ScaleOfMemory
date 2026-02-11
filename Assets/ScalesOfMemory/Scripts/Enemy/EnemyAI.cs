using System.Collections.Generic;
using UnityEngine;

public static class EnemyAI
{
    public static Intent DecideNextIntent(EnemyInstance enemy, int turnNumber)
    {
        var data = enemy.Data;
        float hpRatio = (float)enemy.CurrentHP / enemy.MaxHP;

        // Charging trait: every 2 turns do double damage
        if (data.trait == EnemyTrait.Charging && turnNumber % 2 == 0)
        {
            int dmg = Random.Range(data.minAttack, data.maxAttack + 1) * 2;
            return new Intent(IntentType.Attack, dmg, "Charge!");
        }

        // Summoning trait: every 3 turns summon
        if (data.trait == EnemyTrait.Summoning && turnNumber % 3 == 0)
        {
            return new Intent(IntentType.Summon, 1, "Summon");
        }

        // Weighted random from patterns
        if (data.intentPatterns != null && data.intentPatterns.Length > 0)
        {
            var valid = new List<IntentPattern>();
            int totalWeight = 0;

            foreach (var p in data.intentPatterns)
            {
                if (p.conditionTurnMod > 0 && turnNumber % p.conditionTurnMod != 0)
                    continue;
                if (hpRatio > p.hpThreshold)
                    continue;
                valid.Add(p);
                totalWeight += p.weight;
            }

            if (valid.Count > 0)
            {
                int roll = Random.Range(0, totalWeight);
                int cumulative = 0;
                foreach (var p in valid)
                {
                    cumulative += p.weight;
                    if (roll < cumulative)
                    {
                        int value = 0;
                        if (p.type == IntentType.Attack)
                            value = Random.Range(data.minAttack, data.maxAttack + 1);
                        else if (p.type == IntentType.Defend)
                            value = Random.Range(5, 15);

                        return new Intent(p.type, value, p.description);
                    }
                }
            }
        }

        // Default: basic attack
        int baseDmg = Random.Range(data.minAttack, data.maxAttack + 1);
        return new Intent(IntentType.Attack, baseDmg, "Attack");
    }

    public static void ExecuteIntent(EnemyInstance enemy, PlayerBattleState player)
    {
        var intent = enemy.CurrentIntent;

        switch (intent.Type)
        {
            case IntentType.Attack:
                player.TakeDamage(intent.Value);
                break;
            case IntentType.Defend:
                enemy.AddBlock(intent.Value);
                break;
            case IntentType.Buff:
                enemy.Heal(intent.Value);
                break;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public static class CardEffect
{
    public static void Execute(CardInstance card, PlayerBattleState player, ManaSystem mana,
        EnemyInstance target, List<EnemyInstance> allEnemies, bool quickPlayBonus)
    {
        switch (card.Data.cardType)
        {
            case CardType.Attack:
                ExecuteAttack(card, target, quickPlayBonus);
                break;
            case CardType.Defend:
                ExecuteDefend(card, player);
                break;
            case CardType.Skill:
                ExecuteSkill(card, player, mana);
                break;
        }

        if (card.Data.appliedEffect != null && target != null)
        {
            var effect = new StatusEffectInstance(card.Data.appliedEffect,
                card.Data.effectDuration, card.Data.effectPotency);
            target.AddStatusEffect(effect);
        }
    }

    static void ExecuteAttack(CardInstance card, EnemyInstance target, bool quickPlayBonus)
    {
        if (target == null) return;

        int damage = card.CurrentDamage;
        if (quickPlayBonus)
            damage = Mathf.RoundToInt(damage * BattleConstants.QUICK_PLAY_BONUS);

        target.TakeDamage(damage);
        GameEvents.EnemyDamaged(target, damage);
    }

    static void ExecuteDefend(CardInstance card, PlayerBattleState player)
    {
        if (card.CurrentBlock > 0)
            player.AddBlock(card.CurrentBlock);
        if (card.Data.healAmount > 0)
            player.Heal(card.Data.healAmount);
    }

    static void ExecuteSkill(CardInstance card, PlayerBattleState player, ManaSystem mana)
    {
        if (card.Data.healAmount > 0)
            player.Heal(card.Data.healAmount);
        // drawCount is handled by BattleManager after effect execution
    }
}

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class AssetCreator
{
    [MenuItem("ScalesOfMemory/Create All Assets")]
    public static void CreateAllAssets()
    {
        CreateStatusEffects();
        CreateCards();
        CreateEnemies();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ScalesOfMemory] All assets created!");
    }

    [MenuItem("ScalesOfMemory/Create Run Config")]
    public static void CreateRunConfig()
    {
        EnsureFolders();

        var config = ScriptableObject.CreateInstance<RunConfig>();
        config.startHP = 80;
        config.startScales = 20;
        config.startGold = 0;

        // Starter deck
        string[] starterCards = { "Strike_1", "Strike_2", "Guard_1", "Guard_2",
            "Fireball", "IceShield", "LightningArrow", "NatureHeal", "DarkSlash", "ManaFocus" };
        config.starterDeck = new CardData[starterCards.Length];
        for (int i = 0; i < starterCards.Length; i++)
            config.starterDeck[i] = AssetDatabase.LoadAssetAtPath<CardData>(SOPath + "/Cards/" + starterCards[i] + ".asset");

        // All cards (same as starter for now)
        config.allCards = new CardData[starterCards.Length];
        System.Array.Copy(config.starterDeck, config.allCards, starterCards.Length);

        // Map generation
        config.rowsPerPhase = 5;
        config.minNodesPerRow = 2;
        config.maxNodesPerRow = 4;

        // Room weights
        config.battleWeight = 0.5f;
        config.eliteWeight = 0.15f;
        config.restWeight = 0.15f;
        config.shopWeight = 0.1f;
        config.eventWeight = 0.1f;

        // Enemy pools
        config.normalEnemies = new EnemyData[]
        {
            AssetDatabase.LoadAssetAtPath<EnemyData>(SOPath + "/Enemies/Goblin.asset"),
            AssetDatabase.LoadAssetAtPath<EnemyData>(SOPath + "/Enemies/Slime.asset"),
            AssetDatabase.LoadAssetAtPath<EnemyData>(SOPath + "/Enemies/Bat.asset"),
        };
        config.eliteEnemies = new EnemyData[]
        {
            AssetDatabase.LoadAssetAtPath<EnemyData>(SOPath + "/Enemies/Orc.asset"),
            AssetDatabase.LoadAssetAtPath<EnemyData>(SOPath + "/Enemies/Skeleton.asset"),
        };
        config.bossEnemies = new EnemyData[]
        {
            AssetDatabase.LoadAssetAtPath<EnemyData>(SOPath + "/Enemies/Orc.asset"),
        };

        AssetDatabase.CreateAsset(config, SOPath + "/RunConfig.asset");
        AssetDatabase.SaveAssets();
        Debug.Log("[ScalesOfMemory] RunConfig created at " + SOPath + "/RunConfig.asset");
    }

    static string SOPath = "Assets/ScalesOfMemory/ScriptableObjects";

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/ScalesOfMemory"))
            AssetDatabase.CreateFolder("Assets", "ScalesOfMemory");
        if (!AssetDatabase.IsValidFolder(SOPath))
            AssetDatabase.CreateFolder("Assets/ScalesOfMemory", "ScriptableObjects");
        if (!AssetDatabase.IsValidFolder(SOPath + "/Cards"))
            AssetDatabase.CreateFolder(SOPath, "Cards");
        if (!AssetDatabase.IsValidFolder(SOPath + "/Enemies"))
            AssetDatabase.CreateFolder(SOPath, "Enemies");
        if (!AssetDatabase.IsValidFolder(SOPath + "/StatusEffects"))
            AssetDatabase.CreateFolder(SOPath, "StatusEffects");
    }

    static StatusEffectData _burn, _freeze, _poison;

    static void CreateStatusEffects()
    {
        EnsureFolders();

        _burn = ScriptableObject.CreateInstance<StatusEffectData>();
        _burn.effectName = "Burn";
        _burn.type = StatusEffectType.Burn;
        _burn.damagePerTurn = 3;
        _burn.preventsAction = false;
        _burn.iconColor = new Color32(255, 100, 30, 255);
        AssetDatabase.CreateAsset(_burn, SOPath + "/StatusEffects/Burn.asset");

        _freeze = ScriptableObject.CreateInstance<StatusEffectData>();
        _freeze.effectName = "Freeze";
        _freeze.type = StatusEffectType.Freeze;
        _freeze.damagePerTurn = 0;
        _freeze.preventsAction = true;
        _freeze.iconColor = new Color32(100, 200, 255, 255);
        AssetDatabase.CreateAsset(_freeze, SOPath + "/StatusEffects/Freeze.asset");

        _poison = ScriptableObject.CreateInstance<StatusEffectData>();
        _poison.effectName = "Poison";
        _poison.type = StatusEffectType.Poison;
        _poison.damagePerTurn = 2;
        _poison.preventsAction = false;
        _poison.iconColor = new Color32(100, 200, 50, 255);
        AssetDatabase.CreateAsset(_poison, SOPath + "/StatusEffects/Poison.asset");
    }

    static void CreateCards()
    {
        EnsureFolders();

        // Strike x2
        CreateCard("Strike_1", "Strike", 1, ElementType.None, CardType.Attack, 5, 0, 0, 0, true, null, 0, 0);
        CreateCard("Strike_2", "Strike", 1, ElementType.None, CardType.Attack, 5, 0, 0, 0, true, null, 0, 0);

        // Guard x2
        CreateCard("Guard_1", "Guard", 1, ElementType.None, CardType.Defend, 0, 5, 0, 0, false, null, 0, 0);
        CreateCard("Guard_2", "Guard", 1, ElementType.None, CardType.Defend, 0, 5, 0, 0, false, null, 0, 0);

        // Fireball
        CreateCard("Fireball", "Fireball", 2, ElementType.Fire, CardType.Attack, 8, 0, 0, 0, true, _burn, 2, 3);

        // Ice Shield
        CreateCard("IceShield", "Ice Shield", 1, ElementType.Ice, CardType.Defend, 0, 6, 0, 0, false, null, 0, 0);

        // Lightning Arrow
        CreateCard("LightningArrow", "Lightning Arrow", 2, ElementType.Lightning, CardType.Attack, 10, 0, 0, 0, true, null, 0, 0);

        // Nature Heal
        CreateCard("NatureHeal", "Nature Heal", 2, ElementType.Nature, CardType.Skill, 0, 0, 5, 0, false, null, 0, 0);

        // Dark Slash
        CreateCard("DarkSlash", "Dark Slash", 1, ElementType.Dark, CardType.Attack, 4, 0, 0, 0, true, _poison, 3, 2);

        // Mana Focus
        CreateCard("ManaFocus", "Mana Focus", 0, ElementType.None, CardType.Skill, 0, 0, 0, 2, false, null, 0, 0);
    }

    static void CreateCard(string fileName, string cardName, int cost, ElementType element, CardType type,
        int damage, int block, int heal, int draw, bool needsTarget,
        StatusEffectData effect, int effectDur, int effectPot)
    {
        var card = ScriptableObject.CreateInstance<CardData>();
        card.cardName = cardName;
        card.manaCost = cost;
        card.element = element;
        card.cardType = type;
        card.baseDamage = damage;
        card.baseBlock = block;
        card.healAmount = heal;
        card.drawCount = draw;
        card.needsTarget = needsTarget;
        card.appliedEffect = effect;
        card.effectDuration = effectDur;
        card.effectPotency = effectPot;

        // Description
        string desc = "";
        if (damage > 0) desc += $"Deal {damage} damage. ";
        if (block > 0) desc += $"Gain {block} block. ";
        if (heal > 0) desc += $"Heal {heal} HP. ";
        if (draw > 0) desc += $"Draw {draw} cards. ";
        if (effect != null) desc += $"Apply {effect.effectName}({effectDur}t). ";
        card.description = desc.TrimEnd();

        AssetDatabase.CreateAsset(card, SOPath + "/Cards/" + fileName + ".asset");
    }

    static void CreateEnemies()
    {
        EnsureFolders();

        // Goblin (no element)
        var goblin = CreateEnemy("Goblin", "Goblin", 25, 8, 13, EnemyTrait.None,
            PrimitiveType.Sphere, new Color32(50, 180, 50, 255), Color.red, 0.8f);
        goblin.element = ElementType.None;
        goblin.intentPatterns = new IntentPattern[]
        {
            new IntentPattern { type = IntentType.Attack, weight = 3, hpThreshold = 1f, description = "Attack" },
            new IntentPattern { type = IntentType.Defend, weight = 1, hpThreshold = 0.5f, description = "Guard" }
        };
        EditorUtility.SetDirty(goblin);

        // MiniSlime (Nature)
        var miniSlime = CreateEnemy("MiniSlime", "Mini Slime", 8, 3, 5, EnemyTrait.None,
            PrimitiveType.Sphere, new Color32(50, 200, 200, 255), new Color32(200, 255, 255, 255), 0.4f);
        miniSlime.element = ElementType.Nature;
        miniSlime.intentPatterns = new IntentPattern[]
        {
            new IntentPattern { type = IntentType.Attack, weight = 1, hpThreshold = 1f, description = "Slap" }
        };
        EditorUtility.SetDirty(miniSlime);

        // Slime (Nature)
        var slime = CreateEnemy("Slime", "Slime", 17, 5, 8, EnemyTrait.Splitting,
            PrimitiveType.Sphere, new Color32(50, 200, 200, 255), new Color32(200, 255, 255, 255), 0.7f);
        slime.element = ElementType.Nature;
        slime.splitCount = 2;
        slime.splitInto = miniSlime;
        slime.intentPatterns = new IntentPattern[]
        {
            new IntentPattern { type = IntentType.Attack, weight = 1, hpThreshold = 1f, description = "Bounce" }
        };
        EditorUtility.SetDirty(slime);

        // Bat (Dark)
        var bat = CreateEnemy("Bat", "Bat", 13, 7, 10, EnemyTrait.Flying,
            PrimitiveType.Sphere, new Color32(80, 80, 100, 255), new Color32(255, 50, 50, 255), 0.5f);
        bat.element = ElementType.Dark;
        bat.intentPatterns = new IntentPattern[]
        {
            new IntentPattern { type = IntentType.Attack, weight = 1, hpThreshold = 1f, description = "Bite" }
        };
        EditorUtility.SetDirty(bat);

        // Orc (Fire)
        var orc = CreateEnemy("Orc", "Orc", 42, 13, 20, EnemyTrait.Charging,
            PrimitiveType.Cube, new Color32(140, 90, 50, 255), new Color32(255, 200, 50, 255), 1.2f);
        orc.element = ElementType.Fire;
        orc.intentPatterns = new IntentPattern[]
        {
            new IntentPattern { type = IntentType.Attack, weight = 2, hpThreshold = 1f, description = "Smash" },
            new IntentPattern { type = IntentType.Defend, weight = 1, hpThreshold = 0.5f, description = "Brace" }
        };
        EditorUtility.SetDirty(orc);

        // Skeleton (Dark)
        var skeleton = CreateEnemy("Skeleton", "Skeleton", 30, 10, 17, EnemyTrait.Summoning,
            PrimitiveType.Capsule, new Color32(220, 210, 190, 255), new Color32(100, 255, 100, 255), 0.9f);
        skeleton.element = ElementType.Dark;
        skeleton.intentPatterns = new IntentPattern[]
        {
            new IntentPattern { type = IntentType.Attack, weight = 2, hpThreshold = 1f, description = "Slash" },
            new IntentPattern { type = IntentType.Buff, weight = 1, hpThreshold = 0.4f, description = "Mend" }
        };
        EditorUtility.SetDirty(skeleton);
    }

    static EnemyData CreateEnemy(string fileName, string enemyName, int maxHP, int minAtk, int maxAtk,
        EnemyTrait trait, PrimitiveType shape, Color bodyColor, Color eyeColor, float scale)
    {
        var enemy = ScriptableObject.CreateInstance<EnemyData>();
        enemy.enemyName = enemyName;
        enemy.maxHP = maxHP;
        enemy.minAttack = minAtk;
        enemy.maxAttack = maxAtk;
        enemy.trait = trait;
        enemy.bodyShape = shape;
        enemy.bodyColor = bodyColor;
        enemy.eyeColor = eyeColor;
        enemy.bodyScale = scale;
        AssetDatabase.CreateAsset(enemy, SOPath + "/Enemies/" + fileName + ".asset");
        return enemy;
    }
}
#endif

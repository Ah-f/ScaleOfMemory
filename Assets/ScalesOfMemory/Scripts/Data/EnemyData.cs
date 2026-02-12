using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "ScalesOfMemory/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public ElementType element = ElementType.None;
    public int maxHP;
    public int minAttack;
    public int maxAttack;
    public EnemyTrait trait;

    [Header("Procedural 3D")]
    public PrimitiveType bodyShape = PrimitiveType.Sphere;
    public Color bodyColor = Color.gray;
    public Color eyeColor = Color.red;
    public float bodyScale = 1f;

    [Header("Intent Patterns")]
    public IntentPattern[] intentPatterns;

    [Header("Special")]
    public int splitCount;
    public EnemyData splitInto;
}

[System.Serializable]
public struct IntentPattern
{
    public IntentType type;
    public int weight;
    public int conditionTurnMod; // 0=always, N=every N turns
    public float hpThreshold;   // 1=always, 0.5=below 50% HP
    [TextArea(1, 2)]
    public string description;
}

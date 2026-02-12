using UnityEngine;

[CreateAssetMenu(fileName = "RunConfig", menuName = "ScalesOfMemory/RunConfig")]
public class RunConfig : ScriptableObject
{
    [Header("Player Start")]
    public int startHP = 80;
    public int startScales = 20;
    public int startGold = 0;
    public CardData[] starterDeck;
    public CardData[] allCards;

    [Header("Map Generation")]
    public int rowsPerPhase = 5;
    public int minNodesPerRow = 2;
    public int maxNodesPerRow = 4;

    [Header("Room Weights")]
    [Range(0, 1)] public float battleWeight = 0.5f;
    [Range(0, 1)] public float eliteWeight = 0.15f;
    [Range(0, 1)] public float restWeight = 0.15f;
    [Range(0, 1)] public float shopWeight = 0.1f;
    [Range(0, 1)] public float eventWeight = 0.1f;

    [Header("Enemy Pools")]
    public EnemyData[] normalEnemies;
    public EnemyData[] eliteEnemies;
    public EnemyData[] bossEnemies;
}

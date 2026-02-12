using System.Collections.Generic;

[System.Serializable]
public class MapNode
{
    public int id;
    public RoomType roomType;
    public int row;
    public int column;
    public List<int> connections = new List<int>();
    public bool visited;
    public bool cleared;
    public string[] enemyNames;
    public float xPosition;
    public float yPosition;
}

[System.Serializable]
public class RunState
{
    public int currentHP;
    public int maxHP;
    public int currentScales;
    public int maxScales;
    public int gold;

    public List<string> deckCardNames = new List<string>();

    public List<MapNode> mapNodes = new List<MapNode>();
    public int currentNodeId = -1;
    public int seed;
    public RunPhase currentPhase;

    public MapNode pendingBattleNode;
}

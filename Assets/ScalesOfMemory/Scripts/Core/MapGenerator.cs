using System.Collections.Generic;

public static class MapGenerator
{
    public static List<MapNode> Generate(int seed, RunConfig config)
    {
        var rng = new System.Random(seed);
        var nodes = new List<MapNode>();
        int nextId = 0;
        int totalRows = config.rowsPerPhase;
        var rowNodes = new List<List<MapNode>>();

        for (int r = 0; r < totalRows; r++)
        {
            int nodeCount = rng.Next(config.minNodesPerRow, config.maxNodesPerRow + 1);
            var row = new List<MapNode>();

            for (int c = 0; c < nodeCount; c++)
            {
                var node = new MapNode();
                node.id = nextId++;
                node.row = r;
                node.column = c;
                node.xPosition = (c + 0.5f) / nodeCount;
                node.yPosition = (r + 0.5f) / (totalRows + 1);

                if (r == 0)
                    node.roomType = RoomType.Battle;
                else
                    node.roomType = PickRoomType(rng, config);

                if (node.roomType == RoomType.Battle || node.roomType == RoomType.Elite)
                    node.enemyNames = PickEnemies(rng, node.roomType, config);

                row.Add(node);
                nodes.Add(node);
            }
            rowNodes.Add(row);
        }

        // Boss node
        var boss = new MapNode();
        boss.id = nextId++;
        boss.row = totalRows;
        boss.column = 0;
        boss.xPosition = 0.5f;
        boss.yPosition = (totalRows + 0.5f) / (totalRows + 1);
        boss.roomType = RoomType.Boss;
        boss.enemyNames = PickEnemies(rng, RoomType.Boss, config);
        nodes.Add(boss);
        rowNodes.Add(new List<MapNode> { boss });

        // Generate connections
        for (int r = 0; r < rowNodes.Count - 1; r++)
        {
            var currentRow = rowNodes[r];
            var nextRow = rowNodes[r + 1];
            var connected = new HashSet<int>();

            foreach (var node in currentRow)
            {
                int connectCount = rng.Next(1, 3);
                connectCount = System.Math.Min(connectCount, nextRow.Count);

                var targets = PickConnections(node, nextRow, connectCount);
                foreach (var t in targets)
                {
                    node.connections.Add(t.id);
                    connected.Add(t.id);
                }
            }

            // Ensure every next-row node has at least one parent
            foreach (var nextNode in nextRow)
            {
                if (connected.Contains(nextNode.id)) continue;
                MapNode nearest = currentRow[0];
                float minDist = System.Math.Abs(nearest.xPosition - nextNode.xPosition);
                foreach (var cn in currentRow)
                {
                    float dist = System.Math.Abs(cn.xPosition - nextNode.xPosition);
                    if (dist < minDist) { minDist = dist; nearest = cn; }
                }
                nearest.connections.Add(nextNode.id);
            }
        }

        return nodes;
    }

    static List<MapNode> PickConnections(MapNode from, List<MapNode> nextRow, int count)
    {
        var sorted = new List<MapNode>(nextRow);
        sorted.Sort((a, b) =>
            System.Math.Abs(a.xPosition - from.xPosition).CompareTo(
            System.Math.Abs(b.xPosition - from.xPosition)));

        var result = new List<MapNode>();
        for (int i = 0; i < count && i < sorted.Count; i++)
            result.Add(sorted[i]);
        return result;
    }

    static RoomType PickRoomType(System.Random rng, RunConfig config)
    {
        float roll = (float)rng.NextDouble();
        float cumulative = 0;

        cumulative += config.battleWeight;
        if (roll < cumulative) return RoomType.Battle;

        cumulative += config.eliteWeight;
        if (roll < cumulative) return RoomType.Elite;

        cumulative += config.restWeight;
        if (roll < cumulative) return RoomType.Rest;

        cumulative += config.shopWeight;
        if (roll < cumulative) return RoomType.Shop;

        return RoomType.Event;
    }

    static string[] PickEnemies(System.Random rng, RoomType type, RunConfig config)
    {
        switch (type)
        {
            case RoomType.Battle:
                int count = rng.Next(1, 3);
                return PickRandomNames(rng, config.normalEnemies, count);
            case RoomType.Elite:
                return PickRandomNames(rng, config.eliteEnemies, rng.Next(1, 3));
            case RoomType.Boss:
                if (config.bossEnemies != null && config.bossEnemies.Length > 0)
                    return PickRandomNames(rng, config.bossEnemies, 2);
                return PickRandomNames(rng, config.eliteEnemies, 2);
            default:
                return new string[0];
        }
    }

    static string[] PickRandomNames(System.Random rng, EnemyData[] pool, int count)
    {
        if (pool == null || pool.Length == 0) return new string[] { "Goblin" };
        var names = new string[count];
        for (int i = 0; i < count; i++)
            names[i] = pool[rng.Next(pool.Length)].name;
        return names;
    }
}

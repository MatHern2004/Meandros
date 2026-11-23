using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SpawnType { PlayerStart, Enemy, MiniBoss, Boss, Upgrade, Shop, Coin, Torch }

/// Pure data: "a spawn point of this type belongs in this cell".
public struct SpawnPointInfo
{
    public SpawnType Type;
    public Vector2Int Cell;
    public int WallDir; // torches only: which wall of the cell (0-3). -1 otherwise.

    public SpawnPointInfo(SpawnType type, Vector2Int cell, int wallDir = -1)
    {
        Type = type; Cell = cell; WallDir = wallDir;
    }
}

/// Decides WHERE spawn points go. Uses its own random stream (seed ^ 0x5EED), so changing
/// spawn settings never changes the maze layout for a given seed.
public static class MazeSpawnPlanner
{
    public static List<SpawnPointInfo> Plan(MazeData maze, MazeSettings s, int seed)
    {
        var rng = new System.Random(seed ^ 0x5EED);
        var result = new List<SpawnPointInfo>();
        var taken = new HashSet<Vector2Int>(); // cells that already hold a gameplay spawn

        void Add(SpawnType t, Vector2Int c)
        {
            result.Add(new SpawnPointInfo(t, c));
            taken.Add(c);
        }

        // Fixed points
        Add(SpawnType.PlayerStart, maze.Start);
        Add(SpawnType.Boss, maze.BossCell);
        foreach (var c in maze.AllCells())
        {
            if (c.Type == RoomType.Shop) Add(SpawnType.Shop, c.Pos);
            else if (c.Type == RoomType.Treasure) Add(SpawnType.Upgrade, c.Pos);
        }

        // Mini-bosses: far half of the maze, widely spaced (most constrained, so first)
        var far = maze.GetSpawnCandidates(Mathf.Max(s.minEnemySpawnDistance, maze.MaxDistance / 2));
        foreach (var c in PickSpaced(far, s.miniBossSpawnCount, s.enemyMinSpacing * 2, taken, rng))
            Add(SpawnType.MiniBoss, c);

        // Regular enemies
        var enemyPool = maze.GetSpawnCandidates(s.minEnemySpawnDistance);
        foreach (var c in PickSpaced(enemyPool, s.enemySpawnCount, s.enemyMinSpacing, taken, rng))
            Add(SpawnType.Enemy, c);

        // Extra upgrade spots in dead ends
        var deadEnds = maze.AllCells().Where(c => c.Type == RoomType.Normal && c.IsDeadEnd)
                                      .Select(c => c.Pos).ToList();
        foreach (var c in PickSpaced(deadEnds, s.extraUpgradeSpots, 3, taken, rng))
            Add(SpawnType.Upgrade, c);

        // Coins anywhere normal
        var coinPool = maze.AllCells().Where(c => c.Type == RoomType.Normal).Select(c => c.Pos).ToList();
        foreach (var c in PickSpaced(coinPool, s.coinSpawnCount, 2, taken, rng))
            Add(SpawnType.Coin, c);

        // Torches: special rooms always get one, other cells by chance with spacing
        var torchCells = new List<Vector2Int>();
        var cells = maze.AllCells().ToList();
        Shuffle(cells, rng);
        foreach (var c in cells.OrderBy(c => c.Type == RoomType.Normal ? 1 : 0)) // stable: specials first
        {
            bool forced = c.Type != RoomType.Normal;
            if (!forced)
            {
                if (rng.NextDouble() > s.torchChance) continue;
                if (TooClose(c.Pos, torchCells, s.torchMinSpacing)) continue;
            }
            var walls = new List<int>();
            for (int d = 0; d < 4; d++) if (maze.HasWall(c.Pos, d)) walls.Add(d);
            if (walls.Count == 0) continue;

            result.Add(new SpawnPointInfo(SpawnType.Torch, c.Pos, walls[rng.Next(walls.Count)]));
            torchCells.Add(c.Pos);
        }

        return result;
    }

    /// Picks up to 'count' cells from the pool that are 'spacing' apart. If it cannot find enough,
    /// it halves the spacing and tries again, down to "any free cell".
    static List<Vector2Int> PickSpaced(List<Vector2Int> pool, int count, int spacing,
                                       HashSet<Vector2Int> taken, System.Random rng)
    {
        var result = new List<Vector2Int>();
        var candidates = pool.Where(p => !taken.Contains(p)).ToList();
        Shuffle(candidates, rng);

        for (int sp = Mathf.Max(spacing, 1); sp >= 1 && result.Count < count; sp = sp > 1 ? sp / 2 : 0)
        {
            foreach (var c in candidates)
            {
                if (result.Count >= count) break;
                if (result.Contains(c) || TooClose(c, result, sp)) continue;
                result.Add(c);
            }
        }
        return result;
    }

    static bool TooClose(Vector2Int c, List<Vector2Int> others, int minDistance)
    {
        foreach (var o in others)
            if (Mathf.Abs(c.x - o.x) + Mathf.Abs(c.y - o.y) < minDistance) return true;
        return false;
    }

    static void Shuffle<T>(IList<T> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            T tmp = list[i]; list[i] = list[j]; list[j] = tmp;
        }
    }
}
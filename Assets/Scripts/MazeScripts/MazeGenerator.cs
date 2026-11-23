using UnityEngine;
using System.Collections.Generic;
using System.Linq;

///  Pipeline for the generation:
/// 1. Carve out a perfect maze, using a recursive backtracker with long, winding corridors (we can use a different algorithm if we want, but this is a good start)
/// 2. Braid it by remove some dead ends to create loops
/// 3. Calculate BFS distances from the start
/// 4. Place the special rooms, including the boss, shop, and treasure rooms, using distance and dead end rules

public static class MazeGenerator
{
    public static MazeData Generate(MazeSettings s, int seed)
    {
        var rng = new System.Random(seed);
        var maze = new MazeData(s.width, s.height);

        maze.Start = new Vector2Int(0, 0);
        CarvePassages(maze, rng, maze.Start);
        Braid(maze, rng, s.braidChance);
        maze.ComputeDistances(maze.Start);
        PlaceSpecialRooms(maze, rng, s);

        return maze;
    }

    // First Step
    static void CarvePassages(MazeData maze, System.Random rng, Vector2Int start)
    {
        var stack = new Stack<MazeCell>();
        var first = maze.Get(start);
        first.Visited = true;
        stack.Push(first);
        int[] dirs = {0, 1, 2, 3};

        while (stack.Count > 0)
        {
            var current = stack.Peek();
            Shuffle(dirs, rng);
            bool carved = false;
            foreach (int d in dirs)
            {
                Vector2Int nextPos = current.Pos + Dir.Vec[d];
                if (!maze.InBounds(nextPos)) continue;
                var nextCell = maze.Get(nextPos);
                if (nextCell.Visited) continue;

                // Carve out the passages
                maze.RemoveWall(current.Pos, d);
                nextCell.Visited = true;
                stack.Push(nextCell);
                carved = true;
                break;
            }
            if (!carved) stack.Pop();
        }
    }

    // Second Step
    static void Braid(MazeData maze, System.Random rng, float chance)
    {
        if (chance <= 0f) return;
        var deadEnds = maze.AllCells().Where(c => c.IsDeadEnd).ToList();
        foreach (var cell in deadEnds)
        {
            if (!cell.IsDeadEnd) continue; // this is a check if a neighbor's braid has already fixed it
            if (rng.NextDouble() > chance) continue;

            var options = new List<int>();
            var preferred = new List<int>();
            for (int d = 0; d < 4; d++)
            {
                if (!maze.HasWall(cell.Pos, d)) continue;
                Vector2Int nPos = cell.Pos + Dir.Vec[d];
                if (!maze.InBounds(nPos)) continue;
                options.Add(d);
                if (maze.Get(nPos).IsDeadEnd) preferred.Add(d);
            }
            if (options.Count == 0) continue;
            var pool = preferred.Count > 0 ? preferred : options;
            maze.RemoveWall(cell.Pos, pool[rng.Next(pool.Count)]);
        }
    }

    // Fourth step
    static void PlaceSpecialRooms(MazeData maze, System.Random rng, MazeSettings s)
    {
        var placed = new List<MazeCell>();

        var startCell = maze.Get(maze.Start);
        startCell.Type = RoomType.Start;
        placed.Add(startCell);

        var others = maze.AllCells().Where(c => c != startCell).ToList();

        // For the boss, the farthest dead end, or the farthest cell if braiding removed them all
        var boss = others.OrderBy(c => c.IsDeadEnd ? 0 : 1).ThenByDescending(c => c.DistanceFromStart).First();
        boss.Type = RoomType.Boss;
        maze.BossCell = boss.Pos;
        placed.Add(boss);

        // For the shop room, something middle distance, with dead ends being preferred
        if (s.hasShop)
        {
            var band = others.Where(
                c => c.Type == RoomType.Normal && c.DistanceFromStart >= maze.MaxDistance * 0.3f && c.DistanceFromStart <= maze.MaxDistance * 0.7f)
                .OrderBy(c => c.IsDeadEnd ? 0 : 1)
                .ThenBy(_ => rng.Next());

            var shop = TryPick(band, placed, s.minSpecialSpacing) ?? TryPick(band, placed, 1);
            if (shop != null) { 
                shop.Type = RoomType.Shop; 
                placed.Add(shop); }
        }

        // For the treasure room, it picks dead ends first, but shuffled so that they're spread around
        for (int i = 0; i < s.treasureRooms; i++)
        {
            var candidates = others.Where(c => c.Type == RoomType.Normal).OrderBy(c => c.IsDeadEnd ? 0 : 1).ThenBy(_ => rng.Next());
            var treasure = TryPick(candidates, placed, s.minSpecialSpacing) ?? TryPick(candidates, placed, 1);
            if (treasure == null) break;
            treasure.Type = RoomType.Treasure;
            placed.Add(treasure);
        }


    } 

    static MazeCell TryPick(IEnumerable<MazeCell> candidates, List<MazeCell> placed, int minSpacing)
    {
        foreach (var c in candidates)
        {
            if (c.Type != RoomType.Normal) continue;
            bool ok = true;
            foreach (var p in placed)
            {
                int manhattan = Mathf.Abs(c.Pos.x - p.Pos.x) + Mathf.Abs(c.Pos.y - p.Pos.y);
                if (manhattan < minSpacing) {
                    ok = false; 
                    break; 
                }
            }
            if (ok) return c;
        }
        return null;
    }

    static void Shuffle(int[] a, System.Random rng)
    {
        for (int i = a.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (a[i], a[j]) = (a[j], a[i]);
        }
    }
}

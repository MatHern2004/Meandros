using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum Walls: byte {None = 0, North = 1, East = 2, South = 4, West = 8, All = 15}

public enum RoomType {Normal, Start, Treasure, Boss, Shop, Heal}

/// Direction helpers, index order is N, E, S, W
/// Grid +Y maps to world +Z, which is north, grid +X maps to world +X (east)

public static class Dir {
    public static readonly Vector2Int[] Vec = {
        new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0)
    };
    public static readonly Walls[] Flag = { Walls.North, Walls.East, Walls.South, Walls.West };
    public static int Opposite(int d) => (d + 2) % 4;
}


public class MazeCell {
    
    public Vector2Int Pos;
    public Walls Walls = Walls.All;
    public RoomType Type = RoomType.Normal;
    public int DistanceFromStart = -1;
    public bool Visited; // only used during the carving

    public int OpenCount {
        get {
            int open = 0;
            for (int i = 0; i < 4; i++) if ((Walls & Dir.Flag[i]) == 0) open++;
            return open;
        }
    }
    public bool IsDeadEnd => OpenCount == 1;

}

/// Pure data , so it's easier to hand to A*, enemy spawners, UI minimap, etc.

public class MazeData {
    public readonly int Width, Height;
    public readonly MazeCell[,] Cells;
    public Vector2Int Start;
    public Vector2Int BossCell;
    public int MaxDistance;

    public MazeData(int width, int height) {
        Width = width; Height = height;
        Cells = new MazeCell[width, height];
        for (int x = 0; x < width; x++) 
        {
            for (int y = 0; y < height; y++) 
            {
                Cells[x, y] = new MazeCell { Pos = new Vector2Int(x, y) };
            }
        }
    }

    public bool InBounds(Vector2Int p) => p.x >= 0 && p.x < Width && p.y >= 0 && p.y < Height;
    public MazeCell Get(Vector2Int p) => Cells[p.x, p.y];

    public IEnumerable<MazeCell> AllCells() {
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                yield return Cells[x, y];
            }
        }
    }

    public bool HasWall(Vector2Int p, int dir) =>  (Get(p).Walls & Dir.Flag[dir]) != 0;

    // Open up the wall between p and its neighbor in direction dir (both sides)
    public void RemoveWall(Vector2Int p, int dir) {
        Vector2Int n = p + Dir.Vec[dir];
        Get(p).Walls &= ~Dir.Flag[dir];
        Get(n).Walls &= ~Dir.Flag[Dir.Opposite(dir)];
    }

    // Neighbors that you can walk to directly, used for A* graph
    public IEnumerable<Vector2Int> OpenNeighbors(Vector2Int p) {
        for (int d = 0; d < 4; d++) {
            if (HasWall(p, d)) continue;
            Vector2Int n = p + Dir.Vec[d];
            if (InBounds(n)) yield return n;
        }
    }

    // BFS from the origin; fills DistanceFromStart on every cell
    public void ComputeDistances(Vector2Int origin) {
        foreach (var c in AllCells()) c.DistanceFromStart = -1;
        var queue = new Queue<Vector2Int>();
        Get(origin).DistanceFromStart = 0;
        queue.Enqueue(origin);
        MaxDistance = 0;
        while (queue.Count > 0) {
            var cur = queue.Dequeue();
            int dist = Get(cur).DistanceFromStart;
            foreach (var n in OpenNeighbors(cur)) {
                var nc = Get(n);
                if (nc.DistanceFromStart >= 0) continue;
                nc.DistanceFromStart = dist + 1;
                MaxDistance = Mathf.Max(MaxDistance, dist + 1);
                queue.Enqueue(n);
            }
        }
    }

    // The shortest path between two cells, inclusive. This is helpful for the hint system if the player is stuck
    public List<Vector2Int> FindPath(Vector2Int from, Vector2Int to) {
        var parent = new Dictionary<Vector2Int, Vector2Int>();
        var queue = new Queue<Vector2Int>();
        parent[from] = from;
        queue.Enqueue(from);
        while (queue.Count > 0) {
            var cur = queue.Dequeue();
            if (cur == to) break;
            foreach (var n in OpenNeighbors(cur)) {
                if (parent.ContainsKey(n)) continue;
                parent[n] = cur;
                queue.Enqueue(n);
            }
        }
        var path = new List<Vector2Int>();
        if (!parent.ContainsKey(to)) return path; 
        for (var cur = to; cur != from; cur = parent[cur]) path.Add(cur);
        path.Add(from);
        path.Reverse();
        return path;
    }

    // Normal cells that are far enough from the start, for enemy spawning
    public List<Vector2Int> GetSpawnCandidates(int minDistanceFromStart) {
        var list = new List<Vector2Int>();
        foreach (var c in AllCells()) {
            if (c.Type == RoomType.Normal && c.DistanceFromStart >= minDistanceFromStart) list.Add(c.Pos);
        }
        return list;
    }

    public string ToAscii()
    { var sb = new System.Text.StringBuilder();
        for (int y = Height - 1; y >= 0; y--) // north at the top
        {
        for (int x = 0; x < Width; x++)
            sb.Append(HasWall(new Vector2Int(x, y), 0) ? "+--" : "+  ");
        sb.Append("+\n");
        for (int x = 0; x < Width; x++)
            {
            var c = Cells[x, y];
            sb.Append(HasWall(c.Pos, 3) ? "|" : " ");
            sb.Append(c.Type == RoomType.Start ? "S" : c.Type == RoomType.Boss ? "B" :
                      c.Type == RoomType.Shop ? "$" : c.Type == RoomType.Treasure ? "T" :
                      c.Type == RoomType.Heal ? "H" : ".");
            sb.Append(" ");
            }
        sb.Append("|\n");
        }
    for (int x = 0; x < Width; x++) sb.Append("+--");
    sb.Append("+\n");
    return sb.ToString();
    }
}
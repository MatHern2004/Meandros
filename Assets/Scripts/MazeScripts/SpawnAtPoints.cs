using System.Collections.Generic;
using UnityEngine;

/// Drop-in helper for teammates (no code needed): pick a spawn type, assign a prefab, assign the maze.
/// Re-spawns automatically whenever the maze is regenerated.
public class SpawnAtPoints : MonoBehaviour
{
    [SerializeField] MazeBuilder maze;
    [SerializeField] SpawnType spawnType = SpawnType.Enemy;
    [SerializeField] GameObject prefab;
    [Tooltip("0 = one per spawn point.")]
    [SerializeField] int maxCount = 0;

    readonly List<GameObject> spawned = new List<GameObject>();

    void OnEnable()
    {
        if (maze == null) return;
        maze.MazeBuilt += Spawn;
        if (maze.IsBuilt) Spawn(maze);
    }

    void OnDisable()
    {
        if (maze != null) maze.MazeBuilt -= Spawn;
    }

    void Spawn(MazeBuilder m)
    {
        foreach (var g in spawned) if (g != null) Destroy(g);
        spawned.Clear();
        if (prefab == null) return;

        var points = m.GetSpawnPoints(spawnType);
        int n = maxCount > 0 ? Mathf.Min(maxCount, points.Count) : points.Count;
        for (int i = 0; i < n; i++)
            spawned.Add(Instantiate(prefab, points[i].transform.position, points[i].transform.rotation));
    }
}
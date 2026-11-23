using UnityEngine;

/// One asset per floor: Assets > Create > Knossos > Maze Settings.
[CreateAssetMenu(menuName = "Knossos/Maze Settings", fileName = "Floor1Settings")]
public class MazeSettings : ScriptableObject
{
    [Header("Size")]
    [Min(5)] public int width = 15;
    [Min(5)] public int height = 15;

    [Header("Layout")]
    [Tooltip("0 = perfect maze (one route, many dead ends). Higher = more loops, fewer dead ends.")]
    [Range(0f, 1f)] public float braidChance = 0.25f;

    [Header("Special rooms")]
    public int treasureRooms = 2;
    public bool hasShop = true;
    [Tooltip("Minimum grid (Manhattan) distance between special rooms.")]
    public int minSpecialSpacing = 4;

    [Header("Spawn points: enemies")]
    [Tooltip("Enemies only spawn at least this many steps from the start.")]
    public int minEnemySpawnDistance = 8;
    public int enemySpawnCount = 8;
    public int miniBossSpawnCount = 2;
    [Tooltip("Minimum grid distance between enemy spawn points (relaxed automatically if there is no room).")]
    public int enemyMinSpacing = 4;

    [Header("Spawn points: pickups")]
    [Tooltip("Extra upgrade spots in dead ends, on top of one per treasure room.")]
    public int extraUpgradeSpots = 2;
    public int coinSpawnCount = 15;

    [Header("Torches")]
    [Range(0f, 1f)] public float torchChance = 0.3f;
    public int torchMinSpacing = 2;

    [Header("Seed")]
    public bool useRandomSeed = true;
    public int seed = 12345;
}
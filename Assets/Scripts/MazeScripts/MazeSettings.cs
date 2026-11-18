using UnityEngine;

// One asset per floor, Assets -> Create -> Knossos (or whatever we name or project) -> Maze Settings
// Create Floor1, ... (we will most likely only do 2 floors for demo), then swam them in to change the difficulty
[CreateAssetMenu(menuName = "Knossos/Maze Settings", fileName = "FLoor1Settings")]
public class MazeSettings : ScriptableObject
{
    [Header("Size")]
    [Min(5)] public int width = 20;
    [Min(5)] public int height = 20;

    [Header("Layout")]
    [Tooltip("0 = perface maze (one route, many dead ends). Higher = more loops, fewer dead ends. Loops will matter for the enemy chases.")]
    [Range(0f, 1f)] public float braidChance = 0.25f;

    [Header("Special Rooms")]
    public int treasureRooms = 3;
    public bool hasShop = true;
    [Tooltip("Minimum grid (manhattan) distance between the special rooms")]
    public int minSpecialSpacing = 5;

    [Header("Enemies")]
    [Tooltip("Enemies will only spawn at least this many steps from the start")]
    public int minEnemySpawnDistance = 10;

    [Header("Seed")]
    public bool useRandomSeed = true;
    public int seed = 12345;

}
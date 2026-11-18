using UnityEngine;
// This turns MazeData into GameObjects. It works with no art, if no prefarbs get assigned to it
// It will build the maze from colored cubes so that we can test the maze out right away
// If used, the prefabs should be 1x1x1 cubes, with the pivot at the bottom center, so that they can be scaled to fit the maze cell size
public class MazeBuilder : MonoBehaviour
{
    [SerializeField] MazeSettings settings;

    [Header("Dimensions for World Units")]
    [SerializeField] float cellSize = 4f;
    [SerializeField] float wallHeight = 3f;
    [SerializeField] float wallThickness = 0.4f;
    [SerializeField]bool buildCeiling = false;

    [Header("Optional art, Cubes used if empty")]
    [SerializeField] GameObject floorPrefab;
    [SerializeField] GameObject wallPrefab;
    [SerializeField] Material floorMaterial;
    [SerializeField] Material wallMaterial;

    [Header("Special room lights")]
    [SerializeField] bool spawnRoomLights = true;
    [SerializeField] float roomLightIntensity = 2f;

    public MazeData Data {get; private set;}
    public int CurrentSeed {get; private set;}

    public Vector3 StartWorldPosition => CellToWorld(Data.Start) + Vector3.up * 1f;

    void Start() {Generate();}
    
    public void Generate()
    {
        Generate(settings.useRandomSeed ? Random.Range(int.MinValue, int.MaxValue): settings.seed);
    }

    public void Generate(int seed)
    {
        Clear();
        CurrentSeed = seed;
        Data = MazeGenerator.Generate(settings, seed);
        Build();
        Debug.Log($"Maze generated. Seed: {seed}, size {Data.Width}x{Data.Height}");
    }

    public Vector3 CellToWorld(Vector2Int c) => 
        transform.position + new Vector3(c.x * cellSize, 0f, c.y * cellSize);

    public Vector2Int WorldToCell (Vector3 w)
    {
        Vector3 local = w - transform.position;
        return new Vector2Int(Mathf.RoundToInt(local.x / cellSize), Mathf.RoundToInt(local.z / cellSize));
    }

    void Build()
    {
       foreach (var cell in Data.AllCells())
        {
            Vector3 center = CellToWorld(cell.Pos);

            // For the floor
            var floor = Spawn(floorPrefab, PrimitiveType.Cube, "Floor", center + Vector3.down * 0.1f, new Vector3(cellSize, 0.2f, cellSize), floorMaterial);
            Tint(floor, RoomColor(cell.Type));
            
            // For the ceiling
            if (buildCeiling)
            {
                Spawn(floorPrefab, PrimitiveType.Cube, "Ceiling", center + Vector3.up * (wallHeight + 0.1f), new Vector3(cellSize, 0.2f, cellSize), floorMaterial);
            }
            
            // Every shared wall is only built once, with North/East for every cell and South/West for the outer edges
            if (cell.Walls.HasFlag(Walls.North)) BuildWall(center, 0);
            if (cell.Walls.HasFlag(Walls.East)) BuildWall(center, 1);
            if (cell.Pos.y == 0 && cell.Walls.HasFlag(Walls.South)) BuildWall(center, 2);
            if (cell.Pos.x == 0 && cell.Walls.HasFlag(Walls.West)) BuildWall(center, 3);

            if (spawnRoomLights && cell.Type != RoomType.Normal)
            {
                SpawnRoomLight(center, RoomColor(cell.Type));
            }
        }
    }

    void BuildWall(Vector3 cellCenter, int dir)
    {
        Vector2Int v = Dir.Vec[dir];
        Vector3 pos = cellCenter + new Vector3(v.x, 0f, v.y) * (cellSize * 0.5f) + Vector3.up * (wallHeight * 0.5f);
        bool alongX = dir == 0 || dir == 2;
        Vector3 scale = alongX ? new Vector3(cellSize + wallThickness, wallHeight, wallThickness) : new Vector3(wallThickness, wallHeight, cellSize + wallThickness);
        Spawn(wallPrefab, PrimitiveType.Cube, "Wall", pos, scale, wallMaterial);
    }

    void SpawnRoomLight(Vector3 center, Color color)
    {
        var go = new GameObject("RoomLight");
        go.transform.SetParent(transform);
        go.transform.position = center + Vector3.up * (wallHeight - 0.5f);
        var l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = color;
        l.range = cellSize * 3f;
        l.intensity = roomLightIntensity;
    }

    GameObject Spawn(GameObject prefab, PrimitiveType fallback, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject go = prefab != null ? Instantiate(prefab) : GameObject.CreatePrimitive(fallback);
        go.name = name;
        go.transform.SetParent(transform);
        go.transform.position = pos;
        go.transform.localScale = scale;
        if (mat != null)
        {
            var r = go.GetComponentInChildren<Renderer>();
            if (r != null) r.sharedMaterial = mat;
        }
        return go;
    }

    static void Tint(GameObject go, Color c)
    {
        if (c == Color.clear) return;
        var r = go.GetComponentInChildren<Renderer>();
        if (r != null) r.material.color = c; // this will make a per object material instance
    }

    static Color RoomColor(RoomType t)
    {
        switch(t)
        {// Just the starting colors I randomly picked, change if we want to?
            case RoomType.Start: return new Color(0.3f, 0.8f, 0.3f);
            case RoomType.Treasure: return new Color(1f, 0.8f, 0.2f);
            case RoomType.Shop: return new Color(0.3f, 0.7f, 1f);
            case RoomType.Boss: return new Color(0.9f, 0.15f, 0.15f);
            default: return Color.clear;
        }
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child); else DestroyImmediate(child);
        }
    }

}

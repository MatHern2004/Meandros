using System.Collections.Generic;
using UnityEngine;

/// Builds the maze from MazeData using modular pieces, or just someplain cubes if none get assigned),
/// It'll then create spawn point objects for the rest of the students to use.
///
/// Wall/pillar prefabs are auto-measured: the builder recenters them, aligns the wall's long side
/// along its edge, and adds a collider if the prefab has none. So pivot/orientation quirks of an
/// asset pack mostly do not matter. Keep this object's Transform at position anything, rotation 0, scale 1.
public class MazeBuilder : MonoBehaviour
{
    [SerializeField] MazeSettings settings;

    [Header("Dimensions (world units)")]
    [SerializeField] float cellSize = 4f;
    [SerializeField] bool buildCeiling = false;
    [Tooltip("Used only when no wall prefab is assigned.")]
    [SerializeField] float fallbackWallHeight = 3f;
    [SerializeField] float fallbackWallThickness = 0.4f;

    [Header("Pack pieces (leave empty to use plain cubes)")]
    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject pillarPrefab; // hides seams where walls meet
    [SerializeField] GameObject floorPrefab;
    [SerializeField] GameObject[] floorDebrisPrefabs; // Drag the prefabs from the pack here
    [Range(0f, 1f)]
    [SerializeField] float debrisSpawnChance = 0.35f;
    [SerializeField] GameObject torchPrefab;

    [Header("Materials (assign URP/Lit materials)")]
    [SerializeField] Material floorMaterial;
    [SerializeField] Material wallMaterial;     // cube fallback only

    [Header("Torches")]
    [SerializeField] float torchHeight = 1.8f;
    [Tooltip("Rotate the torch model if it faces the wrong way (try 90, 180, 270).")]
    [SerializeField] float torchYawOffset = 0f;
    [SerializeField] bool addTorchLight = true;
    [SerializeField] float torchLightRange = 7f;
    [SerializeField] float torchLightIntensity = 2f;
    [SerializeField] Color torchLightColor = new Color(1f, 0.6f, 0.25f);

    [Header("Special room lights")]
    [SerializeField] bool spawnRoomLights = true;
    [SerializeField] float roomLightIntensity = 2f;

    [Header("Special room markers")]
    [Tooltip("Glowing frame on the floor of special rooms. Emission ignores how dark the room is.")]
    [SerializeField] bool showRoomMarkers = true;
    [Tooltip("Raise this if fog or dark lighting hides the marker.")]
    [SerializeField] float markerGlow = 3f;
    [Tooltip("Colored boxes around special rooms in the Scene view only (never shown in the game).")]
    [SerializeField] bool drawRoomGizmos = true;

    [Header("Room dressing, props per special room type")]
    [SerializeField] List<RoomDressing> roomDressings = new List<RoomDressing>();

    [Header("Spawn points")]
    [SerializeField] float playerSpawnHeight = 1f;
    [SerializeField] Transform playerToSpawn;

    public MazeData Data { get; private set; }
    public int CurrentSeed { get; private set; }
    public bool IsBuilt { get; private set; }

    /// Fires after the maze AND all spawn points exist (also fires again on every regenerate).
    public event System.Action<MazeBuilder> MazeBuilt;

    class Fitted
    {
        public GameObject Prefab;
        public Quaternion Rot = Quaternion.identity;
        public Vector3 Offset;   // local position that centers the model on the wrapper, bottom at y = 0
        public Vector3 Size;     // size after alignment (x = length, z = thickness for walls)
        public bool HasCollider;
    }

    Fitted wallFit, pillarFit, floorFit;
    readonly Dictionary<GameObject, Fitted> fitCache = new Dictionary<GameObject, Fitted>();
    float wallHeight, wallThickness;
    readonly Dictionary<SpawnType, List<MazeSpawnPoint>> pointsByType = new Dictionary<SpawnType, List<MazeSpawnPoint>>();

    void Start() { Generate(); }

    [ContextMenu("Generate")]
    public void Generate()
    {
        Generate(settings.useRandomSeed ? Random.Range(int.MinValue, int.MaxValue) : settings.seed);
    }

    public void Generate(int seed)
    {
        Clear();
        CurrentSeed = seed;
        Data = MazeGenerator.Generate(settings, seed);
        Debug.Log(Data.ToAscii());

        MeasurePieces();
        Build();
        CreateSpawnPoints(MazeSpawnPlanner.Plan(Data, settings, seed));
        SpawnPlayer();

        IsBuilt = true;
        MazeBuilt?.Invoke(this);
        Debug.Log($"Maze generated. Seed {seed}, {Data.Width}x{Data.Height}, {CountPoints()} spawn points.");
    }

    // ===== Public API for teammates

    public Vector3 CellToWorld(Vector2Int c) =>
        transform.position + new Vector3(c.x * cellSize, 0f, c.y * cellSize);

    public Vector2Int WorldToCell(Vector3 w)
    {
        Vector3 local = w - transform.position;
        return new Vector2Int(Mathf.RoundToInt(local.x / cellSize), Mathf.RoundToInt(local.z / cellSize));
    }

    public IReadOnlyList<MazeSpawnPoint> GetSpawnPoints(SpawnType type)
    {
        List<MazeSpawnPoint> list;
        if (pointsByType.TryGetValue(type, out list)) return list;
        return System.Array.Empty<MazeSpawnPoint>();
    }

    public MazeSpawnPoint GetFirst(SpawnType type)
    {
        var list = GetSpawnPoints(type);
        return list.Count > 0 ? list[0] : null;
    }

    /// Runs the callback now if the maze is ready, otherwise once when it is.
    public void RunWhenBuilt(System.Action<MazeBuilder> callback)
    {
        if (IsBuilt) { callback(this); return; }
        System.Action<MazeBuilder> handler = null;
        handler = m => { MazeBuilt -= handler; callback(m); };
        MazeBuilt += handler;
    }

    // ===== Measuring pieces

    void MeasurePieces()
    {
        wallFit = wallPrefab != null ? Measure(wallPrefab, true) : null;
        pillarFit = pillarPrefab != null ? Measure(pillarPrefab, false) : null;
        floorFit = floorPrefab != null ? Measure(floorPrefab, false) : null;
        wallHeight = wallFit != null ? wallFit.Size.y : fallbackWallHeight;
        wallThickness = wallFit != null ? wallFit.Size.z : fallbackWallThickness;

        if (wallFit != null)
        {
            int n = Mathf.Max(1, Mathf.RoundToInt(cellSize / wallFit.Size.x));
            Debug.Log($"Wall piece: length {wallFit.Size.x:F2}, height {wallFit.Size.y:F2}, thickness {wallFit.Size.z:F2}. " +
                      $"Cell size {cellSize} -> {n} piece(s) per edge, stretched x{cellSize / (n * wallFit.Size.x):F2}. " +
                      "Set cell size to a multiple of the piece length for no stretching.");
        }
    }

    Fitted Measure(GameObject prefab, bool alignLongSideToX)
    {
        var tmp = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        var renderers = tmp.GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length == 0)
        {
            DestroyImmediate(tmp);
            Debug.LogWarning($"{prefab.name} has no MeshRenderer, using cubes instead.");
            return null;
        }
        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        DestroyImmediate(tmp);

        var f = new Fitted { Prefab = prefab, HasCollider = prefab.GetComponentInChildren<Collider>(true) != null };
        Vector3 size = b.size, center = b.center;
        if (alignLongSideToX && size.z > size.x)
        {
            f.Rot = Quaternion.Euler(0f, 90f, 0f);
            center = f.Rot * center;
            size = new Vector3(size.z, size.y, size.x);
        }
        f.Size = size;
        f.Offset = new Vector3(-center.x, -(center.y - size.y * 0.5f), -center.z);
        return f;
    }

    GameObject PlaceFitted(Fitted f, string name, Vector3 pos, float yaw, Vector3 scale)
    {
        var wrapper = new GameObject(name);
        wrapper.transform.SetParent(transform);
        wrapper.transform.position = pos;
        wrapper.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        wrapper.transform.localScale = scale;

        var inst = Instantiate(f.Prefab, wrapper.transform);
        inst.transform.localPosition = f.Offset;
        inst.transform.localRotation = f.Rot;

        if (!f.HasCollider)
        {
            var box = wrapper.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, f.Size.y * 0.5f, 0f);
            box.size = f.Size;
        }
        return wrapper;
    }

    // ===== Building geometry

    void Build()
    {
        var rng = new System.Random(CurrentSeed);

        foreach (var cell in Data.AllCells())
        {
            Vector3 center = CellToWorld(cell.Pos);
            GameObject floor;

            if (floorFit != null)
            {
                // Scales the modular floor tiles to match cellSize, and rotate randomly 90 degrees (0, 90, 180, 270)
                float yaw = rng.Next(4) * 90f;
                Vector3 scale = new Vector3(cellSize / floorFit.Size.x, 1f, cellSize / floorFit.Size.z);
                floor = PlaceFitted(floorFit, "Floor", center, yaw, scale);
            } else
            {
                // If there is no prefab, default to cubes
                floor = Cube("Floor", center + Vector3.down * 0.1f, new Vector3(cellSize, 0.2f, cellSize), floorMaterial);
            }
            Tint(floor, RoomColor(cell.Type));
            if (showRoomMarkers && cell.Type != RoomType.Normal) BuildRoomMarker(center, cell.Type, floor);

            if (cell.Type == RoomType.Normal && floorDebrisPrefabs != null && floorDebrisPrefabs.Length > 0) {DecorateFloor(center, rng);}

            if (buildCeiling)
                Cube("Ceiling", center + Vector3.up * (wallHeight + 0.1f), new Vector3(cellSize, 0.2f, cellSize), floorMaterial);

            // Each shared wall is built once: North/East always, South/West only on the outer edge.
            if (cell.Walls.HasFlag(Walls.North)) BuildWall(center, 0);
            if (cell.Walls.HasFlag(Walls.East)) BuildWall(center, 1);
            if (cell.Pos.y == 0 && cell.Walls.HasFlag(Walls.South)) BuildWall(center, 2);
            if (cell.Pos.x == 0 && cell.Walls.HasFlag(Walls.West)) BuildWall(center, 3);

            if (spawnRoomLights && cell.Type != RoomType.Normal)
                AddLight(transform, center + Vector3.up * (wallHeight - 0.5f), RoomColor(cell.Type),
                         cellSize * 2.5f, roomLightIntensity, "RoomLight");
        }

        if (pillarFit != null) BuildPillars();
    }

    void BuildWall(Vector3 cellCenter, int dir)
    {
        Vector2Int v = Dir.Vec[dir];
        Vector3 edgeCenter = cellCenter + new Vector3(v.x, 0f, v.y) * (cellSize * 0.5f);
        bool alongX = dir == 0 || dir == 2; // N/S walls run along X

        if (wallFit == null)
        {
            Vector3 scale = alongX
                ? new Vector3(cellSize + wallThickness, wallHeight, wallThickness)
                : new Vector3(wallThickness, wallHeight, cellSize + wallThickness);
            Cube("Wall", edgeCenter + Vector3.up * (wallHeight * 0.5f), scale, wallMaterial);
            return;
        }

        // Without corner pillars, extend each wall by one thickness so the corners close up.
        float full = cellSize + (pillarFit == null ? wallThickness : 0f);
        int n = Mathf.Max(1, Mathf.RoundToInt(full / wallFit.Size.x));
        float stretch = full / (n * wallFit.Size.x);
        Vector3 along = alongX ? Vector3.right : Vector3.forward;

        for (int k = 0; k < n; k++)
        {
            float offset = ((k + 0.5f) / n - 0.5f) * full;
            PlaceFitted(wallFit, "Wall", edgeCenter + along * offset, alongX ? 0f : 90f, new Vector3(stretch, 1f, 1f));
        }
    }

    /// A pillar goes on every grid corner where at least one wall touches.
    void BuildPillars()
    {
        for (int i = 0; i <= Data.Width; i++)
        {
            for (int j = 0; j <= Data.Height; j++)
            {
                bool up    = EdgeHasWall(new Vector2Int(i, j), 3);       // west edge of cell (i,j)
                bool down  = EdgeHasWall(new Vector2Int(i, j - 1), 3);   // west edge of cell (i,j-1)
                bool right = EdgeHasWall(new Vector2Int(i, j), 2);       // south edge of cell (i,j)
                bool left  = EdgeHasWall(new Vector2Int(i - 1, j), 2);   // south edge of cell (i-1,j)
                if (!(up || down || right || left)) continue;

                Vector3 pos = CellToWorld(new Vector2Int(i, j)) + new Vector3(-cellSize * 0.5f, 0f, -cellSize * 0.5f);
                PlaceFitted(pillarFit, "Pillar", pos, 0f, Vector3.one);
            }
        }
    }

    /// True if the edge on side 'dir' of 'cell' has a wall, checking both cells that share it.
    bool EdgeHasWall(Vector2Int cell, int dir)
    {
        Vector2Int other = cell + Dir.Vec[dir];
        return (Data.InBounds(cell) && Data.HasWall(cell, dir)) ||
               (Data.InBounds(other) && Data.HasWall(other, Dir.Opposite(dir)));
    }

    // ===== Spawn points

    void SpawnPlayer()
    {
        if (playerToSpawn == null) return;

        MazeSpawnPoint playerStart = GetFirst(SpawnType.PlayerStart);
        if (playerStart == null)
        {
            Debug.LogError("Maze has no PlayerStart spawn point.", this);
            return;
        }

        playerToSpawn.SetPositionAndRotation(playerStart.transform.position, playerStart.transform.rotation);
    }

    void CreateSpawnPoints(List<SpawnPointInfo> infos)
    {
        pointsByType.Clear();
        var root = new GameObject("SpawnPoints").transform;
        root.SetParent(transform, false);
        var groups = new Dictionary<SpawnType, Transform>();

        foreach (var info in infos)
        {
            Transform group;
            if (!groups.TryGetValue(info.Type, out group))
            {
                group = new GameObject(info.Type.ToString()).transform;
                group.SetParent(root, false);
                groups[info.Type] = group;
                pointsByType[info.Type] = new List<MazeSpawnPoint>();
            }

            Vector3 center = CellToWorld(info.Cell);
            Vector3 pos = center;
            Quaternion rot = Quaternion.identity;

            if (info.Type == SpawnType.PlayerStart)
            {
                pos += Vector3.up * playerSpawnHeight;
                for (int d = 0; d < 4; d++)
                {
                    if (Data.HasWall(info.Cell, d)) continue;
                    Vector2Int v = Dir.Vec[d];
                    rot = Quaternion.LookRotation(new Vector3(v.x, 0f, v.y)); // face the first open corridor
                    break;
                }
            }
            else if (info.Type == SpawnType.Torch)
            {
                Vector2Int v = Dir.Vec[info.WallDir];
                Vector3 outward = new Vector3(v.x, 0f, v.y);
                pos = center + outward * (cellSize * 0.5f - wallThickness * 0.5f - 0.05f) + Vector3.up * torchHeight;
                rot = Quaternion.LookRotation(-outward) * Quaternion.Euler(0f, torchYawOffset, 0f);
            }

            var go = new GameObject($"{info.Type}_{info.Cell.x}_{info.Cell.y}");
            go.transform.SetParent(group);
            go.transform.SetPositionAndRotation(pos, rot);

            var sp = go.AddComponent<MazeSpawnPoint>();
            sp.type = info.Type;
            sp.cell = info.Cell;
            pointsByType[info.Type].Add(sp);

            if (info.Type == SpawnType.Torch) DecorateTorch(go.transform);
        }
    }

    void DecorateTorch(Transform point)
    {
        if (torchPrefab != null)
        {
            var torch = Instantiate(torchPrefab, point);
            torch.transform.localPosition = Vector3.zero;
            torch.transform.localRotation = Quaternion.identity;
        }
        if (addTorchLight)
        {
            // Slightly in front of the wall and above the flame
            Vector3 lightPos = point.position + point.forward * 0.4f + Vector3.up * 0.2f;
            AddLight(point, lightPos, torchLightColor, torchLightRange, torchLightIntensity, "TorchLight");
        }
    }

    int CountPoints()
    {
        int n = 0;
        foreach (var kv in pointsByType) n += kv.Value.Count;
        return n;
    }

    // ===== Room Dressing

    void DressRooms(int seed)
    {
        if (roomDressings == null || roomDressings.Count == 0) return;
        var rng = new System.Random(seed ^ 0xD3E55); // own random stream: never changes the maze layout
        var root = new GameObject("RoomProps").transform;
        root.SetParent(transform, false);

        foreach (var cell in Data.AllCells())
        {
            if (cell.Type == RoomType.Normal) continue;
            foreach (var dressing in roomDressings)
            {
                if (dressing.type != cell.Type) continue;
                foreach (var prop in dressing.props)
                    if (prop.prefab != null) PlaceProp(prop, cell, root, rng);
            }
        }
    }

    static KeyValuePair<Vector3, Quaternion> Spot(Vector3 p, Quaternion r) =>
        new KeyValuePair<Vector3, Quaternion>(p, r);

    void PlaceProp(DressingProp prop, MazeCell cell, Transform parent, System.Random rng)
    {
        Vector3 center = CellToWorld(cell.Pos);
        float half = cellSize * 0.5f;
        var spots = new List<KeyValuePair<Vector3, Quaternion>>();

        switch (prop.anchor)
        {
            case PropAnchor.Center:
                spots.Add(Spot(center, Quaternion.identity));
                break;

            case PropAnchor.Corners:
                for (int sx = -1; sx <= 1; sx += 2)
                    for (int sz = -1; sz <= 1; sz += 2)
                    {
                        Vector3 offset = new Vector3(sx, 0f, sz) * (half - prop.inset);
                        if (offset.sqrMagnitude < 0.0001f) continue;
                        spots.Add(Spot(center + offset, Quaternion.LookRotation(-offset)));
                    }
                break;

            case PropAnchor.AgainstWalls:
                for (int d = 0; d < 4; d++)
                {
                    if (!Data.HasWall(cell.Pos, d)) continue; // never block a doorway
                    Vector2Int v = Dir.Vec[d];
                    Vector3 outward = new Vector3(v.x, 0f, v.y);
                    spots.Add(Spot(center + outward * (half - wallThickness * 0.5f - prop.inset),
                                   Quaternion.LookRotation(-outward)));
                }
                break;

            case PropAnchor.Scatter:
                for (int i = 0; i < prop.count; i++)
                {
                    float rx = ((float)rng.NextDouble() * 2f - 1f) * (half - prop.inset);
                    float rz = ((float)rng.NextDouble() * 2f - 1f) * (half - prop.inset);
                    spots.Add(Spot(center + new Vector3(rx, 0f, rz), Quaternion.Euler(0f, rng.Next(360), 0f)));
                }
                break;
        }

        if (prop.anchor != PropAnchor.Scatter) Shuffle(spots, rng);
        int n = (prop.anchor == PropAnchor.Scatter || prop.anchor == PropAnchor.Center)
            ? spots.Count : Mathf.Min(prop.count, spots.Count);

        for (int i = 0; i < n; i++)
        {
            Quaternion rot = spots[i].Value * Quaternion.Euler(0f, prop.yawOffset, 0f);
            Fitted fit = GetFit(prop.prefab);
            GameObject go = fit != null
                ? PlaceFitted(fit, prop.prefab.name, spots[i].Key, rot.eulerAngles.y, Vector3.one * prop.scale)
                : Instantiate(prop.prefab, spots[i].Key, rot);
            go.transform.SetParent(parent); // keeps world position
        }
    }

    /// Auto-centered, sitting on the floor, collider added if missing (same fitting as walls).
    Fitted GetFit(GameObject prefab)
    {
        Fitted f;
        if (!fitCache.TryGetValue(prefab, out f))
        {
            f = Measure(prefab, false);
            fitCache[prefab] = f;
        }
        return f;
    }

    static void Shuffle<T>(List<T> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            T tmp = list[i]; list[i] = list[j]; list[j] = tmp;
        }
    }

    // ===== Special Room Markers

    // A glowing square frame on the floor, which emits light with a darker square on top to leave a glowing border. Boss rooms have a larger one
    void BuildRoomMarker(Vector3 center, RoomType type, GameObject floor)
    {
        Color c = RoomColor(type);
        float size = cellSize * (type == RoomType.Boss ? 0.85f : 0.6f);
        Material baseMat = floorMaterial != null ? floorMaterial : floor.GetComponent<Renderer>().sharedMaterial;

        var frame = Cube("RoomMarker", center + Vector3.up * 0.015f, new Vector3(size, 0.03f, size), null);
        var glow = new Material(baseMat);
        glow.color = c;
        glow.EnableKeyword("_EMISSION");
        glow.SetColor("_EmissionColor", c * markerGlow);
        frame.GetComponent<Renderer>().sharedMaterial = glow;

        var inner = Cube("RoomMarkerInner", center + Vector3.up * 0.02f, new Vector3(size * 0.82f, 0.04f, size * 0.82f), baseMat);

        RemoveCollider(frame);
        RemoveCollider(inner);

    }

    static void RemoveCollider(GameObject go)
    {
        var col = go.GetComponent<Collider>();
        if (col == null) return;
        if (Application.isPlaying) Destroy(col); else DestroyImmediate(col);
    }

    void OnDrawGizmos()
    {
        if (!drawRoomGizmos || Data == null) return;
        foreach (var cell in Data.AllCells())
        {
            if (cell.Type == RoomType.Normal) continue;
            Gizmos.color = RoomColor(cell.Type);
            Gizmos.DrawWireCube(CellToWorld(cell.Pos) + Vector3.up * (wallHeight * 0.5f),
                                new Vector3(cellSize, wallHeight, cellSize));
        }
    }


    // ===== Helpers

    GameObject Cube(string name, Vector3 pos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(transform);
        go.transform.position = pos;
        go.transform.localScale = scale;
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    void DecorateFloor(Vector3 cellCenter, System.Random rng)
    {
        if (rng.NextDouble() > debrisSpawnChance) return;

        GameObject prefab = floorDebrisPrefabs[rng.Next(floorDebrisPrefabs.Length)];
        if (prefab == null) return;

        // Make sure to keep the debris away from the exact walls so it doesn't clip into the bricks
        float margin = cellSize * 0.4f;
        float offsetX = (float)(rng.NextDouble() * 2.0 - 1.0) * margin;
        float offsetZ = (float)(rng.NextDouble() * 2.0 - 1.0) * margin;

        Vector3 pos = cellCenter + new Vector3(offsetX, 0f, offsetZ);
        Quaternion rot = Quaternion.Euler(0f, (float)(rng.NextDouble() * 360.0), 0f);

        var debris = Instantiate(prefab, pos, rot, transform);
        debris.name = "FloorDebris";
    }

    static void AddLight(Transform parent, Vector3 worldPos, Color color, float range, float intensity, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = worldPos;
        var l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = color;
        l.range = range;
        l.intensity = intensity;
        l.shadows = LightShadows.None; // shadows on many point lights get expensive fast
    }

    static void Tint(GameObject go, Color c)
    {
        if (c == Color.clear) return;
        var r = go.GetComponentInChildren<Renderer>();
        if (r != null) r.material.color = c; // per-object material instance
    }

    static Color RoomColor(RoomType t)
    {
        switch (t)
        {
            case RoomType.Start:    return new Color(0.3f, 0.8f, 0.3f);
            case RoomType.Treasure: return new Color(1f, 0.8f, 0.2f);
            case RoomType.Shop:     return new Color(0.3f, 0.7f, 1f);
            case RoomType.Boss:     return new Color(0.9f, 0.15f, 0.15f);
            default:                return Color.clear;
        }
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        IsBuilt = false;
        pointsByType.Clear();
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child); else DestroyImmediate(child);
        }
    }
}
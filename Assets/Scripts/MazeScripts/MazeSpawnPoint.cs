using UnityEngine;

/// Marker component placed by MazeBuilder. Teammates read these; nobody needs the maze internals.
/// Points sit on the floor (player start is raised). The forward (blue) axis is meaningful for
/// PlayerStart (faces the first open corridor) and Torch (faces into the corridor).
public class MazeSpawnPoint : MonoBehaviour
{
    public SpawnType type;
    public Vector2Int cell;

    void OnDrawGizmos()
    {
        Gizmos.color = ColorFor(type);
        Vector3 p = transform.position;
        Gizmos.DrawWireSphere(p + Vector3.up * 0.3f, 0.3f);
        Gizmos.DrawLine(p, p + Vector3.up * 1.2f);
        Gizmos.DrawLine(p + Vector3.up * 0.3f, p + Vector3.up * 0.3f + transform.forward * 0.6f);
    }

    static Color ColorFor(SpawnType t)
    {
        switch (t)
        {
            case SpawnType.PlayerStart: return Color.green;
            case SpawnType.Enemy:       return Color.red;
            case SpawnType.MiniBoss:    return new Color(1f, 0.4f, 0f);
            case SpawnType.Boss:        return Color.magenta;
            case SpawnType.Upgrade:     return Color.yellow;
            case SpawnType.Shop:        return Color.cyan;
            case SpawnType.Coin:        return new Color(1f, 0.85f, 0.3f);
            default:                    return new Color(1f, 0.6f, 0.2f); // torch
        }
    }
}
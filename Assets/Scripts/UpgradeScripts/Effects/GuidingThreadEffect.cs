using UnityEngine;

// Once the player has the Guiding Thread upgrade, a glowing line appears on the floor
// pointing toward the exit. For now it points in a straight line at an "exit" object.
// Once the maze is working, this could follow MazeData.FindPath instead.
[RequireComponent(typeof(PlayerUpgrades))]
public class GuidingThreadEffect : MonoBehaviour
{
    // What the thread points at (the exit / ladder). Found by tag "Finish" if left empty.
    [SerializeField] private Transform target;
    // How far ahead of the player the thread is drawn
    [SerializeField] private float threadLength = 6f;
    // Height above the ground so it doesn't clip into the floor
    [SerializeField] private float heightAboveGround = 0.05f;
    // How far below the player's center their feet are (CharacterController height / 2)
    [SerializeField] private float feetOffset = 1f;
    // Thread look
    [SerializeField] private Color threadColor = new Color(1f, 0.85f, 0.3f);
    [SerializeField] private float threadWidth = 0.08f;
    [SerializeField] private Material threadMaterial;

    private PlayerUpgrades upgrades;
    private LineRenderer line;
    // Number of points along the thread (more = smoother wave)
    private const int PointCount = 20;

    private void Start()
    {
        upgrades = GetComponent<PlayerUpgrades>();

        if (target == null)
        {
            GameObject exit = GameObject.FindWithTag("Finish");
            if (exit != null) target = exit.transform;
        }

        // The line lives on its own child object so it doesn't rotate with the player
        GameObject lineObject = new GameObject("Guiding Thread");
        lineObject.transform.SetParent(transform, false);
        line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = PointCount;
        line.startWidth = threadWidth;
        line.endWidth = 0f; // fades to a point at the end
        line.material = threadMaterial != null ? threadMaterial : new Material(Shader.Find("Sprites/Default"));
        line.startColor = threadColor;
        line.endColor = new Color(threadColor.r, threadColor.g, threadColor.b, 0f);
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.enabled = false;
    }

    private void Update()
    {
        bool hasThread = upgrades.GetTotal(UpgradeType.GuidingThread) > 0f;
        line.enabled = hasThread && target != null;
        if (!line.enabled)
            return;

        // Flat direction from the player to the exit
        Vector3 feet = transform.position + Vector3.down * (feetOffset - heightAboveGround);
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        float length = Mathf.Min(threadLength, toTarget.magnitude);
        Vector3 direction = toTarget.normalized;
        Vector3 side = Vector3.Cross(Vector3.up, direction);

        // Draw the points with a gentle moving wave so it looks like loose thread
        for (int i = 0; i < PointCount; i++)
        {
            float t = i / (float)(PointCount - 1);
            float wave = Mathf.Sin(t * 10f - Time.time * 3f) * 0.12f * t;
            line.SetPosition(i, feet + direction * (t * length) + side * wave);
        }
    }
}

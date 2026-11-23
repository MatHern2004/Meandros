using UnityEngine;

// With the Sixth Sense upgrade, the player sends out a pulse every few seconds.
// Any relic within range shows a tall beam of light that can be seen over the walls.
[RequireComponent(typeof(PlayerUpgrades))]
public class SixthSenseEffect : MonoBehaviour
{
    // Seconds between pulses
    [SerializeField] private float pulseInterval = 6f;
    // How far the pulse reaches
    [SerializeField] private float pulseRadius = 30f;
    // How long relics stay revealed
    [SerializeField] private float revealTime = 3f;
    // Sound when the pulse goes off
    [SerializeField] private AudioClip pulseSound;
    // Ring that expands outward on each pulse
    [SerializeField] private Color ringColor = new Color(0.7f, 0.3f, 1f, 0.8f);
    [SerializeField] private Material ringMaterial;
    [SerializeField] private float feetOffset = 1f;

    private PlayerUpgrades upgrades;
    private float pulseTimer;
    private LineRenderer ring;
    private float ringTime = -1f;      // -1 = ring not showing
    private const float RingDuration = 0.8f;
    private const int RingPoints = 48;

    private void Start()
    {
        upgrades = GetComponent<PlayerUpgrades>();
        pulseTimer = 1f; // first pulse comes quickly after picking it up

        // Expanding ring drawn on the floor
        GameObject ringObject = new GameObject("Sixth Sense Ring");
        ringObject.transform.SetParent(transform, false);
        ring = ringObject.AddComponent<LineRenderer>();
        ring.useWorldSpace = true;
        ring.loop = true;
        ring.positionCount = RingPoints;
        ring.widthMultiplier = 0.12f;
        ring.material = ringMaterial != null ? ringMaterial : new Material(Shader.Find("Sprites/Default"));
        ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ring.enabled = false;
    }

    private void Update()
    {
        UpdateRing();

        if (upgrades.GetTotal(UpgradeType.SixthSense) <= 0f)
            return;

        pulseTimer -= Time.deltaTime;
        if (pulseTimer <= 0f)
        {
            pulseTimer = pulseInterval;
            Pulse();
        }
    }

    // Reveal every relic in range and start the ring animation
    private void Pulse()
    {
        foreach (UpgradePickup pickup in UpgradePickup.All)
        {
            if (Vector3.Distance(transform.position, pickup.transform.position) <= pulseRadius)
                pickup.Reveal(revealTime);
        }

        if (pulseSound != null)
            AudioSource.PlayClipAtPoint(pulseSound, transform.position, 0.6f);

        ringTime = 0f;
    }

    // Grow the ring outward and fade it
    private void UpdateRing()
    {
        if (ringTime < 0f)
        {
            ring.enabled = false;
            return;
        }

        ringTime += Time.deltaTime;
        float t = ringTime / RingDuration;
        if (t >= 1f)
        {
            ringTime = -1f;
            return;
        }

        ring.enabled = true;
        float radius = Mathf.Lerp(0.5f, 12f, t);
        Color color = ringColor;
        color.a *= 1f - t;
        ring.startColor = ring.endColor = color;

        Vector3 center = transform.position + Vector3.down * (feetOffset - 0.05f);
        for (int i = 0; i < RingPoints; i++)
        {
            float angle = i / (float)RingPoints * Mathf.PI * 2f;
            ring.SetPosition(i, center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius);
        }
    }
}

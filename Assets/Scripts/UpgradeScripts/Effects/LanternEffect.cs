using UnityEngine;

// A light that follows the player. Each Lantern upgrade makes it reach farther.
// In a dark maze this is the difference between seeing a corner and seeing the hallway.
[RequireComponent(typeof(PlayerUpgrades))]
public class LanternEffect : MonoBehaviour
{
    // The light that follows the player (created automatically if empty)
    [SerializeField] private Light lanternLight;
    // How far the light reaches with no upgrades
    [SerializeField] private float baseRange = 6f;
    // Smallest range a curse can shrink it to
    [SerializeField] private float minimumRange = 3f;
    // Brightness of the light
    [SerializeField] private float intensity = 3f;
    // Warm torch color
    [SerializeField] private Color lightColor = new Color(1f, 0.75f, 0.45f);
    // How fast the range grows/shrinks to its new value
    [SerializeField] private float changeSpeed = 6f;
    // Small flicker so it feels like a flame
    [SerializeField] private float flickerAmount = 0.15f;

    private PlayerUpgrades upgrades;

    private void Start()
    {
        upgrades = GetComponent<PlayerUpgrades>();

        // Make the light if one wasn't assigned
        if (lanternLight == null)
        {
            GameObject lightObject = new GameObject("Lantern Light");
            lightObject.transform.SetParent(transform, false);
            // Chest height, a little in front of the player
            lightObject.transform.localPosition = new Vector3(0f, 0.6f, 0.5f);
            lanternLight = lightObject.AddComponent<Light>();
            lanternLight.type = LightType.Point;
            lanternLight.shadows = LightShadows.Soft;
        }
        lanternLight.color = lightColor;
        lanternLight.range = baseRange;
    }

    private void Update()
    {
        // Target range = base + all Lantern upgrades (curses are negative)
        float targetRange = Mathf.Max(minimumRange, baseRange + upgrades.GetTotal(UpgradeType.LightRadius));
        // Smoothly move toward the new range so the upgrade "blooms"
        lanternLight.range = Mathf.Lerp(lanternLight.range, targetRange, changeSpeed * Time.deltaTime);

        // Flicker using Perlin noise so it's smooth instead of random jitter
        float flicker = (Mathf.PerlinNoise(Time.time * 8f, 0f) - 0.5f) * 2f * flickerAmount;
        lanternLight.intensity = intensity * (1f + flicker);
    }
}

using System.Collections.Generic;
using UnityEngine;

// A relic sitting on a pedestal. Walk into it to get the upgrade.
// Either set a specific upgrade, or leave it empty and give it a loot table to roll a random one.
// The relic glows in its rarity color so the player can tell how good it is before grabbing it.
public class UpgradePickup : MonoBehaviour
{
    // Every relic in the scene (used by Sixth Sense to find them)
    public static List<UpgradePickup> All = new List<UpgradePickup>();

    [Header("What it gives")]
    // A specific upgrade to give. Leave empty to roll from the loot table instead.
    [SerializeField] private UpgradeData upgrade;
    // Used when no specific upgrade is set
    [SerializeField] private UpgradeLootTable lootTable;

    [Header("Visuals")]
    // The floating relic that spins and glows
    [SerializeField] private Transform relic;
    // Renderer that gets tinted to the rarity color
    [SerializeField] private Renderer relicRenderer;
    // Glow light under the relic
    [SerializeField] private Light glowLight;
    // Tall beam shown when Sixth Sense reveals this relic
    [SerializeField] private GameObject revealBeam;
    [SerializeField] private float spinSpeed = 60f;
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float bobSpeed = 2f;

    [Header("Feedback")]
    // Sound for picking it up (one per rarity: Common, Rare, Epic, Legendary)
    [SerializeField] private AudioClip[] pickupSounds;

    private Vector3 relicStartPosition;
    private bool collected;
    private float revealTimer;

    private void OnEnable()
    {
        All.Add(this);
    }

    private void OnDisable()
    {
        All.Remove(this);
    }

    private void Start()
    {
        // Roll a random upgrade if none was set
        if (upgrade == null && lootTable != null)
        {
            PlayerUpgrades player = FindAnyObjectByType<PlayerUpgrades>();
            upgrade = lootTable.Roll(player);
        }

        if (relic != null)
            relicStartPosition = relic.localPosition;

        ApplyRarityColor();
        if (revealBeam != null)
            revealBeam.SetActive(false);
    }

    private void Update()
    {
        if (collected)
            return;

        // Spin and bob the relic so it catches the player's eye
        if (relic != null)
        {
            relic.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
            relic.localPosition = relicStartPosition + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        }

        // Hide the Sixth Sense beam once its time runs out
        if (revealTimer > 0f)
        {
            revealTimer -= Time.deltaTime;
            if (revealTimer <= 0f && revealBeam != null)
                revealBeam.SetActive(false);
        }
    }

    // Called by Sixth Sense to show the beam for a few seconds
    public void Reveal(float seconds)
    {
        if (collected || revealBeam == null)
            return;
        revealBeam.SetActive(true);
        revealTimer = seconds;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected || upgrade == null)
            return;

        // Only the player can pick it up
        PlayerUpgrades player = other.GetComponentInParent<PlayerUpgrades>();
        if (player == null)
            return;

        // Player already has the max amount of this one
        if (!player.AddUpgrade(upgrade))
        {
            if (UpgradeHUD.Instance != null)
                UpgradeHUD.Instance.ShowMessage(upgrade.displayName + " is maxed out", Color.gray);
            return;
        }

        Collect();
    }

    // Turn off the relic but leave the empty pedestal behind
    private void Collect()
    {
        collected = true;

        int soundIndex = (int)upgrade.rarity;
        if (pickupSounds != null && soundIndex < pickupSounds.Length && pickupSounds[soundIndex] != null)
            AudioSource.PlayClipAtPoint(pickupSounds[soundIndex], transform.position, 0.8f);

        if (relic != null) relic.gameObject.SetActive(false);
        if (glowLight != null) glowLight.enabled = false;
        if (revealBeam != null) revealBeam.SetActive(false);
    }

    // Tint the relic, light and beam to the rarity color
    private void ApplyRarityColor()
    {
        if (upgrade == null)
            return;

        Color color = RarityInfo.GetColor(upgrade.rarity);
        if (relicRenderer != null)
        {
            // URP materials use _BaseColor, older ones use _Color
            relicRenderer.material.color = color;
            if (relicRenderer.material.HasProperty("_BaseColor"))
                relicRenderer.material.SetColor("_BaseColor", color);
        }
        if (glowLight != null)
            glowLight.color = color;
        if (revealBeam != null)
        {
            Renderer beamRenderer = revealBeam.GetComponent<Renderer>();
            if (beamRenderer != null)
            {
                Color beamColor = new Color(color.r, color.g, color.b, 0.45f);
                beamRenderer.material.color = beamColor;
                if (beamRenderer.material.HasProperty("_BaseColor"))
                    beamRenderer.material.SetColor("_BaseColor", beamColor);
            }
        }
    }
}

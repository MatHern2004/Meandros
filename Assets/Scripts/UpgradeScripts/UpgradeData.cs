using UnityEngine;

// Every kind of effect an upgrade can have. Each upgrade adds its "value" to one of these stats.
public enum UpgradeType
{
    MoveSpeed,        // value = percent bonus (0.15 = +15% speed)
    LightRadius,      // value = extra lantern range in meters
    GuidingThread,    // value = 1 turns the thread on
    SixthSense,       // value = 1 turns the relic pulse on
    MaxHealth,        // value = extra max health (no health system yet, stored for later)
    DamageReduction,  // value = percent less damage taken (0.1 = 10%), stored for later
    MysteryBox        // gives a random upgrade plus a random curse
}

// One upgrade definition. Make new ones with Assets -> Create -> Meandros -> Upgrade,
// then tweak the numbers in the Inspector without touching any code.
[CreateAssetMenu(menuName = "Meandros/Upgrade", fileName = "NewUpgrade")]
public class UpgradeData : ScriptableObject
{
    [Header("Display")]
    // Name shown on the pickup popup and the upgrade list
    public string displayName = "New Upgrade";
    // Short description of what it does
    [TextArea] public string description = "Does something.";
    // Small icon for the upgrade list
    public Sprite icon;

    [Header("Rarity")]
    // Decides the glow color and how often it drops
    public UpgradeRarity rarity = UpgradeRarity.Common;

    [Header("Effect")]
    // Which stat this upgrade changes
    public UpgradeType type = UpgradeType.MoveSpeed;
    // How much it changes that stat per stack (negative values are used for curses)
    public float value = 0.15f;
    // How many times the player can have this upgrade (1 = can't stack)
    [Min(1)] public int maxStacks = 3;
    // Curses are bad upgrades that only come out of the Mystery Box
    public bool isCurse = false;
}

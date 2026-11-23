using UnityEngine;

// The tiers an upgrade can be. Higher tiers are stronger and show up less often.
public enum UpgradeRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

// Shared info about each rarity so every script uses the same colors and drop chances
public static class RarityInfo
{
    // Color used for the relic glow, the screen flash and the upgrade list
    public static Color GetColor(UpgradeRarity rarity)
    {
        switch (rarity)
        {
            case UpgradeRarity.Common: return new Color(0.85f, 0.85f, 0.85f); // white/grey
            case UpgradeRarity.Rare: return new Color(0.25f, 0.55f, 1f);      // blue
            case UpgradeRarity.Epic: return new Color(0.7f, 0.3f, 1f);        // purple
            case UpgradeRarity.Legendary: return new Color(1f, 0.65f, 0.1f);  // orange/gold
            default: return Color.white;
        }
    }

    // How likely each rarity is to be rolled (out of the total of all weights)
    // Common 60%, Rare 25%, Epic 12%, Legendary 3%
    public static float GetWeight(UpgradeRarity rarity)
    {
        switch (rarity)
        {
            case UpgradeRarity.Common: return 60f;
            case UpgradeRarity.Rare: return 25f;
            case UpgradeRarity.Epic: return 12f;
            case UpgradeRarity.Legendary: return 3f;
            default: return 0f;
        }
    }
}

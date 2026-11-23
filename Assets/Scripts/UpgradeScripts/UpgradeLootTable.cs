using System.Collections.Generic;
using UnityEngine;

// A list of upgrades that a relic can randomly roll from.
// Different rooms (treasure room, shop, boss drop) can each use their own loot table.
[CreateAssetMenu(menuName = "Meandros/Upgrade Loot Table", fileName = "NewLootTable")]
public class UpgradeLootTable : ScriptableObject
{
    // Normal upgrades that can drop
    public List<UpgradeData> upgrades = new List<UpgradeData>();
    // Curses that the Mystery Box can hand out
    public List<UpgradeData> curses = new List<UpgradeData>();

    // All the rarities in order, so we can loop through them
    private static UpgradeRarity[] rarities = { UpgradeRarity.Common, UpgradeRarity.Rare, UpgradeRarity.Epic, UpgradeRarity.Legendary };

    // Roll a random upgrade. First picks a rarity by weight, then an upgrade of that rarity.
    // Upgrades the player already has at max stacks are skipped.
    public UpgradeData Roll(PlayerUpgrades player)
    {
        // Only keep the upgrades the player can still pick up
        List<UpgradeData> available = new List<UpgradeData>();
        foreach (UpgradeData upgrade in upgrades)
        {
            if (player == null || player.CanAdd(upgrade))
                available.Add(upgrade);
        }

        // Nothing left to give
        if (available.Count == 0)
            return null;

        // Add up the weights of the rarities that are actually available
        float totalWeight = 0f;
        foreach (UpgradeRarity rarity in rarities)
        {
            if (CountOfRarity(available, rarity) > 0)
                totalWeight += RarityInfo.GetWeight(rarity);
        }

        // Pick a random number in the total and find which rarity it lands on
        float roll = Random.Range(0f, totalWeight);
        UpgradeRarity picked = UpgradeRarity.Common;
        foreach (UpgradeRarity rarity in rarities)
        {
            if (CountOfRarity(available, rarity) == 0)
                continue;

            picked = rarity;
            roll -= RarityInfo.GetWeight(rarity);
            if (roll <= 0f)
                break;
        }

        // Make a list of just that rarity and pick one at random
        List<UpgradeData> choices = new List<UpgradeData>();
        foreach (UpgradeData upgrade in available)
        {
            if (upgrade.rarity == picked)
                choices.Add(upgrade);
        }
        return choices[Random.Range(0, choices.Count)];
    }

    // Random curse for the Mystery Box (null if there are none)
    public UpgradeData RollCurse()
    {
        if (curses.Count == 0)
            return null;
        return curses[Random.Range(0, curses.Count)];
    }

    // How many upgrades in the list have this rarity
    private int CountOfRarity(List<UpgradeData> list, UpgradeRarity rarity)
    {
        int count = 0;
        foreach (UpgradeData upgrade in list)
        {
            if (upgrade.rarity == rarity)
                count++;
        }
        return count;
    }
}

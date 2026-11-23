using System.Collections.Generic;
using UnityEngine;

// Goes on the player. Keeps track of every upgrade picked up and adds up the totals
// for each stat. Effect scripts (speed, lantern, etc.) read these totals.
public class PlayerUpgrades : MonoBehaviour
{
    // Used by the Mystery Box to give out a random upgrade and curse
    [SerializeField] private UpgradeLootTable mysteryBoxTable;

    // Every upgrade the player has, in the order they picked them up
    private List<UpgradeData> upgrades = new List<UpgradeData>();
    // How many of each upgrade the player has
    private Dictionary<UpgradeData, int> stacks = new Dictionary<UpgradeData, int>();
    // Running total for each stat
    private Dictionary<UpgradeType, float> totals = new Dictionary<UpgradeType, float>();

    // List of upgrades for the upgrade list UI
    public List<UpgradeData> GetUpgrades()
    {
        return upgrades;
    }

    // Total for one stat (0 if the player has nothing of that type)
    public float GetTotal(UpgradeType type)
    {
        if (totals.ContainsKey(type))
            return totals[type];
        return 0f;
    }

    // How many stacks of an upgrade the player has
    public int GetStacks(UpgradeData upgrade)
    {
        if (stacks.ContainsKey(upgrade))
            return stacks[upgrade];
        return 0;
    }

    // False if the player already has the max number of this upgrade
    public bool CanAdd(UpgradeData upgrade)
    {
        if (upgrade == null)
            return false;
        return GetStacks(upgrade) < upgrade.maxStacks;
    }

    // Give the player an upgrade. Returns false if they're already maxed out on it.
    public bool AddUpgrade(UpgradeData upgrade)
    {
        if (!CanAdd(upgrade))
            return false;

        // The Mystery Box isn't kept as a stat, it turns into a random upgrade and a curse
        if (upgrade.type == UpgradeType.MysteryBox)
        {
            OpenMysteryBox(upgrade);
            return true;
        }

        Apply(upgrade);
        return true;
    }

    // Record the upgrade and add its value to the stat totals
    private void Apply(UpgradeData upgrade)
    {
        // First time getting this upgrade
        if (!stacks.ContainsKey(upgrade))
        {
            stacks[upgrade] = 0;
            upgrades.Add(upgrade);
        }
        stacks[upgrade] = stacks[upgrade] + 1;

        // Add to the stat total
        totals[upgrade.type] = GetTotal(upgrade.type) + upgrade.value;

        Debug.Log("Upgrade picked up: " + upgrade.displayName + " (x" + stacks[upgrade] + ")");

        // Tell the UI so it can show the popup and update the list
        if (UpgradeHUD.Instance != null)
            UpgradeHUD.Instance.ShowUpgrade(upgrade);
    }

    // Mystery Box: one random good upgrade, plus one random curse
    private void OpenMysteryBox(UpgradeData box)
    {
        // Still show the box itself in the list so the player knows they opened one
        Apply(box);

        if (mysteryBoxTable == null)
        {
            Debug.LogWarning("PlayerUpgrades: no Mystery Box loot table assigned.");
            return;
        }

        UpgradeData reward = mysteryBoxTable.Roll(this);
        if (reward != null && reward.type != UpgradeType.MysteryBox)
            Apply(reward);

        UpgradeData curse = mysteryBoxTable.RollCurse();
        if (curse != null)
            Apply(curse);
    }
}

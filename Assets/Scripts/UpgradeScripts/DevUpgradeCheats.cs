using UnityEngine;
using UnityEngine.InputSystem;

// Testing/demo helper: number keys 1-9 give the player the matching upgrade instantly.
// Handy for showing off one specific upgrade without walking to its relic.
// Leave this off the player in the real game.
[RequireComponent(typeof(PlayerUpgrades))]
public class DevUpgradeCheats : MonoBehaviour
{
    // Upgrade for key 1, key 2, and so on (up to 9)
    [SerializeField] private UpgradeData[] hotkeyUpgrades;

    private PlayerUpgrades upgrades;

    private void Start()
    {
        upgrades = GetComponent<PlayerUpgrades>();
    }

    private void Update()
    {
        if (Keyboard.current == null || hotkeyUpgrades == null)
            return;

        for (int i = 0; i < hotkeyUpgrades.Length && i < 9; i++)
        {
            // Key.Digit1 through Key.Digit9 are in order in the Key enum
            Key key = Key.Digit1 + i;
            if (Keyboard.current[key].wasPressedThisFrame && hotkeyUpgrades[i] != null)
            {
                if (!upgrades.AddUpgrade(hotkeyUpgrades[i]) && UpgradeHUD.Instance != null)
                    UpgradeHUD.Instance.ShowMessage(hotkeyUpgrades[i].displayName + " is maxed out", Color.gray);
            }
        }
    }
}

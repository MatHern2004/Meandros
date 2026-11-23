using UnityEngine;
using UnityEngine.UI;

// Temporary on-screen feedback for upgrades (the real UI can replace this later):
//  - a dev panel listing every upgrade the player has
//  - a popup with the name and description when one is picked up
//  - a quick screen flash in the upgrade's rarity color
public class UpgradeHUD : MonoBehaviour
{
    public static UpgradeHUD Instance;

    [SerializeField] private PlayerUpgrades player;
    [SerializeField] private Text upgradeListText;
    [SerializeField] private Text popupTitle;
    [SerializeField] private Text popupDescription;
    [SerializeField] private CanvasGroup popupGroup;
    [SerializeField] private Image screenFlash;
    // How long the popup stays up
    [SerializeField] private float popupTime = 2.5f;
    // How strong the screen flash starts
    [SerializeField] private float flashStrength = 0.35f;
    // How fast the flash fades
    [SerializeField] private float flashFadeSpeed = 1.5f;

    private float popupTimer;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (player == null)
            player = FindAnyObjectByType<PlayerUpgrades>();

        popupGroup.alpha = 0f;
        screenFlash.color = Color.clear;
        RefreshList();
    }

    private void Update()
    {
        // Fade the popup out near the end of its time
        if (popupTimer > 0f)
        {
            popupTimer -= Time.deltaTime;
            popupGroup.alpha = Mathf.Clamp01(popupTimer / 0.5f);
        }

        // Fade the screen flash
        Color flash = screenFlash.color;
        if (flash.a > 0f)
        {
            flash.a = Mathf.Max(0f, flash.a - flashFadeSpeed * Time.deltaTime);
            screenFlash.color = flash;
        }
    }

    // Called by PlayerUpgrades when the player gets an upgrade
    public void ShowUpgrade(UpgradeData upgrade)
    {
        Color color = RarityInfo.GetColor(upgrade.rarity);
        string label = upgrade.rarity.ToString().ToUpper();

        // Curses flash red no matter the rarity
        if (upgrade.isCurse)
        {
            color = new Color(0.8f, 0.1f, 0.1f);
            label = "CURSE";
        }

        ShowPopup(upgrade.displayName + "  <size=22>(" + label + ")</size>", upgrade.description, color);
        screenFlash.color = new Color(color.r, color.g, color.b, flashStrength);
        RefreshList();
    }

    // Small message with no flash (like "maxed out")
    public void ShowMessage(string message, Color color)
    {
        ShowPopup(message, "", color);
    }

    private void ShowPopup(string title, string description, Color color)
    {
        popupTitle.text = title;
        popupTitle.color = color;
        popupDescription.text = description;
        popupGroup.alpha = 1f;
        popupTimer = popupTime;
    }

    // Rebuild the upgrade list text, like "Swift Boots x2"
    private void RefreshList()
    {
        string text = "<b>UPGRADES</b> <size=14>(dev panel, keys 1-7)</size>\n";

        if (player == null || player.GetUpgrades().Count == 0)
        {
            text += "<color=#888888>none yet</color>";
            upgradeListText.text = text;
            return;
        }

        foreach (UpgradeData upgrade in player.GetUpgrades())
        {
            // Curses show up red, everything else uses its rarity color
            Color color = RarityInfo.GetColor(upgrade.rarity);
            if (upgrade.isCurse)
                color = new Color(1f, 0.3f, 0.3f);

            string colorCode = ColorUtility.ToHtmlStringRGB(color);
            int count = player.GetStacks(upgrade);
            string stackText = "";
            if (count > 1)
                stackText = " x" + count;

            text += "<color=#" + colorCode + ">" + upgrade.displayName + stackText + "</color>\n";
        }

        // Stats that don't have a visible effect yet (no health system yet)
        float maxHealth = player.GetTotal(UpgradeType.MaxHealth);
        float reduction = player.GetTotal(UpgradeType.DamageReduction);
        if (maxHealth != 0f || reduction != 0f)
            text += "\n<size=14>Max Health +" + maxHealth + "   Damage Taken -" + Mathf.RoundToInt(reduction * 100f) + "%</size>";

        upgradeListText.text = text;
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerHUDController : MonoBehaviour
{
    [Header("Health UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;

    [Header("Navigation")]
    [SerializeField] private string titleSceneName = "TitleScreen";

    // Call this from player health script whenever health initializes or changes
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(Mathf.Max(0f, currentHealth))} / {Mathf.CeilToInt(maxHealth)}";
        }
    }

    public void PauseToTitleScreen()
    {
        TitleScreenUI.isReturningFromPause = true;
        SceneManager.LoadScene(titleSceneName);
    }
}

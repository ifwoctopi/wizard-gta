
using UnityEngine;
using UnityEngine.UI; // Required for the Slider or Image class

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    // Reference to your UI Health Bar component
    public Slider healthBarSlider; // Or public Image healthBarFillImage;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth); // Prevent health from going below zero

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die(); // Call a function to handle player death
        }
    }

    void UpdateHealthBar()
    {
        // For Slider method:
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth / maxHealth;
        }

        // For Image Fill method (if you used Image Fill):
        // if (healthBarFillImage != null)
        // {
        //     healthBarFillImage.fillAmount = currentHealth / maxHealth;
        // }
    }

    void Die()
    {
        Debug.Log("Player has died!");
        // Add death logic here (e.g., game over screen, respawn)
    }
}

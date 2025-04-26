using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public float armor;  // Add armor stat

    private bool isInvulnerable = false;
    public float invulnerabilityDuration = 0.5f;

    [Header("UI Elements")]
    public Slider healthBar; // <-- Connect your Health Bar Slider here
    public Image hitIndicatorImage;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(int amount)
    {
        if (isInvulnerable) return;

        // Apply armor reduction to damage
        float reducedDamage = Mathf.Max(amount - armor, 0);  // Armor cannot reduce damage below 0
        currentHealth -= Mathf.RoundToInt(reducedDamage);
        currentHealth = Mathf.Max(currentHealth, 0); // Prevent health going below 0

        Debug.Log($"Player took {reducedDamage} damage! Current Health: {currentHealth}");

        UpdateUI();
        FlashHitIndicator();
        StartCoroutine(InvulnerabilityRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }

    private void FlashHitIndicator()
    {
        if (hitIndicatorImage != null)
        {
            StopAllCoroutines();
            StartCoroutine(HitFlashRoutine());
        }
    }

    private IEnumerator HitFlashRoutine()
    {
        float flashDuration = 0.2f; // How long the flash stays visible
        float fadeDuration = 0.3f; // How long it takes to fade out

        // Fade in instantly
        hitIndicatorImage.color = new Color(1f, 0f, 0f, 0.4f); // Semi-transparent red

        // Wait a short moment
        yield return new WaitForSeconds(flashDuration);

        // Fade out smoothly
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0.4f, 0f, timer / fadeDuration);
            hitIndicatorImage.color = new Color(1f, 0f, 0f, alpha);
            yield return null;
        }

        // Make sure it's fully invisible at the end
        hitIndicatorImage.color = new Color(1f, 0f, 0f, 0f);
    }

    private void Die()
    {
        Debug.Log("Player died!");
        // TODO: Handle death (restart, show game over, etc.)
    }

    private void UpdateUI()
    {
        if (healthBar != null)
        {
            float normalizedHealth = (float)currentHealth / maxHealth;
            healthBar.value = normalizedHealth;

            // Hide health bar if health is full, show otherwise
            healthBar.gameObject.SetActive(normalizedHealth < 1f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public CameraShake cameraShake;

    public int maxHealth = 100;
    private int currentHealth;

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

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0); // Prevent health going below 0

        Debug.Log($"Player took {amount} damage! Current Health: {currentHealth}");

        UpdateUI();
        FlashHitIndicator();
        if (cameraShake != null)
        {
            cameraShake.Shake(0.15f, 0.1f); // Subtle shake: duration, magnitude
        }
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
            healthBar.value = (float)currentHealth / maxHealth;
        }
    }
}

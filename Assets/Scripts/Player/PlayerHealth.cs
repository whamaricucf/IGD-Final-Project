using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100;
    public float currentHealth = 100;

    public float armor;

    private bool isInvulnerable = false;
    public float invulnerabilityDuration = 0.5f;

    [Header("UI Elements")]
    public Slider healthBar;
    public Image hitIndicatorImage;

    void Start()
    {
        PlayerStats.Instance.OnStatsChanged += RefreshHealthStats;
    }

    private void OnDestroy()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnStatsChanged -= RefreshHealthStats;
    }

    private void Update()
    {
        if (PlayerStats.Instance != null && PlayerStats.Instance.regen > 0 && currentHealth < maxHealth)
        {
            currentHealth += PlayerStats.Instance.regen * Time.deltaTime;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            UpdateUI();
        }
    }

    public void ForceRefreshStats()
    {
        RefreshHealthStats();
    }

    private void RefreshHealthStats()
    {
        if (PlayerStats.Instance == null || PlayerStats.Instance.health <= 0) return;

        maxHealth = PlayerStats.Instance.health;
        armor = PlayerStats.Instance.armor;

        if (currentHealth <= 0 || currentHealth > maxHealth)
            currentHealth = maxHealth;

        UpdateUI();
    }

    public void TakeDamage(int amount)
    {
        if (isInvulnerable) return;

        float reducedDamage = Mathf.Max(amount - armor, 0);
        currentHealth -= Mathf.RoundToInt(reducedDamage);
        currentHealth = Mathf.Max(currentHealth, 0);

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
        float flashDuration = 0.2f;
        float fadeDuration = 0.3f;

        hitIndicatorImage.color = new Color(1f, 0f, 0f, 0.4f);

        yield return new WaitForSeconds(flashDuration);

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0.4f, 0f, timer / fadeDuration);
            hitIndicatorImage.color = new Color(1f, 0f, 0f, alpha);
            yield return null;
        }

        hitIndicatorImage.color = new Color(1f, 0f, 0f, 0f);
    }

    private void Die()
    {
        Debug.Log("Player died!");

        if (PlayerStats.Instance != null && PlayerStats.Instance.revival > 0)
        {
            PlayerStats.Instance.revival--;
            currentHealth = maxHealth * 0.5f;
            UpdateUI();
            Debug.Log("Player revived at 50% health!");
        }
        else
        {
            Debug.Log("No revivals left. Game over!");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoseGame();
            }
            else
            {
                Debug.LogWarning("GameManager.Instance was null when trying to lose the game!");
            }
        }
    }

    public void UpdateUI()
    {
        if (healthBar != null)
        {
            float normalizedHealth = currentHealth / maxHealth;
            healthBar.value = normalizedHealth;
            healthBar.gameObject.SetActive(normalizedHealth < 1f);
        }
    }
}

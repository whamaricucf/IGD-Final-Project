using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private SaveData currentSaveData;
    public CharacterDatabase characterDatabase; // Assign in inspector

    public TextMeshProUGUI coinsEarnedText;
    public TextMeshProUGUI enemiesDefeatedText;

    private int coinsEarned, enemiesDefeated;

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Start()
    {
        currentSaveData = SaveManager.Load();
        coinsEarned = 0;
        enemiesDefeated = 0;
        UpdateCoinsUI();
        UpdateEnemiesDefeatedUI();

        EnemyBatAI.OnEnemyDied += HandleEnemyDeath;
        EnemyGhostAI.OnEnemyDied += HandleEnemyDeath;
        EnemySpiderAI.OnEnemyDied += HandleEnemyDeath;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            // Get character stats based on selected character ID
            string selectedCharacterID = PlayerPrefs.GetString("SelectedCharacter", "A");
            PlayerData stats = characterDatabase.GetCharacterData(selectedCharacterID);

            // Update magnet zone with character stats
            GameObject magnetZone = GameObject.FindWithTag("Magnet");
            if (magnetZone != null)
            {
                MagnetZone magnet = magnetZone.GetComponent<MagnetZone>();
                if (magnet != null)
                {
                    magnet.playerStats = stats;
                    magnet.UpdateMagnetRange();
                }
            }

            // Grab UI references
            coinsEarnedText = GameObject.FindWithTag("CoinsText")?.GetComponent<TextMeshProUGUI>();
            enemiesDefeatedText = GameObject.FindWithTag("EnemiesText")?.GetComponent<TextMeshProUGUI>();

            UpdateCoinsUI();
            UpdateEnemiesDefeatedUI();
        }
    }

    public void AddCoins(int amount)
    {
        coinsEarned += amount;
        UpdateCoinsUI();

        float dynamicShake = amount >= 3 ? Mathf.Clamp(amount, 5f, 20f) : 0f;
        Color flash = amount >= 3 ? GetFlashColor(dynamicShake, Color.yellow, new Color(1f, 0.84f, 0f)) : Color.yellow;
        PopAndFlashText(coinsEarnedText, flash, 1.2f, 0.2f, dynamicShake);
    }

    public void AddEnemiesDefeated(int amount)
    {
        enemiesDefeated += amount;
        UpdateEnemiesDefeatedUI();

        float dynamicShake = Mathf.Clamp(amount * 2f, 5f, 25f);
        Color flash = GetFlashColor(dynamicShake, Color.red, new Color(0.6f, 0f, 1f));
        PopAndFlashText(enemiesDefeatedText, flash, 1.2f, 0.2f, dynamicShake);
    }

    private Color GetFlashColor(float shake, Color normal, Color critical) => shake >= 15f ? critical : normal;
    private void UpdateCoinsUI() => coinsEarnedText.text = coinsEarned.ToString();
    private void UpdateEnemiesDefeatedUI() => enemiesDefeatedText.text = enemiesDefeated.ToString();

    private void PopAndFlashText(TextMeshProUGUI target, Color flashColor, float scale = 1.2f, float duration = 0.2f, float shake = 0f)
    {
        if (target == null) return;

        Vector3 originalScale = target.rectTransform.localScale;
        Vector3 originalPosition = target.rectTransform.localPosition;
        Color originalColor = target.color;

        target.rectTransform.DOKill();
        target.DOKill();

        target.rectTransform
            .DOScale(originalScale * scale, duration / 2f)
            .SetEase(Ease.OutElastic)
            .OnComplete(() =>
            {
                target.rectTransform.DOScale(originalScale, duration / 2f).SetEase(Ease.InOutElastic).OnComplete(() =>
                {
                    if (shake > 0f)
                        target.rectTransform.DOShakePosition(0.2f, new Vector3(shake, 0f, 0f))
                        .OnComplete(() => target.rectTransform.localPosition = originalPosition);
                });
            });

        target.DOColor(flashColor, duration / 2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => target.DOColor(originalColor, duration / 2f).SetEase(Ease.InQuad));
    }

    private void HandleEnemyDeath()
    {
        AddEnemiesDefeated(1);
        AddCoins(1);
    }
}

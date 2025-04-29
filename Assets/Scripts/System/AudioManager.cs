using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip wandFireClip;
    public AudioClip bibleActivateClip;
    public AudioClip garlicHitClip;
    public AudioClip enemyDeathClip;
    public AudioClip sceneLoadedClip;
    public AudioClip buttonClickClip;
    public AudioClip gameLoseClip;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }


    // Optional: specific functions for clarity
    // Play at half volume
    public void PlayWandFire() => PlaySFX(wandFireClip, 0.5f);
    public void PlayBibleActivate() => PlaySFX(bibleActivateClip, 0.5f);
    public void PlayGarlicHit() => PlaySFX(garlicHitClip, 0.5f);
    public void PlayEnemyDeath() => PlaySFX(enemyDeathClip, 0.5f);

    // Play at normal volume
    public void PlaySceneLoaded() => PlaySFX(sceneLoadedClip);
    public void PlayButtonClick() => PlaySFX(buttonClickClip);
    public void PlayGameLose() => PlaySFX(gameLoseClip);

}

using UnityEngine;

public class FadeEffect : MonoBehaviour
{
    public float fadeDuration = 0.5f;
    private Material mat;
    private Coroutine currentFade;

    private void Awake()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            mat = renderer.material;
        }
    }

    private void OnEnable()
    {
        if (mat != null)
        {
            SetAlpha(0f);
            currentFade = StartCoroutine(FadeTo(1f)); // Fade in
        }
    }

    private void OnDisable()
    {
        if (mat != null && gameObject.activeInHierarchy)
        {
            StopAllCoroutines();
            SetAlpha(1f);
        }
    }

    public void FadeOutAndDisable()
    {
        if (currentFade != null)
            StopCoroutine(currentFade);

        if (gameObject.activeInHierarchy)
            currentFade = StartCoroutine(FadeTo(0f, true));
    }

    private System.Collections.IEnumerator FadeTo(float targetAlpha, bool disableAfter = false)
    {
        float startAlpha = mat.color.a;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(targetAlpha);

        if (disableAfter)
            gameObject.SetActive(false);
    }

    private void SetAlpha(float alpha)
    {
        Color color = mat.color;
        color.a = alpha;
        mat.color = color;
    }
}

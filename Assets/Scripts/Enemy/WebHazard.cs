using System.Collections;
using UnityEngine;

public class WebHazard : MonoBehaviour
{
    public float slowMultiplier = 0.5f; // Player moves at 50% speed inside the web
    public float webDuration = 10f; // How long the web stays before disappearing
    public float fadeDuration = 2f; // How long it takes to fade out

    private Material webMaterial;
    private Color originalColor;
    private Coroutine fadeCoroutine;

    private void OnEnable()
    {
        if (webMaterial == null)
        {
            Renderer renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                webMaterial = renderer.material;
                originalColor = webMaterial.color;
            }
        }

        // Reset the web appearance in case it's reused from the pool
        if (webMaterial != null)
        {
            webMaterial.color = originalColor;
        }

        // Start the fade-out countdown
        Invoke(nameof(BeginFadeOut), webDuration);
    }

    private void OnDisable()
    {
        CancelInvoke();
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
    }

    private void BeginFadeOut()
    {
        fadeCoroutine = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float timer = 0f;
        Color startColor = originalColor;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, timer / fadeDuration);
            if (webMaterial != null)
            {
                webMaterial.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }
            yield return null;
        }

        // Return to pool instead of destroy
        ObjectPooler.Instance.ReturnToPool("SpiderWeb", gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ApplySlow(slowMultiplier);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.RemoveSlow();
            }
        }
    }
}

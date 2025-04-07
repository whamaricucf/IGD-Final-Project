using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebHazard : MonoBehaviour
{
    public float slowMultiplier = 0.5f; // Player moves at 50% speed inside the web
    public float webDuration = 10f; // How long the web stays before disappearing
    public float fadeDuration = 2f; // How long it takes to fade out

    private Material webMaterial;
    private Color originalColor;

    private void Start()
    {
        // Grab the material (assumes the web uses 1 material)
        webMaterial = GetComponentInChildren<Renderer>().material;
        originalColor = webMaterial.color;

        // Start the fade-out countdown
        Invoke(nameof(BeginFadeOut), webDuration);
    }
    private void BeginFadeOut()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float timer = 0f;
        Color color = originalColor;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, timer / fadeDuration);
            webMaterial.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
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
using UnityEngine;
using DG.Tweening; // Make sure DOTween is installed!

public class KingBibleProjectile : MonoBehaviour
{
    private Transform player;
    private float radius;
    private float angle;

    private float rotateSpeed;
    private float damage;
    private float knockback;
    private float critChance;
    private float critMultiplier;

    private Material material; // Add this!

    private void Awake()
    {
        // Grab the material if there's a Renderer
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            material = rend.material;
        }
    }

    private void Update()
    {
        if (player == null) return;

        angle += rotateSpeed * Time.deltaTime;
        if (angle > 360f) angle -= 360f;

        float rad = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;
        transform.position = player.position + offset;
    }

    public void Activate(Transform playerTransform, float orbitRadius, float startAngle, float dmg, float kb, float critChanceVal, float critMultiVal, float spinSpeed)
    {
        player = playerTransform;
        radius = orbitRadius;
        angle = startAngle;

        damage = dmg;
        knockback = kb;
        critChance = critChanceVal;
        critMultiplier = critMultiVal;
        rotateSpeed = spinSpeed;

        // Reset opacity if needed
        if (material != null)
        {
            Color color = material.color;
            color.a = 1f;
            material.color = color;
        }
    }

    public void StartFadeOut()
    {
        if (material != null)
        {
            material.DOFade(0f, 0.5f).OnComplete(() => gameObject.SetActive(false));
        }
        else
        {
            // If no material, just disable
            ObjectPooler.Instance.ReturnToPool("KingBible", gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        if (other.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(Mathf.RoundToInt(damage), knockback, transform.position, critChance, critMultiplier, true);
        }
    }
}

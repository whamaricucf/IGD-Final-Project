using UnityEngine;

public class KingBibleProjectile : MonoBehaviour
{
    private Transform player;
    private float radius;
    private float angle;

    public float rotateSpeed = 100f;
    public float damage = 10f;
    public float knockback = 5f;
    public float critChance = 0f;
    public float critMultiplier = 2f;

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
    }

    void Update()
    {
        if (player == null) return;

        angle += rotateSpeed * Time.deltaTime;
        if (angle > 360f) angle -= 360f;

        float rad = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;
        transform.position = player.position + offset;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(Mathf.RoundToInt(damage), knockback, transform.position, critChance, critMultiplier);
        }
    }
}
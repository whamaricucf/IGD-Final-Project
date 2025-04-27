using System.Collections;
using UnityEngine;

public class MagicWandProjectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float damage;
    private int pierce;
    private float knockback;
    private float critChance;
    private float critMultiplier;

    private int hitCount = 0;
    public float lifetime = 3f;

    private void OnEnable()
    {
        hitCount = 0;
        StartCoroutine(DisableAfterTime(lifetime));
    }

    public void Launch(Vector3 dir, float spd, float dmg, int pierceCount, float kb, float critChanceVal, float critMultiVal)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        pierce = pierceCount;
        knockback = kb;
        critChance = critChanceVal;
        critMultiplier = critMultiVal;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        if (other.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(Mathf.RoundToInt(damage), knockback, transform.position, critChance, critMultiplier);
        }

        hitCount++;
        if (hitCount >= pierce)
        {
            ObjectPooler.Instance.ReturnToPool("MagicWandBullet", gameObject);
        }
    }

    private IEnumerator DisableAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        ObjectPooler.Instance.ReturnToPool("MagicWandBullet", gameObject);
    }
}

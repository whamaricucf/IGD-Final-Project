using System.Collections;
using UnityEngine;

public class MagicWandProjectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float damage;

    public float lifetime = 3f;

    private void OnEnable()
    {
        StartCoroutine(DisableAfterTime(lifetime));
    }

    public void Launch(Vector3 dir, float spd, float dmg)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Assuming enemies have a standard TakeDamage(int)
            other.GetComponent<MonoBehaviour>()?.SendMessage("TakeDamage", (int)damage, SendMessageOptions.DontRequireReceiver);

            // Disable instead of destroy
            gameObject.SetActive(false);
        }
    }

    private IEnumerator DisableAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
}

using UnityEngine;

public class KingBibleProjectile : MonoBehaviour
{
    private Transform player;
    private float radius;
    private float angle;

    public float rotateSpeed = 100f;
    public float damage = 10f;

    public void Activate(Transform playerTransform, float orbitRadius, float startAngle)
    {
        player = playerTransform;
        radius = orbitRadius;
        angle = startAngle;
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
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<MonoBehaviour>()?.SendMessage("TakeDamage", (int)damage, SendMessageOptions.DontRequireReceiver);
        }
    }
}

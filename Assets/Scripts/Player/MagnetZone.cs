using UnityEngine;

public class MagnetZone : MonoBehaviour
{
    public PlayerData playerStats; // Assign via GameManager
    private SphereCollider magnetCollider;

    void Start()
    {
        magnetCollider = GetComponent<SphereCollider>();
        UpdateMagnetRange();
    }

    public void UpdateMagnetRange()
    {
        magnetCollider.radius = 1f + (playerStats.magnet * 0.5f); // Example scale
    }
}

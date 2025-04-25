using UnityEngine;

public class MagnetZone : MonoBehaviour
{
    public PlayerData playerStats; // Assigned by GameManager
    private SphereCollider magnetCollider;

    private void Awake()
    {
        magnetCollider = GetComponent<SphereCollider>();
        if (magnetCollider == null)
        {
            Debug.LogError("MagnetZone: No SphereCollider attached to this object!");
        }
    }

    // This should be called by the GameManager to initialize the magnet stats
    public void Initialize(PlayerData stats)
    {
        if (stats == null)
        {
            Debug.LogError("MagnetZone: PlayerStats is null during initialization!");
            return;
        }

        playerStats = stats;
        UpdateMagnetRange();
    }

    private void Update()
    {
        // Ensure playerStats is valid and then update range
        if (playerStats != null)
        {
            UpdateMagnetRange();
        }
    }

    public void UpdateMagnetRange()
    {
        if (playerStats == null)
        {
            Debug.LogWarning("MagnetZone: playerStats not yet assigned!");
            return;
        }

        // Correcting the range multiplication
        float radius = 1f + (playerStats.magnet * 2f);  // Assuming magnet stat modifies the radius
        if (magnetCollider != null)
        {
            magnetCollider.radius = radius;
            //Debug.Log($"MagnetZone: Updated radius to {radius} based on player stats.");
        }
    }
}

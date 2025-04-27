using UnityEngine;

public class MagnetZone : MonoBehaviour
{
    private SphereCollider magnetCollider;

    private void Awake()
    {
        magnetCollider = GetComponent<SphereCollider>();
        if (magnetCollider == null)
        {
            Debug.LogError("MagnetZone: No SphereCollider attached to this object!");
        }
    }

    private void Start()
    {
        if (PlayerStats.Instance != null)
            UpdateMagnetRange();

        PlayerStats.Instance.OnStatsChanged += UpdateMagnetRange; // Listen to stat changes
    }

    private void OnDestroy()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnStatsChanged -= UpdateMagnetRange; // Clean up event
    }

    public void UpdateMagnetRange()
    {
        if (PlayerStats.Instance == null || magnetCollider == null)
            return;

        float radius = 1f + (PlayerStats.Instance.magnet * 2f);
        magnetCollider.radius = radius;
        // Debug.Log($"MagnetZone: Updated radius to {radius}");
    }
}

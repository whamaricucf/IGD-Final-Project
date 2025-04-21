using UnityEngine;

public class GarlicPulse : MonoBehaviour
{
    private Renderer rend;
    public Transform player;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        if (rend != null && player != null)
        {
            // Transform world position into local space of the GarlicVisual mesh
            Vector3 localPos = rend.transform.InverseTransformPoint(player.position);
            rend.material.SetVector("_CenterPosition", localPos);
        }
    }
}

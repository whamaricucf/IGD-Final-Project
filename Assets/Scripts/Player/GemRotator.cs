using UnityEngine;

public class GemRotator : MonoBehaviour
{
    public float rotationSpeed = 45f;
    [HideInInspector] public bool rotate = true;

    void OnEnable()
    {
        rotate = true; // Reset when enabled
    }

    void Update()
    {
        if (rotate)
        {
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);
        }
    }
}

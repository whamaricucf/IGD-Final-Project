using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Transform playerTx;
    private CharacterController controller;
    public Transform playerVisual;

    [HideInInspector] public float inputMoveX, inputMoveY;

    [Header("Move Settings")]
    public float speed;
    private float originalSpeed;
    private bool isSlowed = false;

    private Vector3 currentVelocity = Vector3.zero;
    public float acceleration = 15f;
    public float deceleration = 30f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 20f;

    [Header("Gravity Settings")]
    public float gravity = -9.81f;
    public float groundedYVelocity = -2f;
    private float verticalVelocity;

    [Header("Toroidal Wrapping Settings")]
    public Vector2 worldMin = new Vector2(-100, -100); // Bottom-left
    public Vector2 worldMax = new Vector2(400, 400);   // Top-right

    void Start()
    {
        Initialize();

        PlayerData data = CharacterSelector.Instance != null ? CharacterSelector.Instance.selectedPlayerData : null;
        if (data != null)
        {
            speed = data.movSpd;
        }

        originalSpeed = speed;
    }

    void Initialize()
    {
        playerTx = transform;
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
        HandleToroidalWrap(); // <-- Wrap after movement
    }

    void HandleMovement()
    {
        inputMoveX = Input.GetAxis("Horizontal");
        inputMoveY = Input.GetAxis("Vertical");

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 move = (cameraRight * inputMoveX) + (cameraForward * inputMoveY);
        if (move.sqrMagnitude > 1f) move.Normalize();

        currentVelocity = move * speed;

        verticalVelocity = controller.isGrounded ? groundedYVelocity : verticalVelocity + gravity * Time.deltaTime;

        Vector3 finalVelocity = currentVelocity;
        finalVelocity.y = verticalVelocity;

        controller.Move(finalVelocity * Time.deltaTime);

        RotateTowardsMovementDirection();
        ApplyTiltEffect();
    }

    void RotateTowardsMovementDirection()
    {
        Vector3 flatVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        if (flatVelocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatVelocity);
            playerTx.rotation = Quaternion.RotateTowards(playerTx.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void ApplyTiltEffect()
    {
        Vector3 flatVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        if (flatVelocity.sqrMagnitude > 0.1f)
        {
            float tiltAmount = 10f;
            float tiltForward = Mathf.Lerp(0f, -tiltAmount, flatVelocity.magnitude / speed);
            playerVisual.localRotation = Quaternion.Euler(tiltForward, 0f, 0f);
        }
        else
        {
            playerVisual.localRotation = Quaternion.identity;
        }
    }

    void HandleToroidalWrap()
    {
        Vector3 pos = playerTx.position;

        if (pos.x < worldMin.x)
            pos.x = worldMax.x - 1f;
        else if (pos.x > worldMax.x)
            pos.x = worldMin.x + 1f;

        if (pos.z < worldMin.y)
            pos.z = worldMax.y - 1f;
        else if (pos.z > worldMax.y)
            pos.z = worldMin.y + 1f;

        playerTx.position = pos;
    }

    public void ApplySlow(float slowMultiplier)
    {
        if (!isSlowed)
        {
            speed *= slowMultiplier;
            isSlowed = true;
        }
    }

    public void RemoveSlow()
    {
        if (isSlowed)
        {
            speed = originalSpeed;
            isSlowed = false;
        }
    }
}

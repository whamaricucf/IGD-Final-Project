using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Transform playerTx;
    private CharacterController controller;
    public Transform playerVisual;

    [HideInInspector] public float inputMoveX, inputMoveY;

    [Header("Move Settings")]
    public float speed;

    private Vector3 currentVelocity = Vector3.zero;
    public float acceleration = 15f;
    public float deceleration = 30f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 20f;

    [Header("Gravity Settings")]
    public float gravity = -9.81f;
    public float groundedYVelocity = -2f;
    private float verticalVelocity;

    private float originalSpeed;
    private bool isSlowed = false;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();

        // Pull stats from selected character if available
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

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
    }

    // private void LateUpdate()
    // {
    //     LockY();
    // }

    // void LockY()
    // {
    //     Vector3 pos = transform.position;
    //     pos.y = -0.53f;
    //     transform.position = pos;
    // }

    void HandleMovement()
    {
        inputMoveX = Input.GetAxis("Horizontal");
        inputMoveY = Input.GetAxis("Vertical");

        // Get camera-relative movement directions
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        // Flatten to the ground plane (no vertical movement)
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate move direction
        Vector3 move = (cameraRight * inputMoveX) + (cameraForward * inputMoveY);

        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        // Directly assign velocity based on input
        currentVelocity = move * speed;

        // Apply gravity manually
        if (controller.isGrounded)
        {
            verticalVelocity = groundedYVelocity; // Small downward force to stay grounded
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime; // Apply gravity over time
        }

        Vector3 finalVelocity = currentVelocity;
        finalVelocity.y = verticalVelocity;

        // Move the player
        controller.Move(finalVelocity * Time.deltaTime);

        // Rotate the player
        RotateTowardsMovementDirection();

        // Apply leaning/tilting visual effect
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
            float tiltAmount = 10f; // Degrees

            // Lean forward when moving
            float tiltForward = Mathf.Lerp(0f, -tiltAmount, flatVelocity.magnitude / speed);

            playerVisual.localRotation = Quaternion.Euler(tiltForward, 0f, 0f);
        }
        else
        {
            // Reset tilt to upright
            playerVisual.localRotation = Quaternion.identity;
        }
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

    // TODO: also going to add cursor lock/unlock for when choosing an upgrade / pausing
}
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Transform playerTx;
    private CharacterController controller;
    public Transform playerVisual;

    [HideInInspector] public float inputMoveX, inputMoveY;

    [Header("Move Settings")]
    public float baseSpeed;
    private float currentSpeed;
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
    public Vector2 worldMin = new Vector2(-100, -100);
    public Vector2 worldMax = new Vector2(400, 400);

    void Start()
    {
        Initialize();

        if (PlayerStats.Instance != null)
            baseSpeed = PlayerStats.Instance.playerData.movSpd;

        currentSpeed = baseSpeed;

        PlayerStats.Instance.OnStatsChanged += UpdateMovementSpeed;
    }

    private void OnDestroy()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnStatsChanged -= UpdateMovementSpeed;
    }

    void Initialize()
    {
        playerTx = transform;
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
        HandleToroidalWrap();
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

        currentVelocity = move * currentSpeed;

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
            float tiltForward = Mathf.Lerp(0f, -tiltAmount, flatVelocity.magnitude / currentSpeed);
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
            currentSpeed *= slowMultiplier;
            isSlowed = true;
        }
    }

    public void RemoveSlow()
    {
        if (isSlowed)
        {
            currentSpeed = baseSpeed;
            isSlowed = false;
        }
    }

    private void UpdateMovementSpeed()
    {
        if (PlayerStats.Instance != null)
        {
            baseSpeed = PlayerStats.Instance.speed;
            if (!isSlowed)
                currentSpeed = baseSpeed;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Transform playerTx;
    private CharacterController controller;

    [HideInInspector] public float inputMoveX, inputMoveY;

    [Header("Move Settings")]
    public float speed;
    private Vector3 lastPos = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        playerTx = transform;
        lastPos = playerTx.position;
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }

    void Movement()
    {
        inputMoveX = Input.GetAxis("Horizontal");
        inputMoveY = Input.GetAxis("Vertical");

        Vector3 calc;
        Vector3 move;

        move = (playerTx.right * inputMoveX) + (playerTx.forward * inputMoveY);
        if (move.magnitude > 1f)
        {
            move = move.normalized;
        }

        calc = move * speed * Time.deltaTime;

        controller.Move(calc);
    }

    // also going to add cursor lock/unlock for when choosing an upgrade / pausing
}

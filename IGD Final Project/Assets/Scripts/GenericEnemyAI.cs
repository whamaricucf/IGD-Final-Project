using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GenericEnemyAI : MonoBehaviour
{
    public Transform player, enemy;
    public NavMeshAgent agent;
    private PlayerController controller;
    private Renderer eRenderer;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        controller = player.GetComponent<PlayerController>(); //unused for now
        eRenderer = enemy.gameObject.GetComponent<Renderer>(); //unused for now
        rb = enemy.gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        ChasePlayer(); // will likely move this into an OnTriggerEnter and then give the enemy "random"-ish movement when the player isn't close enough
    }

    void ChasePlayer() 
    {
        agent.SetDestination(player.position); 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider == null) return;
        else
        {
            //Debug.Log(collision.collider); // this was just for me to figure out the name of the collider, I'll likely delete this in a future version
        }
        if (collision.collider.name == "Player")
        {
            Debug.Log("hit player");
            rb.AddForce(collision.GetContact(0).normal * 50f, ForceMode.Impulse); // this is to "bounce" the enemy away from the player whenever they collide so that they are not continuously colliding
            //this is where I will subtract from player health, likely by a set amount
        }
    }
}

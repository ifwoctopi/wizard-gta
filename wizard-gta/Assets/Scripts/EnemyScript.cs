using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    [SerializeField] private FieldOfView fieldOfView;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer shadowRenderer;
    public Transform[] waypoints;
    public float speed = 2f;
    private int currentWaypoint = 0;
    private Transform chaseTarget;
    private bool isChasing = false;
    public string playerTag = "player";

    void Update()
    {
        // Check if we are chasing the player
        if (isChasing && chaseTarget != null)
        {
            // Move towards player
            Vector3 direction = (chaseTarget.position - transform.position);
            float distance = direction.magnitude;

            if (distance > 0.5f) // stop distance
            {
                Vector3 moveDir = direction.normalized;
                transform.position += moveDir * speed * Time.deltaTime;
                FlipSprite(moveDir.x);
                fieldOfView.SetAimDirection(moveDir);
                fieldOfView.SetOrigin(transform.position);
            }
            else
            {
                // Stop moving when close to player
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                }
            }
        }
        else
        {
            // Patrol waypoints
            if (waypoints.Length == 0) return;

            Transform target = waypoints[currentWaypoint];
            Vector3 moveDir = (target.position - transform.position).normalized;
            transform.position += moveDir * speed * Time.deltaTime;
            FlipSprite(moveDir.x);
            fieldOfView.SetAimDirection(moveDir);
            fieldOfView.SetOrigin(transform.position);
            

            if (Vector3.Distance(transform.position, target.position) < 0.1f)
            {
                currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            }
            
        }
    }

    public void StartChase(Transform target)
    {
        chaseTarget = target;
        isChasing = true;
    }

    public void StopChase()
    {
        chaseTarget = null;
        isChasing = false;
    }

    private void FlipSprite(float moveDirX)
    {
        if (moveDirX > 0)
        {
            spriteRenderer.flipX = true;
            shadowRenderer.flipX = true;
        }
        else if (moveDirX < 0)
        {
            spriteRenderer.flipX = false;
            shadowRenderer.flipX = false;

        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. CONDITIONAL CHECK: Verify the collision is with the Player
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Game Over! Enemy collided with: " + other.gameObject.name);

            // 2. IMPLEMENT LOSE SCREEN LOGIC HERE:

            // Option A: Load a new scene (The most common way for a "Lose Screen")
            // You must ensure this scene is in your Build Settings!
            //SceneManager.LoadScene("LoseScreen"); 

            // Disable the player immediately
            other.gameObject.SetActive(false); 
            
            // Disable the enemy to prevent multiple trigger events
            this.enabled = false;
        }
    }
}

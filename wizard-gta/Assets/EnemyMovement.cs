using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    // --- Public variables for easy editing in the Unity Inspector ---

    [Header("Components & Targets")]
    public Transform playerTransform; // Assign your Player's Transform here

    [Header("Patrol Settings (Circular Movement)")]
    public float circleRadius = 5f;
    public float patrolSpeed = 1f;

    [Header("AI Detection Radii")]
    public float chaseDistance = 7f; // Inner radius: Chase trigger
    public float noticeDistance = 15f; // Outer radius: Investigate trigger (must be > chaseDistance)
    
    [Header("Movement Speeds")]
    public float investigateSpeed = 3f;
    public float chaseSpeed = 5f;

    [Header("Search Settings")]
    public float searchDuration = 3f; // How long to search before giving up
    public float investigationTolerance = 0.5f; // How close to the last known position is "close enough"

    // --- Private State Variables ---
    
    // The required state names
    private enum EnemyState { Patrol, Investigate, Chase, Search }
    private EnemyState currentState = EnemyState.Patrol;
    
    // Stored locations and timers
    private Rigidbody2D rb;
    private Vector2 circleCenter; 
    private float angle; // Used for circular movement
    private Vector2 lastKnownPlayerPosition;
    private float searchTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("EnemyAI requires a Rigidbody2D component!");
            return;
        }

        // Set up Rigidbody2D for non-gravity 2D movement
        rb.gravityScale = 0f;
        rb.freezeRotation = true; 

        // Set the center of the circle to the enemy's starting position
        circleCenter = transform.position;

        Debug.Log($"AI spawned: Starting **{currentState}** state. 🔴");
    }

    void Update()
    {
        // 1. Get current distance to the player
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 2. Global State Transition Logic (Run every frame)
        HandleStateTransitions(distanceToPlayer);
    }
    
    void FixedUpdate()
    {
        // Physics-based movement should be handled here for smoothness
        ExecuteCurrentStateLogic();
    }

    // --- Main Transition Controller ---
    
    void HandleStateTransitions(float dist)
    {
        EnemyState newState = currentState;

        // --- State Transition Logic ---
        if (dist <= chaseDistance)
        {
            newState = EnemyState.Chase; // Closest: CHASE
        }
        else if (dist <= noticeDistance)
        {
            // Noticed, but not close enough to chase. If we are patrolling, we switch to investigate.
            // If we are chasing, we stay chasing until they leave noticeDistance.
            if (currentState == EnemyState.Patrol || currentState == EnemyState.Search)
            {
                 newState = EnemyState.Investigate; // Noticed: INVESTIGATE
            }
        }
        else // Player is far away (dist > noticeDistance)
        {
            // If the enemy loses the player while chasing/investigating, it starts searching
            if (currentState == EnemyState.Chase || currentState == EnemyState.Investigate)
            {
                newState = EnemyState.Search; 
            }
            else if (currentState == EnemyState.Search && searchTimer <= 0)
            {
                newState = EnemyState.Patrol; // Search time runs out: PATROL
            }
        }
        
        // --- State Change Execution ---
        if (newState != currentState)
        {
            rb.velocity = Vector2.zero; // Stop any previous velocity
            Debug.Log($"Transition: **{currentState}** -> **{newState}**");

            // Perform setup actions for the new state
            if (newState == EnemyState.Chase || newState == EnemyState.Investigate)
            {
                // Anytime we see the player, update the last known position
                lastKnownPlayerPosition = playerTransform.position;
            }
            else if (newState == EnemyState.Search)
            {
                searchTimer = searchDuration; // Start the search timer
            }
            else if (newState == EnemyState.Patrol)
            {
                // 🔥 THE FIX: Set the new center for the circular patrol to the current position.
                // This ensures the enemy starts circling from where it stopped searching.
                circleCenter = transform.position;
                // We also reset the angle so the first calculated position is right where they are.
                angle = 0f; 
            }

            currentState = newState;
        }
    }

    // --- State Execution ---

    void ExecuteCurrentStateLogic()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Investigate:
                Investigate();
                break;
            case EnemyState.Chase:
                ChasePlayer();
                break;
            case EnemyState.Search:
                Search();
                break;
        }
    }

    // --- State-Specific Methods ---

    void Patrol()
    {
        // Circular Movement
        angle += Time.deltaTime * patrolSpeed;
        
        // Calculate X and Y coordinates on the circumference of the circle
        float x = Mathf.Cos(angle) * circleRadius;
        float y = Mathf.Sin(angle) * circleRadius;
        
        // Add the offset to the center point
        Vector2 newPosition = circleCenter + new Vector2(x, y);

        // Move the Rigidbody2D to the new position
        rb.MovePosition(newPosition);
    }

    void Investigate()
    {
        // Move towards the last known position.

        // 1. Check if we've reached the destination (last known position)
        if (Vector2.Distance(rb.position, lastKnownPlayerPosition) < investigationTolerance)
        {
            // We reached the spot, transition to Search
            HandleStateTransitions(float.MaxValue); // Force search transition by simulating being far away
            return;
        }

        // 2. Move towards the target position
        Vector2 direction = (lastKnownPlayerPosition - rb.position).normalized;
        rb.velocity = direction * investigateSpeed;
    }

    void ChasePlayer()
    {
        // Actively follow the player.
        Vector2 directionToPlayer = ((Vector2)playerTransform.position - rb.position).normalized;
        rb.velocity = directionToPlayer * chaseSpeed;
        
        // Keep updating the last known position while chasing, in case they disappear behind cover
        lastKnownPlayerPosition = playerTransform.position;
    }

    void Search()
    {
        // Stop movement and run down the timer, simulating the enemy looking around.
        rb.velocity = Vector2.zero;
        
        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0)
        {
            // Time is up, transition back to Patrol
            HandleStateTransitions(float.MaxValue);
        }
    }
}
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    // --- Public variables for easy editing in the Unity Inspector ---

    [Header("Components & Targets")]
    public Transform playerTransform; // Assign your Player's Transform here

    [Header("Patrol Settings (Square Movement)")]
    public float squareSize = 10f; // Side length of the square patrol area
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
    
    [Header("Boundary Settings")]
    [Tooltip("Maximum X position the enemy can move to")]
    public float maxX = 10f;
    [Tooltip("Minimum X position the enemy can move to")]
    public float minX = -10f;
    [Tooltip("Maximum Y position the enemy can move to")]
    public float maxY = 5f;
    [Tooltip("Minimum Y position the enemy can move to")]
    public float minY = -5f;

    // --- Private State Variables ---
    
    // The required state names
    private enum EnemyState { Patrol, Investigate, Chase, Search }
    private EnemyState currentState = EnemyState.Patrol;
    
    // Stored locations and timers
    private Rigidbody2D rb;
    private Vector2 lastKnownPlayerPosition;
    private float searchTimer;

    // --- VARIABLES FOR SQUARE PATROL ---
    private Vector2[] patrolWaypoints = new Vector2[4];
    private int currentWaypointIndex = 0;
    private float patrolTolerance = 0.1f; // How close to a waypoint is considered "reached"
    
    // --- LINE OF SIGHT VARIABLE ---
    private LayerMask wallMask; // Set to the "Wall" layer in Start()
    
    // --- SOUND SYSTEM INTEGRATION ---
    private SoundListener soundListener;
    private Vector3 soundInvestigationTarget;
    private bool investigatingSound = false;
    
    // --- ALERT SYSTEM INTEGRATION ---
    private AlertManager alertManager;

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

        // 🔥 SET LINE OF SIGHT MASK
        wallMask = LayerMask.GetMask("Wall");
        if (wallMask == 0)
        {
            Debug.LogError("Layer 'Wall' not found. Please ensure your wall objects are on a layer named 'Wall'.");
        }

        // Set up the square waypoints based on the starting position
        SetupPatrolWaypoints(transform.position);
        
        // --- Sound System Setup ---
        soundListener = GetComponent<SoundListener>();
        if (soundListener == null)
        {
            Debug.LogWarning("SoundListener component not found on enemy. Sound-based AI will not work properly.");
        }
        
        // --- Alert System Setup ---
        alertManager = AlertManager.Instance;
        if (alertManager != null)
        {
            // Subscribe to global player detection event
            AlertManager.OnPlayerDetectedGlobal += OnGlobalPlayerDetected;
        }
        else
        {
            Debug.LogWarning("AlertManager not found in scene. Alert system will not work properly.");
        }

        Debug.Log($"AI spawned: Starting **{currentState}** state. 🔴");
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (alertManager != null)
        {
            AlertManager.OnPlayerDetectedGlobal -= OnGlobalPlayerDetected;
        }
    }
    
    /// <summary>
    /// Called when any enemy detects the player - all enemies should investigate
    /// </summary>
    void OnGlobalPlayerDetected(Vector3 playerPosition, float detectionIntensity)
    {
        // Don't respond if we're already chasing or investigating
        if (currentState == EnemyState.Chase || currentState == EnemyState.Investigate)
        {
            return;
        }
        
        // Update our last known position
        lastKnownPlayerPosition = playerPosition;
        
        // If detection intensity is high (full detection), transition to investigate
        // This will make all enemies move toward the player's last known position
        if (detectionIntensity >= 0.5f)
        {
            Debug.Log($"{gameObject.name} received global alert! Investigating player position at {playerPosition}");
            currentState = EnemyState.Investigate;
            searchTimer = searchDuration; // Reset search timer
        }
    }

    void Update()
    {
        // 1. Get current distance to the player
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 2. Check for sound investigation
        CheckSoundInvestigation();

        // 3. Global State Transition Logic (Run every frame)
        HandleStateTransitions(distanceToPlayer);
    }
    
    void FixedUpdate()
    {
        // Physics-based movement should be handled here for smoothness
        ExecuteCurrentStateLogic();
    }

    // --- Helper Methods ---

    /// <summary>
    /// Checks if the sound listener has detected a sound and starts investigation
    /// </summary>
    void CheckSoundInvestigation()
    {
        if (soundListener != null && soundListener.IsInvestigatingSound())
        {
            // Get the sound position from the listener
            Vector3 soundPosition = soundListener.GetLastHeardSoundPosition();
            
            // Only investigate if we're not already chasing or investigating the player
            if (currentState == EnemyState.Patrol || currentState == EnemyState.Search)
            {
                investigatingSound = true;
                soundInvestigationTarget = soundPosition;
                
                // Transition to investigate the sound
                if (currentState != EnemyState.Investigate)
                {
                    Debug.Log($"Enemy heard sound at {soundPosition}, investigating...");
                    // Force transition to investigate state
                    HandleStateTransitions(float.MaxValue); // Simulate being far from player
                }
            }
        }
        else if (investigatingSound && soundListener != null && !soundListener.IsInvestigatingSound())
        {
            // Stop investigating sound if listener is no longer investigating
            investigatingSound = false;
        }
    }

    void SetupPatrolWaypoints(Vector2 center)
    {
        float half = squareSize / 2f;
        
        // Define the four corners of the square, relative to the center
        patrolWaypoints[0] = center + new Vector2(half, half);    // Top Right
        patrolWaypoints[1] = center + new Vector2(-half, half);   // Top Left
        patrolWaypoints[2] = center + new Vector2(-half, -half);  // Bottom Left
        patrolWaypoints[3] = center + new Vector2(half, -half);   // Bottom Right

        // Clamp all waypoints to stay within boundaries
        for (int i = 0; i < patrolWaypoints.Length; i++)
        {
            patrolWaypoints[i] = ClampPositionToBoundaries(patrolWaypoints[i]);
        }

        // Find the closest waypoint to start the patrol from
        float minDist = float.MaxValue;
        for (int i = 0; i < patrolWaypoints.Length; i++)
        {
            float dist = Vector2.Distance(transform.position, patrolWaypoints[i]);
            if (dist < minDist)
            {
                minDist = dist;
                currentWaypointIndex = i;
            }
        }
    }

    bool CanSeePlayer()
    {
        // Fire a Linecast from the enemy towards the player, filtering only for the "Wall" layer
        RaycastHit2D hit = Physics2D.Linecast(transform.position, playerTransform.position, wallMask);
        
        // If 'hit.collider' is null, nothing on the "Wall" layer was intersected. LOS is clear.
        return hit.collider == null;
    }

    // --- Main Transition Controller ---
    
    void HandleStateTransitions(float dist)
    {
        EnemyState newState = currentState;
        
        // We check LOS up front
        bool losIsClear = CanSeePlayer();

        // --- State Transition Logic ---
        
        if (dist <= chaseDistance && losIsClear)
        {
            newState = EnemyState.Chase; // Closest and LOS is clear: CHASE
            
            // Report player detection to alert system
            if (alertManager != null)
            {
                alertManager.OnPlayerDetected(playerTransform.position, 1f);
            }
        }
        else if (dist <= noticeDistance && losIsClear)
        {
            // Noticed (in outer range) and LOS is clear.
            if (currentState == EnemyState.Patrol || currentState == EnemyState.Search)
            {
                newState = EnemyState.Investigate; // Noticed: INVESTIGATE
                
                // Report partial detection to alert system
                if (alertManager != null)
                {
                    alertManager.OnPlayerDetected(playerTransform.position, 0.5f);
                }
            }
        }
        else // Player is far away (dist > noticeDistance) OR Player is nearby but blocked (No LOS)
        {
            // If we were chasing or investigating, and now we either lost LOS OR left the notice range
            if (currentState == EnemyState.Chase || currentState == EnemyState.Investigate)
            {
                // Transition to Search if LOS is lost OR the distance is too great
                if (!losIsClear || dist > noticeDistance)
                {
                     newState = EnemyState.Search; 
                }
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
                // Set the last known position only if we can see them
                lastKnownPlayerPosition = playerTransform.position;
            }
            else if (newState == EnemyState.Search)
            {
                searchTimer = searchDuration; // Start the search timer
            }
            else if (newState == EnemyState.Patrol)
            {
                // Recalculate patrol points around the current position
                SetupPatrolWaypoints(transform.position); 
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
    
    /// <summary>
    /// Clamps the enemy's position within the defined boundaries
    /// </summary>
    private void ClampToBoundaries()
    {
        Vector3 currentPosition = transform.position;
        Vector3 clampedPosition = currentPosition;
        
        // Clamp X position
        clampedPosition.x = Mathf.Clamp(currentPosition.x, minX, maxX);
        
        // Clamp Y position
        clampedPosition.y = Mathf.Clamp(currentPosition.y, minY, maxY);
        
        // Apply the clamped position if it changed
        if (clampedPosition != currentPosition)
        {
            transform.position = clampedPosition;
            // Stop velocity when hitting a boundary
            rb.velocity = Vector2.zero;
        }
    }
    
    /// <summary>
    /// Clamps a position vector to the defined boundaries
    /// </summary>
    private Vector2 ClampPositionToBoundaries(Vector2 position)
    {
        return new Vector2(
            Mathf.Clamp(position.x, minX, maxX),
            Mathf.Clamp(position.y, minY, maxY)
        );
    }

    void Patrol()
    {
        // 1. Get the current target waypoint
        Vector2 targetWaypoint = patrolWaypoints[currentWaypointIndex];

        // 2. Check if we've reached the waypoint
        if (Vector2.Distance(rb.position, targetWaypoint) < patrolTolerance)
        {
            // Move to the next waypoint in the sequence (0, 1, 2, 3, then back to 0)
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
            targetWaypoint = patrolWaypoints[currentWaypointIndex]; // Update target to the new waypoint
        }

        // 3. Move towards the current target waypoint
        Vector2 direction = (targetWaypoint - rb.position).normalized;
        rb.velocity = direction * patrolSpeed;
        
        // 4. Apply boundary constraints
        ClampToBoundaries();
    }

    void Investigate()
    {
        Vector2 targetPosition;
        
        // Determine what we're investigating
        if (investigatingSound)
        {
            // Investigating a sound
            targetPosition = soundInvestigationTarget;
        }
        else if (alertManager != null && alertManager.HasRecentLastKnownPosition())
        {
            // Use global last known position from AlertManager
            targetPosition = alertManager.GetLastKnownPlayerPosition();
        }
        else
        {
            // Fallback to local last known position
            targetPosition = lastKnownPlayerPosition;
        }

        // 1. Check if we've reached the destination
        if (Vector2.Distance(rb.position, targetPosition) < investigationTolerance)
        {
            // We reached the spot, transition to Search
            HandleStateTransitions(float.MaxValue); // Force search transition by simulating being far away
            return;
        }

        // 2. Move towards the target position
        Vector2 direction = (targetPosition - rb.position).normalized;
        rb.velocity = direction * investigateSpeed;
        
        // 3. Apply boundary constraints
        ClampToBoundaries();
    }

    void ChasePlayer()
    {
        // Actively follow the player.
        Vector2 directionToPlayer = ((Vector2)playerTransform.position - rb.position).normalized;
        rb.velocity = directionToPlayer * chaseSpeed;
        
        // Keep updating the last known position while chasing, in case they disappear
        lastKnownPlayerPosition = playerTransform.position;
        
        // Apply boundary constraints
        ClampToBoundaries();
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
    
    /// <summary>
    /// Draws the boundary limits in the Scene view for debugging
    /// </summary>
    void OnDrawGizmos()
    {
        // Draw boundary box
        Gizmos.color = Color.red;
        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0);
        Gizmos.DrawWireCube(center, size);
        
        // Draw patrol waypoints if they exist
        if (patrolWaypoints != null && patrolWaypoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolWaypoints.Length; i++)
            {
                Gizmos.DrawWireSphere(patrolWaypoints[i], 0.3f);
                
                // Draw line to next waypoint
                int nextIndex = (i + 1) % patrolWaypoints.Length;
                Gizmos.DrawLine(patrolWaypoints[i], patrolWaypoints[nextIndex]);
            }
        }
    }
}
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
    
    // --- Physics ---
    private Rigidbody2D rb;
    
    // --- Obstacle Avoidance ---
    [Header("Obstacle Avoidance")]
    public LayerMask obstacleLayer;
    public float avoidanceDistance = 1.5f;
    public float avoidanceForce = 2f;
    
    // --- Sound System Integration ---
    private SoundListener soundListener;
    private Vector3 soundInvestigationTarget;
    private bool investigatingSound = false;
    
    // --- Alert System Integration ---
    private AlertManager alertManager;
    
    // --- Memory System ---
    private Vector3 lastKnownPlayerPosition;
    private bool hasLastKnownPosition = false;
    private bool investigatingLastPosition = false;
    private float memoryDuration = 5f; // How long to search at last known position
    private float memoryTimer = 0f;
    
    // --- Search Pattern ---
    [Header("Search Behavior")]
    [Tooltip("Radius to search around last known position")]
    public float searchRadius = 3f;
    [Tooltip("Number of search points to check")]
    public int searchPoints = 8;
    private int currentSearchPoint = 0;
    private Vector3 currentSearchTarget;
    private bool isSearching = false;
    private float searchPointWaitTime = 1f; // Time to wait at each search point
    private float searchPointTimer = 0f;
    
    // --- Return to Patrol System ---
    [Header("Return to Patrol")]
    [Tooltip("Time after losing player before returning to patrol route")]
    public float returnToPatrolDelay = 10f;
    private float timeSinceLastSeenPlayer = 0f;
    private bool returningToPatrol = false;
    private Vector3 patrolReturnPoint; // The waypoint to return to
    
    // --- Stuck Detection ---
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private float stuckThreshold = 2f; // If not moving for 2 seconds, consider stuck
    private float minMovementDistance = 0.1f; // Minimum distance to consider as movement
    
    void Start()
    {
        // Rigidbody Setup
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on enemy! Adding one...");
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f; // No gravity for 2D top-down
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Don't rotate
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision detection
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Smooth movement
        
        // Initialize stuck detection
        lastPosition = transform.position;
        
        // Sound System Setup
        soundListener = GetComponent<SoundListener>();
        if (soundListener == null)
        {
            Debug.LogWarning("SoundListener component not found on enemy. Sound-based AI will not work.");
        }
        
        // Alert System Setup
        alertManager = AlertManager.Instance;
        if (alertManager == null)
        {
            Debug.LogWarning("AlertManager not found in scene. Alert system will not work.");
        }
    }

    void Update()
    {
        // Check for sound investigation
        CheckSoundInvestigation();
        
        // Stuck detection
        CheckIfStuck();
        
        // Update memory timer
        if (investigatingLastPosition)
        {
            memoryTimer -= Time.deltaTime;
            if (memoryTimer <= 0)
            {
                // Stop investigating, resume patrol
                investigatingLastPosition = false;
                hasLastKnownPosition = false;
                Debug.Log("Enemy gave up searching last known position");
            }
        }
        
        // Update return to patrol timer
        // Only count time if not currently chasing and not already on normal patrol
        if (!isChasing && !returningToPatrol && (investigatingLastPosition || investigatingSound || hasLastKnownPosition))
        {
            timeSinceLastSeenPlayer += Time.deltaTime;
            
            Debug.Log($"Time since last seen player: {timeSinceLastSeenPlayer:F1}/{returnToPatrolDelay} seconds");
            
            // After 10 seconds, start returning to patrol
            if (timeSinceLastSeenPlayer >= returnToPatrolDelay)
            {
                StartReturningToPatrol();
            }
        }
        else if (isChasing)
        {
            // Reset timer while chasing
            timeSinceLastSeenPlayer = 0f;
            returningToPatrol = false;
        }
        else if (!investigatingLastPosition && !investigatingSound && !hasLastKnownPosition && !isChasing)
        {
            // Back to normal patrol, reset timer
            timeSinceLastSeenPlayer = 0f;
        }
    }
    
    /// <summary>
    /// Detects if the enemy is stuck and attempts to unstick
    /// </summary>
    void CheckIfStuck()
    {
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        
        // If barely moved
        if (distanceMoved < minMovementDistance)
        {
            stuckTimer += Time.deltaTime;
            
            if (stuckTimer >= stuckThreshold)
            {
                // Enemy is stuck! Apply a random force to unstick
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                Vector2 unstuckPosition = rb.position + randomDirection * 0.5f;
                rb.MovePosition(unstuckPosition);
                
                Debug.Log($"Enemy was stuck! Applying unstuck force in direction {randomDirection}");
                
                stuckTimer = 0f; // Reset timer
            }
        }
        else
        {
            // Moving normally, reset stuck timer
            stuckTimer = 0f;
        }
        
        lastPosition = transform.position;
    }
    
    void FixedUpdate()
    {
        // Priority 1: Chase player if detected
        if (isChasing && chaseTarget != null)
        {
            // Update last known position while chasing
            lastKnownPlayerPosition = chaseTarget.position;
            hasLastKnownPosition = true;
            
            // Move towards player
            Vector3 direction = (chaseTarget.position - transform.position);
            float distance = direction.magnitude;

            if (distance > 0.5f) // stop distance
            {
                Vector2 moveDir = direction.normalized;
                // Apply obstacle avoidance
                moveDir = GetAvoidanceDirection(moveDir, distance);
                Vector2 newPosition = rb.position + moveDir * speed * Time.fixedDeltaTime;
                rb.MovePosition(newPosition);
                FlipSprite(moveDir.x);
                fieldOfView.SetAimDirection(moveDir);
                fieldOfView.SetOrigin(transform.position);
            }
            else
            {
                // Stop moving when close to player
                rb.velocity = Vector2.zero;
            }
        }
        // Priority 2: Return to patrol route
        else if (returningToPatrol)
        {
            // Navigate back to patrol waypoint
            Vector3 direction = (patrolReturnPoint - transform.position);
            float distance = direction.magnitude;

            if (distance > 0.5f) // stop distance
            {
                Vector2 moveDir = direction.normalized;
                // Apply obstacle avoidance
                moveDir = GetAvoidanceDirection(moveDir, distance);
                Vector2 newPosition = rb.position + moveDir * speed * Time.fixedDeltaTime;
                rb.MovePosition(newPosition);
                FlipSprite(moveDir.x);
                fieldOfView.SetAimDirection(moveDir);
                fieldOfView.SetOrigin(transform.position);
            }
            else
            {
                // Reached the patrol point, resume normal patrol
                returningToPatrol = false;
                investigatingLastPosition = false;
                investigatingSound = false;
                hasLastKnownPosition = false;
                timeSinceLastSeenPlayer = 0f;
                Debug.Log("Enemy returned to patrol route, resuming normal patrol");
            }
        }
        // Priority 3: Investigate last known player position
        else if (investigatingLastPosition && hasLastKnownPosition)
        {
            if (!isSearching)
            {
                // Move towards last known position first
                Vector3 direction = (lastKnownPlayerPosition - transform.position);
                float distance = direction.magnitude;

                if (distance > 0.5f) // stop distance
                {
                    Vector2 moveDir = direction.normalized;
                    // Apply obstacle avoidance
                    moveDir = GetAvoidanceDirection(moveDir, distance);
                    Vector2 newPosition = rb.position + moveDir * speed * Time.fixedDeltaTime;
                    rb.MovePosition(newPosition);
                    FlipSprite(moveDir.x);
                    fieldOfView.SetAimDirection(moveDir);
                    fieldOfView.SetOrigin(transform.position);
                }
                else
                {
                    // Reached last known position, start searching pattern
                    Debug.Log("Enemy reached last known position, starting search pattern...");
                    isSearching = true;
                    currentSearchPoint = 0;
                    GenerateSearchPoint();
                }
            }
            else
            {
                // Execute search pattern
                Vector3 direction = (currentSearchTarget - transform.position);
                float distance = direction.magnitude;

                if (distance > 0.5f) // still moving to search point
                {
                    Vector2 moveDir = direction.normalized;
                    // Apply obstacle avoidance
                    moveDir = GetAvoidanceDirection(moveDir, distance);
                    Vector2 newPosition = rb.position + moveDir * speed * Time.fixedDeltaTime;
                    rb.MovePosition(newPosition);
                    FlipSprite(moveDir.x);
                    fieldOfView.SetAimDirection(moveDir);
                    fieldOfView.SetOrigin(transform.position);
                }
                else
                {
                    // Reached search point, wait and look around
                    searchPointTimer += Time.fixedDeltaTime;
                    
                    // Slowly rotate vision while waiting
                    float rotationSpeed = 90f; // degrees per second
                    Vector2 currentDir = new Vector2(Mathf.Cos(searchPointTimer * rotationSpeed * Mathf.Deg2Rad), 
                                                      Mathf.Sin(searchPointTimer * rotationSpeed * Mathf.Deg2Rad));
                    fieldOfView.SetAimDirection(currentDir);
                    fieldOfView.SetOrigin(transform.position);
                    
                    if (searchPointTimer >= searchPointWaitTime)
                    {
                        // Move to next search point
                        searchPointTimer = 0f;
                        currentSearchPoint++;
                        
                        if (currentSearchPoint >= searchPoints)
                        {
                            // Finished searching all points
                            Debug.Log("Enemy finished searching area, found nothing");
                            isSearching = false;
                        }
                        else
                        {
                            GenerateSearchPoint();
                        }
                    }
                }
            }
        }
        // Priority 4: Investigate sound if heard
        else if (investigatingSound)
        {
            // Move towards the sound location
            Vector3 direction = (soundInvestigationTarget - transform.position);
            float distance = direction.magnitude;

            if (distance > 0.5f) // stop distance
            {
                Vector2 moveDir = direction.normalized;
                // Apply obstacle avoidance
                moveDir = GetAvoidanceDirection(moveDir, distance);
                Vector2 newPosition = rb.position + moveDir * speed * Time.fixedDeltaTime;
                rb.MovePosition(newPosition);
                FlipSprite(moveDir.x);
                fieldOfView.SetAimDirection(moveDir);
                fieldOfView.SetOrigin(transform.position);
            }
            else
            {
                // Reached the sound location, stop investigating
                investigatingSound = false;
                Debug.Log("Enemy reached sound location, resuming patrol");
            }
        }
        // Priority 5: Patrol waypoints
        else
        {
            // Patrol waypoints
            if (waypoints.Length == 0) return;

            Transform target = waypoints[currentWaypoint];
            float distanceToWaypoint = Vector3.Distance(transform.position, target.position);
            Vector2 moveDir = (target.position - transform.position).normalized;
            // Apply obstacle avoidance
            moveDir = GetAvoidanceDirection(moveDir, distanceToWaypoint);
            Vector2 newPosition = rb.position + moveDir * speed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
            FlipSprite(moveDir.x);
            fieldOfView.SetAimDirection(moveDir);
            fieldOfView.SetOrigin(transform.position);
            

            if (Vector3.Distance(transform.position, target.position) < 0.1f)
            {
                currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            }
            
        }
    }
    
    /// <summary>
    /// Starts the return to patrol behavior after losing sight of player for too long
    /// </summary>
    void StartReturningToPatrol()
    {
        if (waypoints.Length == 0) return;
        
        // Find the closest waypoint to return to
        float closestDistance = float.MaxValue;
        int closestWaypointIndex = 0;
        
        for (int i = 0; i < waypoints.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, waypoints[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestWaypointIndex = i;
            }
        }
        
        // Set the return point and start returning
        patrolReturnPoint = waypoints[closestWaypointIndex].position;
        currentWaypoint = closestWaypointIndex;
        returningToPatrol = true;
        
        Debug.Log($"Enemy hasn't seen player for {returnToPatrolDelay} seconds, returning to patrol at waypoint {closestWaypointIndex}");
    }

    public void StartChase(Transform target)
    {
        chaseTarget = target;
        isChasing = true;
        
        // Report to alert system
        if (alertManager != null && target != null)
        {
            alertManager.OnPlayerDetected(target.position, 1f);
        }
    }

    public void StopChase()
    {
        chaseTarget = null;
        isChasing = false;
        
        // Reset the timer when chase stops
        timeSinceLastSeenPlayer = 0f;
        
        // Start investigating last known position
        if (hasLastKnownPosition)
        {
            investigatingLastPosition = true;
            memoryTimer = memoryDuration;
            isSearching = false; // Reset search state
            currentSearchPoint = 0;
            searchPointTimer = 0f;
            Debug.Log($"Enemy lost sight of player, investigating last known position: {lastKnownPlayerPosition}");
        }
    }
    
    /// <summary>
    /// Generates a search point around the last known position
    /// </summary>
    void GenerateSearchPoint()
    {
        // Create points in a circle around last known position
        float angle = (360f / searchPoints) * currentSearchPoint * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * searchRadius;
        currentSearchTarget = lastKnownPlayerPosition + (Vector3)offset;
        
        Debug.Log($"Enemy searching point {currentSearchPoint + 1}/{searchPoints} at {currentSearchTarget}");
    }
    
    /// <summary>
    /// Calculates avoidance direction when obstacle detected
    /// </summary>
    Vector2 GetAvoidanceDirection(Vector2 desiredDirection, float distanceToTarget)
    {
        // Reduce avoidance when close to target (within 2 units)
        float avoidanceStrengthMultiplier = Mathf.Clamp01(distanceToTarget / 2f);
        
        // If very close to target, disable avoidance completely
        if (distanceToTarget < 0.8f)
        {
            return desiredDirection;
        }
        
        // Cast rays in multiple directions to detect obstacles
        Vector2[] rayDirections = new Vector2[]
        {
            desiredDirection, // Forward
            Quaternion.Euler(0, 0, 30) * desiredDirection, // 30° right
            Quaternion.Euler(0, 0, -30) * desiredDirection, // 30° left
            Quaternion.Euler(0, 0, 60) * desiredDirection, // 60° right
            Quaternion.Euler(0, 0, -60) * desiredDirection, // 60° left
        };
        
        Vector2 avoidanceDir = Vector2.zero;
        
        foreach (Vector2 dir in rayDirections)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, avoidanceDistance, obstacleLayer);
            
            if (hit.collider != null)
            {
                // Push away from obstacle
                Vector2 awayFromObstacle = ((Vector2)transform.position - hit.point).normalized;
                float strength = 1f - (hit.distance / avoidanceDistance); // Closer = stronger
                avoidanceDir += awayFromObstacle * strength * avoidanceForce * avoidanceStrengthMultiplier;
            }
        }
        
        // Combine desired direction with avoidance
        if (avoidanceDir.magnitude > 0.1f)
        {
            return (desiredDirection + avoidanceDir).normalized;
        }
        
        return desiredDirection;
    }
    
    /// <summary>
    /// Checks if the sound listener has detected a sound and starts investigation
    /// </summary>
    void CheckSoundInvestigation()
    {
        if (soundListener != null && soundListener.IsInvestigatingSound())
        {
            // Get the sound position from the listener
            Vector3 soundPosition = soundListener.GetLastHeardSoundPosition();
            
            // Only investigate if we're not already chasing
            if (!isChasing)
            {
                investigatingSound = true;
                soundInvestigationTarget = soundPosition;
                
                Debug.Log($"Enemy heard sound at {soundPosition}, investigating...");
                
                // Report to alert system
                if (alertManager != null)
                {
                    alertManager.OnSoundHeard(soundPosition, 1f);
                }
            }
        }
        else if (investigatingSound && soundListener != null && !soundListener.IsInvestigatingSound())
        {
            // Stop investigating sound if listener is no longer investigating
            investigatingSound = false;
        }
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
            Debug.Log("Game Over! Enemy caught the player!");

            // Disable the player immediately
            other.gameObject.SetActive(false); 
            
            // Disable the enemy to prevent multiple trigger events
            this.enabled = false;
            
            // Optional: Load lose screen scene
            // Uncomment and add your scene name:
            // UnityEngine.SceneManagement.SceneManager.LoadScene("LoseScreen");
        }
    }
}

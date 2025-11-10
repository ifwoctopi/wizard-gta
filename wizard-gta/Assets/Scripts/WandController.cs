using UnityEngine;
using UnityEngine.InputSystem; 

/// <summary>
/// Handles magical shooting and now also handles the input events for shooting,
/// aiming the projectile toward the mouse cursor (for player) OR a specified
/// target position (for enemies/AI). This script is fully compatible with Rigidbody2D.
/// </summary>
public class WandController : MonoBehaviour
{
    [Header("Shooting Setup (For Editor)")]
    [Tooltip("Drag the Projectile Prefab here. Must have a Rigidbody2D component!")]
    public GameObject projectilePrefab;

    [Tooltip("The position and direction from which the projectile will be fired (e.g., the wand tip).")]
    public Transform firePoint;

    [Tooltip("A string tag to identify this entity (e.g., 'Player' or 'Enemy').")]
    public string entityTag = "Player";
    
    [Tooltip("The speed at which the projectile should travel.")]
    public float launchSpeed = 20f; 

    // --- Input System Integration ---
    private PlayerInput playerInput;
    private Camera mainCamera;
    private Collider2D entityCollider; 

    void Awake()
    {
        playerInput = new PlayerInput();
        mainCamera = Camera.main; 
        // Get the entity's own collider for collision ignoring
        entityCollider = GetComponent<Collider2D>(); 

        if (mainCamera == null)
        {
            Debug.LogError("WandController requires a Main Camera tagged in the scene!");
        }
        if (entityCollider == null)
        {
             Debug.LogError("WandController requires a Collider2D component attached to the entity!");
        }
    }

    void OnEnable()
    {
        // Only the player needs input enabled
        if (entityTag == "Player")
        {
            playerInput.Enable();
            playerInput.Actions.Shoot.performed += OnShootPerformed;
        }
    }

    void OnDisable()
    {
        if (entityTag == "Player")
        {
            playerInput.Actions.Shoot.performed -= OnShootPerformed;
            playerInput.Disable();
        }
    }

    // --- Player Input Execution ---
    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        Debug.Log($"{entityTag} pressed {context.control.displayName} to shoot!");
        // Player aims via mouse position
        Shoot(); 
    }
    
    // --------------------------------------------------------------------------------------------------
    // UNIVERSAL SHOOTING LOGIC
    // --------------------------------------------------------------------------------------------------

    /// <summary>
    /// PUBLIC METHOD FOR AI/ENEMIES: Fires a projectile toward a specified world position.
    /// </summary>
    /// <param name="targetWorldPosition">The world position to aim at (e.g., the Player's position).</param>
    public void Shoot(Vector3 targetWorldPosition)
    {
        if (projectilePrefab == null || firePoint == null || entityCollider == null)
        {
            Debug.LogError($"{gameObject.name} cannot shoot! Missing components.");
            return;
        }

        // 1. Normalize the target position for 2D (XY plane)
        Vector3 finalTargetPosition = targetWorldPosition;
        // Fix the Z depth axis to keep aiming flat on the XY plane.
        finalTargetPosition.z = firePoint.position.z; 
        
        // 2. Calculate the direction vector
        Vector3 aimVector3 = finalTargetPosition - firePoint.position;
        Vector2 targetDirection = new Vector2(aimVector3.x, aimVector3.y).normalized;

        // 3. Launch the projectile with the calculated direction and rotation
        LaunchProjectile(targetDirection);
    }

    /// <summary>
    /// PUBLIC METHOD FOR PLAYER INPUT: Calculates mouse position and calls the core Launch logic.
    /// </summary>
    public void Shoot()
    {
        if (mainCamera == null) return;
        
        // Calculate the mouse world position and pass it to the universal Shoot method.
        Shoot(mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
    }

    /// <summary>
    /// Core logic: Instantiates and launches the projectile in the given direction.
    /// </summary>
    private void LaunchProjectile(Vector2 targetDirection)
    {
        // 1. Calculate rotation based on the direction (for 2D sprites)
        float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        
        // 2. Instantiate and get components
        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, rotation);
        Projectile projectileScript = projectileGO.GetComponent<Projectile>();
        Rigidbody2D rb = projectileGO.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.velocity = targetDirection * launchSpeed;
        }
        else
        {
            Debug.LogError("Projectile prefab must have a Rigidbody2D component to be launched!");
        }
        
        if (projectileScript != null)
        {
            // Pass the entity's Collider2D to the projectile for collision ignoring
            projectileScript.SetShooter(entityTag, entityCollider); 
        }
    }

    /// <summary>
    /// This method is called by the Projectile script when this entity is hit.
    /// </summary>
    /// <param name="attackerTag">The tag of the entity that fired the projectile.</param>
    public void OnHit(string attackerTag)
    {
        if (attackerTag == entityTag) return;
        
        Debug.Log($"*** HIT DETECTED *** {gameObject.name} (Tag: {entityTag}) was successfully hit by: {attackerTag}.");
        // Add damage/health logic here.
    }
}
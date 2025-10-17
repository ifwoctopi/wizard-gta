using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    // A variable to set the movement speed in the Inspector
    public float moveSpeed = 5f;

    // --- Boundary Clamping ---
    [Header("Boundary Settings")]
    [Tooltip("The lowest Y-coordinate the player is allowed to reach in world units.")]
    public float minYBoundary = -5f; // Kept for consistency/potential horizontal boundaries

    // --- Variables for Dash ---
    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;
    private bool canDash = true;
    private bool isDashing = false;
    
    // A reference to the Rigidbody2D component
    private Rigidbody2D rb;

    // A vector to store the raw input
    private Vector2 movement;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on the player GameObject.");
        }
        
        // --- KEY CHANGE: Disable Gravity ---
        rb.gravityScale = 0f; 
        
        // Set the Rigidbody2D body type to Kinematic or ensure collision detection is handled
        // For a purely physics-driven movement with no gravity, setting gravityScale to 0 is often enough.
        // If the player starts floating away, you may want to ensure the Rigidbody2D's 
        // Body Type is set to 'Dynamic' (default) with zero gravity.
    }

    // Update is called once per frame (good for input)
    void Update()
    {
        // Prevent new actions if currently Dashing
        if (isDashing)
        {
            return;
        }

        // --- 1. Get 4-Directional Input ---
        // Now getting both horizontal AND vertical input for 4-directional movement
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        movement.Normalize(); // Ensures diagonal movement isn't faster

        // --- Dash Input (Left Shift) ---
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            // Determine dash direction. Default to horizontal if no input, 
            // but for 4-directional movement, we can dash in the current movement direction.
            Vector2 dashDirection;
            if (movement.magnitude > 0)
            {
                // Dash in the direction of the current movement input
                dashDirection = movement.normalized;
            }
            else
            {
                // If no input, perhaps use the last movement direction or a default, 
                // but for simplicity, we'll only allow a dash while moving.
                return; 
            }
            
            StartCoroutine(Dash(dashDirection));
        }
    }

    // FixedUpdate is called at a fixed time interval (good for physics updates)
    void FixedUpdate()
    {
        // Don't apply movement if performing a dash
        if (isDashing)
        {
            return;
        }

        // 2. Apply 4-Directional Movement
        // Uses both X and Y components of the input vector
        rb.velocity = movement * moveSpeed;
    }
    
    // LateUpdate is called after all Update and FixedUpdate calls.
    void LateUpdate()
    {
        // --- Boundary Clamping (Simplified) ---
        // Since there is no gravity, the landing check is unnecessary.
        // This can still clamp to prevent falling below a certain point if needed.
        if (transform.position.y < minYBoundary)
        {
            // Stop downward movement and set position exactly at the boundary
            Vector3 clampedPosition = transform.position;
            clampedPosition.y = minYBoundary;
            transform.position = clampedPosition;
            
            // NOTE: Since there's no gravity, we don't need to reset canJump/canDash here 
            // unless the boundary is meant to be a 'ground' where dash resets. 
            // For a free-moving player, dash typically resets only after its cooldown.
        }
    }

    // --- Ability Coroutine ---

    // The Dash coroutine is updated to accept a Vector2 direction for 4-directional dashing
    private IEnumerator Dash(Vector2 direction)
    {
        canDash = false;
        isDashing = true;
        
        // --- KEY CHANGE: Apply dash velocity in the full 2D direction ---
        rb.velocity = direction * dashSpeed;
        
        yield return new WaitForSeconds(dashDuration);

        // End the dash
        isDashing = false;
        // Since gravity is 0, we don't restore it.

        // Clear residual dash velocity if no input is being held
        if (movement.magnitude == 0)
        {
            rb.velocity = Vector2.zero;
        }

        yield return new WaitForSeconds(dashCooldown);
        
        // Reset the ability
        canDash = true;
    }
}
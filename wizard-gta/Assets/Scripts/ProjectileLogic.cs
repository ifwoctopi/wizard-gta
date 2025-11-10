using UnityEngine;

/// <summary>
/// Controls the projectile's movement and collision detection.
/// Attach this script to your projectile prefab.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    [Tooltip("Time in seconds before the projectile is automatically destroyed.")]
    public float lifetime = 3f;

    private string shooterTag;
    private Collider2D projectileCollider; 

    void Awake()
    {
        projectileCollider = GetComponent<Collider2D>(); 
        // Automatically destroy the projectile after its lifetime
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Called by the WandController to initialize the projectile's owner and ignore collision with them.
    /// </summary>
    /// <param name="tag">The entity tag of the shooter.</param>
    /// <param name="shooterCollider">The Collider2D of the shooter entity.</param>
    public void SetShooter(string tag, Collider2D shooterCollider)
    {
        shooterTag = tag;
        
        // Prevents the projectile from hitting or bouncing off the shooter
        if (shooterCollider != null && projectileCollider != null)
        {
            // Ignore physical collision between the projectile and the entity that shot it.
            Physics2D.IgnoreCollision(projectileCollider, shooterCollider, true);
        }
    }

    /// <summary>
    /// Use OnTriggerEnter2D for collision detection.
    /// Ensure the projectile's Collider2D is set to 'Is Trigger'.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // Get the WandController component from the object we hit.
        WandController targetCombat = other.GetComponent<WandController>();

        if (targetCombat != null)
        {
            // If the target is NOT the shooter, it's a valid hit.
            if (targetCombat.entityTag != shooterTag)
            {
                targetCombat.OnHit(shooterTag);
                Destroy(gameObject);
            }
            // If we hit the shooter (only happens if the trigger fires slightly delayed), destroy it.
            else 
            {
                 Destroy(gameObject);
            }
        }
        // If we hit something without a WandController (like a wall), destroy the projectile
        else 
        {
            Destroy(gameObject);
        }
    }
}
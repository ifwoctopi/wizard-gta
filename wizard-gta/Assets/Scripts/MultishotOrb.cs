using UnityEngine;

/// <summary>
/// Power-up orb that gives the player multishot ability for 10 seconds when touched.
/// Fires multiple projectiles in a spread pattern.
/// </summary>
public class MultishotOrb : MonoBehaviour
{
    [Header("Multishot Settings")]
    [Tooltip("Number of projectiles to fire (e.g., 3 = fires 3 projectiles)")]
    public int shotCount = 3;
    
    [Tooltip("Angle spread between projectiles in degrees (e.g., 15 = 15 degrees between each shot)")]
    public float spreadAngle = 15f;
    
    [Tooltip("Duration of the multishot powerup in seconds")]
    public float duration = 10f;
    
    [Header("Visual Effects")]
    [Tooltip("Particle effect to play when collected (optional)")]
    public GameObject collectEffect;
    
    [Tooltip("Sound effect to play when collected (optional)")]
    public AudioClip collectSound;
    
    private bool hasBeenCollected = false;
    private AudioSource audioSource;
    
    void Start()
    {
        // Get or create AudioSource for sound effects
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && collectSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player touched the orb
        if (hasBeenCollected) return;
        
        WandController wandController = other.GetComponent<WandController>();
        if (wandController != null && wandController.entityTag == "Player")
        {
            CollectOrb(wandController);
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Also check collision (in case trigger is disabled)
        if (hasBeenCollected) return;
        
        WandController wandController = collision.gameObject.GetComponent<WandController>();
        if (wandController != null && wandController.entityTag == "Player")
        {
            CollectOrb(wandController);
        }
    }
    
    private void CollectOrb(WandController wandController)
    {
        if (hasBeenCollected) return;
        hasBeenCollected = true;
        
        // Apply multishot to player's wand
        wandController.ActivateMultishot(shotCount, spreadAngle, duration);
        
        Debug.Log($"Multishot activated! Firing {shotCount} projectiles for {duration} seconds.");
        
        // Play sound effect
        if (audioSource != null && collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position, 1f);

        }

        // Spawn particle effect if available
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
        
        // Hide the orb (make it disappear)
        gameObject.SetActive(false);
    }
}


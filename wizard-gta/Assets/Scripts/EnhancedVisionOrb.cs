using UnityEngine;

/// <summary>
/// Power-up orb that enhances the player's vision cone for 10 seconds when touched.
/// Makes the vision cone bigger (increases FOV).
/// </summary>
public class EnhancedVisionOrb : MonoBehaviour
{
    [Header("Enhanced Vision Settings")]
    [Tooltip("FOV multiplier (e.g., 2.0 = double the vision cone size)")]
    public float fovMultiplier = 2.0f;
    
    [Tooltip("Duration of the enhanced vision powerup in seconds")]
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
        
        // Try to find PlayerFOV component on the player or its children
        PlayerFOV playerFOV = other.GetComponent<PlayerFOV>();
        if (playerFOV == null)
        {
            // Try to find it in children (in case FOV is on a child object)
            playerFOV = other.GetComponentInChildren<PlayerFOV>();
        }
        
        if (playerFOV != null)
        {
            CollectOrb(playerFOV);
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Also check collision (in case trigger is disabled)
        if (hasBeenCollected) return;
        
        // Try to find PlayerFOV component on the player or its children
        PlayerFOV playerFOV = collision.gameObject.GetComponent<PlayerFOV>();
        if (playerFOV == null)
        {
            // Try to find it in children (in case FOV is on a child object)
            playerFOV = collision.gameObject.GetComponentInChildren<PlayerFOV>();
        }
        
        if (playerFOV != null)
        {
            CollectOrb(playerFOV);
        }
    }
    
    private void CollectOrb(PlayerFOV playerFOV)
    {
        if (hasBeenCollected) return;
        hasBeenCollected = true;
        
        // Apply enhanced vision to player
        playerFOV.ActivateEnhancedVision(fovMultiplier, duration);
        
        Debug.Log($"Enhanced vision activated! FOV increased by {fovMultiplier}x for {duration} seconds.");
        
        // Play sound effect
        if (audioSource != null && collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
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


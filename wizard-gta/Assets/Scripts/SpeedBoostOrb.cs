using UnityEngine;

/// <summary>
/// Power-up orb that gives the player a temporary speed boost when touched
/// </summary>
public class SpeedBoostOrb : MonoBehaviour
{
    [Header("Speed Boost Settings")]
    [Tooltip("Speed multiplier (e.g., 2.0 = double speed)")]
    public float speedMultiplier = 2.0f;
    
    [Tooltip("Duration of the speed boost in seconds")]
    public float boostDuration = 10f;
    
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
        
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            CollectOrb(player);
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Also check collision (in case trigger is disabled)
        if (hasBeenCollected) return;
        
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            CollectOrb(player);
        }
    }
    
    private void CollectOrb(PlayerController player)
    {
        if (hasBeenCollected) return;
        hasBeenCollected = true;
        
        // Apply speed boost to player
        player.ApplySpeedBoost(speedMultiplier, boostDuration);
        
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
        // Option 1: Disable the GameObject
        gameObject.SetActive(false);
        
        // Option 2: Destroy after sound finishes (if you want sound to play)
        // if (audioSource != null && collectSound != null)
        // {
        //     Destroy(gameObject, collectSound.length);
        // }
        // else
        // {
        //     Destroy(gameObject);
        // }
    }
}


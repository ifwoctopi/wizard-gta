using UnityEngine;

/// <summary>
/// Defines how different surfaces affect the final noise level.
/// </summary>
public enum SurfaceType
{
    Grass,      // Default surface (1.0x multiplier)
    Carpet,     // Quietest (e.g., 0.5x multiplier)
    Stone,      // Slightly louder (e.g., 1.15x multiplier)
    Metal,      // Loudest (e.g., 1.5x multiplier)
    Glass,      // Breaking glass (highest base intensity)
    Default     // Standard noise (same as Grass)
}

/// <summary>
/// Component attached to objects/player that can generate sound events.
/// </summary>
public class SoundEmitter : MonoBehaviour
{
    [Header("Base Noise Settings")]
    [Tooltip("The standard sound intensity emitted by this object (e.g., 3 for footsteps).")]
    public float BaseNoiseIntensity = 3f;

    [Tooltip("The maximum distance the sound can travel before fully dissipating (e.g., 2 units).")]
    public float MaxSoundRange = 2f;

    [Header("Surface Settings")]
    [Tooltip("The type of surface this emitter is currently on.")]
    public SurfaceType CurrentSurface = SurfaceType.Grass;
    
    [Tooltip("Enable automatic surface detection (requires SurfaceDetector component)")]
    public bool useAutomaticSurfaceDetection = true;

    [Header("Audio Settings")]
    [Tooltip("Audio clip for footstep sounds")]
    public AudioClip footstepSound;
    
    [Tooltip("Base volume for footstep sounds (will be multiplied by surface type)")]
    [Range(0f, 1f)]
    public float baseFootstepVolume = 0.5f;
    
    [Tooltip("AudioSource component for playing sounds (will be auto-created if not assigned)")]
    public AudioSource audioSource;

    [Header("Debug")]
    [Tooltip("If enabled, logs footstep/loud noise emissions with computed intensity/range.")]
    public bool enableEmitterDebugLogs = false;
    
    private SurfaceType lastSurfaceType = SurfaceType.Grass;
    private bool isPlayerMoving = false;
    private float lastMovementTime = 0f;
    private float fadeOutDuration = 0.3f; // How fast it shrinks when stopping (in seconds)
    
    private void Awake()
    {
        // Get or create AudioSource component
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Configure AudioSource for continuous footstep playback
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.loop = true; // Loop the footstep audio
        audioSource.clip = footstepSound;
    }
    
    private void Update()
    {
        // Update volume if surface type changed while playing
        if (audioSource != null && audioSource.isPlaying)
        {
            SurfaceType currentSurface = useAutomaticSurfaceDetection ? GetDetectedSurface() : CurrentSurface;
            if (currentSurface != lastSurfaceType)
            {
                UpdateFootstepVolume();
                lastSurfaceType = currentSurface;
            }
        }
        
        // Check if player is moving (by checking if footstep audio is playing)
        bool wasMoving = isPlayerMoving;
        isPlayerMoving = audioSource != null && audioSource.isPlaying;
        
        if (isPlayerMoving)
        {
            lastMovementTime = Time.time;
        }
    }

    /// <summary>
    /// Gets the multiplier for the current surface type.
    /// In a final game, this data would ideally be stored in a ScriptableObject/Data Table.
    /// </summary>
    private float GetSurfaceMultiplier(SurfaceType surface)
    {
        switch (surface)
        {
            case SurfaceType.Grass: return 1.0f; // Default normal sound
            case SurfaceType.Carpet: return 0.5f; // Quiet
            case SurfaceType.Stone: return 2.0f; // 2x louder than grass
            case SurfaceType.Metal: return 1.5f; // Loud
            case SurfaceType.Glass: return 2.0f; // Breaking glass should be very loud
            case SurfaceType.Default: return 1.0f; // Same as Grass
            default: return 1.0f;
        }
    }

    /// <summary>
    /// Starts playing continuous footstep audio. Called when player starts moving.
    /// </summary>
    public void StartFootstepAudio()
    {
        if (footstepSound != null && audioSource != null && !audioSource.isPlaying)
        {
            UpdateFootstepVolume();
            audioSource.Play();
            
            if (enableEmitterDebugLogs)
            {
                Debug.Log($"[SoundEmitter:{gameObject.name}] Started footstep audio");
            }
        }
    }
    
    /// <summary>
    /// Pauses footstep audio. Called when player stops moving.
    /// </summary>
    public void PauseFootstepAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            
            if (enableEmitterDebugLogs)
            {
                Debug.Log($"[SoundEmitter:{gameObject.name}] Paused footstep audio");
            }
        }
    }
    
    /// <summary>
    /// Updates the footstep volume based on current surface type
    /// </summary>
    private void UpdateFootstepVolume()
    {
        if (audioSource == null) return;
        
        SurfaceType surfaceToUse = useAutomaticSurfaceDetection ? GetDetectedSurface() : CurrentSurface;
        float multiplier = GetSurfaceMultiplier(surfaceToUse);
        float finalVolume = baseFootstepVolume * multiplier;
        finalVolume = Mathf.Clamp01(finalVolume);
        
        audioSource.volume = finalVolume;
        
        if (enableEmitterDebugLogs)
        {
            Debug.Log($"[SoundEmitter:{gameObject.name}] Updated volume | surface={surfaceToUse} | volume={finalVolume:F2}");
        }
    }
    
    /// <summary>
    /// Generates a footstep sound event for AI detection. Called by player movement logic at intervals.
    /// </summary>
    public void EmitFootstep()
    {
        // Use automatic surface detection if enabled
        SurfaceType surfaceToUse = useAutomaticSurfaceDetection ? GetDetectedSurface() : CurrentSurface;
        
        float multiplier = GetSurfaceMultiplier(surfaceToUse);
        float finalIntensity = BaseNoiseIntensity * multiplier;
        float finalRange = MaxSoundRange * multiplier; // scale radius with surface

        // Generate gameplay sound event for AI detection
        if (NoiseManager.Instance != null)
        {
            NoiseManager.Instance.GenerateSound(transform.position, finalIntensity, finalRange);
            if (enableEmitterDebugLogs)
            {
                Debug.Log($"[SoundEmitter:{gameObject.name}] Footstep | surface={surfaceToUse} | mult={multiplier:F2} | intensity={finalIntensity:F2} | range={finalRange:F2}");
            }
        }
    }

    /// <summary>
    /// Generates a loud, one-off sound (e.g., breaking an object).
    /// </summary>
    public void EmitLoudNoise(float loudBaseIntensity, float loudMaxRange)
    {
        // Use automatic surface detection if enabled
        SurfaceType surfaceToUse = useAutomaticSurfaceDetection ? GetDetectedSurface() : CurrentSurface;
        
        float multiplier = GetSurfaceMultiplier(surfaceToUse);
        float finalIntensity = loudBaseIntensity * multiplier;
        float finalRange = loudMaxRange * multiplier; // scale radius with surface

        if (NoiseManager.Instance != null)
        {
            NoiseManager.Instance.GenerateSound(transform.position, finalIntensity, finalRange);
            if (enableEmitterDebugLogs)
            {
                Debug.Log($"[SoundEmitter:{gameObject.name}] Loud | surface={surfaceToUse} | mult={multiplier:F2} | intensity={finalIntensity:F2} | range={finalRange:F2}");
            }
        }
    }
    
    /// <summary>
    /// Gets the surface type from the SurfaceDetector if available
    /// </summary>
    private SurfaceType GetDetectedSurface()
    {
        SurfaceDetector detector = GetComponent<SurfaceDetector>();
        if (detector != null)
        {
            return detector.GetCurrentSurface();
        }
        return CurrentSurface; // Fallback to manual setting
    }
    
    [Header("Debug Visualization")]
    [Tooltip("Show the sound radius in the Scene view using Gizmos")]
    public bool showSoundRadius = true;
    
    [Tooltip("Show how walls block the sound radius")]
    public bool showWallBlocking = true;
    
    [Tooltip("Number of rays to cast for wall detection (more = smoother but slower)")]
    [Range(8, 64)]
    public int wallDetectionRays = 32;
    
    [Tooltip("Layer mask for walls that block sound")]
    public LayerMask wallLayerMask = -1;
    
    /// <summary>
    /// Draws the sound radius in the Scene view, showing where walls block it
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showSoundRadius) return;
        
        // Calculate fade out progress (0 = fully visible, 1 = fully hidden)
        float timeSinceStopped = Time.time - lastMovementTime;
        float fadeProgress = Mathf.Clamp01(timeSinceStopped / fadeOutDuration);
        
        // Only show if moving or within fade out period
        if (!isPlayerMoving && fadeProgress >= 1f)
        {
            return; // Don't draw if stopped and fade out is complete
        }
        
        SurfaceType surfaceToUse = useAutomaticSurfaceDetection ? GetDetectedSurface() : CurrentSurface;
        float multiplier = GetSurfaceMultiplier(surfaceToUse);
        float baseFinalRange = MaxSoundRange * multiplier;
        
        // Shrink the radius during fade out
        float scaleFactor = 1f - fadeProgress; // Goes from 1.0 to 0.0
        float finalRange = baseFinalRange * scaleFactor;
        
        // Fade out the alpha as well
        float alphaMultiplier = scaleFactor;
        
        // If wall blocking visualization is enabled, draw the radius with wall occlusion
        if (showWallBlocking)
        {
            DrawSoundRadiusWithWalls(finalRange, alphaMultiplier);
        }
        else
        {
            // Draw simple circles without wall checking
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f * alphaMultiplier); // Orange, semi-transparent
            Gizmos.DrawWireSphere(transform.position, finalRange);
            
            Gizmos.color = new Color(1f, 0.3f, 0f, 0.5f * alphaMultiplier); // Darker orange
            Gizmos.DrawWireSphere(transform.position, MaxSoundRange * scaleFactor);
        }
        
        // Draw the center point (fade out too)
        Gizmos.color = new Color(1f, 0.2f, 0f, 1f * alphaMultiplier); // Solid orange
        Gizmos.DrawWireSphere(transform.position, 0.2f * scaleFactor);
    }
    
    /// <summary>
    /// Draws the sound radius showing where walls block it
    /// </summary>
    private void DrawSoundRadiusWithWalls(float maxRange, float alphaMultiplier = 1f)
    {
        Vector3 center = transform.position;
        float angleStep = 360f / wallDetectionRays;
        
        // Get wall layer mask from NoiseManager if available, otherwise use the one set on this component
        LayerMask effectiveWallMask = wallLayerMask;
        if (NoiseManager.Instance != null)
        {
            effectiveWallMask = NoiseManager.Instance.WallLayerMask;
        }
        
        // Draw rays in all directions, stopping at walls
        for (int i = 0; i < wallDetectionRays; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            
            // Cast ray to check for walls
            RaycastHit2D hit = Physics2D.Raycast(center, direction, maxRange, effectiveWallMask);
            
            float drawDistance;
            Color rayColor;
            
            if (hit.collider != null)
            {
                // Hit a wall - draw up to the wall
                drawDistance = hit.distance;
                rayColor = new Color(1f, 0.3f, 0f, 0.6f * alphaMultiplier); // Darker orange for blocked
                
                // Draw a small marker at the wall hit point
                Gizmos.color = new Color(1f, 0f, 0f, 0.8f * alphaMultiplier); // Red at wall
                Gizmos.DrawWireSphere(hit.point, 0.15f);
            }
            else
            {
                // No wall - draw full range
                drawDistance = maxRange;
                rayColor = new Color(1f, 0.6f, 0f, 0.4f * alphaMultiplier); // Brighter orange for unblocked
            }
            
            // Draw the ray
            Gizmos.color = rayColor;
            Vector3 endPoint = center + direction * drawDistance;
            Gizmos.DrawLine(center, endPoint);
        }
        
        // Draw the base range circle (unblocked)
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f * alphaMultiplier);
        Gizmos.DrawWireSphere(center, MaxSoundRange);
    }
}

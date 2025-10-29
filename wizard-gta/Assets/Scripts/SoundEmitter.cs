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

    [Header("Debug")]
    [Tooltip("If enabled, logs footstep/loud noise emissions with computed intensity/range.")]
    public bool enableEmitterDebugLogs = false;

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
    /// Generates a footstep sound event. Called by player movement logic.
    /// </summary>
    public void EmitFootstep()
    {
        // Use automatic surface detection if enabled
        SurfaceType surfaceToUse = useAutomaticSurfaceDetection ? GetDetectedSurface() : CurrentSurface;
        
        float multiplier = GetSurfaceMultiplier(surfaceToUse);
        float finalIntensity = BaseNoiseIntensity * multiplier;
        float finalRange = MaxSoundRange * multiplier; // scale radius with surface

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
}

using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Defines a sound event that listeners react to.
/// </summary>
public struct SoundEvent
{
    public Vector3 Position;
    public float Intensity; // The base volume of the sound at its source.
    public float MaxRange;  // How far the sound can theoretically travel.
}

/// <summary>
/// A global singleton to manage all sound propagation, wall dampening, and debug visualization.
/// </summary>
public class NoiseManager : MonoBehaviour
{
    public static NoiseManager Instance { get; private set; }

    // Event invoked when a sound is generated. Listeners subscribe to this.
    public static event Action<SoundEvent> OnSoundGenerated;

    [Header("Wall Dampening Settings")]
    [Tooltip("Layer Mask containing all wall/obstacle colliders.")]
    public LayerMask WallLayerMask;

    [Tooltip("The percentage reduction in sound intensity when passing through a wall (0.0 to 1.0).")]
    [Range(0.1f, 1.0f)]
    public float WallDampeningFactor = 0.5f;

    [Header("Debug Settings")]
    [Tooltip("Enable to show the sound radius using Gizmos in the Scene view.")]
    public bool IsDebugMode = true;
    
    [Tooltip("Show hearing ranges of all SoundListeners")]
    public bool ShowListenerRanges = true;
    
    [Tooltip("Show wall obstruction lines")]
    public bool ShowWallObstructions = true;
    
    [Tooltip("If enabled, will log generated sound events to the Console.")]
    public bool EnableDebugLogs = false;
    
    [Tooltip("How long to show sound events (in seconds)")]
    public float soundEventDisplayTime = 2f;
    
    private SoundEvent? lastSoundEvent = null; // Store the last sound for debug drawing
    private float lastSoundTime = 0f; // When the last sound was generated
    private List<SoundListener> activeListeners = new List<SoundListener>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Find all active SoundListeners in the scene
        RefreshActiveListeners();
    }
    
    /// <summary>
    /// Refreshes the list of active SoundListeners in the scene
    /// </summary>
    public void RefreshActiveListeners()
    {
        activeListeners.Clear();
        SoundListener[] listeners = FindObjectsOfType<SoundListener>();
        activeListeners.AddRange(listeners);
    }
    
    /// <summary>
    /// Debug method to test sound visualization - call this from a button or key press
    /// </summary>
    [ContextMenu("Test Sound Visualization")]
    public void TestSoundVisualization()
    {
        if (IsDebugMode)
        {
            GenerateSound(transform.position, 3f, 2f);
            Debug.Log("Test sound generated at NoiseManager position!");
        }
    }

    /// <summary>
    /// Generates a sound event and broadcasts it to all listeners.
    /// </summary>
    /// <param name="position">The source location of the noise.</param>
    /// <param name="baseIntensity">The initial volume of the noise (e.g., 50).</param>
    /// <param name="range">The maximum physical range of the sound (e.g., 20m).</param>
    public void GenerateSound(Vector3 position, float baseIntensity, float range)
    {
        // 1. Calculate dampened intensity (This currently returns baseIntensity, as wall checks are in Listener)
        float dampenedIntensity = CalculateDampening(position, baseIntensity);

        SoundEvent newEvent = new SoundEvent
        {
            Position = position,
            Intensity = dampenedIntensity,
            MaxRange = range
        };

        // 2. Broadcast the event
        OnSoundGenerated?.Invoke(newEvent);

        if (EnableDebugLogs)
        {
            Debug.Log($"[NoiseManager] Sound generated at {position} | intensity={dampenedIntensity:F2} | range={range:F2}");
        }

        // 3. Store for debug visualization (This is the critical step for showing the radius)
        lastSoundEvent = newEvent;
        lastSoundTime = Time.time;
    }

    /// <summary>
    /// Calculates the effective sound intensity after factoring in potential wall obstruction.
    /// </summary>
    private float CalculateDampening(Vector3 position, float baseIntensity)
    {
        // We defer the raycast/wall check to the Listener for accuracy, so we return the base intensity here.
        return baseIntensity;
    }
    
    /// <summary>
    /// Checks if a wall blocks the path between two points using raycast
    /// </summary>
    /// <param name="from">Source position</param>
    /// <param name="to">Target position</param>
    /// <returns>True if wall blocks the path, false if clear</returns>
    public bool IsPathBlockedByWall(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;
        float distance = Vector3.Distance(from, to);
        
        // Cast a ray from source to target
        RaycastHit2D hit = Physics2D.Raycast(from, direction, distance, WallLayerMask);
        
        // If we hit something, the path is blocked
        return hit.collider != null;
    }

    /// <summary>
    /// Draws the sound radius using Gizmos in the Unity Scene View.
    /// This only executes in the editor when the NoiseManager object is selected or the Gizmos are enabled.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!IsDebugMode)
            return;

        // Draw the last sound event if it's still within display time
        if (lastSoundEvent != null && (Time.time - lastSoundTime) < soundEventDisplayTime)
        {
            // Draw the sphere representing the sound's maximum range (semi-transparent orange)
            Gizmos.color = new Color(1f, 0.6f, 0f, 0.3f); 
            Gizmos.DrawSphere(lastSoundEvent.Value.Position, lastSoundEvent.Value.MaxRange);

            // Draw a solid circle at the base indicating the sound source
            Gizmos.color = new Color(1f, 0.4f, 0f, 1f); 
            Gizmos.DrawWireSphere(lastSoundEvent.Value.Position, 0.5f);

            // Draw intensity-based circles
            float intensity = lastSoundEvent.Value.Intensity;
            for (int i = 1; i <= 5; i++)
            {
                float radius = (intensity / 5f) * i;
                if (radius <= lastSoundEvent.Value.MaxRange)
                {
                    Gizmos.color = new Color(1f, 0.6f, 0f, 0.2f - (i * 0.03f));
                    Gizmos.DrawWireSphere(lastSoundEvent.Value.Position, radius);
                }
            }
        }

        // Draw listener hearing ranges
        if (ShowListenerRanges)
        {
            foreach (var listener in activeListeners)
            {
                if (listener != null)
                {
                    // Draw hearing sensitivity range
                    Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
                    Gizmos.DrawWireSphere(listener.transform.position, listener.HearingSensitivityThreshold);
                    
                    // Draw max hearing range (approximate)
                    Gizmos.color = new Color(0f, 0.8f, 0f, 0.1f);
                    Gizmos.DrawWireSphere(listener.transform.position, 6f); // Max reasonable hearing range
                }
            }
        }
    }
}

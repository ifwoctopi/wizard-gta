using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Global alert and memory system that tracks player detection across the entire level.
/// Manages alert levels, heat tracking, and shared memory between all guards.
/// </summary>
public class AlertManager : MonoBehaviour
{
    public static AlertManager Instance { get; private set; }
    
    [Header("Alert Level Settings")]
    [Tooltip("Maximum alert level (0 = calm, 1 = high alert)")]
    [Range(0f, 1f)]
    public float maxAlertLevel = 1f;
    
    [Tooltip("How fast alert level decreases when player is hidden")]
    public float alertDecayRate = 0.5f;
    
    [Tooltip("Minimum alert level (never goes below this)")]
    [Range(0f, 1f)]
    public float minAlertLevel = 0f;
    
    [Header("Heat System")]
    [Tooltip("How fast heat level increases when player is detected")]
    public float heatIncreaseRate = 2f;
    
    [Tooltip("How fast heat level decreases when player is hidden")]
    public float heatDecayRate = 1f;
    
    [Tooltip("Maximum heat level")]
    public float maxHeatLevel = 100f;
    
    [Header("Memory System")]
    [Tooltip("How long guards remember the last seen position")]
    public float memoryDuration = 10f;
    
    [Tooltip("How long to remember sounds")]
    public float soundMemoryDuration = 5f;
    
    [Header("Debug")]
    [Tooltip("Show alert level in debug UI")]
    public bool showDebugUI = true;
    
    [Tooltip("Enable debug logs")]
    public bool enableDebugLogs = false;
    
    // Current alert and heat levels
    private float currentAlertLevel = 0f;
    private float currentHeatLevel = 0f;
    
    // Memory system
    private Vector3 lastKnownPlayerPosition;
    private float lastSeenTime;
    private bool hasLastKnownPosition = false;
    
    private List<SoundMemory> soundMemories = new List<SoundMemory>();
    
    // Events for other systems to subscribe to
    public static event Action<float> OnAlertLevelChanged;
    public static event Action<float> OnHeatLevelChanged;
    public static event Action<Vector3> OnLastKnownPositionUpdated;
    public static event Action<Vector3, GameObject> OnBackupCalled; // position, guard who called
    public static event Action<Vector3, float> OnPlayerDetectedGlobal; // position, intensity - triggers all enemies to investigate
    
    // Struct for storing sound memories
    [System.Serializable]
    public struct SoundMemory
    {
        public Vector3 position;
        public float intensity;
        public float timeHeard;
        public float duration;
    }
    
    // Alert level states
    public enum AlertState
    {
        Calm,           // 0.0 - 0.3
        Suspicious,     // 0.3 - 0.6
        Alert,          // 0.6 - 0.8
        HighAlert       // 0.8 - 1.0
    }
    
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
    
    private void Update()
    {
        // Decay alert level over time
        if (currentAlertLevel > minAlertLevel)
        {
            currentAlertLevel -= alertDecayRate * Time.deltaTime;
            currentAlertLevel = Mathf.Max(currentAlertLevel, minAlertLevel);
            OnAlertLevelChanged?.Invoke(currentAlertLevel);
        }
        
        // Decay heat level over time
        if (currentHeatLevel > 0f)
        {
            currentHeatLevel -= heatDecayRate * Time.deltaTime;
            currentHeatLevel = Mathf.Max(currentHeatLevel, 0f);
            OnHeatLevelChanged?.Invoke(currentHeatLevel);
        }
        
        // Clean up old sound memories
        CleanupOldSoundMemories();
    }
    
    /// <summary>
    /// Called when a guard detects the player
    /// </summary>
    public void OnPlayerDetected(Vector3 playerPosition, float detectionIntensity = 1f, GameObject callingGuard = null)
    {
        // Update last known position
        lastKnownPlayerPosition = playerPosition;
        lastSeenTime = Time.time;
        hasLastKnownPosition = true;
        
        // Increase alert level
        float alertIncrease = detectionIntensity * 0.3f;
        currentAlertLevel = Mathf.Min(currentAlertLevel + alertIncrease, maxAlertLevel);
        
        // Increase heat level
        float heatIncrease = detectionIntensity * heatIncreaseRate;
        currentHeatLevel = Mathf.Min(currentHeatLevel + heatIncrease, maxHeatLevel);
        
        // Notify other systems
        OnAlertLevelChanged?.Invoke(currentAlertLevel);
        OnHeatLevelChanged?.Invoke(currentHeatLevel);
        OnLastKnownPositionUpdated?.Invoke(playerPosition);
        
        // Notify all enemies to investigate the player position
        OnPlayerDetectedGlobal?.Invoke(playerPosition, detectionIntensity);
        
        if (enableDebugLogs)
        {
            Debug.Log($"[AlertManager] Player detected! Alert: {currentAlertLevel:F2}, Heat: {currentHeatLevel:F2}");
        }
    }
    
    /// <summary>
    /// Called when a guard calls for backup (other guards respond to this location)
    /// </summary>
    public void CallForBackup(Vector3 playerPosition, GameObject callingGuard)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[AlertManager] BACKUP CALLED at {playerPosition} by {callingGuard.name}!");
        }
        
        // Notify all guards
        OnBackupCalled?.Invoke(playerPosition, callingGuard);
        
        // Also trigger detection to increase alert levels
        OnPlayerDetected(playerPosition, 1.5f, callingGuard); // Higher intensity for backup calls
    }
    
    /// <summary>
    /// Called when a guard hears a sound
    /// </summary>
    public void OnSoundHeard(Vector3 soundPosition, float soundIntensity)
    {
        // Add to sound memories
        SoundMemory newMemory = new SoundMemory
        {
            position = soundPosition,
            intensity = soundIntensity,
            timeHeard = Time.time,
            duration = soundMemoryDuration
        };
        soundMemories.Add(newMemory);
        
        // Slight increase in alert level for sounds
        float alertIncrease = soundIntensity * 0.1f;
        currentAlertLevel = Mathf.Min(currentAlertLevel + alertIncrease, maxAlertLevel);
        
        OnAlertLevelChanged?.Invoke(currentAlertLevel);
        
        if (enableDebugLogs)
        {
            Debug.Log($"[AlertManager] Sound heard! Alert: {currentAlertLevel:F2}");
        }
    }
    
    /// <summary>
    /// Gets the current alert level (0-1)
    /// </summary>
    public float GetAlertLevel()
    {
        return currentAlertLevel;
    }
    
    /// <summary>
    /// Gets the current heat level (0-maxHeatLevel)
    /// </summary>
    public float GetHeatLevel()
    {
        return currentHeatLevel;
    }
    
    /// <summary>
    /// Gets the current alert state
    /// </summary>
    public AlertState GetAlertState()
    {
        if (currentAlertLevel < 0.3f) return AlertState.Calm;
        if (currentAlertLevel < 0.6f) return AlertState.Suspicious;
        if (currentAlertLevel < 0.8f) return AlertState.Alert;
        return AlertState.HighAlert;
    }
    
    /// <summary>
    /// Gets the last known player position
    /// </summary>
    public Vector3 GetLastKnownPlayerPosition()
    {
        return lastKnownPlayerPosition;
    }
    
    /// <summary>
    /// Checks if we have a recent last known position
    /// </summary>
    public bool HasRecentLastKnownPosition()
    {
        return hasLastKnownPosition && (Time.time - lastSeenTime) < memoryDuration;
    }
    
    /// <summary>
    /// Gets all recent sound memories
    /// </summary>
    public List<SoundMemory> GetRecentSoundMemories()
    {
        List<SoundMemory> recentMemories = new List<SoundMemory>();
        float currentTime = Time.time;
        
        foreach (var memory in soundMemories)
        {
            if (currentTime - memory.timeHeard < memory.duration)
            {
                recentMemories.Add(memory);
            }
        }
        
        return recentMemories;
    }
    
    /// <summary>
    /// Cleans up old sound memories
    /// </summary>
    private void CleanupOldSoundMemories()
    {
        float currentTime = Time.time;
        soundMemories.RemoveAll(memory => currentTime - memory.timeHeard > memory.duration);
    }
    
    /// <summary>
    /// Resets all alert and heat levels (useful for level restart)
    /// </summary>
    public void ResetAlertSystem()
    {
        currentAlertLevel = minAlertLevel;
        currentHeatLevel = 0f;
        hasLastKnownPosition = false;
        soundMemories.Clear();
        
        OnAlertLevelChanged?.Invoke(currentAlertLevel);
        OnHeatLevelChanged?.Invoke(currentHeatLevel);
        
        if (enableDebugLogs)
        {
            Debug.Log("[AlertManager] Alert system reset");
        }
    }
    
    /// <summary>
    /// Draws debug UI showing alert and heat levels
    /// </summary>
    private void OnGUI()
    {
        if (!showDebugUI) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label($"Alert Level: {currentAlertLevel:F2} ({GetAlertState()})");
        GUILayout.Label($"Heat Level: {currentHeatLevel:F1}/{maxHeatLevel}");
        
        if (HasRecentLastKnownPosition())
        {
            GUILayout.Label($"Last Seen: {lastKnownPlayerPosition}");
            GUILayout.Label($"Time Ago: {Time.time - lastSeenTime:F1}s");
        }
        else
        {
            GUILayout.Label("No recent sightings");
        }
        
        GUILayout.Label($"Sound Memories: {soundMemories.Count}");
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}

using UnityEngine;

/// <summary>
/// Automatically detects the surface type under the player and updates the SoundEmitter accordingly.
/// This component should be attached to the same GameObject as the SoundEmitter.
/// </summary>
public class SurfaceDetector : MonoBehaviour
{
    [Header("Surface Detection Settings")]
    [Tooltip("Distance to check below the player for surface detection")]
    public float detectionDistance = 5f;
    
    [Tooltip("Layer mask for surfaces that can be detected")]
    public LayerMask surfaceLayerMask = -1;
    
    [Tooltip("How often to check for surface changes (in seconds)")]
    public float checkInterval = 0.1f;
    [Tooltip("Small downward offset to start the ray below the player's own collider to avoid self-hits")]
    public float rayOriginDownOffset = 0.1f;
    
    [Header("Surface Tags")]
    [Tooltip("Tag for grass surfaces (default)")]
    public string grassTag = "Grass";
    [Tooltip("Tag for carpet surfaces")]
    public string carpetTag = "Carpet";
    [Tooltip("Tag for stone surfaces")]
    public string stoneTag = "Stone";
    [Tooltip("Tag for metal surfaces")]
    public string metalTag = "Metal";
    [Tooltip("Tag for glass surfaces")]
    public string glassTag = "Glass";
    
    private SoundEmitter soundEmitter;
    private SurfaceType currentDetectedSurface = SurfaceType.Grass;
    private float lastCheckTime;

    [Header("Layer Mapping (optional)")]
    [Tooltip("If true, layer names will be used to detect surfaces when tags are missing.")]
    public bool useLayerNamesForDetection = true;
    [Tooltip("Layer name for grass surfaces")] public string grassLayerName = "Default";
    [Tooltip("Layer name for carpet surfaces")] public string carpetLayerName = "Carpet";
    [Tooltip("Layer name for stone surfaces")] public string stoneLayerName = "Stone";
    [Tooltip("Layer name for metal surfaces")] public string metalLayerName = "Metal";
    [Tooltip("Layer name for glass surfaces")] public string glassLayerName = "Glass";
    
    [Header("Debug")] public bool enableDebugLogs = true;
    
    void Start()
    {
        soundEmitter = GetComponent<SoundEmitter>();
        if (soundEmitter == null)
        {
            Debug.LogError("SurfaceDetector requires a SoundEmitter component on the same GameObject!");
        }
        else
        {
            Debug.Log($"[SurfaceDetector] Initialized on {gameObject.name}");
            Debug.Log($"[SurfaceDetector] Detection Distance: {detectionDistance}");
            Debug.Log($"[SurfaceDetector] Looking for Stone tag: '{stoneTag}'");
            Debug.Log($"[SurfaceDetector] Layer Mask: {surfaceLayerMask.value}");
        }
    }
    
    void Update()
    {
        // Check for surface changes at intervals
        if (Time.time - lastCheckTime >= checkInterval)
        {
            DetectSurface();
            lastCheckTime = Time.time;
        }
    }
    
    /// <summary>
    /// Detects the surface type below the player using raycast
    /// </summary>
    private void DetectSurface()
    {
        // Build an effective mask that excludes the player's own layer to avoid self-hits
        int playerLayer = gameObject.layer;
        int effectiveMask = surfaceLayerMask & ~(1 << playerLayer);

        // Cast a ray downward from slightly below the player's position to further avoid self-hits
        Vector3 origin = transform.position + Vector3.down * rayOriginDownOffset;
        
        // Use RaycastAll to detect triggers (for walkable surfaces like stairs)
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, detectionDistance, effectiveMask);
        
        if (enableDebugLogs)
        {
            Debug.Log($"[SurfaceDetector] Raycast found {hits.Length} hits from position {origin}");
        }
        
        // Find the first valid hit (excluding self)
        RaycastHit2D hit = new RaycastHit2D();
        foreach (RaycastHit2D h in hits)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[SurfaceDetector]   - Hit: {h.collider.gameObject.name} (tag: {h.collider.tag}, layer: {LayerMask.LayerToName(h.collider.gameObject.layer)}, isTrigger: {h.collider.isTrigger})");
            }
            
            if (h.collider.gameObject != gameObject) // Skip self
            {
                hit = h;
                break;
            }
        }
        
        SurfaceType newSurface = SurfaceType.Default;
        
        if (hit.collider != null)
        {
            // Check the tag of the hit object to determine surface type
            string hitTag = hit.collider.tag;
            int hitLayer = hit.collider.gameObject.layer;
            string hitLayerName = LayerMask.LayerToName(hitLayer);
            
            if (hitTag == grassTag)
                newSurface = SurfaceType.Grass;
            else if (hitTag == carpetTag)
                newSurface = SurfaceType.Carpet;
            else if (hitTag == stoneTag)
                newSurface = SurfaceType.Stone;
            else if (hitTag == metalTag)
                newSurface = SurfaceType.Metal;
            else if (hitTag == glassTag)
                newSurface = SurfaceType.Glass;
            else if (useLayerNamesForDetection)
            {
                // Fallback to layer name mapping
                if (hitLayerName == metalLayerName)
                    newSurface = SurfaceType.Metal;
                else if (hitLayerName == stoneLayerName)
                    newSurface = SurfaceType.Stone;
                else if (hitLayerName == carpetLayerName)
                    newSurface = SurfaceType.Carpet;
                else if (hitLayerName == glassLayerName)
                    newSurface = SurfaceType.Glass;
                else if (hitLayerName == grassLayerName)
                    newSurface = SurfaceType.Grass;
                else
                    newSurface = SurfaceType.Grass; // Default to grass
            }
            else
            {
                newSurface = SurfaceType.Grass; // Default to grass for untagged surfaces
            }
            
            if (enableDebugLogs)
            {
                Debug.Log($"[SurfaceDetector] Ray@{origin} d={detectionDistance} mask={effectiveMask} | Hit {hit.collider.name} | tag={hitTag} | layer={hitLayerName} => surface={newSurface}");
            }
        }
        else
        {
            // No surface detected (like green background), assume grass (default)
            newSurface = SurfaceType.Grass;
            if (enableDebugLogs)
            {
                Debug.Log($"[SurfaceDetector] Ray@{origin} d={detectionDistance} mask={effectiveMask} | No hit -> default Grass");
            }
        }
        
        // Update the SoundEmitter if the surface changed
        if (newSurface != currentDetectedSurface)
        {
            currentDetectedSurface = newSurface;
            if (soundEmitter != null)
            {
                soundEmitter.CurrentSurface = newSurface;
                Debug.Log($"Surface changed to: {newSurface}");
            }
        }
    }
    
    /// <summary>
    /// Manually set the surface type (useful for special cases)
    /// </summary>
    public void SetSurfaceType(SurfaceType surfaceType)
    {
        currentDetectedSurface = surfaceType;
        if (soundEmitter != null)
        {
            soundEmitter.CurrentSurface = surfaceType;
        }
    }
    
    /// <summary>
    /// Get the currently detected surface type
    /// </summary>
    public SurfaceType GetCurrentSurface()
    {
        return currentDetectedSurface;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw the detection ray in the Scene view
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * detectionDistance);
        Gizmos.DrawWireSphere(transform.position + Vector3.down * detectionDistance, 0.1f);
    }
}

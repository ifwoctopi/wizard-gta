using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple script that loads a new scene when the player collides with this object.
/// Attach this to any GameObject and add a Collider2D component.
/// </summary>
public class SceneTransition : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Name of the scene to load. Make sure it's added to Build Settings!")]
    public string targetSceneName = "NextScene";
    
    [Header("Player Settings")]
    [Tooltip("The tag of the player GameObject")]
    public string playerTag = "Player";
    
    [Header("Collision Type")]
    [Tooltip("Check this if using a Trigger collider (Is Trigger = true).\n" +
             "Uncheck if using a regular Collider (Is Trigger = false).")]
    public bool useTrigger = true;

    /// <summary>
    /// Called when a collider enters this trigger
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!useTrigger) return;
        
        // Check if the colliding object is the player
        if (collision.CompareTag(playerTag))
        {
            LoadTargetScene();
        }
    }

    /// <summary>
    /// Called when a collision occurs (non-trigger)
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (useTrigger) return;
        
        // Check if the colliding object is the player
        if (collision.gameObject.CompareTag(playerTag))
        {
            LoadTargetScene();
        }
    }

    /// <summary>
    /// Loads the target scene
    /// </summary>
    private void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError($"{gameObject.name}: Target scene name is not set! Please set it in the Inspector.");
            return;
        }

        Debug.Log($"Loading scene: {targetSceneName}");
        SceneManager.LoadScene(targetSceneName);
    }
}


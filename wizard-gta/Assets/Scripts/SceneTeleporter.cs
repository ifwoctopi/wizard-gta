using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Teleports the player to another scene when they touch this object.
/// Attach this script to any GameObject that should act as a teleporter.
/// </summary>
public class SceneTeleporter : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Name of the scene to load when touched. Make sure the scene is added to Build Settings!")]
    [SerializeField] private string targetSceneName = "Lvl2";
    
    [Header("Trigger Settings")]
    [Tooltip("If true, uses OnTriggerEnter2D (requires a Collider2D with 'Is Trigger' checked).\n" +
             "If false, uses OnCollisionEnter2D (requires a Collider2D without 'Is Trigger').")]
    [SerializeField] private bool useTrigger = true;
    
    [Header("Player Tag")]
    [Tooltip("The tag of the player GameObject. Default is 'Player'.")]
    [SerializeField] private string playerTag = "Player";
    
    [Header("Optional Settings")]
    [Tooltip("Delay in seconds before loading the scene. Useful for animations or effects.")]
    [SerializeField] private float loadDelay = 0f;
    
    [Tooltip("If true, only allows one teleportation (disables after first use).")]
    [SerializeField] private bool oneTimeUse = false;
    
    private bool hasBeenUsed = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!useTrigger) return;
        
        if (collision.CompareTag(playerTag) && !hasBeenUsed)
        {
            TeleportToScene();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (useTrigger) return;
        
        if (collision.gameObject.CompareTag(playerTag) && !hasBeenUsed)
        {
            TeleportToScene();
        }
    }

    private void TeleportToScene()
    {
        if (hasBeenUsed && oneTimeUse)
        {
            return;
        }

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("SceneTeleporter: Target scene name is not set!");
            return;
        }

        if (oneTimeUse)
        {
            hasBeenUsed = true;
        }

        Debug.Log($"Teleporting to scene: {targetSceneName}");
        
        if (loadDelay > 0f)
        {
            Invoke(nameof(LoadScene), loadDelay);
        }
        else
        {
            LoadScene();
        }
    }

    private void LoadScene()
    {
        SceneManager.LoadScene(targetSceneName);
    }

    // Optional: Visual feedback in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}


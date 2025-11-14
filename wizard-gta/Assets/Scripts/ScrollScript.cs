using UnityEngine;

public class SimpleScroll : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] private bool useTrigger = true;
    [SerializeField] private string playerTag = "Player";

    [Tooltip("The UI GameObject to show when the player touches this scroll.")]
    [SerializeField] private GameObject scrollObject;

    [Header("Sound Effects")]
    [Tooltip("Plays ONCE when the player first contacts the scroll.")]
    public AudioSource paperSound;

    [Tooltip("Loops until the player presses E to close the scroll.")]
    public AudioSource magicLoopSound;

    private bool hasActivated = false;
    private bool scrollOpen = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!useTrigger) return;

        if (collision.CompareTag(playerTag) && !hasActivated)
        {
            ActivateScroll();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (useTrigger) return;

        if (collision.gameObject.CompareTag(playerTag) && !hasActivated)
        {
            ActivateScroll();
        }
    }

    private void ActivateScroll()
    {
        if (scrollObject != null)
        {
            // Show scroll UI
            scrollObject.SetActive(true);
            scrollOpen = true;
            hasActivated = true;

            // Play paper sound once
            if (paperSound != null)
                paperSound.Play();

            // Start looping magic sound
            if (magicLoopSound != null && !magicLoopSound.isPlaying)
                magicLoopSound.Play();
        }
        else
        {
            Debug.LogError("SimpleScroll: No scrollObject assigned in the Inspector!");
        }
    }

    private void Update()
    {
        // If the scroll is open, pressing E hides it
        if (scrollOpen && Input.GetKeyDown(KeyCode.E))
        {
            // Hide scroll
            scrollObject.SetActive(false);
            scrollOpen = false;

            // Stop magic loop sound
            if (magicLoopSound != null)
                magicLoopSound.Stop();
        }
    }
}


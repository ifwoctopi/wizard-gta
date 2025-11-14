using UnityEngine;

public class SimpleScroll : MonoBehaviour
{
    [SerializeField] private bool useTrigger = true;
    [SerializeField] private string playerTag = "Player";

    [Tooltip("The GameObject to show when the player touches this scroll.")]
    [SerializeField] private GameObject scrollObject;

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
            scrollObject.SetActive(true);
            scrollOpen = true;
            hasActivated = true;
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
            scrollObject.SetActive(false);
            scrollOpen = false;
        }
    }
}

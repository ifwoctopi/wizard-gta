using UnityEngine;

public class ScrollAudioController : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource paperSound;   // plays once on contact
    public AudioSource magicSound;   // loops until E is pressed

    private bool scrollOpen = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Play paper sound ONCE when player touches the scroll
        if (collision.CompareTag("Player"))
        {
            if (!paperSound.isPlaying)
                paperSound.Play();
        }
    }

    private void Update()
    {
        // If scroll is open, E should close it and stop magic sound
        if (scrollOpen && Input.GetKeyDown(KeyCode.E))
        {
            CloseScroll();
        }
    }

    public void OpenScroll()
    {
        // Scroll UI turns on here in your other script
        scrollOpen = true;

        // Start looping magic sound
        if (!magicSound.isPlaying)
            magicSound.Play();
    }

    public void CloseScroll()
    {
        // Scroll UI turns off here in your other script
        scrollOpen = false;

        // Stop magic sound instantly
        magicSound.Stop();
    }
}


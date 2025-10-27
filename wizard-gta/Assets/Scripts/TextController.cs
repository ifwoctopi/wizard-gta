using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TextController : MonoBehaviour
{

    [Header("Fade Settings")]
    [SerializeField] private float waitBeforeFade = 5f; // How long to stay visible
    [SerializeField] private float fadeDuration = 2f;   // How long the fade lasts
    [SerializeField] private bool disableAfterFade = true; // Optional: disable GameObject after fade

    private Text uiText;
    private Color originalColor;
    private float timer = 0f;
    private bool fading = false;

    private void Awake()
    {
        uiText = GetComponent<Text>();
        if (uiText == null)
        {
            Debug.LogError("FadeTextUI: No UI Text component found on this GameObject!");
            enabled = false;
            return;
        }

        originalColor = uiText.color;
    }

    private void OnEnable()
    {
        // Reset state each time the text is re-enabled
        uiText.color = originalColor;
        timer = 0f;
        fading = false;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // Start fading after wait time
        if (!fading && timer >= waitBeforeFade)
        {
            fading = true;
            timer = 0f; // Reset timer for fade duration
        }

        // Handle fade out
        if (fading)
        {
            float t = timer / fadeDuration;
            float newAlpha = Mathf.Lerp(originalColor.a, 0f, t);

            uiText.color = new Color(originalColor.r, originalColor.g, originalColor.b, newAlpha);

            // Fade complete
            if (timer >= fadeDuration)
            {
                uiText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

                if (disableAfterFade)
                    gameObject.SetActive(false);

                enabled = false; // Stop updating
            }
        }
    }
}


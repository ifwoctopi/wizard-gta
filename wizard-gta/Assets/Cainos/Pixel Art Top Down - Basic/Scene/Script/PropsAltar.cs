using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cainos.PixelArtTopDown_Basic
{
    public class PropsAltar : MonoBehaviour
    {
        [Header("Runes Settings")]
        public List<SpriteRenderer> runes;
        public float lerpSpeed = 3f;

        [Header("Win Settings")]
        public float winWait = 3f;
        public string winSceneName = "WinScreen";

        private Color targetColor;
        private Color curColor;
        private float timer = 0f;
        private bool playerInside = false;

        private void Start()
        {
            if (runes == null || runes.Count == 0)
            {
                Debug.LogError("No runes assigned to PropsAltar!");
                return;
            }

            // Start with runes invisible
            curColor = runes[0].color;
            targetColor = curColor;
            targetColor.a = 0f;
            SetRunesColor(curColor);
        }

        private void Update()
        {
            // Smoothly transition rune color
            curColor = Color.Lerp(curColor, targetColor, lerpSpeed * Time.deltaTime);
            SetRunesColor(curColor);

            // Count time only if player is inside
            if (playerInside)
            {
                timer += Time.deltaTime;
                if (timer >= winWait)
                {
                    Debug.Log("Loading Win Screen...");
                    SceneManager.LoadScene(winSceneName);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("Player entered altar.");
                playerInside = true;
                timer = 0f; // reset timer on entry
                targetColor.a = 1.0f; // make runes glow
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("Player left altar.");
                playerInside = false;
                targetColor.a = 0.0f; // fade runes back out
            }
        }

        private void SetRunesColor(Color color)
        {
            foreach (var r in runes)
            {
                if (r != null)
                    r.color = color;
            }
        }
    }
}

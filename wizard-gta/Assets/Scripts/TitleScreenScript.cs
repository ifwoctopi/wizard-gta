using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button startButton; // Assign in inspector

    [Header("Scene Settings")]
    [SerializeField] private string demo = "DB Demo"; // Name of your game scene

    [Header("Audio Settings")]
    [SerializeField] private AudioSource musicSource; // Assign an AudioSource component here
    [SerializeField] private AudioClip titleMusic;    // Assign your title song (mp3, wav, etc.)

    private void Awake()
    {
        // Ensure start button is assigned
        if (startButton == null)
        {
            Debug.LogError("Start Button is not assigned in the inspector!");
            return;
        }

        // Add click listener to the button
        startButton.onClick.AddListener(OnStartButtonPressed);

        // Play background music if available
        if (musicSource != null && titleMusic != null)
        {
            musicSource.clip = titleMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("Missing AudioSource or AudioClip on TitleScreen!");
        }
    }

    private void OnDestroy()
    {
        // Clean up listener
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonPressed);
        }
    }

    private void OnStartButtonPressed()
    {
        Debug.Log("Start button pressed! Loading game...");

        // Optionally stop or fade out the music
        if (musicSource != null)
        {
            musicSource.Stop();
        }

        SceneManager.LoadScene(demo);
    }
}
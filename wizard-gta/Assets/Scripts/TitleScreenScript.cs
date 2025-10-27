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

    private void Awake()
    {
        // Optional: Ensure start button is assigned
        if (startButton == null)
        {
            Debug.LogError("Start Button is not assigned in the inspector!");
            return;
        }

        // Add click listener to the button
        startButton.onClick.AddListener(OnStartButtonPressed);
    }

    private void OnDestroy()
    {
        // Clean up listener to avoid memory leaks
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonPressed);
        }
    }

    private void OnStartButtonPressed()
    {
        // Optional: Add a fade-out or animation here
        Debug.Log("Start button pressed! Loading game...");

        // Load the game scene
        SceneManager.LoadScene(demo);
    }

}

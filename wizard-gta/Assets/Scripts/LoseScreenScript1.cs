using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoseScreen : MonoBehaviour
{

    [Header("UI Elements")]
    [SerializeField] private Button tryAgainButton; // Assign in inspector
    [SerializeField] private Button menuButton;
    [Header("Scene Settings")]
    [SerializeField] private string menu = "TitleScreen"; // Name of game scene
    [SerializeField] private string demo = "DB Demo";
    private void Awake()
    {
        // Optional: Ensure start button is assigned
        if (menuButton == null)
        {
            Debug.LogError("Menu Button is not assigned in the inspector!");
            return;
        }

        if (tryAgainButton == null)
        {
            Debug.LogError("Try Again Button is not assigned in the inspector!");
            return;
        }

        // Add click listener to the button
        tryAgainButton.onClick.AddListener(OnTryButtonPressed);
        menuButton.onClick.AddListener(OnMenuButtonPressed);
    }

    private void OnDestroy()
    {
        // Clean up listener to avoid memory leaks
        if (tryAgainButton != null)
        {
            tryAgainButton.onClick.RemoveListener(OnTryButtonPressed);
        }
        
        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(OnMenuButtonPressed);
        }
    }

    private void OnTryButtonPressed()
    {
        // Optional: Add a fade-out or animation here
        Debug.Log("Try Again button pressed! Loading game...");

        // Load the game scene
        SceneManager.LoadScene(demo);
    }
    
    private void OnMenuButtonPressed()
    {
        // Optional: Add a fade-out or animation here
        Debug.Log("Menu button pressed! Loading menu...");

        // Load the game scene
        SceneManager.LoadScene(menu);
    }

}

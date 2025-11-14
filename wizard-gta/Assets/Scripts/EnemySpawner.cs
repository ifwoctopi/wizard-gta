// EnemySpawner.cs

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    // Assign your enemy prefab in the Inspector
    [Header("Enemy Configuration")]
    [SerializeField] private GameObject enemyPrefab; 
    [SerializeField] private int numberOfEnemiesToSpawn = 5;

    [Header("Random Spawn Area (Define Map Boundaries)")]
    // Set these values in the Inspector for each specific level
    [SerializeField] private float minSpawnX = -10f;
    [SerializeField] private float maxSpawnX = 10f;
    [SerializeField] private float minSpawnY = -5f;
    [SerializeField] private float maxSpawnY = 5f;
    [SerializeField] private LayerMask groundLayer; // Layer for environment/ground check (optional but recommended)

    // The specific levels where spawning should occur
    private List<string> levelsToSpawnOn = new List<string> { "Lvl2", "Lvl3", "DB demo" };

    void Awake()
    {
        // 1. Add this line to make the object persistent
        DontDestroyOnLoad(gameObject);
        
        // 2. Subscribe to the scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (levelsToSpawnOn.Contains(scene.name))
        {
            // --- NEW: Find and load dynamic boundary data ---
            LevelData data = FindObjectOfType<LevelData>();

            if (data != null)
            {
                minSpawnX = data.minX;
                maxSpawnX = data.maxX;
                minSpawnY = data.minY;
                maxSpawnY = data.maxY;
                Debug.Log($"Loaded boundaries for {scene.name}: X:{minSpawnX} to {maxSpawnX}");
            }
            else
            {
                Debug.LogError($"LevelData component not found in {scene.name}! Spawning may use wrong boundaries.");
                // If not found, it will use the default boundaries set on the persistent spawner.
            }
            // ------------------------------------------------

            SpawnEnemies();
        }
    }
    
    private void SpawnEnemies()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab is not assigned in the EnemySpawner! Cannot spawn.");
            return;
        }

        // Loop to spawn the required number of enemies
        for (int i = 0; i < numberOfEnemiesToSpawn; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            
            // --- NEW: Simple Check (Ensure you have a GroundLayer defined in the Inspector) ---
            // Check a small radius to see if the random spot is blocked by an obstacle
            Collider2D hit = Physics2D.OverlapCircle(spawnPosition, 0.5f, groundLayer); 

            // If the spot is clear (hit is null), then spawn
            if (hit == null)
            {
                Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                Debug.Log($"Spawned enemy {i + 1} at: {spawnPosition}");
            }
            else
            {
                Debug.LogWarning($"Skipped spawn, position blocked at: {spawnPosition}");
                // Optional: Decrement 'i' or retry the position search
            }
        }
    }
    
    /// <summary>
    /// Generates a random position within the defined map boundaries.
    /// </summary>
    private Vector3 GetRandomSpawnPosition()
    {
        // Calculate a random X position between the minimum and maximum boundaries
        float randomX = Random.Range(minSpawnX, maxSpawnX);
        
        // Calculate a random Y position between the minimum and maximum boundaries
        float randomY = Random.Range(minSpawnY, maxSpawnY);
        
        // Assuming a 2D environment where Z is usually 0 or fixed
        Vector3 randomPosition = new Vector3(randomX, randomY, 0f); 
        
        return randomPosition;
    }
}
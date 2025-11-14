// LevelData.cs (Attach this to an object in each level scene)
using UnityEngine;

public class LevelData : MonoBehaviour
{
    [Header("Level Spawn Boundaries")]
    public float minX = -10f; // Set this in Inspector for each scene
    public float maxX = 10f;  // Set this in Inspector for each scene
    public float minY = -5f;
    public float maxY = 5f;
}
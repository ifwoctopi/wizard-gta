using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    private static PersistentObject instance;

    void Awake()
    {
        // Check if an instance already exists
        if (instance == null)
        {
            // If not, this is the first instance, so set it and make it persistent
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If an instance already exists, destroy this duplicate
            Destroy(gameObject);
        }
    }
}
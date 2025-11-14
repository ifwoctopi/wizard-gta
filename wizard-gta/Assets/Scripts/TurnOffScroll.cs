using UnityEngine;

public class TurnOffScroll : MonoBehaviour
{
    [SerializeField] private GameObject scrollObject;   // object to turn off
    private bool scrollOpen = true;                     // assume it's open

    private void Update()
    {
        // Press E to turn off the scroll
        if (Input.GetKeyDown(KeyCode.E))
        {
            scrollObject.SetActive(false);
            scrollOpen = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DebugText : MonoBehaviour
{
    public Text debugText;        // assign the UI Text in inspector
    private string log = "";

    void Awake()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        log += logString + "\n";

        // Limit lines to avoid overflow
        string[] lines = log.Split('\n');
        if (lines.Length > 20)
        {
            log = string.Join("\n", lines, lines.Length - 20, 20);
        }

        if (debugText != null)
            debugText.text = log;
    }
}

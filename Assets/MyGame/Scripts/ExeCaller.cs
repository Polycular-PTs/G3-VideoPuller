using System.Diagnostics;
using System.IO;
using UnityEngine;

public class ExeCaller : MonoBehaviour
{
    private static ExeCaller instance;
    private Process masterBackendProcess;

    void Awake()
    {
        // If an ExeCaller already exists from the first load, destroy the duplicate in the new scene
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Persists across scene reloads


        string batPath = Path.Combine(Application.streamingAssetsPath, "start_ai_backends.bat");

        if (!File.Exists(batPath))
        {
            UnityEngine.Debug.LogError("Setup script not found at: " + batPath);
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/c \"" + batPath + "\"",
            // Show the window so the user sees the download progress on the first run
            CreateNoWindow = true,
            UseShellExecute = false,
        };

        masterBackendProcess = Process.Start(startInfo);
        UnityEngine.Debug.Log($"AI Backends launching... Main Process ID: {masterBackendProcess.Id}");
    }

    void OnApplicationQuit()
    {
        UnityEngine.Debug.Log($"Trying to Kill Process Tree... Main Process ID: {masterBackendProcess.Id}");
        KillProcessTree(masterBackendProcess);
    }


    void KillProcessTree(Process process)
    {
        if (process == null || process.HasExited)
        {
            return;
        }

        UnityEngine.Debug.Log($"Shutting down AI Backends (PID {process.Id})...");

        // The /T kills the .bat file AND both Python scripts it spawned
        Process.Start(new ProcessStartInfo
        {
            FileName = "taskkill",
            Arguments = $"/PID {process.Id} /T /F",
            CreateNoWindow = true,
            UseShellExecute = false
        });

        process.Dispose();
    }
}
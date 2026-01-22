using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;


public class ExeCaller : MonoBehaviour
{
    Process objectDetectionProgram;
    Process poseDetectionProgram;
    




    void Awake()
    {
        
        string objectDetectionExePath = Path.Combine(Application.streamingAssetsPath, "objectDetection_build_noWindow.exe");
        string poseDetectionExePath = Path.Combine(Application.streamingAssetsPath, "poseDetection_build_noWindow.exe");

        ProcessStartInfo objectDetectionstartInfo = new ProcessStartInfo
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            FileName = objectDetectionExePath,
        };

        ProcessStartInfo poseDetectionstartInfo = new ProcessStartInfo
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            FileName = poseDetectionExePath,
        };

        if (!File.Exists(objectDetectionExePath))
        {
            UnityEngine.Debug.LogError("Object detection build not found at path: " + objectDetectionExePath);
        }

        if (!File.Exists(poseDetectionExePath))
        {
            UnityEngine.Debug.LogError("Pose detection build not found at path: " + poseDetectionExePath);
        }


        StartCoroutine(StartPrograms(objectDetectionstartInfo, poseDetectionstartInfo));
        

        


    }

    void OnApplicationQuit()
    {
        
            KillProcessTree(objectDetectionProgram);
        
            KillProcessTree(poseDetectionProgram);
    }


    IEnumerator StartPrograms(ProcessStartInfo objectDetection, ProcessStartInfo poseDetection)
    {
        
        objectDetectionProgram = Process.Start(objectDetection);
        yield return new WaitForSeconds(1);
        UnityEngine.Debug.Log($"Object Detection Process started: {objectDetectionProgram.ProcessName}");
        poseDetectionProgram = Process.Start(poseDetection);
        yield return new WaitForSeconds(1);
        UnityEngine.Debug.Log($"Pose Detection Process started: {poseDetectionProgram.ProcessName}");

    }

    void KillProcessTree(Process process)
    {
        if (process == null || process.HasExited)
        {
            UnityEngine.Debug.Log($"Already quit Process {process.ProcessName}");
            return;
        }

        else
        {
            UnityEngine.Debug.Log($"Trying to Quit the following Program: {process.ProcessName}");
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

}

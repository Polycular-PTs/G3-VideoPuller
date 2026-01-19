using System;
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



        objectDetectionProgram = Process.Start(objectDetectionstartInfo);
        UnityEngine.Debug.Log("Action Detection Process started");

        poseDetectionProgram = Process.Start(poseDetectionstartInfo);
        UnityEngine.Debug.Log("Pose Detection Process started");


    }

    void OnApplicationQuit()
    {
        UnityEngine.Debug.Log("Trying to Quit Object Exe");
            objectDetectionProgram.Kill();
            objectDetectionProgram.Dispose();

        UnityEngine.Debug.Log("Trying to Quit Pose Exe");
            poseDetectionProgram.Kill();
            poseDetectionProgram.Dispose();
    }


}

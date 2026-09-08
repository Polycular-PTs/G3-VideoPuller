using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class DebugSkipButton : MonoBehaviour
{
    [SerializeField] Text buttonText;
    [SerializeField] Image buttonBackground;
    public static bool skipCameraRequirement = false;

    private void Awake()
    {
        UpdateButtonVisuals();
    }

    public void ToggleCameraSkip()
    {
        skipCameraRequirement = !skipCameraRequirement;
        UpdateButtonVisuals();
        Debug.Log($"[Debug] Camera requirement skip: {skipCameraRequirement}");
    }

    public void UpdateButtonVisuals()
    {
        if (buttonText != null)
        {
            buttonText.text = skipCameraRequirement ? "Cam Skip: ON" : "Cam Skip: OFF";
        }

        if (buttonBackground != null)
        {
            buttonBackground.color = skipCameraRequirement ? Color.green : Color.white;
        }
    }
}
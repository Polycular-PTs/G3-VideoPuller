using UnityEngine;
using UnityEngine.UI;

public class KurbelRotation : MonoBehaviour
{
    public float targetAngle = 0f;
    private float currentVisualAngle = 0f;

    // Diese Property simuliert die alte Variable für VideoManager.cs und ChestOpen.cs
    public float currentAngle
    {
        get { return currentVisualAngle; }
        set
        {
            currentVisualAngle = value;
            targetAngle = value;
        }
    }

    public float stepAngle = 30f;
    public Text gradText;

    private float rotationVelocity = 0f;
    public bool isLocked = false;

    void Update()
    {
        if (!isLocked)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                targetAngle += stepAngle;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                targetAngle -= stepAngle;
            }

            targetAngle = Mathf.Clamp(targetAngle, 0f, 180f);

            if (targetAngle >= 180f)
            {
                isLocked = true;
            }
        }

        currentVisualAngle = Mathf.SmoothDamp(currentVisualAngle, targetAngle, ref rotationVelocity, 0.5f);

        ApplyRotation();
        UpdateUI();
    }

    void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, currentVisualAngle);
    }

    void UpdateUI()
    {
        if (gradText != null)
        {
            gradText.text = $"Gradzahl: {currentVisualAngle:F1} ";
        }
    }
}
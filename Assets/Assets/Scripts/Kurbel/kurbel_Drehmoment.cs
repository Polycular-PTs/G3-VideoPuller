using UnityEngine;
using UnityEngine.UI;



public class KurbelRotation : MonoBehaviour
{ 

    public float currentAngle = 0f;
    public float stepAngle = 10f;
    public Text gradText; 

    void Update()
    {
       
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentAngle += stepAngle;
            ApplyRotation();
        }

        
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentAngle -= stepAngle;
            ApplyRotation();
        }
        UpdateUI();
    }

    void ApplyRotation()
    {
        
        currentAngle = currentAngle % 360f;
        if (currentAngle < 0) currentAngle += 360f;

        
        transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
    }
    void UpdateUI()
    {
        if (gradText != null)
        {

            
            gradText.text = $"Gradzahl: {currentAngle:F1} ";

        }
    }
}
 


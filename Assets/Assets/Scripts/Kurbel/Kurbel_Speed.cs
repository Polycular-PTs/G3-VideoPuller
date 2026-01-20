using UnityEngine;
using UnityEngine.UI;

public class KurbelGeschwindigkeit : MonoBehaviour
{
    public Text speedText;
    private float lastHitTime = 0f;
    private float rotationSpeed = 0f;
    private int impulseCount = 0;

    void Update()
    {
       
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            float currentTime = Time.time;

            
            if (lastHitTime > 0f)
            {
                float deltaTime = currentTime - lastHitTime;

                
                rotationSpeed = 1f / deltaTime;
            }

            lastHitTime = currentTime;
            impulseCount++;
            UpdateUI();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            float currentTime = Time.time;

           
            if (lastHitTime > 0f)
            {
                float deltaTime = currentTime - lastHitTime;

                
                rotationSpeed = 1f / deltaTime;
            }

            lastHitTime = currentTime;
            impulseCount++;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (speedText != null)
        {
            
            float rpm = (rotationSpeed / 10f) * 60f;
            string v = $"Drehzahl: {rpm:F1} RPM";
            speedText.text = v;

        }
    }
}


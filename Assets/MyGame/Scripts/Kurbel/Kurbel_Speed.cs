using UnityEngine;
using UnityEngine.UI;

public class KurbelGeschwindigkeit : MonoBehaviour
{
    public Text speedText;
    private float lastHitTime = 0f;
    private float rotationSpeed = 0f;
    private int impulseCount = 0;
    public float rpm;

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
        }
        rpm = (rotationSpeed / 10f) * 60f;
    }

}


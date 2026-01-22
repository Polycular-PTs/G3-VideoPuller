using UnityEngine;
using UnityEngine.UI;

public class CountdownManager : MonoBehaviour
{
    [SerializeField] Text countdownText;
    [SerializeField] GameObject reconnectButton;
    public int EstimatedConnectionTime;
    private int timeLeft;
    public bool isConnected;

    
    void Update()
    {
        {
            if (isConnected == false)
            {
                timeLeft = Mathf.RoundToInt(EstimatedConnectionTime - Time.time);
                if (timeLeft >= 0)
                {
                    countdownText.text = timeLeft.ToString();
                }


            }

            else if (countdownText.enabled == true && isConnected == true)
            {
                countdownText.enabled = false;
                reconnectButton.SetActive(false);
            }


            
        }
        
    }
}

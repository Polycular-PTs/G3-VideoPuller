using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class KurbelIdee : MonoBehaviour
{
    public Text counterText;
    private int counter = 0;
    
    void Update()
    {
       if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            counter++;
            UpdateText();
        }


        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            counter--;
            UpdateText();
        }


        
    }

    void UpdateText()
    {
        if (counterText != null)
        {
            counterText.text = "Zaehler" + counter;
        }
    }
}

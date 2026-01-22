using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class KurbelIdee : MonoBehaviour
{
    public Text counterText;
    public int counter = 0;
    
    void Update()
    {
       if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            counter++;
        }


        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            counter--;
        }


        
    }

}

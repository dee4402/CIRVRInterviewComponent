using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Dashboard_buttonHandler : MonoBehaviour
{
    // Start is called before the first frame update
    public void ToggleType()
    {
        Text txt = transform.Find("Text").GetComponent<Text>();
        if (SizeKeeper.TypeOfGraph == 1)
        {
            SizeKeeper.TypeOfGraph = 0;
            txt.text = "Static Graph";
        }
        else
        {
            SizeKeeper.TypeOfGraph = 1;
            txt.text = "Dynamic Graph";
        }
        
        
        
    }
}

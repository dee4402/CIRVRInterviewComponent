using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class InfoCard : MonoBehaviour
{
    
    public Text txt;
    // Start is called before the first frame update
    void Start()
    {
        txt.text = "Interview Date: " + System.DateTime.Now.Date.ToString();
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

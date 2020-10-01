using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility.Events;


public class ControlRecordingIndicator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventSystem.current.RegisterListener<EndDialog>(IndicatorOn);
        EventSystem.current.RegisterListener<BeginDialog>(IndicatorOff);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IndicatorOn(EndDialog e)
    {

    }

    public void IndicatorOff(BeginDialog e)
    {
        
    }
}

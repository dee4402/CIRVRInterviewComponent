using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Gaming;

public class RegionOfInterestTracker : MonoBehaviour
{
    private GazeAware m_gazeAware;
    private GazeOverlayBehavior m_gazeOverlayScript;

    void Start()
    {
        m_gazeAware = gameObject.GetComponent<GazeAware>();
        m_gazeOverlayScript = GameObject.Find("Canvas/GazeOverlay").GetComponent<GazeOverlayBehavior>();
    }
    
    void Update()
    {
        if(m_gazeAware.HasGazeFocus)
        {
            //register gaze hit with Gaze Overlay UI
            m_gazeOverlayScript.RegisterGazeHit(gameObject.name);
            Debug.Log("===> Gaze focus is on \"" + gameObject.name + "\".");
        }
    }
}

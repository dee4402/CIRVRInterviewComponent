using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Gaming;

public class GazeTargetManager : MonoBehaviour
{
    public GameObject[] m_gazeTargets;

    void Start()
    {
        TobiiAPI.SetCurrentUserViewPointCamera(GameObject.Find("StandardPlayerController/MainCamera").GetComponent<Camera>());

        foreach(GameObject g in m_gazeTargets)
        {
            if (g != null)
            {
                //TODO: consider adding collider if one is not already attached; otherwise, must be set up in inspector
                g.AddComponent<GazeAware>();
                g.AddComponent<RegionOfInterestTracker>();
            }
        }
    }
    
    void Update()
    {
    }
}

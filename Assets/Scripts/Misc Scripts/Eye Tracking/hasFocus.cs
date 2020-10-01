using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Gaming;

public class hasFocus : MonoBehaviour
{
    public bool currentFocus;
    GazeAware gaze;

    // Start is called before the first frame update
    void Start()
    {
        gaze = gameObject.GetComponent<GazeAware>();
        currentFocus = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (gaze.HasGazeFocus)
        {
            currentFocus = true;
        }
        else
        {
            currentFocus = false;
        }
    }
}

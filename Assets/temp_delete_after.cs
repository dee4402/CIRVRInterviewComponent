using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Gaming;

public class temp_delete_after : MonoBehaviour
{
    GazeAware ga;

    // Start is called before the first frame update
    void Start()
    {
        ga = gameObject.GetComponent<GazeAware>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ga.HasGazeFocus)
            print("TESTING. USER IS LOOKING AT " + gameObject.name);

        string viewedObject = TobiiAPI.GetFocusedObject() != null ? TobiiAPI.GetFocusedObject().name : "null";

        print ("current focused object is " + viewedObject);

        //GazePoint gp = TobiiAPI.GetGazePoint();
        //print(gp.Screen.x.ToString() + ", " + gp.Screen.y.ToString());
    }
}

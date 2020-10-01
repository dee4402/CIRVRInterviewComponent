using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeScript : MonoBehaviour
{
    private Camera demoCam;
    private Camera normCam;
    private bool hit = false;
    // Start is called before the first frame update
    void Start()
    {
        demoCam = GameObject.Find("sitLeftSide").GetComponent<Camera>();
        normCam = GameObject.Find("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && hit == false)
        {
            var test = gameObject.GetComponent<Canvas>();
            test.worldCamera = demoCam;
            hit = !hit;
        }
        else if(Input.GetKeyDown(KeyCode.Space) && hit == true)
        {
            var test = gameObject.GetComponent<Canvas>();
            test.worldCamera = normCam;
            hit = !hit;
        }
    }
}

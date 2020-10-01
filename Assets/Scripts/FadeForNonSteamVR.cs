using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility.Events;

/*
 * Author : Michael Breen
 * This script fades the user screen to black and teleports them to the correct 
 * location if they are wearing a vr device that does not have steamVR capability
 * It might make sense to combine this script with CameraInterpolation.cs under a new name
 * Or to take any of the data ie the anchors and move them to some static data script
*/

public class FadeForNonSteamVR : MonoBehaviour
{
    private SpriteRenderer fadeSprite;
    public List<GameObject> locations = new List<GameObject>();
    private int nextLoc;
    public Rigidbody FoveRig;
    public Camera camera;
    public Dictionary<string, Transform> anchors = new Dictionary<string, Transform>();
    private bool switchBool, whiteboardBool;

    // Start is called before the first frame update
    void Start()
    {
        //The sprite rests directly in front of the camera and is used to simulate a fade out fade in
        fadeSprite = gameObject.GetComponent<SpriteRenderer>();
        //The locations the user will be teleported to, the locations are then compensated for local position (bad fix) !!!
        var allLocations = GameObject.FindGameObjectsWithTag("Anchor");
        foreach(GameObject x in allLocations)
        {
            anchors.Add(x.name, x.transform);
        }
        foreach(KeyValuePair<string, Transform> x in anchors)
        {
            if(!x.Key.Contains("Base"))// || !x.Key.Contains("Right"))
            {
                anchors[x.Key].position = new Vector3(anchors["InterviewViewAnchorBase"].position.x + anchors[x.Key].localPosition.x, 
                                                      anchors["InterviewViewAnchorBase"].position.y + anchors[x.Key].localPosition.y,
                                                      anchors["InterviewViewAnchorBase"].position.z + anchors[x.Key].localPosition.z);
            }
        }
        
        EventSystem.current.RegisterListener<EndWhiteBoardSection>(EndWBQ);
        EventSystem.current.RegisterListener<BeginWhiteBoard>(BeginWB);
        EventSystem.current.RegisterListener<PlayerEnteredInterviewRoom>(AllowSitDown);
    }

    // Update is called once per frame
    void Update()
    {
        //Check for key downs to start fade to black, switch bool ensures that the user can't fade at any time
        //if((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0)) && switchBool)
        if(switchBool)
        {
            StartCoroutine(WaitToFade(1f));
            
            switchBool = false;
        }
        else if(whiteboardBool)
        {
            StartCoroutine(WaitToFade(0.5f));
            
            whiteboardBool = false;
        }
    }
    //Fade function
    private IEnumerator Fade(bool start, GameObject location = null, bool midPoint = false)
    {
        float changer = .05f;
        Color tmp = fadeSprite.color;
 
        if(!start)
        {
            changer *= -1f;
        }

        yield return new WaitForSeconds(.05f);

        tmp.a += changer;
        fadeSprite.color = tmp;

        if(!((start && fadeSprite.color.a >= 1) || (!start && fadeSprite.color.a <= 0)))
        {
            StartCoroutine(Fade(start, location, midPoint));
        }
        else
        {
            if(!midPoint)
            {
                midPoint = ChangeLocation(location);
                yield return new WaitForSeconds(1f);
                StartCoroutine(Fade(!start, location, midPoint));
            }
        }
    }
    //Makes sure the cam is in the correct position
    private IEnumerator FixCamPos(Transform correctPos)
    {
        var diff = correctPos.position.y - camera.transform.position.y;

        yield return new WaitForSeconds(.01f);
        if(diff > .1f || diff < -.1f)
        {
            if(correctPos.position.y <= camera.transform.position.y)
            {
                FoveRig.transform.position = new Vector3(FoveRig.transform.position.x, FoveRig.transform.position.y - .05f, FoveRig.transform.position.z);
            }
            else if(correctPos.position.y >= camera.transform.position.y)
            {
                FoveRig.transform.position = new Vector3(FoveRig.transform.position.x, FoveRig.transform.position.y + .05f, FoveRig.transform.position.z);
            }
            StartCoroutine(FixCamPos(correctPos));
        }
    }

    private IEnumerator WaitToFade(float time) {
        yield return new WaitForSeconds(time);
        StartCoroutine(Fade(true, locations[nextLoc]));
    }
    //Teleports the rig
    private bool ChangeLocation(GameObject location)
    {
        FoveRig.transform.position = anchors[location.name].position;
        StartCoroutine(FixCamPos(anchors[location.name].transform));
        return true;
    }
    //Events that handle the next locations and fade bool
    private void BeginWB(BeginWhiteBoard e)
    {
        nextLoc = 1;
        whiteboardBool = true;
    }

    private void EndWBQ(EndWhiteBoardSection e)
    {
        whiteboardBool = true;
        nextLoc = 0;
    }

    void AllowSitDown(PlayerEnteredInterviewRoom e)
    {
        
        switchBool = true;
        nextLoc = 0;        
    }
}

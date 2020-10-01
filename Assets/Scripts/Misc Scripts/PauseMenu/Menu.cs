using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility.Events;

public class Menu : MonoBehaviour
{
    GameObject[] menuOptions;
    GameObject pause;
    //bool mouseInvis = true;
    private WebCam webCam;
    private E4ClientManager e4ClientManager;
    private TobiiGazeLoggingNoHMD gaze;

    // Start is called before the first frame update
    void Start()
    {
        pause = GameObject.Find("PauseScreen");
        menuOptions = GameObject.FindGameObjectsWithTag("Paused");
        hidePaused();
        webCam = GameObject.Find("WebCam").GetComponent<WebCam>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(Time.timeScale == 1f)
            {
                webCam.isPaused = true; //to stop taking pictures when paused

                //Michael 7/2/19: added Cursor.* ; also, in start, hidePaused is called to start menu in unpaused state.
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                pause.SetActive(true);
                Time.timeScale = 0f;
                EventSystem.current.FireEvent(new PauseEvent("String", true));
            }
            else if(Time.timeScale == 0f)
            {
                hidePaused();
                RestartEverything();
            }
        }
    }

    //hides objects with ShowOnPause tag
    public void hidePaused()
    {
        //pause.SetActive(false);
        foreach (GameObject g in menuOptions)
        {
            g.SetActive(false);
        }
    }

    public void RestartEverything()
    {
        webCam.isPaused = false; //to resume taking pictures

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        EventSystem.current.FireEvent(new PauseEvent("String", false));
    }
}

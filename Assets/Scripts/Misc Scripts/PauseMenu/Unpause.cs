using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility.Events;

public class Unpause : MonoBehaviour
{
    GameObject[] menuOptions;
    GameObject pause;
    private WebCam webCam;

    void Start()
    {
        pause = GameObject.Find("PauseScreen");
        menuOptions = GameObject.FindGameObjectsWithTag("Paused");
        webCam = GameObject.Find("WebCam").GetComponent<WebCam>();
      
    }

    public void hidePaused()
    {
        webCam.isPaused =  false; //to resume taking pictures

        Time.timeScale = 1f;
        pause.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        EventSystem.current.FireEvent(new PauseEvent("String", false));
        foreach (GameObject g in menuOptions)
        {
            g.SetActive(false);
        }
    }
}

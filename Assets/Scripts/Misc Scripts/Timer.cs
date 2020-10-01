using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Utility.Events;

/// <summary>
/// Timer class sets and moves the circular timer for each whiteboard question
/// </summary>
public class Timer : MonoBehaviour
{
    //Variables
    private Image graphicTimer;
    public float seconds = 0;
    private float totalTime;
    public float clockSpeed = 1.0f;
    private bool waitForOneSecond = false, startOfQuestion = false, stopTimer = false;


    void Start()
    {
        //Grab sprite
        graphicTimer = gameObject.GetComponent<Image>();
        //initializes time vars
        totalTime = seconds = 0;
        //registers listeners
        EventSystem.current.RegisterListener<SetTimer>(SetTime);
        EventSystem.current.RegisterListener<WBQAnswered>(StopTime);
    }

    void Update()
    {
        //if there is time left continue
        if (seconds > 0 && !stopTimer)
        {
            seconds -= Time.deltaTime;
            graphicTimer.fillAmount = seconds / totalTime;
            ChangeColor(seconds);
        }
    }
    IEnumerator startTime()
    {
        yield return new WaitForSeconds(1f);
        waitForOneSecond = false;
        seconds--;
        ChangeColor(seconds);
        graphicTimer.fillAmount = seconds / totalTime;
    }

    //Event that sets the timer for each question as it comes
    public void SetTime(SetTimer e)
    {
        totalTime = seconds = e.time;
        ChangeColor(e.time);
        stopTimer = false;
        graphicTimer.fillAmount = 1;
        waitForOneSecond = false;
        startOfQuestion = true;
    }
    
    //even to stop timer at the same time as the digital timer
    public void StopTime(WBQAnswered e)
    {
        stopTimer = true;
        startOfQuestion = false;
    }
    //Changes color of timer when it reaches half and a third
    private void ChangeColor(float time)
    {
        if(time <= (totalTime/2) && time >= (totalTime/4))
        {
            graphicTimer.color = Color.yellow;
        }
        if (time <= (totalTime/4))
        {
            graphicTimer.color = Color.red;
        }
        if(time > 15)
            graphicTimer.color = Color.green;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class footstep : MonoBehaviour
{
    // Use this for initialization
    float oldPosX;
    AudioSource footSound;
    float timeInBetween;
    float nowPlay;

    void Start()
    {
        nowPlay = 0f;
        timeInBetween = .3f;
        footSound = GetComponent<AudioSource>();
        oldPosX = transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        //If the player moves then add the frame count to nowPlay, once nowPlay reaches timeInBetween play the audio file and reset nowPlay
        if ((transform.position.x < oldPosX || transform.position.x > oldPosX))
        {
            if (timeInBetween <= nowPlay)
            {
                nowPlay = 0.0f;
                footSound.Play();
            }
            else
            {
                nowPlay += Time.deltaTime;
            }
        }

        oldPosX = transform.position.x;

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudio : MonoBehaviour {

    public AudioClip MusicClip;

    public AudioSource MusicSource;
    public bool played;

    // Use this for initialization
    void Start () {
        MusicSource.clip = MusicClip;
        played = false;
	}

    // Update is called once per frame
    void Update()
    {
        if ((Input.GetKeyDown("joystick button 2")) && played == false)    // Button Circle
        {
            MusicSource.Play();
            played = true;
        }
    }
}

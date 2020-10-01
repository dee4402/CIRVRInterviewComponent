using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class anim_man : MonoBehaviour {

    public Animator anim;
    public bool played;

    // Use this for initialization
    void Start () {
        anim = GetComponent<Animator>(); //access animator
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown("joystick button 2") && played == false)    // Button Circle
        {
            //anim.SetBool("r_waving", true);
            anim.Play("standardpose"); //standardpose
            played = true;

        }
    }
}

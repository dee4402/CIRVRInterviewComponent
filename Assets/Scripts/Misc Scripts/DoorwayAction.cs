using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility.Events;
using UnityStandardAssets.Utility.Config;

public class DoorwayAction : MonoBehaviour {

	Collider col;
	GameObject interviewer;
	Animator interviewerAnim;
	// Use this for initialization
	void Start () {
		col = GetComponent<Collider>(); //access collider
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void OnTriggerEnter(Collider other)
  	{
		GameObject go = other.gameObject;
		if (go.tag == "Player") {
			EventSystem.current.FireEvent(new PlayerMoveEvent("The player entered the room", "receptionRoom", "interviewRoom"));
		}
  	}

	private void OnTriggerExit(Collider other)
  	{
		GameObject go = other.gameObject;
		if (go.tag == "Player") {
			EventSystem.current.FireEvent(new PlayerMoveEvent("The player entered the room", "interviewRoom", "receptionRoom"));
		}
  	}
}

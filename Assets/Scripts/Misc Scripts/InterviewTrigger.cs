using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility.Events;
using UnityStandardAssets.Utility.Config;

public class InterviewTrigger : MonoBehaviour {

	private bool startedInterview;
	private void OnTriggerEnter(Collider other)
  	{
		GameObject go = other.gameObject;
		if (go.tag == "Player" || go.tag == "MainCamera" && startedInterview == false) 
		{
			EventSystem.current.FireEvent(new PlayerBeginInterview("Player sat down to begin the interview"));
			startedInterview = true;
		}
  	}

	private void OnTriggerExit(Collider other)
  	{
		GameObject go = other.gameObject;
		if (go.tag == "Player") 
		{
			//EventSystem.current.FireEvent(new PlayerLeaveInterview("Player stood up to leave the interview"));
		}
  	}
}

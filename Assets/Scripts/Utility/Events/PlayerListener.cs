using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility.Logging;
using UnityStandardAssets.Utility.Config;


namespace UnityStandardAssets.Utility.Events
{
    // Maybe don't need a separate class
    public class PlayerListener : MonoBehaviour
    {
        private static ILogger interviewLogger = Debug.unityLogger;
        private static string kTAG = "PlayerEvent";
        private InterviewLogHandler interviewLogHandler;

        void Start()
        {
            // Instantiate the handler to replace unity's default logger with ours
            interviewLogHandler = new InterviewLogHandler();
            EventSystem.current.RegisterListener<PlayerMoveEvent>(OnPlayerMove);
        }

        void OnPlayerMove(PlayerMoveEvent moveInfo)
        {
            string now = DateTime.Now.ToString("HH:MM:ss:fff");
            GameObject interviewer;
	        Animator interviewerAnim;

            // Whether player left or entered interview room
            if (moveInfo.toLocation == "interviewRoom") {

                interviewer = GameObject.Find(ConfigInfo.envSettings.interviewer);
                //interviewer = GameObject.Find("man1@sittingpose1");
                
                // Set animation trigger
                interviewerAnim = interviewer.GetComponent<Animator>();
                interviewerAnim.SetTrigger("enterRoom");

                // Notify server of event (just for testing purposes)
                //ClientSocketManager.SetSendMsg("The user entered the interview room.");
            } else {

	    		interviewer = GameObject.Find(ConfigInfo.envSettings.interviewer);
                //interviewer = GameObject.Find("man1@sittingpose1");

                // Set animation trigger
                interviewerAnim = interviewer.GetComponent<Animator>();
                interviewerAnim.SetTrigger("exitRoom");

                // Notify server of event (just for testing purposes)
                ClientSocketManager.SetSendMsg("The user exited the interview room.");
            }
        }
    }
}
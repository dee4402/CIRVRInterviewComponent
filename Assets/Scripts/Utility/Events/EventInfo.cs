using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cirvr.ConversationManager;

namespace UnityStandardAssets.Utility.Events
{

    public class EventInfo
    {
        public string EventDesc;
        public EventInfo(string msg)
        {
            EventDesc = msg;
        }

        public string toJSONString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    public class PlayerEnteredInterviewRoom : EventInfo
    {
        public PlayerEnteredInterviewRoom(string msg)
            : base(msg)
        {

        }
    }

    // Events that can happen during the Main Task
    public class PlayerMoveEvent : EventInfo
    {
        public string fromLocation;
        public string toLocation;

        public PlayerMoveEvent(string msg, string from = null, string to = null)
            : base(msg)
        {
            toLocation = to;
            fromLocation = from;
        }
    }

    public class PlayerBeginInterview : EventInfo
    {
        public PlayerBeginInterview(string msg)
            : base(msg)
        {

        }
    }

    public class PlayerEndInterview : EventInfo
    {
        public PlayerEndInterview(string msg)
            : base(msg)
        {
            SettingsInfo.sceneName = "MainMenu";
            UnityEngine.SceneManagement.SceneManager.LoadScene("LoadingScene");
        }
    }

    public class PlayerLeaveInterview : EventInfo
    {
        public PlayerLeaveInterview(string msg)
            : base(msg)
        {

        }
    }

    
    public class BeginDialog : EventInfo
    {
        public Dialog dialog;
        public bool dialogBegan;
        public BeginDialog(string msg, Dialog dialog)
            : base(msg)
        {
            this.dialog = dialog;
            this.dialogBegan = true;
        }

    }

    public class EndDialog : EventInfo
    {
        public Dialog dialog;
        public EndDialog(string msg, Dialog dialog)
            : base(msg)
        {
            this.dialog = dialog;
        }

    }
    public class BeginWhiteBoard : EventInfo
    {
        public BeginWhiteBoard(string msg)
            : base(msg)
        {
        }
    }

    public class SetTimer : EventInfo
    {
        public int time;
        public SetTimer(string msg, int time)
            : base(msg)
        {
            this.time = time;
        }
    }
    public class WBQAnswered : EventInfo
    {
        public WBQAnswered(string msg)
            : base(msg)
        {

        }
    }

    public class InterviewEndedEvent : EventInfo
    {
        public InterviewEndedEvent(string msg)
            : base(msg)
        {
        }
    }

    public class RecognizedSpeechEvent : EventInfo
    {
        public string utterance;
        public RecognizedSpeechEvent(string msg, string utterance)
            : base(msg)
        {
            this.utterance = utterance;
        }
    }

    public class ReceivedE4Data : EventInfo
    {
        public float E4Data;
        public ReceivedE4Data(string msg, float e4data)
            : base(msg)
        {
            this.E4Data = e4data;
        }
    }

    public class ReceivedAzurePath : EventInfo
    {
        public string rcvdMsg;
        public ReceivedAzurePath(string msg, string messageContent)
            : base(msg)
        {
            rcvdMsg = messageContent;
        }
    }
    public class MoveOn : EventInfo
    {
        public string holdMe;
        public MoveOn(string msg)
            : base(msg)
        {

        }
    }

    public class PauseEvent : EventInfo
    {
        public bool audioPause = true;
        public PauseEvent(string catcher, bool boo)
            : base(catcher)
        {
            audioPause = boo;
        }

    }
    public class EndWhiteBoardSection : EventInfo
    {
        public EndWhiteBoardSection(string catcher)
            : base(catcher)
        {

        }
    }

    public class CatchViveTriggerHit : EventInfo {
        public bool hit;
        public CatchViveTriggerHit(string catcher, bool sentBool) 
            : base(catcher) {
                this.hit = sentBool;
            }
    }

    public class EndInterruption : EventInfo
    {
        public Dialog dialog;
        public string interruptee;
        public string interrupter;
        public EndInterruption(string msg, Dialog dialog, string interruptee)
             : base(msg)
        {
            this.dialog = dialog;
            this.interruptee = interruptee;
        }
    }

    public class SetAudioLength : EventInfo
    {
        public float audioLen;
        public SetAudioLength(string msg, float audioLen)
            : base(msg)
        {
            this.audioLen = audioLen;
        }
    }

    public class QuestionWasRepeated : EventInfo
    {
        public QuestionWasRepeated(string msg)
            : base(msg)
        {

        }
    }
    
    public class QuestionWasSkipped : EventInfo 
    {
        public QuestionWasSkipped(string msg)
            : base(msg)
            {
                
            }
    }

    public class CatchAlternate : EventInfo
    {
        public int altIndex;
        public CatchAlternate(string msg, int alternateIndex)
            : base (msg)
        {
            this.altIndex = alternateIndex;
        }
    }

    public class IntervieweeInterruption : EventInfo
    {
        public string interruptionText;
        public IntervieweeInterruption(string msg, string text)
            : base(msg)
        {
            this.interruptionText = text;
        }
    }

}
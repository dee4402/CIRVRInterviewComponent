using System.Collections.Generic;
using UnityEngine;
using System;

namespace Cirvr.ConversationManager
{
    [Serializable]
    public class Dialog
    {
        public string DialogID;
        public string DialogText;
        public List<string> dynamicParams;
        public List<string> staticParams;
        public List<string> alternates;
        public string NextDialogID;
        public float sentimentPivot;
        public int numberEntityPivot;
        public ChildMap childMap;
        public string filterType;
        public List<string> entityTypes;
        public string unrecognizedResponse;
        public int timeLimit;
        public int totalTime;
        public bool requireResponse;
        public List<string> intents;
        public bool finalQuestion;
        public string WBQID;
        public string Interruptee;
        public float InterruptionTime;
        public string[] InterruptionContent;
        public string whiteboardType;
        public int count;
        public string[] requiredParams;
        public string placement;
        public string section;
        public bool userInterruptionEnabled;
        //Beginning of the Question Section
        public bool questionSection;
        //If the user gave an adequate response skip the follow up questions
        public string nextTopicId;
        //flag for catching stressful questions

        public string[] easierQuestions;
    }

    [Serializable]
    public class ChildMap
    {
        public string LESS_THAN;
        public string GREATER_THAN;
        public string EQUAL_TO;
    }
}
using System;
using System.Collections.Generic;
using UnityStandardAssets.Utility.Events;
using UnityEngine;

namespace Cirvr.ConversationManager { 

    public sealed class ConversationContext
    {

        // Keep track of where we are in our dialog sets and question sets
        private Dictionary<string, DialogSet> Dialogs = new Dictionary<string, DialogSet>();
        private string currentDialogSetKey, currentWhiteBoardSetKey;

        // Don't need to keep track of where we are in our FX set
        private Dictionary<String, AudioFX> audioFX;

        public UserState UserState {get; set;} = new UserState();
        private float startTime {get; } = Time.time;
        public int IntervieweeTurns {get; } = 0; 
        public int CurrentSetTurns {get; private set; } = 0;
        public int CurrentWBSetTurns { get; private set; } = 0;

      


        private static readonly ConversationContext instance = new ConversationContext();

        static ConversationContext() { }

        private ConversationContext() { }

        public static ConversationContext Instance
        {
            get
            {
                return instance;
            }
        }

        public bool DialogExists(string id)
        {
            foreach (KeyValuePair<string, DialogSet> kv in Dialogs)
            {
                if (kv.Value.DialogExists(id))
                {
                    return true;
                }
            }

            return false;
        }

        public Dialog GetCurrentDialog()
        {
            return Dialogs[currentDialogSetKey].getCurrentDialog();
        }


        public bool NextDialog()
        {
            return Dialogs[currentDialogSetKey].NextDialog();
        }

        public bool SetDialog(string key)
        {
            return Dialogs[currentDialogSetKey].SetDialog(key);
        }

        public void SetDialogNext(string key)
        {
            Dialogs[currentDialogSetKey].SetDialogNext(key);
        }

        public void SwitchDialogSet(string newSetID)
        {
            currentDialogSetKey = newSetID;
            CurrentSetTurns = 0;
        }

      

        

        public Dialog GetDialogById(string id)
        {
            foreach (KeyValuePair<string, DialogSet> setkv in Dialogs)
            {
                // If we found it here, we'll just make the clip on the fly in the Convo manager
                if (setkv.Value.DialogExists(id))
                {
                    return setkv.Value.DialogDictionary[id];
                }
            }
            
            return null;
        }

        public DialogSet GetDialogSetByDialogId(string id)
        {
            foreach (KeyValuePair<string, DialogSet> setkv in Dialogs)
            {
                // If we found it here, we'll just make the clip on the fly in the Convo manager
                if (setkv.Value.DialogExists(id))
                {
                    return setkv.Value;
                }
            }

            return null;
        }

        public string GetNextDialogText()
        {
            return Dialogs[currentDialogSetKey].GetNextDialogText();    //Michael : this is returning the Dialog ID whenever I use it. It's only use was in Intent filter for skip question.
        }

        public Dialog GetNextDialog()
        {
            return Dialogs[currentDialogSetKey].GetNextDialog();

        }

        public void AddDialogSet(DialogSet set)
        {
            Dialogs.Add(set.DialogSetID, set);
            // Initialize to the first Dialog in the list
            //if (!Dialogs[set.DialogSetID].NextDialog()){
            // throw empty set added
            //}
        }

        public void AddAudioFXSet(Dictionary<String, AudioFX> fxset)
        {
            audioFX = fxset;
        }

        public string GetAudioFXSource(string FXname)
        {
            return audioFX[FXname].source;
        }

        public (string goName, AudioClip clip) GetClipInfoFromSoundID(string id)
        {
            string goTAG = String.Empty;
            DialogSet set = null;

            // Loop through dialog sets to find that ID
            foreach (KeyValuePair<string, DialogSet> setkv in Dialogs)
            {
                // If we found it here, we'll just make the clip on the fly in the Convo manager
                if (setkv.Value.DialogExists(id))
                {
                    set = setkv.Value;
                    return (set.audioSrcGameObjectName, null);
                }
            }

            // If it is in the FX set
            if (set == null && audioFX.ContainsKey(id))
            {
                return (audioFX[id].source, audioFX[id].clip);
            }

            return default;
        }

        /*
         * WB functions
         */
       
    }

}
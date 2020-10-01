using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Cirvr.ConversationManager {

    public class DialogSet
    {
        public string DialogSetID {get; }
        public Dictionary<string, Dialog> DialogDictionary { get; set;}
        private string m_currentDialogId;
        public string audioSrcGameObjectName;

        /*
        public DialogSet(List<string>placeHolder, Dictionary<string, Dialog> dicPlaceHolder, string id)
        {
            ListEnumerator = placeHolder;
            DialogDictionary = dicPlaceHolder;
            this.DialogSetID = id;
        }
        */
        public DialogSet(string id, string start, List<Dialog> dialogs, string srcObjectName)
        {
            Dictionary<string, Dialog> dictionary = new Dictionary<string, Dialog>();
            // Go through and key the dictionary
            foreach (var dialog in dialogs) {
                dictionary.Add(dialog.DialogID, dialog);
            }
            this.DialogSetID = id;
            this.DialogDictionary = dictionary;
            this.m_currentDialogId = start;
            this.audioSrcGameObjectName = srcObjectName;
        }

        public bool DialogExists (string id)
        {
            if (DialogDictionary.ContainsKey(id))
            {
                return true;
            }

            return false;
        }

        /*
        public void AddDialogs(List<Dialog> dialogs)
        {
            foreach(var dialog in dialogs)
            {

            }
            foreach (var dialog in dialogs) {
                DialogList.Add(dialog);
            }
            curr = DialogList.GetEnumerator();
        }
        */

        public bool NextDialog()  
        {
            string nextKey = DialogDictionary[m_currentDialogId].NextDialogID;
            if (DialogDictionary.ContainsKey(nextKey)) {
                m_currentDialogId = DialogDictionary[nextKey].DialogID;
                return true;
            }
            return false;
        }

        public Dialog GetNextDialog()
        {
            string nextKey = DialogDictionary[m_currentDialogId].NextDialogID;
            if (!DialogDictionary.ContainsKey(nextKey))
            {
                //throw
            }
            return DialogDictionary[nextKey];
        }

        public string GetNextDialogText()
        {
            string nextKey = DialogDictionary[m_currentDialogId].NextDialogID;
            if (!DialogDictionary.ContainsKey(nextKey))
            {
                // throw 
            }
            return DialogDictionary[nextKey].DialogID;
        }

        public Dialog getCurrentDialog()
        {
            return DialogDictionary[m_currentDialogId];
        }

        public bool SetDialog(string key)
        {
            if (DialogDictionary.ContainsKey(key))
            {
                m_currentDialogId = DialogDictionary[key].DialogID;
                return true;
            }
            return false;
        }

        public void SetDialogNext(string key)
        {
            DialogDictionary[m_currentDialogId].NextDialogID = key;
        }

    }

}
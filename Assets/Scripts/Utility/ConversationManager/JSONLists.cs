using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace Cirvr.ConversationManager 
{
    public static class FileHandler
    {
        public static void GrabJSON<T>(ref T JsonArrs, string fileName)
        {
            string jsonStringHolder;
            string filePath = Path.Combine(Application.streamingAssetsPath, "Data", "JSON files", fileName);//"Stress_Based_Interruptions.json");
            if (!File.Exists(filePath))
            {
                Debug.LogError("File could not be located");
                // throw
            }

            //Use reader to read whole Json file into jsonholder string
            using (StreamReader reader = File.OpenText(filePath))
            {
                jsonStringHolder = reader.ReadToEnd();
                JsonArrs = JsonUtility.FromJson<T>(jsonStringHolder);
            }  
        }
    }

    [Serializable]
    public class DialogList : MonoBehaviour
    {
        public List<Dialog> dialogs;
    }

    [Serializable]
    public class TopLvlArrList
    {
        public List<Dialog> interviewerDialogs;
    }

   
    
}
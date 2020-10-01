using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityStandardAssets.Utility.Exceptions;
using SimpleJSON;

namespace UnityStandardAssets.Utility.Config
{
    
	public static class ConfigInfo
	{

		static CirvrSettings azureSettings; 
		static CirvrSettings difficultySettings;
		static CirvrSettings playerSettings;
        public static DialogueSettings dialogueSettings;
		public static CirvrSettings envSettings;
		static ConfigInfo()
		{
            // json = readConfig("azure.json");
            // azureSettings = JsonUtility.FromJson<CirvrSettings>(json);

            // json = readConfig("difficulty.json");
            // difficultySettings = JsonUtility.FromJson<CirvrSettings>(json);

            // json = readConfig("player.json");
            // playerSettings = JsonUtility.FromJson<CirvrSettings>(json);

            //dialogueSettings = parseDialogueSettings(readConfig("dialogueConfig.json"));
            envSettings = JsonUtility.FromJson<CirvrSettings>(readConfig("envConfig.json"));
		}

		// Maybe use enum for config file identifier
		static private string readConfig(string filename)
		{
			// Assets/Config/
			filename = Application.streamingAssetsPath + "/Config/" + filename;
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
					return sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
				Debug.unityLogger.Log("Could not read the config file: " + e.Message);
				throw new ConfigException(filename, "Could not read config file " + filename);
            }
        }

        /*
        public static DialogueSettings parseDialogueSettings(string jsonFile)
        {

            string audioBasePath = "ConversationAudio/Interviewer/";
            // make new dialogue settings object
            DialogueSettings settings = new DialogueSettings();
            var json = JSON.Parse(jsonFile);

            Dictionary<string, List<InterviewerQuestion>> sectionQuestions = new Dictionary<string, List<InterviewerQuestion>>();

            // @todo Use SImpleJSON key enumerator on the root node
            string[] sectionNames = { "greetingSection" ,"transitionSection","experienceSection","educationSection","personalSection","closingSection" };
            foreach (string i in sectionNames)
            {
                JSONArray questions = json["questions"][i].AsArray;
                
                List<InterviewerQuestion> questionList = new List<InterviewerQuestion>();
                foreach (JSONNode node in questions.Children)
                {
                    InterviewerQuestion question = new InterviewerQuestion().ConstructFromNode(node);
                    if (!File.Exists(audioBasePath + question.audioPath))
                    {
                        // throw new InitializeAudioError();
                    }


                    question.audioClip = Resources.Load<AudioClip>(audioBasePath + i + "/" + question.audioPath);
                    questionList.Add(question);
                }

                settings.questions.Add(i, questionList);
            }

            return settings;
        }
        */
    }

	[System.Serializable]
	public class CirvrSettings
	{
		public string interviewer;
		public string receptionist;
		public int lighting;
	}

    public class DialogueSettings
    {
        //public Dictionary<string, List<InterviewerQuestion>> questions;

        //public DialogueSettings()
        //{
        //    questions = new Dictionary<string, List<InterviewerQuestion>>();
        //}
    }


}
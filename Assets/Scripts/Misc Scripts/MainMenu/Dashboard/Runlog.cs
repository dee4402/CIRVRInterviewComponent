//Author(s): Spencer Hunt, Joshua Wade
//Purpose: Parses debug log for statements from interviewer and answers from participant, then creates text objects with time between each based 
//on the timestamp.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Runlog : MonoBehaviour
{
    //Logcontrol object instantiated for calling later on.
    [SerializeField]
    private TextLogControl logControl;

    //Lists that contain user and interview dialogue and timestamps converted to double.
    private List<string> responseLog;
    private List<double> timeStamps = new List<double>();

    //public GameObject image;

    //Colors for question and answers
    private Color question = new Color(0, 0, 0, 1); //try black for question
    private Color answer = new Color(0, 0, 1, 1); //try blue for answers

    //Extracts chatlog on start, controls if chat is dynamic or static.
    private void Start()
    {
        responseLog = ExtractChatLog(System.IO.Path.Combine(Application.streamingAssetsPath, "Data", "LogFiles", "testlog.log"));
        
        
        StartCoroutine(Wait());
    }

    //Calls function to create text log.
    public void aLogText(string text, Color textcolor)
    {
        
        logControl.logText(text, textcolor);
    }

    //Extracts chat log and parses it.
    public  List<string> ExtractChatLog(string filePath)
    {
        List<string> dupeQuestions = new List<string>();
        List<string> chatLog = new List<string>();
        string searchToken = "query\":\"";
        string[] rows = (new StreamReader(filePath).ReadToEnd()).Split('\n');
        foreach (string row in rows)
        {
            //check whether the log file entry contains the *user's response* information
            if (row.Contains("{\"topScoringIntent"))
            {
                //extract the timestamp component of the log entry
                string timestamp = row.Substring(0, row.IndexOf(' '));
                //extract the user's response component of the log entry
                int startIndex = row.IndexOf(searchToken) + searchToken.Length;
                string partialUserResponse = row.Substring(startIndex);
                string userResponse = partialUserResponse.Substring(0, partialUserResponse.IndexOf('\"'));
                //return the extracted information
                chatLog.Add(userResponse);
                //image.GetComponent<Image>().color = Color.red;
                //image.GetComponent<Image>().enabled = true;
            }
            //Checks for question and duplicates, if found adds timestamps to seperate list and questions to chatlog.
            if(row.Contains("QUESTION"))
            {
                //image.GetComponent<Image>().color = new Color32(0, 0, 1, 1);

                string timestamp = row.Substring(0, row.IndexOf(' '));
                string question = row.Substring(row.IndexOf(' '));
                timeStamps.Add(ConvertTimeStampToDouble(timestamp));
                if (!dupeQuestions.Contains(question))
                {
                    chatLog.Add( question);
                    dupeQuestions.Add(question);
                    
                }
            }
            
        }
        return chatLog;
    }

    //Wait statement to make chat wait between outputing text.
    IEnumerator Wait()
    {
        //Gets first question timestamp to calculate time between wait.
        float lastquestion = (float)timeStamps[0];
        //Goes through each timestamp and prints log according to time between each chat.
        for (int i = 0; i < timeStamps.Count; i++)
        {
            if(SizeKeeper.TypeOfGraph == 1)
                yield return new WaitForSeconds((float)timeStamps[i] - lastquestion);
            lastquestion = (float)timeStamps[i];
            if (responseLog[i].Contains("QUESTION"))
            {
                aLogText(responseLog[i].Replace("QUESTION", ""), question);
            }
            else
                aLogText(responseLog[i], answer);
            
        }
    }

    //Converts string timestamp to double.
    private double ConvertTimeStampToDouble(string timeStamp)
    {
        string[] splitter = timeStamp.Split(new char[] { ':' });

        double hour = Convert.ToDouble(splitter[0]);

        double minute = 0;
        if (splitter.Length > 1)
            minute = Convert.ToDouble(splitter[1]);

        double second = 0;
        if (splitter.Length > 2)
            second = Convert.ToDouble(splitter[2]);

        double millisecond = 0;
        if (splitter.Length > 3)
            millisecond = Convert.ToDouble(splitter[3]);
        millisecond = millisecond / 1000;

        return 3600 * hour + 60 * minute + second + millisecond;
    }
}

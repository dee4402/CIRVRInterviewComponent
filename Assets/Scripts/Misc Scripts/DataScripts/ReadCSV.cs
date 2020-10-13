using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using UnityStandardAssets.Utility.Events;
using Cirvr.ConversationManager;
using SimpleJSON;

//for reading in the emotion csv and sending the data to the graphing scripts. also used to display the emotion raw data from main menu
public class ReadCSV : MonoBehaviour
{
    List<CSVInfo> dataList = new List<CSVInfo>();
    public Text textComponent;
    private int fileLength;
    private string[] data;

    //the graph scripts the data is fed to
    private WindowGraph emotionScript1;
    private WindowGraph emotionScript2;
    private HeadGraphs headScript;
    private StressGraph stressScript;

    //called when a dropdown option is selected
    public void PrepData(int index)
    {
        emotionScript1 = GameObject.Find("Data Branches/Tracked emotions (Graph) (8 lines)/Window_Graph").GetComponent<WindowGraph>();
        emotionScript2 = GameObject.Find("Data Branches/Tracked emotions (Graph) (Valence)/Window_Graph").GetComponent<WindowGraph>();
        headScript = GameObject.Find("Data Branches/Head movement graph/Window_Graph").GetComponent<HeadGraphs>();
        stressScript = GameObject.Find("Data Branches/Stress Graph/Window_Graph").GetComponent<StressGraph>();

        if (index != 0)
        {
            textComponent.text = "";
            dataList.Clear();

            FillCSVData("CSV files", index); //read in all the csv data
            FillCSVData("Events", index);
            FillCSVData("StressCSVs", index);

            for (int i = 1; i < fileLength - 1; ++i)
            {
                string[] row = data[i].Split(new char[] { ',' });

                for (int k = 14; k < row.Length; ++k) //loop through and format the data, if needed
                {
                    if (row[k].Trim() == "0")
                        row[k] = "0.000";
                    if (row[k].Trim().Length == 4)
                        row[k] += "0";
                    if (row[k].Trim() == "-")
                        row[k] = "-------";
                }

                CSVInfo q = new CSVInfo(); //fill in data
                q.Index = row[0];
                q.anger = row[14];
                q.contempt = row[15];
                q.disgust = row[16];
                q.fear = row[17];
                q.happiness = row[18];
                q.neutral = row[19];
                q.sadness = row[20];
                q.surprise = row[21];

                dataList.Add(q);
            }
            CheckForTextComponent();
        }
    }

    //read in the data from the files
    private void FillCSVData(string pathName, int index)
    {
        List<string> list = new List<string>();
        string[] files = System.IO.Directory.GetFiles("Assets/Data/" + pathName);
        foreach (var i in files)
        {
            if (i.EndsWith("csv"))
                list.Add(i);
        }

        string file = "";
        if (pathName == "CSV files")
            file = System.IO.File.ReadAllText(list[index - 1]);
        else if (pathName == "Events")
            file = System.IO.File.ReadAllText(list[list.Count - 1]); //temporary, will eventually be list[index - 1]
        else
            file = System.IO.File.ReadAllText(list[0]); //temporary, will eventually be list[index - 1]

        string[] dataArray = file.Split(new char[] { '\n' });

        if (pathName == "CSV files") //set the data for the corresponding scripts 
        {
            emotionScript1.dataArray = emotionScript2.dataArray = headScript.dataArray = dataArray;
            fileLength = dataArray.Length;
            data = dataArray;
        }
        else if (pathName == "Events")
        {
             emotionScript1.eventArray = emotionScript2.eventArray = stressScript.eventArray = headScript.eventArray = dataArray;
        }
        else
        {
             stressScript.dataArray = dataArray;
        }
    }

    void CheckForTextComponent()
    {
        //If text hasn't been assigned, disable ourselves
        if (textComponent == null)
        {
            Debug.Log("You must assign a text component!");
            enabled = false;
            return;
        }
        UpdateText(MakeDataString());
    }

    //creates the large string that holds all the data that is fed to the text component for displaying
    string MakeDataString()
    {
        string completeString = "";

        for (int i = 0; i < fileLength - 2; ++i)
        {
            if (i < 99) //need to format it differently for indexes over 100
            {
                completeString += "   " + dataList[i].Index.Trim() + "\t\t\t  " + dataList[i].anger.Trim() + "\t\t\t  " + dataList[i].contempt.Trim()
                 + "\t\t\t  " + dataList[i].disgust.Trim() + "\t\t\t  " + dataList[i].fear.Trim() + "\t\t\t  " + dataList[i].happiness.Trim()
                 + "\t\t\t  " + dataList[i].neutral.Trim() + "\t\t\t  " + dataList[i].sadness.Trim() + "\t\t\t  " + dataList[i].surprise.Trim() + "\n";
            } else
            {
                completeString += "  " + dataList[i].Index.Trim() + "\t\t  " + dataList[i].anger.Trim() + "\t\t\t  " + dataList[i].contempt.Trim()
                 + "\t\t\t  " + dataList[i].disgust.Trim() + "\t\t\t  " + dataList[i].fear.Trim() + "\t\t\t  " + dataList[i].happiness.Trim()
                 + "\t\t\t  " + dataList[i].neutral.Trim() + "\t\t\t  " + dataList[i].sadness.Trim() + "\t\t\t  " + dataList[i].surprise.Trim() + "\n";
            }
        }
        return completeString;
    }

    //updates the text component to display the information 
    void UpdateText(string value)
    {
        var rect = textComponent.GetComponent<RectTransform>();
        rect.position = new Vector3(0, 150, 0);

        //Update the text shown in the text component by setting the `text` variable
        if (fileLength < 2)
            textComponent.text = "No data to show.";
        else
            textComponent.text = value;
    }
}

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

//for displaying the data to the main menu
public class FullReportManager : MonoBehaviour
{
    List<FullReportInfo> dataList = new List<FullReportInfo>();
    public Text textComponent;
    private int fileLength;
    private string[] data;

    //called when a dropdown option is selected, this processes the data for displaying 
    public void PrepData(int index)
    {
        if (index != 0)
        {
            textComponent.text = ""; //clear out old stuff
            dataList.Clear();

            FillCSVData("CombinedData CSVs", index);

            for (int i = 1; i < fileLength; ++i) //loop through the full data csv and format the data
            {
                if (data[i].Trim() != "")
                {
                    string[] row = data[i].Split(new char[] { ',' });

                    for (int k = 0; k < row.Length; ++k)
                    {
                        if (k == 1 && row[k].Trim().Length < 8 && !row[1].Contains("-")) //format the timestamp 
                        {
                            string[] splitter = row[1].Split(new char[] { ':' });
                            if (splitter[0].Length == 1)
                                row[k] = "0" + row[k];
                            if (splitter[1].Length == 1)
                                row[k] = row[k].Insert(row[k].IndexOf(":") + 1, "0");
                            if (splitter[2].Length == 1)
                                row[k] = row[k].Insert(row[k].Length - 1, "0");
                        }

                        if (k == 4 && !row[4].Contains("-") && row[4].Trim().Length == 1) //format interruptions
                            row[4] = "0" + row[4].Trim();

                        if (row[k].Trim() == "0" && k != 4) //format zeros
                            row[k] = "0.000";
                       
                        if (row[k].Trim() == "-") //format blank elements
                            row[k] = "-------";

                        if (row[k].Trim().Contains(".") && k > 4 && row[k].Trim().Length < 5) //format the eye gaze times
                        {
                            int t = 5 - row[k].Trim().Length;
                            for (int o = 0; o < t; ++o)
                                row[k] += "0";
                        }
                    }

                    FullReportInfo q = new FullReportInfo(); //fill in data for this line
                    q.questionNum = row[0];
                    q.ts = row[1];
                    q.responseLength = row[3];
                    q.interruptions = row[4];
                    q.eyes = row[5];
                    q.nose = row[6];
                    q.mouth = row[7];
                    q.cheeks = row[8];
                    q.forehead = row[9];
                    q.desk = row[10];
                    q.monitor = row[11];
                    q.keyboard = row[12];
                    q.lamp = row[13];
                    q.window = row[14];
                    q.clock = row[15];
                    q.picture = row[16];
                    q.mouse = row[17];
                    q.emotion = row[18];
                 
                    dataList.Add(q);
                }
            }
            CheckForTextComponent();
        }
    }

    //read in the csv data
    private void FillCSVData(string pathName, int index)
    {
        List<string> list = new List<string>();
        string[] files = System.IO.Directory.GetFiles("Assets/Data/" + pathName);
        foreach (var i in files)
        {
            if (i.EndsWith("csv"))
                list.Add(i);
        }

        string file = System.IO.File.ReadAllText(list[0]); //temporary, eventually will be list[index - 1] when we get more data
        data = file.Split(new char[] { '\n' });
        fileLength = data.Length;
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

    //makes a string that will display all the data
    string MakeDataString()
    {
        string completeString = "";
      
        for (int i = 0; i < fileLength - 3; ++i)
        {
            if (!dataList[i].questionNum.Contains("s")) //for all regular rows
            {
                completeString += "     " + dataList[i].questionNum.Trim() + "\t   " + dataList[i].ts.Trim() + "\t   " + dataList[i].responseLength.Trim()
                + "\t   " + dataList[i].interruptions.Trim() + "\t   " + dataList[i].eyes.Trim() + "\t   " + dataList[i].nose.Trim()
                + "\t   " + dataList[i].mouth.Trim() + "\t   " + dataList[i].cheeks.Trim() + "\t   " + dataList[i].forehead.Trim() 
                + "\t   " + dataList[i].desk.Trim() + "\t   " + dataList[i].monitor.Trim() + "\t   " + dataList[i].keyboard.Trim() 
                + "\t   " + dataList[i].lamp.Trim() + "\t   " + dataList[i].window.Trim() + "\t   " + dataList[i].clock.Trim()
                + "\t   " + dataList[i].picture.Trim() + "\t   " + dataList[i].mouse.Trim() + "\t   " + dataList[i].emotion.Trim() + "\n";
            } else if (dataList[i].questionNum.Trim() == "Totals") //for totals row
            {
                completeString += dataList[i].questionNum.Trim() + "\t    ----------\t   \t   " + dataList[i].responseLength.Trim()
               + "\t   " + dataList[i].interruptions.Trim().Substring(0, 2) + "\t   " + dataList[i].eyes.Trim() + "\t   " + dataList[i].nose.Trim()
               + "\t   " + dataList[i].mouth.Trim() + "\t   " + dataList[i].cheeks.Trim() + "\t   " + dataList[i].forehead.Trim()
               + "\t   " + dataList[i].desk.Trim() + "\t   " + dataList[i].monitor.Trim() + "\t   " + dataList[i].keyboard.Trim()
               + "\t   " + dataList[i].lamp.Trim() + "\t   " + dataList[i].window.Trim() + "\t   " + dataList[i].clock.Trim()
               + "\t   " + dataList[i].picture.Trim() + "\t   " + dataList[i].mouse.Trim() + "\t   " + dataList[i].emotion.Trim() + "\n";
            } else //for avgs row
            {
                 completeString += " " + dataList[i].questionNum.Trim() + "\t    ----------\t   \t   " + dataList[i].responseLength.Trim()
               + "\t   " + dataList[i].interruptions.Trim().Substring(0, 2) + "\t   " + dataList[i].eyes.Trim() + "\t   " + dataList[i].nose.Trim()
               + "\t   " + dataList[i].mouth.Trim() + "\t   " + dataList[i].cheeks.Trim() + "\t   " + dataList[i].forehead.Trim()
               + "\t   " + dataList[i].desk.Trim() + "\t   " + dataList[i].monitor.Trim() + "\t   " + dataList[i].keyboard.Trim()
               + "\t   " + dataList[i].lamp.Trim() + "\t   " + dataList[i].window.Trim() + "\t   " + dataList[i].clock.Trim()
               + "\t   " + dataList[i].picture.Trim() + "\t   " + dataList[i].mouse.Trim() + "\t   " + dataList[i].emotion.Trim() + "\n";
            }
           
        }
        return completeString;
    }

    //sets the actual text field with the data to display
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

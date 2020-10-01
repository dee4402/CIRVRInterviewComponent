using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;

public class CompassScript : MonoBehaviour
{
    private string[] fileData;

    private List<string> SceneObjects = new List<string>();
    private List<string> QuestionsStamps = new List<string>();
    private List<double> TimeStampsDouble = new List<double>();

    // Start is called before the first frame update
    void Start()
    {
        ReadCSV();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void ReadCSV()
    {
        int filelength;
        List<string> filelist = new List<string>();

        string[] files = System.IO.Directory.GetFiles("Assets/Data/EyeTracker files");

        //Reads in each file from directory
        foreach (var i in files)
        {
            if (i.EndsWith("csv"))
            {
                filelist.Add(i);
            }
        }

        //Reads in text from file at top of folder. Needs to be changed to the index of menu when more data is availible.
        string file = System.IO.File.ReadAllText(filelist[filelist.Count - 1]);
        fileData = file.Split(new char[] { ',', '\n' });
        filelength = fileData.Length;

        //Adds data to corresponding lists, increments by 22 to account for changing rows, each number added to I is the index of a column in the CSV
        for (int i = 5; i < filelength - 1; i += 5)
        {
            TimeStampsDouble.Add(ConvertTimeStampToDouble(fileData[i]));
            SceneObjects.Add(fileData[i + 3]);
            QuestionsStamps.Add(fileData[i + 4]);
        }
    }
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

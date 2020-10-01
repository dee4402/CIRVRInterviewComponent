//Creates file combining data from stress, emotion, and gaze data files into larger file.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;


public class CombineData : MonoBehaviour
{

    //Lists that store relative Data items from different CSVs
    private List<string> gazeItems = new List<string>();
    private List<double> gazeTimes = new List<double>();
    private List<double> stressTimes = new List<double>();
    private List<double> stressData = new List<double>();
    private List<double> emotionTimes = new List<double>();
    private List<string> emotionTimesString = new List<string>();
    private List<double> anger = new List<double>();
    private List<double> contempt = new List<double>();
    private List<double> disgust = new List<double>();
    private List<double> fear = new List<double>();
    private List<double> happiness = new List<double>();
    private List<double> neutral = new List<double>();
    private List<double> sadness = new List<double>();
    private List<double> suprise = new List<double>();
    private List<string> questionTimestamps = new List<string>();
    private List<double> emotionData = new List<double>();
    private List<string> objects = new List<string>();
    private List<int> objectCounts = new List<int>();
    private List<string> objectRow = new List<string>();
    private List<string> output = new List<string>();

    //Holds data from first file reads
    string[] stressFiledata, emotionFiledata, gazeFiledata;
    void Start()
    {
        Debug.Log(Application.persistentDataPath);
        ReadCSV();
        GazetoEmotion();
        FindObjects();
        CalcValues();
        TestData();
        //WriteData();
    }


    //Reads in data from the different CSV files and stores them in the different relevant lists
    private void ReadCSV()
    {
        //Holds files
        string[] stressfiles, emotionfiles, gazefiles;
        string file;

        //Holds files from different directories
        List<string> filelist = new List<string>();

        //Checks to see if run is using Unity editor or build. Changes file path accordingly
        if(Application.isEditor)
        {
            stressfiles = System.IO.Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "Data", "StressCSVs"));//("Assets/Data/StressCSVs");
            emotionfiles = System.IO.Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "Data", "CSV files"));//("Assets/Data/CSV files");
            gazefiles = System.IO.Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "Data", "Eyetracker files"));//("Assets/Data/Eyetracker files");
        }
        else
        {
            stressfiles = System.IO.Directory.GetFiles(Application.persistentDataPath + "/StressFiles");
            emotionfiles = System.IO.Directory.GetFiles(Application.persistentDataPath + "/EmotionFiles");
            gazefiles = System.IO.Directory.GetFiles(Application.persistentDataPath + "/GazeFiles");
        }

        //Adds each file to filelist
        foreach(var i in stressfiles)
        {
            if(i.EndsWith("csv"))
            {
                filelist.Add(i);
            }
        }

        //Reads in text from file
        file = System.IO.File.ReadAllText(filelist[filelist.Count - 2]);

        //Splits file along rows and columns
        stressFiledata = file.Split(new char[] { ',', '\n' });

        //Adds data from file to lists
        for(int i =2; i < stressFiledata.Length-1; i+=2)
        {
            stressTimes.Add(ConvertTimeStampToDouble(stressFiledata[i]));
            stressData.Add(Convert.ToDouble(stressFiledata[i + 1]));
                
        }
        filelist.Clear();

        foreach(var i in emotionfiles)
        {
            if(i.EndsWith("csv"))
            {
                filelist.Add(i);
            }
        }

        file = System.IO.File.ReadAllText(filelist[filelist.Count- 2]);
        emotionFiledata = file.Split(new char[] { ',', '\n' });
        for(int i = 22; i<emotionFiledata.Length-1; i+=22)
        {
            emotionTimes.Add(ConvertTimeStampToDouble(emotionFiledata[i + 1]));
            emotionTimesString.Add(emotionFiledata[i + 1]);
            if (!emotionFiledata[i + 14].Contains("-")) 
            {
                anger.Add(Convert.ToDouble(emotionFiledata[i + 14]));
                contempt.Add(Convert.ToDouble(emotionFiledata[i + 15]));
                disgust.Add(Convert.ToDouble(emotionFiledata[i + 16]));
                fear.Add(Convert.ToDouble(emotionFiledata[i + 17]));
                happiness.Add(Convert.ToDouble(emotionFiledata[i + 18]));
                neutral.Add(Convert.ToDouble(emotionFiledata[i + 19]));
                sadness.Add(Convert.ToDouble(emotionFiledata[i + 20]));
                suprise.Add(Convert.ToDouble(emotionFiledata[i + 21]));
            }
            else
            {
                anger.Add(99);
                contempt.Add(99);
                disgust.Add(99);
                fear.Add(99);
                happiness.Add(99);
                neutral.Add(99);
                sadness.Add(99);
                suprise.Add(99);
            }
        }
        filelist.Clear();

        foreach(var i in gazefiles)
        {
            if(i.EndsWith("csv"))
            {
                filelist.Add(i);
            }
        }

        file = System.IO.File.ReadAllText(filelist[filelist.Count - 2]);
        gazeFiledata = file.Split(new char[] { ',', '\n' });
        
        for(int i =5; i < gazeFiledata.Length-1; i+=5)
        {
            gazeItems.Add(gazeFiledata[i + 3]);
            gazeTimes.Add(ConvertTimeStampToDouble(gazeFiledata[i]));
            if(gazeFiledata[i+4].Contains("Question"))
            {
                questionTimestamps.Add(gazeFiledata[i + 4] + "|" + gazeFiledata[i]);
            }

        }

        filelist.Clear();



    }

    //Finds timestamps for questions and stores them in a list
    private void FindTimeStamps()
    {
        List<double> timeStamps = new List<double>();
        string[] questionLines;
        for(int i =0; i< questionTimestamps.Count-1; i++)
        {
            questionLines = questionTimestamps[i].Split(new char[] { '|' });
            timeStamps.Add(ConvertTimeStampToDouble(questionLines[0]));
        }
    }

    //Converts timestamp format into double for comparisons
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

        return 3600 * hour + 60 * minute + second;
    }

    //Calculates values for Emotion and compacts gaze data into 5 second increments
    private void CalcValues()
    {
        double value,endtime;
        endtime = gazeTimes[gazeTimes.Count - 1];
        for (int i = 0; i < happiness.Count; i++)
        {
            if (happiness[i] != 99)
            {
                value = .5 + (.5 * happiness[i]) - (.5 * (anger[i] + contempt[i] + disgust[i] + fear[i] + sadness[i] + suprise[i]));
                emotionData.Add(value);
            }
            else
                emotionData.Add(-5);
        }        
    }
    //Finds each unique object from gaze data and adds it to a list.
    private void FindObjects()
    {
        for(int i = 0; i < gazeItems.Count; i++)
        {
            if (i == 0)
            {
                objects.Add(gazeItems[i]);
                objectCounts.Add(0);
            }
            else
            {
                if (!objects.Contains(gazeItems[i]))
                {
                    objects.Add(gazeItems[i]);
                    objectCounts.Add(0);
                }
            }
        }
    }

    //Counts the number of each object looked at in a time period and adds the count to a string and adds that to a list for later printing to file
    private void CountObjects(int increment, int start)
    {
        string row = "";
        //if increment is 0 then there are no objects to count since last time, so set all as 0
        if (increment == 0)
        {
            row = "";
            for (int x = 0; x < objectCounts.Count; x++)
            {
                row = row + "0,";
            }
            objectRow.Add(row);
        }
        else
        {
            //Counts objects between start point until increment is over
            for (int i = start; i < start + increment; i++)
            {
                for (int x = 0; x < objects.Count; x++)
                {
                    if (gazeItems[i] == objects[x])
                    {
                        objectCounts[x]++;
                    }
                }
            }
            //Adds objects to string to be added to file later
            for (int i = 0; i < objectCounts.Count; i++)
            {
                if (i == objectCounts.Count - 1)
                    row = row + objectCounts[i].ToString() + ",";
                else
                    row = row + objectCounts[i].ToString() + ",";

                objectCounts[i] = 0;
            }
            objectRow.Add(row);
        }
    }
       
    //Adds points to start of emotion to account for gaze
    private void GazetoEmotion()
    {
        double starttime = emotionTimes[0];
        List<double> tempList = new List<double>();
        for ( int i = 0; i < emotionTimes.Count; i++)
        {
            if(emotionTimes[i] < gazeTimes[0])
            {
                tempList.Add(emotionTimes[i]);
                gazeItems.Insert(0, "Hasnt Started");
            }
        }
        tempList.AddRange(gazeTimes);
        gazeTimes = tempList;
    }
    //Used for making emotion data and gaze data compatible.
    
    private void TestData()
    {
        int start = 0, increment = 0;
        for (int i = 0; i < emotionTimes.Count; i++)
        {
            increment = 0;
            for (int x = start; x < gazeTimes.Count; x++)
            {                
                if (gazeTimes[x] <= emotionTimes[i])
                {
                    increment++;
                }
                else
                {
                    x = x + gazeTimes.Count;
                }
            }
            CountObjects(increment, start);
            start = start + increment;            
        }
    }
    //Used to create a CSV file and write data to it with time stamp
    private void WriteData()
    {
        //Gets date and time and replaces characters to make it compatible with Windows File conventions
        string name = "" + System.DateTime.UtcNow.ToString();
        name = name.Replace("/", "-");
        name = name.Replace(":", "_");
        output.Add(name);
        int stresscount = 0;
        //Filepath for data to be written using date time for file name.
        string filePath = Application.dataPath + "/CSV/" +"Combined_data"+ name + ".csv";

        //Begining of header
        string row = "Timestamp, Stress data,";
        for (int x = 0; x < objectCounts.Count; x++)
        {
            row = row + objects[x] + ",";
        }
        row = row + "Emotion data";
        //Creates stream for writing to file
        StreamWriter outstream = System.IO.File.CreateText(filePath);
        output.Add(row);

        //Adds data from different lists together in output list
        row = "";
        for (int i = 0; i < emotionTimesString.Count; i++)
        {
            row = "";
            row = emotionTimesString[i]+",";
            if(i%3 == 0)
            {
                row = row + stressData[stresscount]+",";
                stresscount++;
            }
            else
            {
                row = row+ ",";
            }

            row = row + objectRow[i] + emotionData[i];
            
            output.Add(row);
        }
        //Sets filestream flush to true to clear stream buffer every time it writes a line
        outstream.AutoFlush = true;

        //Puts all data into file.
        for (int i = 0; i < output.Count; i++)
        {
            outstream.WriteLine(output[i]);
        }
    }
}

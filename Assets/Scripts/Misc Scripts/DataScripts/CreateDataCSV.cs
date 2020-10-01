using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Threading.Tasks;

//uncomment everything to create the full data csv from the main menu
/*
public class CreateDataCSV : MonoBehaviour
{
    private const string dataHeaders = "Question Number, TimeStamp, Question, Response Length, Interruptions, Eyes, Nose, Mouth, Cheeks, Forehead, Desk, Monitor, Keyboard, Lamp, Window, Clock, Picture, Mouse, Avg. Overall Emotion\n";
    private string[] eventData, emotionData, eyeData;
    public FileStream currentCSVFile;
    private double[] itemTimes = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; //Eyes, Nostrils, Mouth, Cheeks, Forehead, Desk, Monitor, Keyboard, Lamp, Window, Clock, Picture, Mouse
    private double[] totalItemTimes = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; //Eyes, Nostrils, Mouth, Cheeks, Forehead, Desk, Monitor, Keyboard, Lamp, Window, Clock, Picture, Mouse
    private int lastIndex, numQuestions, numResponseQuestions, numEmotionQuestions;
    private double totalResponseLength, totalInterruptions, totalEmotion;

    void Start()
    {
        CreateCSVFile();
        lastIndex = 1;
        numQuestions = numResponseQuestions = numEmotionQuestions = 0;
        totalResponseLength = totalInterruptions = totalEmotion = 0;
    }

    public void ValueChanged(int index)
    {
        if (index != 0)
        {
            GetData("Events"); //question stuff
            GetData("CSV files", index); //emotion stuff
            GetData("EyeTracker files", index); //eye gaze stuff
            //add stress stuff here

            for (int i = 1; i < eventData.Length; ++i) //loop through each line of the event csv 
            {
                if (eventData[i].Trim() != "") 
                {
                    ++numQuestions;
                    string[] row = eventData[i].Split(new char[] { ',' });
                    if (int.Parse(row[1]) < 10) //some formatting
                        row[1] = "0" + row[1].Trim();
                    if (row[3].Trim().Length < 5 && !row[3].Contains("-"))
                        row[3] += "0";
                    if (row[3].Contains("-"))
                        row[3] = "0.000";

                    string line = row[1] + ", " + row[0] + ", " + row[2].Trim() + ", " + row[3].Trim() + ", " + row[4].Trim(); //add the data from the event csv

                    if (!row[3].Contains("-"))
                    {
                        totalResponseLength += double.Parse(row[3].Trim()); //used for total response length and avg response length at end of file
                        ++numResponseQuestions;
                    }
                    totalInterruptions += double.Parse(row[4].Trim());

                    line = AddEyeData(line, numQuestions); //add the eye tracking data 

                    //add the emotion data
                    if (i < eventData.Length - 1)
                        line = AddEmotionData(line, row[0], eventData[i + 1].Split(new char[] { ',' })[0]);
                    else
                        line = AddEmotionData(line, row[0]);

                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(line); //write line to file
                    currentCSVFile.Write(bytes, 0, bytes.Length);
                    line = "";
                }
            }
            AddTotalsAvgsRow(); //at the end, append the totals and avgs line to the file
        }
    }

    private void AddTotalsAvgsRow()
    {
        //add total row
        string total = "\nTotals, -----, -----, " + FormatDouble(totalResponseLength) + ", " + FormatDouble(totalInterruptions) + ", ";
        total += FormatDouble(totalItemTimes[0]) + ", " + FormatDouble(totalItemTimes[1]) + ", " + FormatDouble(totalItemTimes[2]) + ", " + FormatDouble(totalItemTimes[3]) +
            ", " + FormatDouble(totalItemTimes[4]) + ", " + FormatDouble(totalItemTimes[5]) + ", " + FormatDouble(totalItemTimes[6]) + ", " + FormatDouble(totalItemTimes[7]) +
            ", " + FormatDouble(totalItemTimes[8]) + ", " + FormatDouble(totalItemTimes[9]) + ", " + FormatDouble(totalItemTimes[10]) + ", " + FormatDouble(totalItemTimes[11]) +
            ", " + FormatDouble(totalItemTimes[12]) + ", -----\n";

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(total);
        currentCSVFile.Write(bytes, 0, bytes.Length);

        //add avg row
        string avg = "Avgs, -----, -----, " + FormatDouble(totalResponseLength / numResponseQuestions) + ", " + FormatDouble(totalInterruptions / numQuestions) + ", ";
        avg += FormatDouble(totalItemTimes[0] / numQuestions) + ", " + FormatDouble(totalItemTimes[1] / numQuestions) + ", " + FormatDouble(totalItemTimes[2] / numQuestions)
            + ", " + FormatDouble(totalItemTimes[3] / numQuestions) + ", " + FormatDouble(totalItemTimes[4] / numQuestions) + ", " + FormatDouble(totalItemTimes[5] / numQuestions)
            + ", " + FormatDouble(totalItemTimes[6] / numQuestions) + ", " + FormatDouble(totalItemTimes[7] / numQuestions) + ", " + FormatDouble(totalItemTimes[8] / numQuestions)
            + ", " + FormatDouble(totalItemTimes[9] / numQuestions) + ", " + FormatDouble(totalItemTimes[10] / numQuestions) + ", " + FormatDouble(totalItemTimes[11] / numQuestions)
            + ", " + FormatDouble(totalItemTimes[12] / numQuestions) + ", " + FormatDouble(totalEmotion / numEmotionQuestions) + "\n";

        byte[] bytes1 = System.Text.Encoding.UTF8.GetBytes(avg);
        currentCSVFile.Write(bytes1, 0, bytes1.Length);
    }

    //used for the totals and avgs lines, makes sure all the numbers are formatted correctly for main menu displaying
    private string FormatDouble(double d)
    {
        string temp = d.ToString().Trim();
        if (!temp.Contains(".") && temp.Length < 5) //set it to 5 characters long
        {
            int t = 5 - temp.Length;
            for (int i = 0; i < t; ++i)
                temp = " " + temp;
            return temp;
        }

        if (temp == "0") //format zeros
            return "0.000";

        if (temp.Length < 5) //add zeros to the end of decimals to 5 character length
        {
            int t = 5 - temp.Length;
            for (int i = 0; i < t; ++i)
                temp += "0";
            return temp;
        }
        else if (temp.Length > 5) //cut off decimals that are too long
        {
            return temp.Substring(0, 5);
        }
        else
        {
            return temp;
        }
    }

    //reads in the data from a csv file
    private void GetData(string name, int index = 0)
    {
        List<string> list = new List<string>();
        string[] files = System.IO.Directory.GetFiles("Assets/Data/" + name);
        foreach (var i in files) //add the actual files in the directory to a list
        {
            if (i.EndsWith("csv"))
                list.Add(i);
        }

        //get the desired file to read from 
        string file = "";
        file = System.IO.File.ReadAllText(list[0]); //for testing
         
        //if (name == "Events")
        //    file = System.IO.File.ReadAllText(list[0]); //temporary, will be list[index - 1] eventually once we get more data 
        //else
        //    file = System.IO.File.ReadAllText(list[index - 1]);

        if (name == "Events") //set the data variables 
            eventData = file.Split(new char[] { '\n' });
        else if (name == "CSV files")
            emotionData = file.Split(new char[] { '\n' });
        else
            eyeData = file.Split(new char[] { '\n' });
    }

    //this takes the eye gaze csv data and computes the time spent looking at each gaze point.Will do my best to explain it but ask Harrison if you need help understanding
    private string AddEyeData(string line, int questionNum)
    {
        //find the starting point for this question (find the next row that has a "Question x started" element)
        int start = lastIndex;
        for (int i = lastIndex; i < eyeData.Length; ++i)
        {
            string[] row = eyeData[i].Split(new char[] { ',' });
            if (row.Length == 5 && row[4].Trim().StartsWith("Question"))
            {
                start = i; //set starting point
                break;
            }
        }

        string item = ""; //the current gaze point object 
        int firstIndex = 0; //the first index of the current gaze point object
        for (int j = start; j < eyeData.Length; ++j)
        {
            string[] row = eyeData[j].Split(new char[] { ',' });
            if (j == start || row.Length == 4 || (row.Length == 5 && row[4].Trim() == "")) //keep looping until the next "question x started" in row[4] is found
            {
                if (item == "") //for the first point, set the item being looked at and the index 
                {
                    firstIndex = j;
                    item = row[3].Trim();
                }

                if (row[3] != item) //new gaze point object detected, compute the time spent looking at the previous object and record it
                {
                    double time = ((double)ConvertTimestampToInt(eyeData[j - 1].Split(new char[] { ',' })[0]) - (double)ConvertTimestampToInt(eyeData[firstIndex].Split(new char[] { ',' })[0])) / 1000.0; //in seconds
                    UpdateTimeValue(time, item);
                    item = row[3]; //set the new current gaze point object and its first index
                    firstIndex = j;
                }
            }
            else //next question worth of gaze points found, done for now
            {
                double time = ((double)ConvertTimestampToInt(eyeData[j - 1].Split(new char[] { ',' })[0]) - (double)ConvertTimestampToInt(eyeData[firstIndex].Split(new char[] { ',' })[0])) / 1000.0; //in seconds
                UpdateTimeValue(time, item);
                lastIndex = j; //set where we left off so we know where to start for the next call
                for (int k = 0; k < itemTimes.Length; ++k)
                {
                    line += ", " + itemTimes[k]; //add the data to the current line of data
                    totalItemTimes[k] += itemTimes[k]; //add up totals for the totals/avgs lines at end
                    itemTimes[k] = 0; //reset times for next run
                }
                break;
            }
        }
        return line;
    }

    //keep track of times spent looking at each object
    private void UpdateTimeValue(double time, string item)
    {
        if (item == "Eyes")
            itemTimes[0] += time;
        else if (item == "Nostrils")
            itemTimes[1] += time;
        else if (item == "Mouth")
            itemTimes[2] += time;
        else if (item == "CheekR" || item == "CheekL")
            itemTimes[3] += time;
        else if (item == "Forehead")
            itemTimes[4] += time;
        else if (item == "Desk")
            itemTimes[5] += time;
        else if (item == "Monitored")
            itemTimes[6] += time;
        else if (item == "Keyboard")
            itemTimes[7] += time;
        else if (item == "Lamp")
            itemTimes[8] += time;
        else if (item == "Office window")
            itemTimes[9] += time;
        else if (item == "clock")
            itemTimes[10] += time;
        else if (item == "picture")
            itemTimes[11] += time;
        else if (item == "mouse")
            itemTimes[12] += time;
    }

    //used to get time spent looking at each gaze point object
    private int ConvertTimestampToInt(string timeStamp)
    {
        string[] splitter = timeStamp.Split(new char[] { ':' });

        int hour = Convert.ToInt32(splitter[0]);

        int minute = 0;
        if (splitter.Length > 1)
            minute = Convert.ToInt32(splitter[1]);

        int second = 0;
        if (splitter.Length > 2)
            second = Convert.ToInt32(splitter[2]);

        int millisecond = 0;
        if (splitter.Length > 3)
            millisecond = Convert.ToInt32(splitter[3]);

        return 3600000 * hour + 60000 * minute + 1000 * second + millisecond;
    }

    //used for adding the emotion data to the csv
    private string AddEmotionData(string line, string startTime, string endTime = "")
    {
        double total = 0;
        int count = 0;
        int start = ConvertTimestampToInt(startTime);
        int end = 0;
        if (endTime != "")
            end = ConvertTimestampToInt(endTime);

        if (end != 0) //can't compute for the last question because there is no end time
        {
            for (int i = 1; i < emotionData.Length; ++i)
            {
                if (emotionData[i].Trim() != "")
                {
                    string[] row = emotionData[i].Split(new char[] { ',' });
                    int time = ConvertTimestampToInt(row[1]);
                    if (time >= start && time <= end && row[2].Trim() != "-") //if timestamp is in the given window, compute the emotion and add it to the total 
                    {
                        double value = .5 + (.5 * double.Parse(row[18])) - (.5 * (double.Parse(row[14]) + double.Parse(row[15]) + double.Parse(row[16]) + double.Parse(row[17]) + double.Parse(row[20]) + double.Parse(row[21])));
                        total += value;
                        ++count;
                    }
                }
            }

            if (count == 0) //no emotion data for this window
            {
                line += ", ----- \n";
            }
            else //divide by the count to get an avg overall emotion for this specific time window 
            {
                double avg = total / count;
                if (avg.ToString().Length > 5) //format the avg to correct num of digits
                {
                    string temp = avg.ToString().Substring(0, 5);
                    avg = double.Parse(temp);
                }
                else if (avg.ToString().Length < 5)
                {
                    int numZeros = 5 - avg.ToString().Length;
                    string tmp = avg.ToString();
                    for (int i = 0; i < numZeros; ++i)
                    {
                        tmp += "0";
                    }
                    avg = double.Parse(tmp);
                }
                line += ", " + avg + "\n";
                totalEmotion += avg; //add to the total for the totals/avgs lines 
                ++numEmotionQuestions;
            }
        }
        else //is the last question, can't compute anything
        {
            line += ", ----- \n";
        }
        return line;
    }

    //creates the csv file to write to
    async private void CreateCSVFile()
    {
        await Task.Run(() =>
        {
            string name = "" + DateTime.Now.ToString();
            name = name.Replace("/", "-");
            name = name.Replace(":", "_");
            currentCSVFile = File.Create("Assets/Data/CombinedData CSVs/" + name + "_AllData.csv");
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(dataHeaders);
            int maxBytes = System.Text.Encoding.UTF8.GetMaxByteCount(dataHeaders.Length);

            // Add header
            currentCSVFile.Write(bytes, 0, bytes.Length);
        });
    }

    private void OnDestroy()
    {
        currentCSVFile.Close();
    }
}

    */
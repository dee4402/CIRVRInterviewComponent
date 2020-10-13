//Authors: Spencer Hunt,
//Purpose: Script to find the number of questions asked in an interview. Then records them in public variable to share between scripts as well
//as keeps track of if the graph should be static or dynamic. Also adds question bars to graph to show when questions were asked.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dashboard_Timekeeping : MonoBehaviour
{

    //Different variables to be used in taking in data
    private int numberevents, graphSize;
    private List<string> events = new List<string>();
    private List<double> eventTimeStamps = new List<double>();
    private string[] fileData;
    private List<RectTransform> dashToEnable = new List<RectTransform>();
    private List<RectTransform> labelToEnable = new List<RectTransform>();

    //Different Rectforms for creating objects
    private RectTransform XaxisBackground;
    private RectTransform questionBar;
    private RectTransform TemplateX;

    void Awake()
    {

        //Reads in data to find # of questions and thier timestamps
        int filelength;
        List<string> filelist = new List<string>();

        List<double> timeBetweenQuest = new List<double>();
        string[] files = System.IO.Directory.GetFiles(System.IO.Path.Combine(Application.streamingAssetsPath, "Data", "EyeTracker files"));

        foreach (var i in files)
        {
            if (i.EndsWith("csv"))
            {
                filelist.Add(i);
            }
        }

        string file = System.IO.File.ReadAllText(filelist[filelist.Count - 1]);
        fileData = file.Split(new char[] { ',', '\n' });
        filelength = fileData.Length;
        //Adds data to corresponding lists, increments by 5 to account for changing rows, each number added to I is the index of a column in the CSV
        for (int i = 5; i < filelength - 1; i += 5)
        {

            if (fileData[i + 4].Contains("Q"))
            {
                events.Add(fileData[i + 4]);
                eventTimeStamps.Add(ConvertTimeStampToDouble(fileData[i]));

            }      
            
        }
        //Sets graph size to the number of seconds the interview took.
        SizeKeeper.graphSize = Convert.ToInt32(ConvertTimeStampToDouble(fileData[filelength - 6]) - ConvertTimeStampToDouble(fileData[5]));
        graphSize = SizeKeeper.graphSize;
        numberevents = events.Count - 1;
        QuestionBarCreator();
        if (SizeKeeper.TypeOfGraph == 1)
            StartCoroutine(Wait());
    }
    //Converts timestamps from data into double for question time keeping//
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

        return 3600 * hour + 60 * minute + second+millisecond;
    }

    //Creates Question bars on graph to show when a question was asked//
    private void QuestionBarCreator()
    {

        XaxisBackground = transform.Find("XAxisBackground").GetComponent<RectTransform>();
        TemplateX = XaxisBackground.Find("TemplateX").GetComponent<RectTransform>();
        questionBar = XaxisBackground.Find("DashY").GetComponent<RectTransform>();
        float maxHeight = XaxisBackground.sizeDelta.y;
        XaxisBackground.sizeDelta = new Vector2((2f * SizeKeeper.graphSize), maxHeight);
        float firstQuestion = (float)eventTimeStamps[0];

        //Adds question lines to graph.
        for(int i = 0; i <numberevents; i++)
        {
            float xPosition = ((float)eventTimeStamps[i] - firstQuestion);
            //questionTimeStamps.Add(xPosition);
            xPosition = xPosition +xPosition;
            RectTransform labelX = Instantiate(TemplateX);
            RectTransform dashY = Instantiate(questionBar);
            labelX.SetParent(XaxisBackground);
            dashY.SetParent(XaxisBackground);
            var colorChanger = dashY.GetComponent<Image>();
            if(i == 7||i == 11||i == 15||i == 23 ||i==16)           
                colorChanger.color = new Color(1, 0, 0, 1);


            if (SizeKeeper.TypeOfGraph == 1)
            {
                labelToEnable.Add(labelX);
                labelX.gameObject.SetActive(false);
                dashToEnable.Add(dashY);
                dashY.gameObject.SetActive(false);
            }
            else
            {
                labelX.gameObject.SetActive(true);
                dashY.gameObject.SetActive(true);
            }
            labelX.anchoredPosition = new Vector2(xPosition, 0f);
            dashY.anchoredPosition = new Vector2(xPosition, 20f);
            labelX.GetComponent<Text>().text = ("Q" + i);
            

        }

    }
    IEnumerator Wait()
    {
        Runlog newRun = new Runlog();
        float lastquestion = (float)eventTimeStamps[0];
        for (int i = 0; i < dashToEnable.Count; i++)
        {
            
            yield return new WaitForSeconds((float)eventTimeStamps[i] - lastquestion);
            lastquestion = (float)eventTimeStamps[i];
            dashToEnable[i].gameObject.SetActive(true);
            labelToEnable[i].gameObject.SetActive(true);
        }
    }
}

//Public Class with variables to share between scripts.//
public static class SizeKeeper
{
    public static int graphSize;
    public static int TypeOfGraph=0; //0 for static, 1 for dynamic

}


//Authors: Spencer Hunt,
//Purpose: Script to create Attentive portion of supergraph on dashboard. Graphs data using time on X-axis and attentive points on the 
//y-axis. Should match Valence and stress graph's X-axis, allowing for all 3 to have matching times for Questions asked.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
public class Dashboard_AttentiveGraph : MonoBehaviour
{

    //Rectform variables to create objects
    private RectTransform AttentiveGraph;
    private RectTransform YaxisBack;
    private RectTransform TemplateY;

    //Used to store data from file
    private string[] fileData;

    //Lists used to store data from csv files and data for calculation
    private List<string> SceneObjects = new List<string>();
    private List<string> QuestionsStamps = new List<string>();
    private List<double> TimeStampsDouble = new List<double>();
    private List<double> valueList = new List<double>();
    private List<GameObject> objectsToEnable = new List<GameObject>();



    private void Start()
    {


        AttentiveGraph = transform.Find("Attentive Graph (Dashboard)").GetComponent<RectTransform>();
        TemplateY = AttentiveGraph.Find("TemplateY").GetComponent<RectTransform>();
        YaxisBack = AttentiveGraph.parent.parent.Find("YAxisBackground Attentive").GetComponent<RectTransform>();
        float maxHeight = AttentiveGraph.sizeDelta.y;

        AttentiveGraph.sizeDelta = new Vector2((2f * SizeKeeper.graphSize), maxHeight);
        ReadCSV();
        DataProcessing();
        ShowGraph();
        if (SizeKeeper.TypeOfGraph == 1)
            StartCoroutine(Wait());
    }

    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(AttentiveGraph, false);
        gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("ColorImages/white");
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        return gameObject;
    }

    //Reads in data from CSV file from the Assets/Data/CSV Files folder. Places data into seperate lists//
    private void ReadCSV()
    {
        int filelength;
        List<string> filelist = new List<string>();

        string[] files = System.IO.Directory.GetFiles(System.IO.Path.Combine(Application.streamingAssetsPath, "Data", "EyeTracker files"));//("Assets/Data/EyeTracker files");

        //Reads in each file from directory
        foreach (var i in files)
        {
            if (i.EndsWith("csv"))
            {
                filelist.Add(i);
            }
        }

        //Reads in text from file at top of folder. Needs to be changed to the index of menu when more data is availible.
        string file = System.IO.File.ReadAllText(filelist[filelist.Count-1]);
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


    //Processing data from csv file.
    private void DataProcessing()
    {
        int incCount;
        double total;

        //Changes object names for easier ID later
        for(int i = 0; i < SceneObjects.Count-1; i ++)
        {
            if (SceneObjects[i] == "CheekL" || SceneObjects[i] == "CheekR" || SceneObjects[i] == "Eyes"|| SceneObjects[i] == "Forehead" || SceneObjects[i] =="Mouth" || SceneObjects[i] == "Nostrils")
                SceneObjects[i] = "face";
        }

        //Counts to keep track of begining of every 5 seconds.
        incCount = 0;
        while(incCount<TimeStampsDouble.Count-1)
        {
            total = 0;
            int start = incCount;

            //Counts differences between data points to find 5 seconds
            while (total < 5)
            {
                if (incCount == 0)
                    total = 0;
                else if(incCount >= TimeStampsDouble.Count)
                {
                    total += 5;

                }
                else 
                {
                    //Used to add points for gaps in data
                    if((TimeStampsDouble[incCount] - TimeStampsDouble[incCount-1]) > 5)
                    {
                        for (int i = 0; i < (int)(TimeStampsDouble[incCount] - TimeStampsDouble[incCount - 1]); i++)
                        {
                            valueList.Add(0);
                        }
                        total += 5;
                    }
                    else
                        total = total + (TimeStampsDouble[incCount] - TimeStampsDouble[incCount - 1]);
                }
                incCount++;
                
            }
            //Calculates attentive point
            CalcAttentive(incCount, start);
            
            
        }
        
    }

    //Calculates attentive value using objects looked and determining whether each is the face or not. Uses 3 data points per question
    private void CalcAttentive(int increment, int start)
    {
        
        double faceCount = 0, nonFaceCount = 0;
        if(increment >= TimeStampsDouble.Count)
        {
            increment = TimeStampsDouble.Count - 1;
        }
        for (int i = start; i < increment; i++)
        {
            if (SceneObjects[i] != "face")
            {
                nonFaceCount++;
            }
            else if(SceneObjects[i] == "face")
            {
                faceCount++;
                
            }
        }
        
        valueList.Add(faceCount / (increment-start));
        
    }

    //Displays graph. Takes in Value list provided by ReadCSV, calls functions to place circles, adds axes.//
    private void ShowGraph()
    {
        float maxHeight = AttentiveGraph.sizeDelta.y;
        float maxY = 1f;
        float xSize = 2f;
        GameObject lastcircle = null;

        for(int i = 0; i < valueList.Count; i++)
        {
            float xPosition = i * xSize;
            float yPosition = (float)(valueList[i] / maxY) * maxHeight;
            GameObject CircleObject = CreateCircle(new Vector2(xPosition, yPosition));

            if( lastcircle != null)
            {
                CreateDotConnection(lastcircle.GetComponent<RectTransform>().anchoredPosition, CircleObject.GetComponent<RectTransform>().anchoredPosition);
            }

            lastcircle = CircleObject;
            Destroy(CircleObject);


        }
        Destroy(lastcircle);


        
        int seperator = 5;
        for(int i = 0; i<seperator; i++)
        {
            RectTransform labelY = Instantiate(TemplateY);
            labelY.SetParent(YaxisBack);
            labelY.gameObject.SetActive(true);
            float normalizedvalue = i * 1f / seperator;
            labelY.anchoredPosition = new Vector2(2f, (normalizedvalue * maxHeight));
            labelY.GetComponent<Text>().text = (normalizedvalue * maxY)*100+"%";

        }
    }

    //Funciton takes in 2 dot positions and creates a line between the 2 using CodeMonkey angle vector function.//
    private void CreateDotConnection(Vector2 positionA, Vector2 positionB)
    {
        GameObject gameobject = new GameObject("dotConnection", typeof(Image));
        gameobject.transform.SetParent(AttentiveGraph, false);
        gameobject.GetComponent<Image>().color = new Color(0, 255, 0, 1f);
        RectTransform recTransform = gameobject.GetComponent<RectTransform>();
        Vector2 dir = (positionB - positionA).normalized;
        float distance = Vector2.Distance(positionA, positionB);
        recTransform.anchorMin = new Vector2(0, 0);
        recTransform.anchorMax = new Vector2(0, 0);
        recTransform.sizeDelta = new Vector2(distance, 3f);
        recTransform.anchoredPosition = positionA + dir * distance * .5f;
        recTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));

        //Used to enable and disable Static or dynamic graphing
        if (SizeKeeper.TypeOfGraph == 1)
        {
            objectsToEnable.Add(gameobject);
            gameobject.SetActive(false);
        }
    }

    //Converts time stamp to double for use in graphing.//
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

    //Used to graph points Dynamically//
    IEnumerator Wait()
    {
        for (int i = 0; i < objectsToEnable.Count - 1; i++)
        {
            yield return new WaitForSeconds(1);
            objectsToEnable[i].SetActive(true);
        }
    }
}

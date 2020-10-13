//Authors: Spencer Hunt,
//Purpose: Script to create Valence portion of supergraph on dashboard. Graphs data using time on X-axis and combination of emotions on the 
//y-axis. Should match Attentive and stress graph's X-axis, allowing for all 3 to have matching times for Questions asked.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
public class Dashboard_ValenceGraph : MonoBehaviour
{
    //Rectform variables to create objects
    private RectTransform ValenceGraph;
    private RectTransform TemplateY;
    private RectTransform YaxisBack;


    //Used to store data from file.
    private string[] fileData;

    //Lists used to store data from CSV file
    private List<double> anger = new List<double>();                 //index14 in CSV
    private List<double> contempt = new List<double>();              //index15
    private List<double> disgust = new List<double>();               //index16
    private List<double> fear = new List<double>();                  //index17
    private List<double> happiness = new List<double>();             //index18
    private List<double> neutral = new List<double>();               //index19
    private List<double> sadness = new List<double>();               //index20
    private List<double> suprise = new List<double>();               //index21
    private List<string> timeStamps = new List<string>();            //index1
    private List<double> valueList = new List<double>();
    private List<GameObject> objectsToEnable = new List<GameObject>();

    //Called upon play, finds all objects, reads in data, creates graph.//
    void Start()
    {
        ValenceGraph = transform.Find("Valence Graph (Dashboard)").GetComponent<RectTransform>();
        TemplateY = ValenceGraph.Find("TemplateY").GetComponent<RectTransform>();
        YaxisBack = ValenceGraph.parent.parent.Find("YAxisBackground Valence").GetComponent<RectTransform>();

        ReadCSV();
        CalculateData();
        float maxHeight = ValenceGraph.sizeDelta.y;
        
        ValenceGraph.sizeDelta = new Vector2((2f * SizeKeeper.graphSize), maxHeight);              //Sets graph size to scale for # of data points
        ShowGraph(valueList);
        if (SizeKeeper.TypeOfGraph == 1)     
            StartCoroutine(Wait());
    }


    //Function that creates circle on point referenced by stress level and time stamp.//
    //Takes in Postion for circle on graph                                            //
    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));                            //Creates Circle gameobject
        gameObject.transform.SetParent(ValenceGraph, false);                                        //Sets circle to child of graph
        gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("ColorImages/white");      //Grabs circle image 
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();                     //gets recttransform for cirlce
        rectTransform.anchoredPosition = anchoredPosition;                                          //Sets anchored position of circle to input value
        rectTransform.sizeDelta = new Vector2(11, 11);                                              //Sets size of circle
        rectTransform.anchorMin = new Vector2(0, 0);                                                //Sets anchorpoints
        rectTransform.anchorMax = new Vector2(0, 0);

        return gameObject;                                                                          //Returns circle object
    }


    //Function used for calculating valence data using 8 points.//
    void CalculateData()
    {
        double value;
        for( int i = 0; i < happiness.Count; i ++)
        {
            value = .5 + (.5 * happiness[i]) - (.5 * (anger[i] + contempt[i] + disgust[i] + fear[i] + sadness[i] + suprise[i]));
            valueList.Add(value);
        }
    }

    //Reads in data from CSV file from the Assets/Data/CSV Files folder. Places data into seperate lists//
    private void ReadCSV()
    {
        int filelength;
        List<string> filelist = new List<string>();

        string[] files = System.IO.Directory.GetFiles(System.IO.Path.Combine(Application.streamingAssetsPath, "Data", "CSV Files"));

        //Reads in each file from directory
        foreach(var i in files)
        {
            if( i.EndsWith("csv"))
            {
                filelist.Add(i);
            }
        }

        //Reads in text from file at top of folder. Needs to be changed to the index of menu when more data is availible.
        string file = System.IO.File.ReadAllText(filelist[filelist.Count-1]);
        fileData = file.Split(new char[] { ',', '\n' });
        filelength = fileData.Length;

        //Adds data to corresponding lists, increments by 22 to account for changing rows, each number added to I is the index of a column in the CSV
        for (int i = 22; i < filelength-1; i += 22)
        {
            timeStamps.Add(fileData[i + 1]);
            if (!fileData[i + 14].Contains("-"))
            {
                anger.Add(Convert.ToDouble(fileData[i + 14]));
                contempt.Add(Convert.ToDouble(fileData[i + 15]));
                disgust.Add(Convert.ToDouble(fileData[i + 16]));
                fear.Add(Convert.ToDouble(fileData[i + 17]));
                happiness.Add(Convert.ToDouble(fileData[i + 18]));
                neutral.Add(Convert.ToDouble(fileData[i + 19]));
                sadness.Add(Convert.ToDouble(fileData[i + 20]));
                suprise.Add(Convert.ToDouble(fileData[i + 21]));
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
    }

    //Displays graph. Takes in Value list provided by ReadCSV, calls functions to place circles, adds axes.
    private void ShowGraph(List<double> valueList)
    {
        float maxHeight = ValenceGraph.sizeDelta.y;

        float maxY = 1f;                                                                            //Sets max Y
        float xSize = 10f;                                                                          //Space between points plotted
        GameObject lastcircle = null;                                                               //Object for storing previous circle for drawing connection

        //Creates circles on graph for each value in list
        for(int i = 0; i <valueList.Count; i++)
        {
            if (valueList[i] > 0)
            {
                float xPosition = i * xSize;
                float yPostion = (float)(valueList[i] / maxY) * maxHeight;                              //Y position of dot using max height and value of data
                GameObject CircleObject = CreateCircle(new Vector2(xPosition, yPostion));

                //Checks if there is a previous cirlce plotted, then creates connection
                if (lastcircle != null)
                {
                    CreateDotConnection(lastcircle.GetComponent<RectTransform>().anchoredPosition, CircleObject.GetComponent<RectTransform>().anchoredPosition);
                }

                lastcircle = CircleObject;                                                              //Sets previous circle for connection

                Destroy(CircleObject);                                                                  //Destroys circle to keep only lines
            }
        }
        Destroy(lastcircle);

        //Creates Y labels
        int seperator = 5;
        for( int i = 0; i < seperator; i++)
        {
            RectTransform labelY = Instantiate(TemplateY);                                          //Creates Y label from template
            labelY.SetParent(YaxisBack);                                                            //Sets parent to Y axis background
            labelY.gameObject.SetActive(true);                                                      //Activates label
            float normalizedvalue = i * 1f / seperator;                                             //Gets normalized value for graph for cleaner look
            labelY.anchoredPosition = new Vector2(2f, (normalizedvalue * maxHeight));               //Sets anchors for label
            labelY.GetComponent<Text>().text =(normalizedvalue * maxY).ToString();                 //Sets text
        }


    }


    //Funciton takes in 2 dot positions and creates a line between the 2 using CodeMonkey angle vector function.//
    private void CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
    {
        GameObject gameobject = new GameObject("dotConnection", typeof(Image));
        gameobject.transform.SetParent(ValenceGraph, false);
        gameobject.GetComponent<Image>().color = new Color(1,(float).92, (float).016, 1);
        RectTransform recTransform = gameobject.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        recTransform.anchorMin = new Vector2(0, 0);
        recTransform.anchorMax = new Vector2(0, 0);
        recTransform.sizeDelta = new Vector2(distance, 3f);
        recTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
        recTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));
        if (SizeKeeper.TypeOfGraph == 1)
        {
            objectsToEnable.Add(gameobject);
            gameobject.SetActive(false);
        }
    }
    //Used to graph points Dynamically//
    IEnumerator Wait()
    {
        for (int i = 0; i < objectsToEnable.Count - 1; i++)
        {
            yield return new WaitForSeconds(5);
            objectsToEnable[i].SetActive(true);
        }
    }
}

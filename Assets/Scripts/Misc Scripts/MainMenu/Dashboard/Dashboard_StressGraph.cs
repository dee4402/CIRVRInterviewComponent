//Authors: Spencer Hunt, 
//Purpose: Script to create Stress portion of supergraph on dashboard. Graphs data using time on X-axis and stress points on the 
//y-axis. Should match Valence and Attentive graph's X-axis, allowing for all 3 to have matching times for Questions asked.


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;




public class Dashboard_StressGraph : MonoBehaviour
{

    //Recttransform Variables to create and change objects. 
    private RectTransform StressGraph;  //Used to place points on graph.
    private RectTransform TemplateX;    //Used to place X axis markers.
    private RectTransform TemplateY;    //Used to place Y axis markers.
    private RectTransform YAxisBack;    //Grey background for Y axis markers.
    private RectTransform questionBar;  //Blackline used to denote question, atteched to X axis markers.

    //Used to read in data from file.
    private string[] fileData;

    //3 lists to be used to read in 3 columns of data in Stress CSV files
    public List<string> timestamps = new List<string>();
    public List<string> data = new List<string>();

    //List used to hold values of stress to be placed on graph.
    private List<double> valueList = new List<double>();

    private List<GameObject> objectsToEnable = new List<GameObject>();


    //Called upon play, finds all objects, reads in data, creates graph.//
    private void Start()
    {
        StressGraph = transform.Find("Stress Graph (Dashboard)").GetComponent<RectTransform>();
        TemplateY = StressGraph.Find("TemplateY").GetComponent<RectTransform>();
        YAxisBack = StressGraph.parent.parent.Find("YAxisBackground Stress").GetComponent<RectTransform>();
        float maxHeight = StressGraph.sizeDelta.y;
        StressGraph.sizeDelta = new Vector2((2f * SizeKeeper.graphSize), maxHeight);               //Sets graph size to scale for # of data points

        ReadCSV();
        ShowGraph(valueList);
        if (SizeKeeper.TypeOfGraph == 1)     
            StartCoroutine(Wait());

    }

    //Function that creates circle on point referenced by stress level and time stamp.//
    //Takes in Postion for circle on graph                                            //
    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));                            //Creates Circle gameobject
        gameObject.transform.SetParent(StressGraph, false);                                         //Sets circle to child of graph
        gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("ColorImages/white");      //Grabs circle image 
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();                     //gets recttransform for cirlce
        rectTransform.anchoredPosition = anchoredPosition;                                          //Sets anchored position of circle to input value
        rectTransform.sizeDelta = new Vector2(11, 11);                                              //Sets size of circle
        rectTransform.anchorMin = new Vector2(0, 0);                                                //Sets anchorpoints
        rectTransform.anchorMax = new Vector2(0, 0);

        return gameObject;                                                                          //Returns circle object
    }

    //Displays graph. Takes in Value list provided by ReadCSV, calls functions to place circles, adds axes.//
    private void ShowGraph(List<double> valueList)
    {

        float maxHeight = StressGraph.sizeDelta.y;
        
        float maxY = 1f;                                                                            //Max y height
        float xSize = 30f;                                                                          //Size of gaps between X
        GameObject lastCircleGameObject = null;                                                     //Game object to hold last cirlce to connect

        //Loop that creates circles using Value list
        for (int i = 0; i < valueList.Count; i++)
        {
            
            //Position Variables for circles
            float xPosition = i * xSize;
            float yPosition = (float)(valueList[i] / maxY) * maxHeight;
            GameObject circleGameObject =  CreateCircle(new Vector2(xPosition, yPosition));         //Creates circle object
            if( lastCircleGameObject != null)
            {
                CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition);

            }
            lastCircleGameObject = circleGameObject;                                                //Stores previous circle to be able t   o create a connection between the 2
            

            
            Destroy(circleGameObject);                                                              //Destroys circle to leave only lines left
        }
        Destroy(lastCircleGameObject);
        
        //Creating Y labels//

        //Used to indicated number items on the y axis
        int seperator = 5;
        for (int i = 0; i < seperator; i++)
        {
            RectTransform labely = Instantiate(TemplateY);                                          //Creates Y labels
            labely.SetParent(YAxisBack);                                                            //Sets parent of Y label to Yaxisback for viewing
            labely.gameObject.SetActive(true);                                                      //Activates Y label
            float normalizedValue = i * 1f / seperator;                                             //Normalizes value for cleaner label
            labely.anchoredPosition = new Vector2(2f, (normalizedValue * maxHeight));
            labely.GetComponent<Text>().text = (normalizedValue * maxY).ToString();

        }

    }

  


    
    //Converts timestamps from data into Int to display on graph.
    private int ConvertTimeStampToInt(string timeStamp)
    {
        string[] splitter = timeStamp.Split(new char[] { ':' });

        int hour = Convert.ToInt32(splitter[0]);

        int minute = 0;
        if (splitter.Length > 1)
            minute = Convert.ToInt32(splitter[1]);

        int second = 0;
        if (splitter.Length > 2)
            second = Convert.ToInt32(splitter[2]);

        return 3600 * hour + 60 * minute + second;
    }
    
 

    //Reads in data from CSV file from the Assets/Data/StressCSVs folder. Places data into seperate lists//
    private void ReadCSV()
    {
        int filelength;
        List<string> filelist = new List<string>();


        string[] files = System.IO.Directory.GetFiles(System.IO.Path.Combine(Application.streamingAssetsPath, "Data", "StressCSVs"));

        //Reads in each file in directory
        foreach (var i in files)
        {
            if (i.EndsWith("csv"))
            {
                    filelist.Add(i);
            }
        }

        //Reads in text from file at top of folder. Needs to be changed to the index of menu when more data is availible
        string file = System.IO.File.ReadAllText(filelist[filelist.Count-1]);
        fileData = file.Split(new char[] { ',', '\n' });
        filelength = fileData.Length;

        //Adds data to corresponding lists, increments by 3 to account for changing rows
        for (int i =2; i < filelength; i+=2)
        {
            
            timestamps.Add(fileData[i]);
            valueList.Add(Convert.ToDouble(fileData[i+1]));
            

        }

        
    }
    //Funciton takes in 2 dot positions and creates a line between the 2 using CodeMonkey angle vector function.//
    private void CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
    {
        GameObject gameobject = new GameObject("dotConnection", typeof (Image));
        gameobject.transform.SetParent(StressGraph, false);
        gameobject.GetComponent<Image>().color = new Color(0, 1, 1, 1);
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
            yield return new WaitForSeconds(15);
            objectsToEnable[i].SetActive(true);
        }
    }
    
}

using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HeadGraphs : MonoBehaviour
{
    //rectTransform
    private RectTransform graphContainer;

    //label templates that are instantiated to make the axis labels
    private RectTransform labelTemplateX, labelTemplateY, labelTemplateXTop;

    //holds the csv face data and csv question/event data
    public string[] dataArray, eventArray;

    //x-axis labels
    private RectTransform labelX, labelXTop;

    //index for each data point
    private const int yawIndex = 10;
    private const int pitchIndex = 8;

    //keep track of how many graph lines drawn, how many graphs drawn, and how many question lines drawn
    private int lineCounter, graphCounter, questionCounter;

    //text component that displays the interview question asked
    public Text textComponent;

    //Two lists to keep track of all circle/line/question line objects created
    private List<GameObject> circles = new List<GameObject>();
    private List<GameObject> lines = new List<GameObject>();
    private List<GameObject> questionLines = new List<GameObject>();

    //holds time stamp of first picture in the face csv file
    private int firstPicTimeStamp;

    //holds the time stamps of the questions and what they actually ask
    private List<int> eventTimeStamps = new List<int>();
    private List<string> eventQuestions = new List<string>();

    private void Awake()
    {
        //find all the necessary stuff
        graphContainer = transform.Find("GraphContainer").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("labelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("labelTemplateY").GetComponent<RectTransform>();
        labelTemplateXTop = graphContainer.Find("labelTemplateXTop").GetComponent<RectTransform>();

        graphCounter = 0;
    }

    //called when a dropdown option is selected
    public void GetData(int index)
    {
        ++graphCounter;
        questionCounter = lineCounter = 0;

        if (graphCounter > 1) //clear old graph
            ClearLastGraph();

        if (dataArray.Length > 1 && dataArray[0].Trim() != "")
        {
            if (index != 0)
            {
            SetUpEventLists(); //read in event data

            ShowGraph(GraphHelper(yawIndex)); //display the graphs
            ShowGraph(GraphHelper(pitchIndex));
            }
        }      
    }

    //reads in the csv and pulls out the desired data
    private List<double> GraphHelper(int index)
    {
        List<double> list = new List<double>();
        double previousValue = 90;
        for (int i = 1; i < dataArray.Length; ++i)
        {
            if (dataArray[i].Trim() != "")
            {
                string[] row = dataArray[i].Split(new char[] { ',' });
                if (row[index].Trim() != "-") //if there is real data or not (didn't recognize a face)
                {
                    previousValue = Convert.ToDouble(row[index]) + 90;
                    list.Add(previousValue);
                }
                else //add a default value of 90 for the empty indexes
                {
                    list.Add(previousValue);
                }
            }
        }
        return list;
    }

    //reads in the event csv and keeps track each question's text and timestamp 
    private void SetUpEventLists()
    {
        for (int i = 1; i < eventArray.Length; ++i)
        {
            if (eventArray[i].Trim() != "")
            {
                string[] eventRow = eventArray[i].Split(new char[] { ',' });
                eventTimeStamps.Add(ConvertTimestampToInt(eventRow[0]));
                eventQuestions.Add(eventRow[2]);
            }
        }

        if (dataArray[1].Trim() != "") //gets the timestamp of the first picture for comparing the questions to 
        {
            string[] picRow = dataArray[1].Split(new char[] { ',' });
            string temp = picRow[1];
            firstPicTimeStamp = ConvertTimestampToInt(temp);
        }
    }

    //returns the timestamp in terms of seconds
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

        return 3600 * hour + 60 * minute + second;
    }

    //clears the previous graph
    private void ClearLastGraph()
    {
        //clears the event data
        if (eventTimeStamps != null)
            eventTimeStamps.Clear();
        if (eventQuestions != null)
            eventQuestions.Clear();

        //clears the x axis labels
        foreach (Transform i in graphContainer.transform)
        {
            if (i.name.Contains("label") && i.name != "labelTemplateX" && i.name != "labelTemplateXTop")
                Destroy(i.gameObject);
        }

        //clears the circles, dot connections, and question lines
        foreach (var i in circles)
            Destroy(i);
        foreach (var j in lines)
            Destroy(j);
        foreach (var k in questionLines)
            Destroy(k);
    }

    //creates each physical circle
    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject1 = new GameObject("circle", typeof(Image));
        gameObject1.transform.SetParent(graphContainer, false);
        RectTransform rectTransform = gameObject1.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(6, 6);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        gameObject1.GetComponent<Image>().sprite = Resources.Load<Sprite>("Color Images/" + lineCounter);

        return gameObject1;
    }

    //creates the question lines
    private GameObject CreateLine(float xPos, float yHeight)
    {
        questionCounter++;
        GameObject gameObject1 = new GameObject("square", typeof(Image));
        gameObject1.name = "" + questionCounter;

        gameObject1.transform.SetParent(graphContainer, false);
        RectTransform rectTransform = gameObject1.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(xPos, yHeight / 2);
        rectTransform.sizeDelta = new Vector2(25, yHeight);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        gameObject1.GetComponent<Image>().sprite = Resources.Load<Sprite>("Color Images/eventMarker");

        EventTrigger test = gameObject1.AddComponent<EventTrigger>(); //sets a trigger on them, shows the question text when clicked on 
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((eventData) => { OnPointerClick(gameObject1); });
        test.triggers.Add(entry);

        return gameObject1;
    }

    //called when a question line is clicked, displays the question text
    public void OnPointerClick(GameObject o)
    {
        RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, 0);
        textComponent.text = "Question " + o.name + ": \"" + eventQuestions[Convert.ToInt32(o.name) - 1] + "\"";
        textComponent.gameObject.SetActive(true);
    }

    //displays a graph
    private void ShowGraph(List<double> valueList)
    {
        ++lineCounter;
     
        float graphHeight = graphContainer.sizeDelta.y;
        float yMaximum = 180f;
        float xSize = 20f;
        float startingXSize = xSize;

        GameObject lastCircleGameObject = null;
        for (int i = 0; i < valueList.Count; ++i) //loops through the list of points
        {
            float xPosition = 50f + i * xSize; //set the position of the current point
            float yPosition = (float)(valueList[i] / yMaximum) * graphHeight;
            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition));

            if (lastCircleGameObject != null) //create the connection between points
                CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition);

            lastCircleGameObject = circleGameObject;

            //Adds circle objects to list
            circles.Add(circleGameObject);

           if (lineCounter == 1) //if its the first line, set up the x axis labels
           {
                labelX = Instantiate(labelTemplateX);
                labelX.SetParent(graphContainer);
                labelX.gameObject.SetActive(true);
                labelX.anchoredPosition = new Vector2(xPosition, -10f);
                labelX.GetComponent<Text>().text = ((i + 1).ToString());
                //Changes color of x axis text to black
                labelX.GetComponent<Graphic>().color = Color.black;

                if (graphCounter != 1)
                    labelX.GetComponent<Text>().fontSize = 16; 
           }
        }

        if (lineCounter == 1) //if its the first graph, set up the x axis labels above the graph (for the question lines)
        {
            for (int i = 0; i < eventTimeStamps.Count; ++i)
            {
                float x = Math.Abs(eventTimeStamps[i] - firstPicTimeStamp) * 4 + startingXSize; //set the position of each question based off comparing timestamps
                GameObject line = CreateLine(x, graphHeight);
                questionLines.Add(line);

                labelXTop = Instantiate(labelTemplateXTop);
                labelXTop.SetParent(graphContainer);
                labelXTop.gameObject.SetActive(true);

                labelXTop.anchoredPosition = new Vector2(x, 17f);
                labelXTop.GetComponent<Text>().text = ("Q" + (i + 1));
                labelXTop.GetComponent<Graphic>().color = Color.black;

                if (graphCounter != 1)
                    labelXTop.GetComponent<Text>().fontSize = 16;
            }
        }
        
        /*
        //y axis labels
        if (lineCounter == 1)
        {
            int separatorCount = 10;
            for (int i = 0; i <= separatorCount; ++i)
            {
                RectTransform labelY = Instantiate(labelTemplateY);
                labelY.SetParent(graphContainer);
                labelY.gameObject.SetActive(true);
                float normalizedValue = i * 1f / separatorCount;
                labelY.anchoredPosition = new Vector2(-10f, normalizedValue * graphHeight);
                labelY.GetComponent<Text>().text = (normalizedValue).ToString();
            }
        }*/
    }

    //create the dot connections between the points of each graph
    private void CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
    {
        GameObject gameObject1 = new GameObject("dotConnection", typeof(Image));
        gameObject1.transform.SetParent(graphContainer, false);
        RectTransform rectTransform = gameObject1.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);

        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 2f);
        rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));

        gameObject1.GetComponent<Image>().sprite = Resources.Load<Sprite>("Color Images/" + lineCounter);
        lines.Add(gameObject1);
    }
}

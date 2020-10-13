using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class WindowGraph : MonoBehaviour
{
    //rectTransform
    private RectTransform graphContainer;

    //label templates that are instantiated to make the axis labels
    private RectTransform labelTemplateX, labelTemplateY, labelTemplateXTop;

    //holds the csv face data and csv question/event data
    public string[] dataArray, eventArray; 

    //used to differentiate between 8 line and 1 line graph
    public bool graph1;

    //x-axis labels
    private RectTransform labelX, labelXTop; 

    //keep track of how many graph lines drawn, how many graphs drawn, and how many question lines drawn
    private int lineCounter, graphCounter, questionCounter;

    //text component that displays the interview question asked and the color Key
    public Text textComponent, keyText;

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

        keyText.gameObject.SetActive(graph1); //display the key if it is the 8 line graph
    }

    //called when a dropdown option is selected    
    public void GetData(int index)
    {
        ++graphCounter;
        lineCounter = questionCounter = 0;

        if (graphCounter > 1) //clear last graph if necessary
            ClearLastGraph();

        if (dataArray.Length > 1 && dataArray[0].Trim() != "")
        {
            if (index != 0)
            {
                SetUpEventLists();

                List<double> angerList = GraphHelper(14); //set up the lists of emotion data 
                List<double> contemptList = GraphHelper(15);
                List<double> disgustList = GraphHelper(16);
                List<double> fearList = GraphHelper(17);
                List<double> happinessList = GraphHelper(18);
                List<double> neutralList = GraphHelper(19);
                List<double> sadnessList = GraphHelper(20);
                List<double> surpriseList = GraphHelper(21);

                if (graph1) //if its the 8 line graph, draw all 8 lines
                {
                    ShowGraph(angerList);
                    ShowGraph(contemptList);
                    ShowGraph(disgustList);
                    ShowGraph(fearList);
                    ShowGraph(happinessList);
                    ShowGraph(neutralList);
                    ShowGraph(sadnessList);
                    ShowGraph(surpriseList);
                }
                else //else, draw the one line 
                {
                    List<double> combinedList = new List<double>();
                    for (int i = 0; i < angerList.Count; ++i)
                    {
                        //compute the single emotion level
                        double value = .5 + (.5 * happinessList[i]) - (.5 * (angerList[i] + contemptList[i] + disgustList[i] + fearList[i] + sadnessList[i] + surpriseList[i]));
                        combinedList.Add(value);
                    }
                    ShowGraph(combinedList);
                }
            }
        }
    }

    //read in the desired data from the csv
    private List<double> GraphHelper(int index)
    {
        List<double> list = new List<double>();
        double previousValue = 0;
        for (int i = 1; i < dataArray.Length; ++i)
        {
            if (dataArray[i].Trim() != "")
            {
                string[] row = dataArray[i].Split(new char[] { ',' });
                if (row[index].Trim() != "-") //checks to see if the current row has real data
                {
                    previousValue = Convert.ToDouble(row[index]);
                    list.Add(previousValue);
                }
                else //else it adds the previous value
                {
                    list.Add(previousValue);
                }
            }
        }
        return list;
    }

    //reads in the event csv data 
    private void SetUpEventLists()
    {
        for (int i = 1; i < eventArray.Length; ++i)
        {
            if (eventArray[i].Trim() != "")
            {
                string[] eventRow = eventArray[i].Split(new char[] { ',' });
                eventTimeStamps.Add(ConvertTimestampToInt(eventRow[0])); //add the question time stamps and text to their respective lists
                eventQuestions.Add(eventRow[2]);
            }
        }

        if (dataArray[1].Trim() != "") //record the timestamp of the first pic 
        {
            string[] picRow = dataArray[1].Split(new char[] { ',' });
            string temp = picRow[1];
            firstPicTimeStamp = ConvertTimestampToInt(temp);
        }
    }

    //for converting timestamps to number of seconds
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
        //clears event data stuff
        if (eventTimeStamps != null)
            eventTimeStamps.Clear();
        if (eventQuestions != null)
            eventQuestions.Clear();
       
        //clears x axis label stuff
        foreach (Transform i in graphContainer.transform)
        {
            if (i.name.Contains("label") && i.name != "labelTemplateX" && i.name != "labelTemplateXTop")
                Destroy(i.gameObject);
        }

        //clears all the graph stuff
        foreach (var i in circles)
            Destroy(i);
        foreach (var j in lines)
            Destroy(j);
        foreach (var k in questionLines)
            Destroy(k);
    }

    //creates the actual point
    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject1 = new GameObject("circle", typeof(Image));
        gameObject1.transform.SetParent(graphContainer, false);
        RectTransform rectTransform = gameObject1.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(6, 6);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        if (graph1) //correctly colors each line 
            gameObject1.GetComponent<Image>().sprite = Resources.Load<Sprite>("Color Images/" + lineCounter);
        else
            gameObject1.GetComponent<Image>().sprite = Resources.Load<Sprite>("Color Images/1");

        return gameObject1;
    }

    //creates each question line
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

        EventTrigger test = gameObject1.AddComponent<EventTrigger>(); //add the event trigger for clicking on a question line
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((eventData) => { OnPointerClick(gameObject1); });
        test.triggers.Add(entry);

        return gameObject1;
    }

    //called when a question line is clicked on, displays the questions text
    public void OnPointerClick(GameObject o)
    {
        RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, 0);
        textComponent.text = "Question " + o.name + ": \"" + eventQuestions[Convert.ToInt32(o.name) - 1] + "\"";
        textComponent.gameObject.SetActive(true);
    }
    
    //draws the actual graph
    private void ShowGraph(List<double> valueList)
    {
       
        if (graph1) //if its the 8 line graph, keep track of what line is being plotted
            ++lineCounter;

        float graphHeight = graphContainer.sizeDelta.y;
        float yMaximum = 1f;
        float xSize = 20f;
        float startingXSize = xSize;

        GameObject lastCircleGameObject = null;
        for (int i = 0; i < valueList.Count; ++i) //loop through values and add points
        {
            float xPosition = 50f + i * xSize;            
            float yPosition = (float)(valueList[i] / yMaximum) * graphHeight;
            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition));

            if (lastCircleGameObject != null)
                CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition);

            lastCircleGameObject = circleGameObject;

            //Adds circle objects to list
            circles.Add(circleGameObject);

            if (!graph1 || lineCounter == 1) //draw the x axis labels 
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

        if (!graph1 || lineCounter == 1) //creates each question line as well as the x axis labels above them 
        {
            for (int i = 0; i < eventTimeStamps.Count; ++i)
            {
                float x = Math.Abs(eventTimeStamps[i] - firstPicTimeStamp) * 4 + startingXSize;
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
        if (lineCounter == 1 || !graph1)
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

    //create the dot connections connecting the points
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

        if (graph1)
            gameObject1.GetComponent<Image>().sprite = Resources.Load<Sprite>("Color Images/" + lineCounter);
        else
            gameObject1.GetComponent<Image>().sprite = Resources.Load<Sprite>("Color Images/1");

        lines.Add(gameObject1);
    }
}

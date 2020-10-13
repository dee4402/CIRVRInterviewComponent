using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public class StressGraph : MonoBehaviour
{
    //rectTransform
    private RectTransform graphContainer;
    //label templates that are instantiated to make the axis labels
    private RectTransform labelTemplateX, labelTemplateY, labelTemplateXTop, containerBackground;
    //holds the csv face data and csv question/event data
    public string[] dataArray, eventArray;
    //x-axis labels
    private RectTransform labelX, labelXTop;
    //keep track of how many graph lines drawn, how many graphs drawn, and how many question lines drawn
    private int graphCounter, questionCounter, graphSize;
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
    public List<float> debug = new List<float>();
    private void Awake()
    {
        //find all the necessary stuff
        graphContainer = transform.Find("GraphContainer").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("labelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("labelTemplateY").GetComponent<RectTransform>();
        labelTemplateXTop = graphContainer.Find("labelTemplateXTop").GetComponent<RectTransform>();
        containerBackground = graphContainer.Find("Container background").GetComponent<RectTransform>();
        graphCounter = 0;
    }
    //called when a dropdown option is selected
    public void GetData(int index)
    {
        questionCounter = 0;
        if (++graphCounter > 1) //clear last graph if needed
            ClearLastGraph();
        if (dataArray.Length > 1 && dataArray[0].Trim() != "")
        {
            if (index != 0)
            {
                SetUpEventLists();
                graphSize = eventTimeStamps[eventTimeStamps.Count - 1] - eventTimeStamps[0];
                if (graphSize >= containerBackground.sizeDelta.x)
                {
                    float maxHeight = containerBackground.sizeDelta.y;
                    containerBackground.sizeDelta = new Vector2((1f * graphSize), maxHeight);
                    maxHeight = graphContainer.sizeDelta.y;
                    graphContainer.sizeDelta = new Vector2((1f * graphSize), maxHeight);
                }
                ShowGraph(GraphHelper(2));
            }
        }
    }
    //read in the data desired from the stress csv
    private List<double> GraphHelper(int index)
    {
        List<double> list = new List<double>();
        for (int i = 1; i < dataArray.Length; ++i)
        {
            if (dataArray[i].Trim() != "")
            {
                string[] row = dataArray[i].Split(new char[] { ',' });
                double val = Convert.ToDouble(row[index]); //add each stress val to a list
                list.Add(val);
            }
        }
        return list;
    }
    //reads in the event csv
    private void SetUpEventLists()
    {
        for (int i = 1; i < eventArray.Length; ++i)
        {
            if (eventArray[i].Trim() != "")
            {
                string[] eventRow = eventArray[i].Split(new char[] { ',' });
                eventTimeStamps.Add(ConvertTimestampToInt(eventRow[0])); //adds each question timestamp and text to respective lists
                eventQuestions.Add(eventRow[2]);
            }
        }
        if (dataArray[1].Trim() != "") //gets timestamp of first stress value
        {
            string[] picRow = dataArray[1].Split(new char[] { ',' });
            firstPicTimeStamp = ConvertTimestampToInt(picRow[0]);
        }
    }
    //converts timestamp to number of seconds
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
    //clear out previous graph
    private void ClearLastGraph()
    {
        //clear question stuff
        if (eventTimeStamps != null)
            eventTimeStamps.Clear();
        if (eventQuestions != null)
            eventQuestions.Clear();
        //clear x axis label stuff
        foreach (Transform i in graphContainer.transform)
        {
            if (i.name.Contains("label") && i.name != "labelTemplateX" && i.name != "labelTemplateXTop")
                Destroy(i.gameObject);
        }
        //clear graph stuff
        foreach (var i in circles)
            Destroy(i);
        foreach (var j in lines)
            Destroy(j);
        foreach (var k in questionLines)
            Destroy(k);
    }
    //create the actual point
    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject1 = new GameObject("circle", typeof(Image));
        gameObject1.transform.SetParent(graphContainer, false);
        RectTransform rectTransform = gameObject1.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(6, 6);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        if (anchoredPosition.y > 130)
            gameObject1.GetComponent<Image>().sprite = Resources.Load<Sprite>("Color Images/8"); //red
        else
            gameObject1.GetComponent<Image>().sprite = Resources.Load<Sprite>("Color Images/5"); //green
        return gameObject1;
    }
    //create a question line
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
        EventTrigger test = gameObject1.AddComponent<EventTrigger>(); //add a trigger to display question text when clicked on
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((eventData) => { OnPointerClick(gameObject1); });
        test.triggers.Add(entry);
        return gameObject1;
    }
    //called when a question line is clicked on
    public void OnPointerClick(GameObject o)
    {
        RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, 0);
        textComponent.text = "Question " + o.name + ": \"" + eventQuestions[Convert.ToInt32(o.name) - 1] + "\"";
        textComponent.gameObject.SetActive(true);
    }
    //displays the graph
    private void ShowGraph(List<double> valueList)
    {
        float graphHeight = graphContainer.sizeDelta.y;
        float yMaximum = 1f;
        float xSize = 30f;
        float startingXSize = xSize;
        GameObject lastCircleGameObject = null;
        for (int i = 0; i < valueList.Count; ++i)
        {
            float xPosition = i * xSize; //compute the x and y positions and then create a circle
            float yPosition = (float)(valueList[i] / yMaximum) * graphHeight;
            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition));
            if (lastCircleGameObject != null) //create a dot connection
                CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition);
            lastCircleGameObject = circleGameObject;
            //Adds circle objects to list
            circles.Add(circleGameObject);
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
        for (int i = 0; i < eventTimeStamps.Count; ++i) //plot each question line where it belongs
        {
            float x = Math.Abs(eventTimeStamps[i] - firstPicTimeStamp);
            debug.Add(x);
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
        /*
        //y axis labels
        int separatorCount = 10;
        for (int i = 0; i <= separatorCount; ++i)
        {
            RectTransform labelY = Instantiate(labelTemplateY);
            labelY.SetParent(graphContainer);
            labelY.gameObject.SetActive(true);
            float normalizedValue = i * 1f / separatorCount;
            labelY.anchoredPosition = new Vector2(-10f, normalizedValue * graphHeight);
            labelY.GetComponent<Text>().text = (normalizedValue).ToString();
            RectTransform dashY = Instantiate(dashTemplateY);
            dashY.SetParent(graphContainer, false);
            dashY.gameObject.SetActive(true);
            dashY.anchoredPosition = new Vector2(-4f, normalizedValue * graphHeight);
        }
        */
    }
    //create the connections between plotted points
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
        if (dotPositionA.y > 130 && dotPositionB.y > 130)
        {
            gameObject1.GetComponent<Image>().sprite = Resources.Load<Sprite>("Color Images/8");
        }
        else if (dotPositionA.y <= 130 && dotPositionB.y <= 130)
        {
            gameObject1.GetComponent<Image>().sprite = Resources.Load<Sprite>("Color Images/5");
        }
        else if (dotPositionA.y <= 130)
        {
            gameObject1.GetComponent<Image>().sprite = Resources.Load<Sprite>("Color Images/green-red");
        } else
        {
            gameObject1.GetComponent<Image>().sprite = Resources.Load<Sprite>("Color Images/red-green");
        }
        lines.Add(gameObject1);
    }
}
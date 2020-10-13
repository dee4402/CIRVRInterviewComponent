using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

//for real time eye gaze tracking
public class DashboardEyeTrack : MonoBehaviour
{
    private RectTransform graphContainer;
    private List<GameObject> circles = new List<GameObject>();
    float graphHeight, graphWidth;
    public Vector2 newPtPos;
    private Vector2 oldPt;
    private Sprite red, orange, green;
    List<float> xPosList = new List<float>();
    List<float> yPosList = new List<float>();
    private SpriteRenderer testing;
    private string currentObject;
    private string path;
    private string[] fileData;
    private List<double> TimeStampsDouble = new List<double>();
    private int i = 0;


    private void Start()
    {
        testing = gameObject.GetComponent<SpriteRenderer>();
        ClearLastGraph();
        oldPt = newPtPos = new Vector2(0, 0);
        red = Resources.Load<Sprite>("Color Images/8");
        ReadCSV();
    }
    private void ReadCSV()
    {
        int filelength;
        List<string> filelist = new List<string>();

        string[] files = System.IO.Directory.GetFiles(System.IO.Path.Combine(Application.streamingAssetsPath, "Data", "EyeTracker files"));

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
            xPosList.Add(float.Parse(fileData[i + 2], System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            yPosList.Add(float.Parse(fileData[i + 1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
        }
        StartCoroutine(Wait());
    }

    //clear out the last graph and any points in it 
    public void ClearLastGraph()
    {
        foreach (var i in circles)
            Destroy(i);
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

    //draw a point in the graph
    public void PlotPoint(Vector2 pt)
    {
        graphContainer = GetComponent<RectTransform>();
        graphHeight = graphContainer.rect.height;
        graphWidth = graphContainer.rect.width;

        float xPos = pt.x * 260f;
        float yPos = pt.y * 130f;

        if(xPos > 0 && yPos > 0)
            {
                circles.Add(CreateCircle(new Vector2(xPos, yPos)));
            }
        if (circles.Count >= 251) //max number of points plotted, starts deleting the first ones once full, like a queue
        {
            Destroy(circles[0]);
            circles.RemoveAt(0);
        }
    }

    //create the actual point that is plotted
    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject1 = new GameObject("circle", typeof(Image));
        gameObject1.transform.SetParent(graphContainer, false);
        RectTransform rectTransform = gameObject1.GetComponent<RectTransform>();
        gameObject1.GetComponent<Image>().sprite = red;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(2, 2);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        
        return gameObject1;
        

    }
    IEnumerator Wait(){
        if(i >= xPosList.Count)
        {
            i = 0;
        }
        yield return new WaitForSeconds(0.02f);
            newPtPos.x = xPosList[i];
            newPtPos.y = yPosList[i];
            PlotPoint(newPtPos);
            oldPt = newPtPos;
            i++;

            StartCoroutine(Wait());
    }
}
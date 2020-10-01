using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

//for the main menu eye tracker graphs
public class EyeTracker : MonoBehaviour
{
    public Text textComponent;

    private RectTransform graphContainer;
    private List<GameObject> circles = new List<GameObject>();

    float graphHeight, graphWidth;
    int face, Leye, Reye; //how many points in each area

    public void Start()
    {
        face = Leye = Reye = 0;
    }

    public void ValueChanged(int index) //when a dropdown option is selected
    {
        ClearLastGraph(); //clear previous graph
        if (index != 0)
        {
            List<string> fileNames = new List<string>();
            string[] filesArray = System.IO.Directory.GetFiles("Assets/Data/EyeTracker files");
            foreach (var i in filesArray)
            {
                if (i.EndsWith("csv"))
                    fileNames.Add(i);
            }

            string file = System.IO.File.ReadAllText(fileNames[index - 1]); //read in the correct csv
            string[] data = file.Split(new char[] { '\n' });

            int num = 1;
            if (data.Length > 500) //make sure it doesn't plot too many points 
                num = data.Length / 500;

            List<double> xVals = new List<double>();
            List<double> yVals = new List<double>();
  
            for (int i = 1; i < data.Length; ++i) //loop through eye csv file and add each point to the x and y lists
            {
                if (data[i].Trim() != "")
                {
                    string[] row = data[i].Split(new char[] { ',' });

                    if (i % num == 0)
                    {
                        if (Convert.ToDouble(row[1]) < 1)
                            xVals.Add(Convert.ToDouble(row[1]));
                        if (Convert.ToDouble(row[2]) < 1)
                            yVals.Add(Convert.ToDouble(row[2]));
                    }
                }
            }
            ShowGraph(xVals, yVals); //plot all the points
            UpdateTextBox();
        }
    }

    private void UpdateTextBox()
    {
        textComponent.text = "\tArea\t\t\tPoints\n\n\tFace\t\t\t" + face + "\n\tLEye\t\t\t" + Leye + "\n\tREye\t\t\t" + Reye;
    }

    public void ClearLastGraph()
    {
        textComponent.text = "";

        foreach (var i in circles) //destroy all the old points
            Destroy(i);

        face = Leye = Reye = 0;
    }

    //draws the graph
    public void ShowGraph(List<double> x, List<double> y)
    {
        graphContainer = GameObject.Find("GraphBackground").GetComponent<RectTransform>();
        graphHeight = graphContainer.rect.height;
        graphWidth = graphContainer.rect.width;

        if (x.Count != y.Count)
            return;

        for (int i = 0; i < x.Count; ++i) //loop through the lists of points
        {
            float xPos = (float)(x[i] * graphWidth); //convert the percent of screen width to percent of graph width
            float yPos = (float)(y[i] * graphHeight);

            if (xPos < 0) //set any points outside to (0,0)
                xPos = 0;
            if (yPos < 0)
                yPos = 0;

            //see if the points fall in the areas
            CheckCircle("Face", xPos, yPos);
            CheckCircle("L eye", xPos, yPos);
            CheckCircle("R eye", xPos, yPos);

            GameObject circle = CreateCircle(new Vector2(xPos, yPos)); //actually create the point
            circles.Add(circle);
        }
        //Debug.Log("There are " + face + " points in the face area");
        //Debug.Log("There are " + Leye + " points in the left eye area");
        //Debug.Log("There are " + Reye + " points in the right eye area");
    }

    //creates each physical point
    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject1 = new GameObject("circle", typeof(Image));
        gameObject1.transform.SetParent(graphContainer, false);
        RectTransform rectTransform = gameObject1.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(3, 3);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        gameObject1.GetComponent<Image>().sprite = Resources.Load<Sprite>("Color Images/8");
        
        return gameObject1;
    }

    //checks to see if a point falls in the specified area
    public void CheckCircle(string areaName, float x, float y) 
    {
        RectTransform q = GameObject.Find(areaName).GetComponent<RectTransform>();
        float height = q.rect.height; 
        float width = q.rect.width;
        float r = width / 2; //radius of circle

        Vector2 temp = q.anchoredPosition;
        Vector2 areaPos = new Vector2(temp.x + graphWidth/2, temp.y + graphHeight/2); //center of area that will be checked for points

        if (Math.Abs(x - areaPos.x) <= (width/2) && Math.Abs(y - areaPos.y) <= (height/2)) //if its in the square of height and width = r*2
        {
            if (InCircle(areaPos, r, x, y)) //sees if its inside the circle, not just the rectangle
            {
                if (areaName == "Face")
                    ++face;
                else if (areaName == "L eye")
                    ++Leye;
                else
                    ++Reye;
            }
        }
    }

    //checks to see if a point falls in the circle
    private bool InCircle(Vector2 center, float radius, float xPos, float yPos)
    {
        float dist = (float)Math.Sqrt(Math.Pow((xPos - center.x), 2) + Math.Pow((yPos - center.y), 2));
        return dist <= radius;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

//for real time eye gaze tracking
public class RealTimeEyeTrackerGraph : MonoBehaviour
{
    private RectTransform graphContainer;
    private List<GameObject> circles = new List<GameObject>();
    float graphHeight, graphWidth;
    public Vector2 newPtPos;
    private Vector2 oldPt;
    private Sprite red, orange, green;
    private SpriteRenderer testing;
    private string currentObject;

    private void Start()
    {
        testing = gameObject.GetComponent<SpriteRenderer>();
        ClearLastGraph();
        oldPt = newPtPos = new Vector2(0, 0);
        red = Resources.Load<Sprite>("Color Images/8");
        green = Resources.Load<Sprite>("Color Images/5");
        orange = Resources.Load<Sprite>("Color Images/6");
    }

    private void Update()
    {
        if (oldPt != newPtPos || newPtPos.x < .7)
        //if (oldPt != newPtPos && newPtPos.x < 1) //if a new point is received and it is located on the left part of the screen (interview part), plot it
        {
            PlotPoint(newPtPos);
            oldPt = newPtPos;
        }
    }

    //clear out the last graph and any points in it 
    public void ClearLastGraph()
    {
        foreach (var i in circles)
            Destroy(i);
    }

    //draw a point in the graph
    public void PlotPoint(Vector2 pt)
    {
        graphContainer = GetComponent<RectTransform>();
        graphHeight = graphContainer.rect.height;
        graphWidth = graphContainer.rect.width;

        //do some shifting here in order to correctly plot the points (since only 2/3 of the screen is the actual interview, other third is the data stuff)
        float xPos = (pt.x + .15f) * graphWidth;
        float yPos = pt.y * graphHeight;
        //Debug.Log($"pt.y {pt.y} : pt.x {pt.x} : xpos {xPos} : ypos {yPos}");

        if (xPos < 0)
            xPos = 0;
        if (yPos < 0)
            yPos = 0;

        circles.Add(CreateCircle(new Vector2(xPos, yPos)));
        if (circles.Count == 251) //max number of points plotted, starts deleting the first ones once full, like a queue
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
        //Debug.Log($"local transform {gameObject1.transform.localPosition}");
        RectTransform rectTransform = gameObject1.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(2, 2);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        Image test = gameObject1.GetComponent<Image>();
        Color tempColor = test.color;
        tempColor.b = 0f;
        tempColor.g = 0f;
        tempColor.a = .3f;
        gameObject1.GetComponent<Image>().color = tempColor;


        //SpriteRenderer spRend = gameObject1.AddComponent<SpriteRenderer>();
        //GetViewedObject();
        //if(this.currentObject == "face" || this.currentObject == "head")
        //{
        //    spRend.sprite = green;
        //}
        //else
        //{
        //    spRend.sprite = red;
        //}

        // Debug.Log($"currentObject {currentObject} : this.Current {this.currentObject}");

        //gameObject1.AddComponent<SpriteRenderer>();

        //gameObject1.GetComponent<SpriteRenderer>().sprite = gameObject1.GetComponent<Image>().sprite;
        ////Color col = gameObject1.GetComponent<Image>().color;
        //SpriteRenderer spRend = gameObject1.GetComponent<SpriteRenderer>();
        //spRend.color = new Color(1f, 1f, 1f, .2f);

        //SpriteRenderer test = gameObject1.GetComponent<SpriteRenderer>();

        //test.sprite = gameObject1.GetComponent<Image>().sprite;
        //Color col = test.color;
        //Debug.Log($"col {col}");
        //col.a = 0f;
        //Debug.Log($"a {col}");

        //test.color = col;
        return gameObject1;
    }

    public void SetViewedObject(string curObject)
    {
        if (curObject == "Eyes" || curObject == "Mouth")
        {
            curObject = "face";
        }
        this.currentObject = curObject;
    }
    private string GetViewedObject()
    {
        return this.currentObject;
    }
}
//Author(s): Spencer Hunt, 
//Purpose: Contains a set text function that will set the text and text color for a text object when called.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextlogItem : MonoBehaviour
{
    public void SetText(string myText, Color myColor)
    {
        GetComponent<Text>().text = myText;
        GetComponent<Text>().color = myColor;
    }
}

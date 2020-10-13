//Author(s):Spencer Hunt
//Purpose: Creates a text item and sets it active, as well as contains a list to store text items if needed in the future.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextLogControl : MonoBehaviour
{

    //Text gameobject for creating new text objects.
    [SerializeField]
    private GameObject textTemplate;

    //List to hold all text items.
    private List<GameObject> textItems;

    //Creates new text object, sets it active. Uses set text from TextLogItem.cs
    public void logText(string newTextString, Color newColor)
    {
        GameObject newText = Instantiate(textTemplate) as GameObject;
        newText.SetActive(true);

        //Gets text component and uses SetText to change its color and text.
        newText.GetComponent<TextlogItem>().SetText(newTextString, newColor);

        //Sets text objects parent to the canvas.
        newText.transform.SetParent(textTemplate.transform.parent, false);
    }
}

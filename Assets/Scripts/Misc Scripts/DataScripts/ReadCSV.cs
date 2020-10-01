using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using UnityStandardAssets.Utility.Events;
using Cirvr.ConversationManager;
using SimpleJSON;

//for reading in the emotion csv and sending the data to the graphing scripts. also used to display the emotion raw data from main menu
public class ReadCSV : MonoBehaviour
{
    List<CSVInfo> dataList = new List<CSVInfo>();
    public Text textComponent;
    private int fileLength;
    private string[] data;

    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownForInterviewSelection : MonoBehaviour
{
    public Dropdown dropdown;
    public Canvas startingCanvas;
    public Canvas nextCanvas;   

    public void PopulateList()
    {
        dropdown.ClearOptions(); //make sure you start fresh every time 
        List<string> files = new List<string>();
        files.Add("Please select one...");

        string[] filesArray = System.IO.Directory.GetFiles("Assets/Data/CSV files");

        foreach (var i in filesArray) //read in the actual csv files, discard the meta files and other stuff
        {
            if (i.Trim() != "" && i.EndsWith("csv") && !files.Contains(i))
                files.Add(i);
        }

        for (int i = 1; i < files.Count; ++i) //cut off the unnecessary info on their name 
        {
            files[i] = files[i].Substring(22);
            int index = files[i].IndexOf('.');
            files[i] = files[i].Remove(index);
        }
        dropdown.AddOptions(files); //add the options to the dropdown
    }

    public void IndexChanged(int index) //called when an option is selected from the dropdown, most of the actual data stuff is called from other scripts
    {
        if (index != 0)
        {
            startingCanvas.gameObject.SetActive(false);
            nextCanvas.gameObject.SetActive(true);
        }
    }
}

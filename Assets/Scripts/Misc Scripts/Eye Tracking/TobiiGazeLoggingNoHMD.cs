using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Gaming; /*GetGazePoint*/
using System.IO; /*StreamWriter, File*/
using System; /*DateTime*/
using UnityStandardAssets.Utility.Events;
using Cirvr.ConversationManager;

public class TobiiGazeLoggingNoHMD : MonoBehaviour
{
    private int onlyOnceStart;
    private bool interviewStart = false;
    //private bool responding;
    private string path, date;
    GazePoint gazePoint = new GazePoint();
    public GameObject hitObject;
    public static string currentlyViewedObject = "";
    public bool isPaused;
    private static ConversationContext Context;
    private List<float> pointsX = new List<float>();
    private List<float> pointsY = new List<float>();
    private List<string> objects = new List<string>();



    //Listening for the beginning and endings of questions
    private void RegisterListeners()
    {
        EventSystem.current.RegisterListener<PlayerBeginInterview>(BeginInterview);
    }

    void Start ()
    {
        Context = ConversationContext.Instance;
        onlyOnceStart = 0;
        RegisterListeners();

        isPaused = false;
        //Path for csv file
        date = DateTime.Now.ToString().Replace("/", "-");
        date = date.Replace(":", "_");
        path = Path.Combine(Application.streamingAssetsPath, "Data", "EyeTracker files", $"Tobii_{date}_GazeData.csv");
        //realTimeEyeTrackerGraph = GameObject.Find("Canvas").GetComponent<RealTimeEyeTrackerGraph>();
    }

    void Update()
    {
        //Get current gaze point every frame
        gazePoint = TobiiAPI.GetGazePoint();
        //Calling LogGaze function
        if(pointsX.Count == 100 || pointsY.Count == 100 || objects.Count == 100) {
            Epochs(pointsX, pointsY, objects);
            pointsX.Clear();
            pointsY.Clear();
            objects.Clear();
        }

        if (interviewStart && !isPaused)
        {
            LogGaze();
        }
    }
    
    //Logs gaze data to csv file named after Date and time.
    void LogGaze()
    {
        string X, Y;
        if (gazePoint.IsValid)
        {

            //Format x and y pos to only go to three decimal places
            X = (gazePoint.Screen.x / Screen.width).ToString();//String.Format("{0:F3}", (gazePoint.Screen.x / Screen.width));
            Y = (gazePoint.Screen.y / Screen.height).ToString();//String.Format("{0:F3}", (gazePoint.Screen.y / Screen.height));
            Debug.Log($"x in tobii script {X} | y in tobii script {Y}");

            pointsX.Add(float.Parse(X));
            pointsY.Add(float.Parse(Y));

            hitObject = TobiiAPI.GetFocusedObject();

            

            //If an object is being hit
            if (hitObject != null)
            {
                currentlyViewedObject = hitObject.name;
                //Names objects that are more specific to more general
                if (currentlyViewedObject == "LipCornerL" || currentlyViewedObject.Contains("jaw"))
                {
                    currentlyViewedObject = "Mouth";
                }
                else if (currentlyViewedObject.Contains("eye"))
                {
                    currentlyViewedObject = "Eyes";
                }
                //Create file if not already there
                if (!File.Exists(path))
                {
                    //Writing using streamwriter
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine("ts,X,Y,viewedObjects");
                        writeFunc(sw, X, Y, currentlyViewedObject);
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        writeFunc(sw, X, Y, currentlyViewedObject);
                    }
                }
                objects.Add(currentlyViewedObject);
            }
            else {
                    if (!File.Exists(path))
                    {
                        //Writing using streamwriter
                        using (StreamWriter sw = File.CreateText(path))
                        {
                            sw.WriteLine("ts,X,Y,viewedObjects,Question");
                            writeFunc(sw, X, Y);
                        }
                    }
                    else
                    {
                        using (StreamWriter sw = File.AppendText(path))
                        {
                            writeFunc(sw, X, Y);
                        }
                    }
                    objects.Add("No Object Tracked");
            }
        }
    }
    
    private string CurrentTime()
    {
        return DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK");
    }
    
    //Function for writing the file contents aside from header
     private void writeFunc(StreamWriter sw, string X, string Y, string objectViewed = "No Object Tracked") {
             sw.WriteLine(
                 CurrentTime() + ","
                           + X + ","
                           + Y + ","
                           + objectViewed);
        sw.Flush();
    }

    private void Epochs(List<float> X, List<float> Y, List<string> allObjects) {
        float xMin, xMax, yMin, yMax;
        List<string> objects = new List<string>();

        X.Sort();
        Y.Sort();

        xMin = X[0];
        xMax = X[0];
        yMin = Y[0];
        yMax = Y[0];

        foreach(var item in allObjects) {
            if(!objects.Contains(item)) {
                objects.Add(item);
            }
        }   

        string objectsAsString = "";

        foreach(var item in objects) {
            objectsAsString += " | " + item;
        }

        string line = $"{xMin}, {xMax}, {yMin}, {yMax}, {objectsAsString}";


    }

    // private void StreamStart(string columnHeader, string line) {
    //     using ()
    // }

    private void newWriteFunc(StreamWriter sw, string line) {
        sw.WriteLine(
            line
        );
    }
     
    //Waits until start of interview to start logging, possibly want one for end too
    public void BeginInterview(PlayerBeginInterview o)
    {
        interviewStart = true;
    }
}
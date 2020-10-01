using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility.Events;
using Tobii.G2OM;
using System.IO; /*StreamWriter, File*/
using System; /*DateTime*/

/* 
    This script is attached to all objects that we want to keep track of when the user looks
    at them. 
 */


namespace Tobii.XR
{
//MonoBehaviour which implements the "IGazeFocusable" interface, meaning it will be called on when the object receives focus
    public class TobiiGazeLoggingVive : MonoBehaviour, IGazeFocusable
    {
        /* Vars */
        private string objectName;
        private string path, date;
        private bool interviewStarted;
        private TobiiXR_EyeTrackingData gazeData;
        
        private void Start()
        {
            objectName = gameObject.name;
            date = DateTime.Now.ToString().Replace("/", "-");
            date = date.Replace(":", "_");
            path = Path.Combine(Application.streamingAssetsPath, "Data", "EyeTracker files", $"Vive_{date}_GazeData.csv");
            RegisterListeners();
        }

        private void Update() {
            //If the vive is in use then keep track of gaze data, otherwise Stop();
            if(SettingsInfo.VR == "vive")
            {
                gazeData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);
                Debug.Log($"Vive rays {gazeData.GazeRay.Origin} | direction {gazeData.GazeRay.Direction}");
                //Debug.Log($"Vive Eye Gaze Data {gazeData.GazeRay.IsValid} | {gazeData.Timestamp} | {CurrentTime()} | {gazeData.GazeRay.Direction.x} | {gazeData.GazeRay.Direction.y}");
            }
            else {
                TobiiXR.Stop();
            }
        }

        private void RegisterListeners()
        {
            EventSystem.current.RegisterListener<PlayerBeginInterview>(BeginInterview);
        }

        //The method of the "IGazeFocusable" interface, which will be called when this object receives or loses focus
        public void GazeFocusChanged(bool hasFocus)
        {
            //If this object received focus, then record that
            if (hasFocus && interviewStarted)
            {
                //Maybe use timestamp for data?
                RecordEyeGaze();
            }

        }

        //Starts keep track once the interview has started
        private void BeginInterview(PlayerBeginInterview o) {
            interviewStarted = true;
        }

        private void RecordEyeGaze() {
                if (!File.Exists(path)) {
                    //Writing using streamwriter
                    using (StreamWriter sw = File.CreateText(path)) { 
                            sw.WriteLine("ts,viewedObjects");
                            sw.WriteLine(
                            CurrentTime() + ","
                                    // + X + ","
                                    // + Y + ","
                                    + objectName);
                            sw.Flush();
                    }
                }
                else {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine(
                        CurrentTime() + ","
                                // + X + ","
                                // + Y + ","
                                + objectName );
                        sw.Flush();
                    }
                }
        }

        private string CurrentTime()
        {
            return DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK");
        }
    }
}


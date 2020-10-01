using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using UnityStandardAssets.Utility.Events;
using System;
using System.IO;
using System.Threading.Tasks;

public class E4ClientManager : MonoBehaviour
{
    public Slider m_Slider;
    public E4SensorClient client;
    [HideInInspector] public string recvData;
    bool connected;
    public bool isPaused;
    private string path;
    private double test;

    // Start is called before the first frame update
    void Start()
    {
        string date = DateTime.Now.ToString().Replace("/", "-");
        date = date.Replace(":", "_");
        path = Path.Combine(Application.streamingAssetsPath, "Data", "StressCSVs", $"Stress_{date}.csv");
        recvData = "";
        connected = isPaused = false;
    }

    void Update()
    {
        //User must press P to connect the Unity system to the server that sends out stress values
        if (Input.GetKeyDown(KeyCode.P))
        {
            //129.59.79.247 Michael: Not sure what this is, Harrison or Dayi wrote this I believe
            Debug.Log("e4: connecting to server...");
            client = new E4SensorClient("127.0.0.1", 19002);
            connected = client.ConnectToServer();
            Debug.Log("connected = " + connected.ToString());
        }
    
        //gets incoming E4 data and updates the slider
        if(connected && client.PollAndRecv(ref recvData) && !isPaused)
        {
            Debug.Log("Received E4 data. Stress level: " + recvData + ". Updating slider...");
            // if(m_Slider.ActiveSelf) {
            //     m_Slider.value = float.Parse(recvData);
            // }

            // Added by Aaron 11/20/19
            EventSystem.current.FireEvent(new ReceivedE4Data("Received E4 Data", float.Parse(recvData)));
     
            //This section creates and writes to a CSV that contains stress data
            if (!File.Exists(path))
                {
                    //Writing using streamwriter
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine("Time, Stress");
                        writeFunc(sw, String.Format("{0:0.#}",recvData));
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        writeFunc(sw, String.Format("{0:0.#}",recvData));
                    }
                }
        }
    }

    private string CurrentTime()
    {
        return DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK");
    }
    
    //Function for writing the file contents aside from header
     private void writeFunc(StreamWriter sw, string stressValue) {
             sw.WriteLine(
                 CurrentTime() + "," 
                 + stressValue);
        sw.Flush();
    }

    private void OnDestroy()
    {
        
    }
    
    //This is strictly for testing purposes, please ignore and will delete after Josh confirms that the real method works
    // private IEnumerator TestData() {
    //     Debug.Log("prime");
    //     yield return new WaitForSeconds(5f);
    //     var random = new System.Random();
    //     Debug.Log(random);
    //     test = random.NextDouble();
    //     if (!File.Exists(path)) 
    //             {
    //                 //Writing using streamwriter
    //                 using (StreamWriter sw = File.CreateText(path))
    //                 {
    //                     sw.WriteLine("Time, Stress");
    //                     writeFunc(sw, float.Parse(String.Format("{0:0.#}", test)));
    //                 }
    //             }
    //             else
    //             {
    //                 using (StreamWriter sw = File.AppendText(path))
    //                 {
    //                     writeFunc(sw, float.Parse(String.Format("{0:0.#}", test)));
    //                 }
    //             }
            
    //     StartCoroutine(TestData());
    // }
}
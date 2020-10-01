using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class KeyboardRating : MonoBehaviour
{
    private string path;
    private string date;

    // Start is called before the first frame update
    void Start()
    {
        date = DateTime.Now.ToString().Replace("/", "-");
        date = date.Replace(":", "_");
        path = Path.Combine(Application.streamingAssetsPath, "Data", "SessionStressRating", $"StressGradeing_{date}.csv");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Keypad1)) {
            RecordKeyPress(1);
        }
        if(Input.GetKeyDown(KeyCode.Keypad2)) {
            RecordKeyPress(2);
        }
        if(Input.GetKeyDown(KeyCode.Keypad3)) {
            RecordKeyPress(3);
        }
        if(Input.GetKeyDown(KeyCode.Keypad4)) {
            RecordKeyPress(4);
        }
        if(Input.GetKeyDown(KeyCode.Keypad5)) {
            RecordKeyPress(5);
        }
        if(Input.GetKeyDown(KeyCode.Keypad6)) {
            RecordKeyPress(6);
        }
        if(Input.GetKeyDown(KeyCode.Keypad7)) {
            RecordKeyPress(7);
        }
        if(Input.GetKeyDown(KeyCode.Keypad8)) {
            RecordKeyPress(8);
        }
        if(Input.GetKeyDown(KeyCode.Keypad9)) {
            RecordKeyPress(9);
        }
        if(Input.GetKeyDown(KeyCode.Keypad0)) {
            RecordKeyPress(10);
        }
    }

    private void RecordKeyPress(int keyPressed) {
        if(!File.Exists(path)) {
            using(StreamWriter sw = File.CreateText(path)) {
                sw.WriteLine("timestamp (UTC),stress value");
                sw.WriteLine(CurrentTime()  + "," +   keyPressed);
            }
        }
        else {
            using(StreamWriter sw = File.AppendText(path)) {
                sw.WriteLine(CurrentTime() + "," +  keyPressed);
            }
        }
    }

    private string CurrentTime() {
        return DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK");
    }
}

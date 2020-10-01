using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.IO;

/* 
Static script used to keep track of settings for all associated scripts
Assosated Scripts : VisualSettings.cs (rename to VisualControl or rename other two), AudioControl.cs, GameplayControl.cs
*/

public static class SettingsInfo
{
    //container for user assigned settings
    public static Hashtable settings = new Hashtable();
    //container for default settings
    public static Hashtable defaultSettings = new Hashtable();


    //The settings saved to the default settings ini
    #region DefaultSettings
    public static class DefaultSettings
    {
        public static int masterVolume 
        {
            get{return (int)SettingsInfo.defaultSettings["master"];}
            set{
                SettingsInfo.defaultSettings["master"] = value;
                }
        }
        public static int environmentVolume 
        {
            get{return (int)SettingsInfo.defaultSettings["environment"];}
            set{
                SettingsInfo.defaultSettings["environment"] = value;
                }
        }
        public static int characterVolume 
        {
            get{return (int)SettingsInfo.defaultSettings["character"];}
            set{
                SettingsInfo.defaultSettings["character"] = value;
                }
        }
        public static float brightness 
        {
            get{return (float)SettingsInfo.defaultSettings["brightness"];}
            set{
                SettingsInfo.defaultSettings["brightness"] = value;
                }
        }
        public static bool colorblind 
        {
            get{return (bool)SettingsInfo.defaultSettings["colorblind"];}
            set{
                SettingsInfo.defaultSettings["colorblind"] = value;
                }
        }
        public static string resolution
        {
            get{return (string)SettingsInfo.defaultSettings["resolution"];}
            set{
                SettingsInfo.defaultSettings["resolution"] = value;
            }
        }
        public static int qualityLevel
        {
            get{return (int)SettingsInfo.defaultSettings["qualitylevel"];}
            set{
                SettingsInfo.defaultSettings["qualitylevel"] = value;
            }
        }
        public static bool fullscreen
        {
            get{return (bool)SettingsInfo.defaultSettings["fullscreen"];}
            set{
                SettingsInfo.defaultSettings["fullscreen"] = value;
            }
        }
        public static bool subtitles
        {
            get{return (bool)SettingsInfo.defaultSettings["subtitles"];}
            set{
                SettingsInfo.defaultSettings["subtitles"] = value;
            }
        }
        public static float subtitlesSize
        {
            get {return (float)SettingsInfo.defaultSettings["subtitlessize"];}
            set{
                SettingsInfo.defaultSettings["subtitlessize"] = value;
            }
        }
        public static string spokenLanguage
        {
            get{return (string)SettingsInfo.defaultSettings["spokenlanguage"];}
            set{
                SettingsInfo.defaultSettings["spokenlanguage"] = value;
            }
        }
        public static string writtenLanguage
        {
            get{return (string)SettingsInfo.defaultSettings["writtenlanguage"];}
            set{
                SettingsInfo.defaultSettings["writtenlanguage"] = value;
            }
        }
    }
    #endregion

    //Audio mixer is used to set the volume values
    public static AudioMixer audioMixer;

    //The next scene to load, move this to something else later (probably some master game controller)
    public static string sceneName;

    //Static object initialization
    static SettingsInfo()
    {
        //Gets the settings into the appropriate INI
        ReadInINI("settings.ini", ref settings);
        ReadInINI("defaultSettings.ini", ref defaultSettings);
        SetResolution();

        settings.Add("vr", "main");
    }

    //I probably need to rethink this just because I have to write
    //writtenToINI = false;
    //everytime I change a setting,
    public static bool writtenToINI = true;

    //Static properties used to manage the current user sttings
    #region INIProperties
    public static int masterVolume 
    {
        get{return (int)settings["master"];}
        set{
            settings["master"] = value;
            }
    }
    public static int environmentVolume 
    {
        get{return (int)settings["environment"];}
        set{
            settings["environment"] = value;
            }
    }
    public static int characterVolume 
    {
        get{return (int)settings["character"];}
        set{
            settings["character"] = value;
            }
    }
    public static float brightness 
    {
        get{return (float)settings["brightness"];}
        set{
            settings["brightness"] = value;
            }
    }
    public static bool colorblind 
    {
        get{return (bool)settings["colorblind"];}
        set{
            settings["colorblind"] = value;
            }
    }
    public static string resolution
    {
        get{return (string)settings["resolution"];}
        set{
            settings["resolution"] = value;
        }
    }
    public static int qualityLevel
    {
        get{return (int)settings["qualitylevel"];}
        set
        {
            settings["qualitylevel"] = value;
        }
    }
    public static bool fullscreen
    {
        get{return (bool)settings["fullscreen"];}
        set{
            settings["fullscreen"] = value;
        }
    }
    public static bool subtitles
    {
        get{return (bool)settings["subtitles"];}
        set{
            settings["subtitles"] = value;
        }
    }
    public static float subtitlesSize
    {
        get{return (float)settings["subtitlessize"];}
        set{
            settings["subtitlessize"] = value;
        }
    }
    public static string spokenLanguage
    {
        get{return (string)settings["spokenlanguage"];}
        set{
            settings["spokenlanguage"] = value;
        }
    }
    public static string writtenLanguage
    {
        get{return (string)settings["writtenlanguage"];}
        set{
            settings["writtenlanguage"] = value;
        }
    }
    #endregion
    //This shouldn't be set by user so don't worry about setting in ini
    public static string VR
    {
        get{return (string)settings["vr"];}
        set{
            settings["vr"] = value;
        }
    }

    //Reads in ini and assings values to associated Hashtable with correct type
    private static void ReadInINI(string fileName, ref Hashtable holder)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "config", fileName);
        string line;
        
        //Streamreader used to read in the ini based on path
        using(StreamReader x = new StreamReader(path))
        {
            //Read until the end of file
            while(!x.EndOfStream)
            {
                //read each line and consider it a setting if it contains '='
                line = x.ReadLine();
                if(line.Contains("="))
                {
                    //Grabs the left (name) and right (value) elements of the appropriate lines
                    var seperated = line.Split('=');
                    //elifs that saves to appropriate Hashtable with the correct type
                    if(seperated[1] == "True" || seperated[1] == "False")
                    {
                        holder.Add(seperated[0], seperated[1] == "True");
                    }
                    else if(int.TryParse(seperated[1], out int intResult))
                    {
                        holder.Add(seperated[0], intResult);
                    }
                    else if(float.TryParse(seperated[1], out float floatResult))
                    {
                        holder.Add(seperated[0], floatResult);
                    }
                    else
                    {
                        holder.Add(seperated[0], seperated[1]);
                    }
                }
            }
        }
    }

    //When reset to default is hit this is used to the main ini via Default Ini
    public static void ResetToDefault(string setting)
    {
        switch(setting)
        {
            case "audio":
            masterVolume = DefaultSettings.masterVolume;
            characterVolume = DefaultSettings.characterVolume;
            environmentVolume = DefaultSettings.environmentVolume;
            spokenLanguage = DefaultSettings.spokenLanguage;
            break;
            case "video":
            resolution = DefaultSettings.resolution;
            qualityLevel = DefaultSettings.qualityLevel;
            fullscreen = DefaultSettings.fullscreen;
            brightness = DefaultSettings.brightness;
            colorblind = DefaultSettings.colorblind;
            break;
            case "gameplay":
            subtitles = DefaultSettings.subtitles;
            subtitlesSize = DefaultSettings.subtitlesSize;
            writtenLanguage = DefaultSettings.writtenLanguage;
            break;
        }
    }

    //If the user does not want to save changes to settings
    //TODO: this could possibly be replaced with the ReadINI function if I used default values for when 
    //Resetting is needed.
    public static void ResetValues()
    {
        var path = Path.Combine(Application.streamingAssetsPath, "config", "settings.ini");
        string line;

        using(StreamReader x = new StreamReader(path))
        {
            while(!x.EndOfStream)
            {
                line = x.ReadLine();
                if(line.Contains("="))
                {
                    var seperated = line.Split('=');
                    if(seperated[1] == "True" || seperated[1] == "False")
                    {
                        settings[seperated[0]] = (seperated[1] == "True");
                    }
                    else if(int.TryParse(seperated[1], out int intResult))
                    {
                        settings[seperated[0]] = intResult;
                    }
                    else if(float.TryParse(seperated[1], out float floatResult))
                    {
                        settings[seperated[0]] = floatResult;
                    }
                    else
                    {
                        settings[seperated[0]] = seperated[1];
                    }
                }
            }
        }
    }

    //This is called when the user agrees to save changes 
    public static void WriteToINI()
    {
        var path = Path.Combine(Application.streamingAssetsPath, "config", "settings.ini");
        string text;
        string newFileText = "";

        using(StreamReader sReader = new StreamReader(path))
        {   
            while(!sReader.EndOfStream)
            {
                text = sReader.ReadLine();
                if(text.Contains("="))
                {
                    var keyAndValue = text.Split('=');
                    Debug.Log($"{keyAndValue[0]}={settings[keyAndValue[0]].ToString()}");
                    if(settings[keyAndValue[0]].ToString() != keyAndValue[1])
                    {
                        text = $"{keyAndValue[0]}={settings[keyAndValue[0]].ToString()}";
                    }
                }
                newFileText += text + "\n";
            }
        }
        using(StreamWriter sWriter = new StreamWriter(path))
        {
            sWriter.Write(newFileText);
            writtenToINI = true;
        }
    }

    //Sets the initial resolution of the game via value found in ini
    public static void SetResolution()
    {
        //resolution = WxH and is saved as a string so I split at x and save each element as an int
        int width, height;
        var widthAndHeight = resolution.Split('x');
        int.TryParse(widthAndHeight[0], out width);
        int.TryParse(widthAndHeight[1], out height);
        //then use built in function to change resolution
        Screen.SetResolution(width, height, SettingsInfo.fullscreen);
    }
    public static void SetFullScreenOrNot()
    {
        if(SettingsInfo.fullscreen)
        {
            Screen.fullScreen = true;
        }
        else
        {
            Screen.fullScreen = false;
        }
    }
}

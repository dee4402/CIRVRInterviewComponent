using UnityEngine;
using System.Collections;
using System.IO;
using System;

namespace UnityStandardAssets.Utility.Logging
{
    public class InterviewLogHandler : ILogHandler
    {
        private FileStream fileStream;
        private StreamWriter streamWriter;
       // private ILogHandler defaultLogHandler = Debug.unityLogger.logHandler;

        public InterviewLogHandler()
        {
            var name = DateTime.Now.ToString().Replace("/", "-");
            name = name.Replace(":", "_");
            name += "_LogFile";
            string filePath = Application.persistentDataPath + $"/{name}.log";//"{DateTime.Now.Year.ToString()}_{DateTime.Now.Month.ToString()} {DateTime.Now.Day.ToString()} {DateTime.Now.Hour.ToString()} {DateTime.Now.Minute.ToString()} {DateTime.Now.Second.ToString()}.log";

            fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            streamWriter = new StreamWriter(fileStream);

            Debug.unityLogger.logHandler = this;
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            // Write to our custom file
            streamWriter.WriteLine($"{DateTime.Now.Hour.ToString()}:{DateTime.Now.Minute.ToString()}:{DateTime.Now.Second.ToString()}:{DateTime.Now.Millisecond.ToString()} {String.Format(format, args)}");
            streamWriter.Flush();

            // Maintain default logger behaviour
           // defaultLogHandler.LogFormat(logType, context, format, args); 
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            //defaultLogHandler.LogException(exception, context);
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using System.Linq;
namespace Cirvr.ConversationManager
{
    public class UserState
    {
        public UserState() { }


        Func<FixedSizedQueue, float, float> E4IndexReducer = (history, newE4Index) => history.Avg();

        public string personName { get; set; }
        public string developmentTool { get; set; }
        public string ProgrammingLanguage { get; set; }
        public string responseLoopingStructure { get; set; }
        public string responseBitShift { get; set; }
        public string responseDataType { get; set; }
        public string hoursPerWeek { get; set; }
        public string subject { get; set; }
        public string spreadsheetSoftware {get; set;}
        public string wordProcessor { get; set; }

        private float m_e4Index;
        public float E4Index
        {
            get { return m_e4Index; }
            set
            {
                E4IndexHistory.Enqueue(value);
                this.m_e4Index = E4IndexReducer(E4IndexHistory, value);
            }
        }
        public FixedSizedQueue E4IndexHistory { get; set; } = new FixedSizedQueue(3);


       

        public T GetPropertyValue<T>(string propName)
        {
            if (this.GetType().GetProperty(propName) == null)
            {
                return default;
            }
            return (T)this.GetType().GetProperty(propName).GetValue(this, null);
        }

        public void SetPropertyValue<T>(string propName, T propValue)
        {
            if (this.GetType().GetProperty(propName) == null)
            {
                // throw trying to set user property that does not exist
            }
            this.GetType().GetProperty(propName).SetValue(this, propValue, null);
        }
    }

    public class FixedSizedQueue
    {
        public FixedSizedQueue(int size) 
        {
            this.Limit = size;
        }

        public ConcurrentQueue<float> q = new ConcurrentQueue<float>();
        private object lockObject = new object();

        public int Limit { get; set; }
        public void Enqueue(float obj)
        {
            q.Enqueue(obj);
            lock (lockObject)
            {
                float overflow;
                while (q.Count > Limit && q.TryDequeue(out overflow)) ;
            }
        }

        public float Avg()
        {
            return q.ToArray().Average();
        }
    }
}
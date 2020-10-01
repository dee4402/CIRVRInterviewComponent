using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility.Exceptions;

namespace UnityStandardAssets.Utility.Events
{
	public class EventSystem : MonoBehaviour
	{
		void OnEnable()
        {
            __current = this;
        }

        static private EventSystem __current;
        static public EventSystem current
        {
            get
            {
                if(__current == null)
                {
                    __current = GameObject.FindObjectOfType<EventSystem>();
                }

                return __current;
            }
        }

		Dictionary<System.Type, List<EventListener>> eventListeners;
		delegate void EventListener(EventInfo e);
		public void RegisterListener<T>(System.Action<T> listener) where T : EventInfo
		{

			System.Type eventType = typeof(T);
			if (eventListeners == null) 
			{
				eventListeners = new Dictionary<System.Type, List<EventListener>>();
			}

			if (!eventListeners.ContainsKey(eventType) || eventListeners[eventType] == null) 
			{
				eventListeners[eventType] = new List<EventListener>();
			}

			EventListener wrapper = (e) => { listener((T)e); };
            eventListeners[eventType].Add(wrapper);
		}

		public void UnregisterListener<T>(System.Action<T> listener) where T : EventInfo
		{
			NotImplementedException e = new NotImplementedException("UnregisterListener");
			throw e;
		}

		public void FireEvent(EventInfo eventInfo)
        {
            System.Type eventType = eventInfo.GetType();
            if (eventListeners == null || eventListeners[eventType] == null)
            {
                return;
            }

            foreach(EventListener el in eventListeners[eventType])
            {
                el( eventInfo );
            }
        }

	}
}
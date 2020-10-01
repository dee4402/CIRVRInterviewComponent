//-----------------------------------------------------------------------
// Copyright 2016 Tobii AB (publ). All rights reserved.
//-----------------------------------------------------------------------

using Tobii.Gaming.Internal;
using UnityEngine;
using Tobii.G2OM;

namespace Tobii.Gaming
{
	/// <summary>
	/// Component that makes the game object GazeAware, meaning aware if the 
	/// user's eye-gaze is on it or not.
	/// </summary>
	[AddComponentMenu("Eye Tracking/Gaze Aware")]
	public class GazeAware : MonoBehaviour, Tobii.Gaming.Internal.IGazeFocusable , Tobii.G2OM.IGazeFocusable
	{
		public bool HasGazeFocus { get; private set; }

		void OnEnable()
		{
			GazeFocusHandler().RegisterFocusableComponent(this);
		}

		void OnDisable()
		{
			GazeFocusHandler().UnregisterFocusableComponent(this);
		}

		/// <summary>
		/// Function called from the gaze focus handler when the gaze focus for
		/// this object changes. Since the implementation is explicit, it will 
		/// not be visible on instances of this component (unless cast to 
		/// <see cref="IGazeFocusable"/>).
		/// </summary>
		/// <param name="hasFocus"></param>
		void Tobii.Gaming.Internal.IGazeFocusable.UpdateGazeFocus(bool hasFocus)
		{
			HasGazeFocus = hasFocus;
		}

		private IRegisterGazeFocusable GazeFocusHandler()
		{
			return (IRegisterGazeFocusable)TobiiHost.GetInstance().GazeFocus;
		}
		
		public void GazeFocusChanged(bool hasFocus)
    	{
	        Debug.Log($"GameObject {gameObject.name} focus == {hasFocus}");
    	}
	}
}

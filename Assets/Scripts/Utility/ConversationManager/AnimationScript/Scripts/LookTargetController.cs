// LookTargetController.cs
// Tore Knabe
// Copyright 2019 ioccam@ioccam.com


using UnityEngine;
using UnityEngine.Events;
using System.Collections;

using Random = UnityEngine.Random;

#if UNITY_2017_2_OR_NEWER
	using UnityEngine.XR;
#else
	using UnityEngine.VR;
#endif

namespace RealisticEyeMovements {

	public class LookTargetController : MonoBehaviour
	{
		#region fields

			[Tooltip("Drag objects here for the actor to look at. If empty, actor will look in random directions.")]
			public Transform[] pointsOfInterest;

            [Tooltip("Drag your points here that will be used for nodding up/down or left/right")]
            public Transform[] Nodding;

			[Tooltip("Ratio of how often to look at player vs elsewhere. 0: never, 1: always")]
			[Range(0,1)]
			public float lookAtPlayerRatio = 0.1f;

			[Tooltip("How likely the actor is to look back at the player when player stares at actor.")]
			[Range(0,1)]
			public float stareBackFactor = 0;

			[Tooltip("If player is closer than this, notice him")]
			[Range(0, 100)]
			public float noticePlayerDistance = 0;

			[Tooltip("If player is closer than this, look away (overrides noticing him)")]
			[Range(0, 4)]
			public float personalSpaceDistance = 0;

			[Tooltip("Minimum time to look at a target")]
			[Range(1f, 100f)]
			public float minLookTime = 3f;

			[Tooltip("Maximum time to look at a target")]
			[Range(1f, 100f)]
			public float maxLookTime = 10f;

			[Tooltip("For 3rd person games, set this to the player's eye center transform")]
			#if !UNITY_WP8 && !UNITY_WP_8_1 && !UNITY_METRO
				[UnityEngine.Serialization.FormerlySerializedAs ("playerEyeCenter")]
			#endif
			public Transform thirdPersonPlayerEyeCenter;

			[Tooltip("Keep trying to track target even when it moves out of sight")]
			public bool keepTargetEvenWhenLost = true;

			[Header("Events")]
			public UnityEvent OnStartLookingAtPlayer;
			public UnityEvent OnStopLookingAtPlayer;
			public UnityEvent OnPlayerEntersPersonalSpace;
			public UnityEvent OnLookAwayFromShyness;

			EyeAndHeadAnimator eyeAndHeadAnimator;

			const float minLookAtMeTimeToReact = 4;

			Transform targetPOI;
		
			Transform mainCameraXform;
			Transform mainCameraParentXform;
			Transform playerEyeCenterXform;
			Transform playerLeftEyeXform;
			Transform playerRightEyeXform;
			Transform usedThirdPersonPlayerEyeCenter;

			GameObject createdVRParentGO;
			GameObject createdPlayerEyeCenterGO;
			GameObject createdPlayerLeftEyeGO;
			GameObject createdPlayerRightEyeGO;
		
			float lastDistanceToPlayer = -1;
			float playerLookingAtMeTime;
			float nextChangePOITime;
			float stareBackDeadtime;	
			float timeOfLastNoticeCheck = -1000;
			float timeOfLastLookBackCheck = -1000;
			float timeOutsideOfAwarenessZone = 1000;
			float timeInsidePersonalSpace;

			bool useNativeVRSupport;
			bool useVR;

			bool isInitialized;
			bool wasInPersonalSpaceLastFrame;

			enum State
			{
				LookingAtPlayer,
				LookingAroundIdly,
				LookingAtPoiDirectly,
				LookingAwayFromPlayer
			}
			State state = State.LookingAroundIdly;

		#endregion
	
	

		void Awake()
		{
			// For VR we use Unity's InputTracking to find the user's eyes, but that only gives local positions, and
			// in case the main camera has no parent, we need to create a reference parent to compute the global position from
			// the local one.
			mainCameraParentXform = new GameObject("Original Camera Position").transform;
			mainCameraParentXform.parent = transform;
			mainCameraParentXform.hideFlags = HideFlags.HideInHierarchy;
		}



		public void Blink()
		{
			eyeAndHeadAnimator.Blink();
		}


		void ChangeStateTo(State newState)
		{
			if ( state != State.LookingAtPlayer && newState == State.LookingAtPlayer )
				if ( OnStartLookingAtPlayer != null )
					OnStartLookingAtPlayer.Invoke();

			if ( state == State.LookingAtPlayer && newState != State.LookingAtPlayer )
				if ( OnStopLookingAtPlayer != null )
					OnStopLookingAtPlayer.Invoke();

			state = newState;
		}


		Vector3 ChooseNextHeadTargetPoint()
		{
			bool hasBoneEyelidControl = eyeAndHeadAnimator.controlData.eyelidControl == ControlData.EyelidControl.Bones;
			float angleVert = Random.Range(-0.5f * (hasBoneEyelidControl ? 6f : 3f), hasBoneEyelidControl ? 6f : 4f);
			float angleHoriz = Random.Range(-10f, 10f);

			Vector3 forward = (eyeAndHeadAnimator.headWeight <= 0)	? eyeAndHeadAnimator.GetHeadDirection()
																									: eyeAndHeadAnimator.headParentXform.forward;
			Vector3 distortedForward = Quaternion.Euler(angleVert, angleHoriz, 0) * forward;
			Vector3 point = eyeAndHeadAnimator.GetOwnEyeCenter() + 2 * eyeAndHeadAnimator.eyeDistanceScale * Random.Range(3.0f, 5.0f) *distortedForward;

			return point;
		}



		Transform ChooseNextHeadTargetPOI()
		{
			if ( pointsOfInterest == null || pointsOfInterest.Length == 0 )
				return null;

			int numPOIsInView = 0;
			for (int i=0;  i<pointsOfInterest.Length;  i++)
			{
				if  ( pointsOfInterest[i] != null && pointsOfInterest[i] != targetPOI && eyeAndHeadAnimator.CanGetIntoView(pointsOfInterest[i].position) && pointsOfInterest[i].gameObject.activeInHierarchy )
					numPOIsInView++;
			}
			if ( numPOIsInView == 0 )
				return targetPOI;
			
			
			int targetVisibleIndex = Random.Range(0, numPOIsInView);
			int visibleIndex = 0;
			for (int i=0;  i<pointsOfInterest.Length;  i++)
			{
				if  ( pointsOfInterest[i] != null && pointsOfInterest[i] != targetPOI && eyeAndHeadAnimator.CanGetIntoView(pointsOfInterest[i].position) && pointsOfInterest[i].gameObject.activeInHierarchy )
				{
					if ( visibleIndex == targetVisibleIndex )
						return pointsOfInterest[i];

					visibleIndex++;
				}
			}
			
			return null;
		}



		public void ClearLookTarget()
		{
			eyeAndHeadAnimator.ClearLookTarget();
			nextChangePOITime = -1;
		}



		Transform FindPlayerCamera()
		{
			if ( thirdPersonPlayerEyeCenter != null )
				return thirdPersonPlayerEyeCenter;
				
			if ( Camera.main != null )
				return Camera.main.transform;
				
			foreach ( Camera cam in FindObjectsOfType<Camera>() )
				if ( cam.targetTexture == null )
					return cam.transform;
					
			return null;
		}
		
				
								
		public void Initialize(bool lookAroundIdly = true)
		{
			if ( isInitialized )
				return;

			if ( createdVRParentGO != null )
			{
				DestroyNotifier destroyNotifier = createdVRParentGO.GetComponent<DestroyNotifier>();
				if ( destroyNotifier != null )
					destroyNotifier.OnDestroyedEvent -= OnPlayerEyesParentDestroyed;

				Destroy(createdVRParentGO);

				createdVRParentGO = null;
				createdPlayerEyeCenterGO = null;
				createdPlayerLeftEyeGO = null;
				createdPlayerRightEyeGO = null;
			}

			eyeAndHeadAnimator = GetComponent<EyeAndHeadAnimator>();
			eyeAndHeadAnimator.Initialize();

			eyeAndHeadAnimator.OnTargetDestroyed -= OnTargetDestroyed;
			eyeAndHeadAnimator.OnCannotGetTargetIntoView -= OnCannotGetTargetIntoView;
			eyeAndHeadAnimator.OnTargetOutOfSight -= OnTargetOutOfSight;
			eyeAndHeadAnimator.OnUpdate2Finished -= VeryLateUpdate;

			eyeAndHeadAnimator.OnTargetDestroyed += OnTargetDestroyed;
			eyeAndHeadAnimator.OnCannotGetTargetIntoView += OnCannotGetTargetIntoView;
			eyeAndHeadAnimator.OnTargetOutOfSight += OnTargetOutOfSight;
			eyeAndHeadAnimator.OnUpdate2Finished += VeryLateUpdate;

			playerEyeCenterXform = playerLeftEyeXform = playerRightEyeXform = null;

			//*** Player eyes: either user main camera or VR cameras
			{
				#if UNITY_2017_2_OR_NEWER
					useNativeVRSupport = useVR = XRDevice.isPresent && XRSettings.enabled;
				#else
					useNativeVRSupport = useVR = VRDevice.isPresent && VRSettings.enabled;
				#endif

				GameObject ovrRigGO = GameObject.Find("OVRCameraRig");
				if ( ovrRigGO != null )
				{
					useVR = true;
					useNativeVRSupport = false;

					playerLeftEyeXform = Utils.FindChildInHierarchy(ovrRigGO, "LeftEyeAnchor").transform;
					playerRightEyeXform = Utils.FindChildInHierarchy(ovrRigGO, "RightEyeAnchor").transform;
					playerEyeCenterXform = Utils.FindChildInHierarchy(ovrRigGO, "CenterEyeAnchor").transform;
				}
				else if ( useNativeVRSupport )
				{
					if ( FindPlayerCamera() == null )
					{
						Debug.LogWarning("Main camera not found. Please set the main camera's tag to 'MainCamera'.");
						useVR = false;
						useNativeVRSupport = false;
						lookAtPlayerRatio = 0;
					}
					else
					{
						mainCameraXform = FindPlayerCamera();
						createdPlayerEyeCenterGO = new GameObject("CreatedPlayerCenterVREye") { hideFlags = HideFlags.HideInHierarchy };
						createdPlayerLeftEyeGO = new GameObject("CreatedPlayerLeftVREye") { hideFlags = HideFlags.HideInHierarchy };
						createdPlayerRightEyeGO = new GameObject("CreatedPlayerRightVREye") { hideFlags = HideFlags.HideInHierarchy };

						playerEyeCenterXform = createdPlayerEyeCenterGO.transform;
						playerLeftEyeXform = createdPlayerLeftEyeGO.transform;
						playerRightEyeXform = createdPlayerRightEyeGO.transform;

						Transform playerXform = mainCameraXform;
						createdVRParentGO = new GameObject("PlayerEyesParent") { hideFlags = HideFlags.HideInHierarchy };
						DontDestroyOnLoad(createdVRParentGO);
						DestroyNotifier destroyNotifier = createdVRParentGO.AddComponent<DestroyNotifier>();
						destroyNotifier.OnDestroyedEvent += OnPlayerEyesParentDestroyed;
						createdVRParentGO.transform.position = playerXform.position;
						createdVRParentGO.transform.rotation = playerXform.rotation;
						createdVRParentGO.transform.parent = playerXform.parent;

						createdPlayerEyeCenterGO.transform.parent = createdVRParentGO.transform;
						createdPlayerLeftEyeGO.transform.parent = createdVRParentGO.transform;
						createdPlayerRightEyeGO.transform.parent = createdVRParentGO.transform;

						UpdateNativeVREyePositions();
					}
				}

				if ( false == useVR )
				{
					if ( FindPlayerCamera() != null )
						playerEyeCenterXform = FindPlayerCamera();
					else
					{
						Debug.LogWarning("Main camera not found. Please set the main camera's tag to 'MainCamera' or set Player Eye Center.");
						lookAtPlayerRatio = 0;
					}
				}
			}

			UpdatePlayerEyeTransformReferences();

			isInitialized = true;

			if ( lookAroundIdly )
				LookAroundIdly();

			nextChangePOITime = 0;
		}



		public bool IsLookingAtPlayer()
		{
			return state == State.LookingAtPlayer;
		}



		public bool IsPlayerInView()
		{
			UpdatePlayerEyeTransformReferences();
			
			return (playerEyeCenterXform != null) && eyeAndHeadAnimator.IsInView( playerEyeCenterXform.position );
		}




		// To keep looking at the player until new command, set duration to -1
		public void LookAtPlayer(float duration=-1, float headLatency=0.075f)
		{
			if (false == isInitialized)
				Initialize(false);

			UpdatePlayerEyeTransformReferences();

			if ( playerLeftEyeXform != null && playerRightEyeXform	!= null )
				eyeAndHeadAnimator.LookAtFace( playerLeftEyeXform, playerRightEyeXform, playerEyeCenterXform, headLatency );
			else if ( playerEyeCenterXform != null )
				eyeAndHeadAnimator.LookAtFace( playerEyeCenterXform, headLatency );
			else
				return;
			
			nextChangePOITime = (duration >= 0) ? (Time.time + duration) : -1;

			targetPOI = null;
			timeOutsideOfAwarenessZone = 0;

			ChangeStateTo(State.LookingAtPlayer);
		}
	
	
	
		public void LookAroundIdly()
		{
			if (false == isInitialized)
				Initialize(false);

			if ( state == State.LookingAtPlayer )
				stareBackDeadtime = Random.Range(10.0f, 30.0f);
			
			targetPOI = ChooseNextHeadTargetPOI();

			if ( targetPOI != null )
				eyeAndHeadAnimator.LookAtAreaAround( targetPOI );
			else
				eyeAndHeadAnimator.LookAroundIdly();

			nextChangePOITime = Time.time + Random.Range(Mathf.Min(minLookTime, maxLookTime), Mathf.Max(minLookTime, maxLookTime));
					
			ChangeStateTo(State.LookingAroundIdly);
		}

        public void Nod(int noddingLook)
        {
            if (Nodding == null || Nodding.Length == 0)
                return;

            LookAtPoiDirectly(Nodding[noddingLook], .075f);
            StartCoroutine(WaitToNod(.08f, noddingLook++));
            //LookAtPoiDirectly(Nodding[1], 1f);
            Debug.Log($"getting to end");
            return;
        }

        IEnumerator WaitToNod(float howLong = 1f, int secondNodIndex = 0)
        {
            yield return new WaitForSeconds(howLong);
            Debug.Log($"wait to nod {howLong}");
            LookAtPoiDirectly(Nodding[secondNodIndex], .075f);
        }


        // To keep looking at the poi until new command, set duration to -1
        public void LookAtPoiDirectly( Transform poiXform, float duration=-1, float headLatency=0.075f )
		{
			if (false == isInitialized)
				Initialize(false);

			eyeAndHeadAnimator.LookAtSpecificThing( poiXform, headLatency );
			nextChangePOITime = (duration >= 0) ? (Time.time + duration) : -1;
			ChangeStateTo(State.LookingAtPoiDirectly);
		}
	
	
	
		// To keep looking at the poi until new command, set duration to -1
		public void LookAtPoiDirectly( Vector3 poi, float duration=-1, float headLatency=0.075f )
		{
			if (false == isInitialized)
				Initialize(false);

			eyeAndHeadAnimator.LookAtSpecificThing( poi, headLatency: headLatency );
			nextChangePOITime = (duration >= 0) ? (Time.time + duration) : -1;
			ChangeStateTo(State.LookingAtPoiDirectly);
		}
	
	
	
		void LookAwayFromPlayer()
		{
			if ( playerEyeCenterXform == null )
				return;

			OnLookAwayFromShyness.Invoke();

			stareBackDeadtime = Random.Range(5.0f, 10.0f);
			
			bool isPlayerOnMyLeft = eyeAndHeadAnimator.headParentXform.InverseTransformPoint( playerEyeCenterXform.position ).x < 0;
			Vector3 awayPoint = eyeAndHeadAnimator.headParentXform.TransformPoint( eyeAndHeadAnimator.GetOwnEyeCenter() + 10 * (Quaternion.Euler(0, isPlayerOnMyLeft ? 50 : -50, 0 ) * Vector3.forward));
			eyeAndHeadAnimator.LookAtAreaAround( awayPoint );

			nextChangePOITime = Time.time + Random.Range(Mathf.Min(minLookTime, maxLookTime), Mathf.Max(minLookTime, maxLookTime));

			ChangeStateTo(State.LookingAwayFromPlayer);
		}



		void OnCannotGetTargetIntoView()
		{
			bool shouldKeepTryingToLookAtTarget =	(state == State.LookingAtPoiDirectly || (state == State.LookingAtPlayer && nextChangePOITime == -1)) &&
																		keepTargetEvenWhenLost;
			if ( false == shouldKeepTryingToLookAtTarget && eyeAndHeadAnimator.CanChangePointOfAttention() )
				OnTargetLost();
		}


		
		void OnDestroy()
		{
			if ( createdVRParentGO != null )
			{
				DestroyNotifier destroyNotifier = createdVRParentGO.GetComponent<DestroyNotifier>();
				if ( destroyNotifier != null )
					destroyNotifier.OnDestroyedEvent -= OnPlayerEyesParentDestroyed;

				Destroy(createdVRParentGO);
			}

			if ( isInitialized && eyeAndHeadAnimator != null )
			{
				eyeAndHeadAnimator.OnTargetDestroyed -= OnTargetDestroyed;
				eyeAndHeadAnimator.OnCannotGetTargetIntoView -= OnCannotGetTargetIntoView;
				eyeAndHeadAnimator.OnTargetOutOfSight -= OnTargetOutOfSight;
				eyeAndHeadAnimator.OnUpdate2Finished -= VeryLateUpdate;
			}
		}



		void OnPlayerEyesParentDestroyed(DestroyNotifier destroyNotifier)
		{
			if ( destroyNotifier.gameObject != createdVRParentGO )
			{
				Debug.LogWarning("Received OnPlayerEyesParentDestroyed from unknown gameObject " + destroyNotifier, destroyNotifier.gameObject);

				return;
			}

			createdVRParentGO = null;
			createdPlayerEyeCenterGO = null;
			createdPlayerLeftEyeGO = null;
			createdPlayerRightEyeGO = null;

			isInitialized = false;
			Initialize(true);
		}



		void OnTargetDestroyed()
		{
			OnTargetLost();
		}



		void OnTargetLost()
		{
			float r = Random.value;
			if ( r <= lookAtPlayerRatio && IsPlayerInView() )
				LookAtPlayer(Random.Range(Mathf.Min(minLookTime, maxLookTime), Mathf.Max(minLookTime, maxLookTime)));
			else
				LookAroundIdly();
		}



		void OnTargetOutOfSight()
		{
			if ( state == State.LookingAroundIdly )
				OnTargetLost();
		}



		void Start()
		{
			StartCoroutine(AssignCamera());
			if ( false == isInitialized )
				Initialize(true);
		}

		private IEnumerator AssignCamera()
		{
			yield return new WaitForSeconds(1f);
			thirdPersonPlayerEyeCenter = GameObject.Find(Camera.main.name).transform;
		}


		void UpdateNativeVREyePositions()
		{
			#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_1_OR_NEWER || UNITY_2017_1_OR_NEWER
				if (useNativeVRSupport && usedThirdPersonPlayerEyeCenter == null)
				{
					if ( mainCameraXform == null )
					{
						if ( Camera.main == null )
						{
							Debug.LogError("Main camera not found");
							return;
						}
						mainCameraXform = Camera.main.transform;
					}

					if ( mainCameraXform.parent != null)
					{
						#if UNITY_2017_2_OR_NEWER
							playerEyeCenterXform.position = mainCameraXform.parent.TransformPoint(InputTracking.GetLocalPosition(XRNode.CenterEye));
							playerEyeCenterXform.rotation = mainCameraXform.parent.rotation * InputTracking.GetLocalRotation(XRNode.CenterEye);
							playerLeftEyeXform.position = mainCameraXform.parent.TransformPoint(InputTracking.GetLocalPosition(XRNode.LeftEye));
							playerRightEyeXform.position = mainCameraXform.parent.TransformPoint(InputTracking.GetLocalPosition(XRNode.RightEye));
						#else
							playerEyeCenterXform.position = mainCameraXform.parent.TransformPoint(InputTracking.GetLocalPosition(VRNode.CenterEye));
							playerEyeCenterXform.rotation = mainCameraXform.parent.rotation * InputTracking.GetLocalRotation(VRNode.CenterEye);
							playerLeftEyeXform.position = mainCameraXform.parent.TransformPoint(InputTracking.GetLocalPosition(VRNode.LeftEye));
							playerRightEyeXform.position = mainCameraXform.parent.TransformPoint(InputTracking.GetLocalPosition(VRNode.RightEye));
						#endif
					}
					else
					{
						#if UNITY_2017_2_OR_NEWER
							mainCameraParentXform.rotation = mainCameraXform.rotation * Quaternion.Inverse(InputTracking.GetLocalRotation(XRNode.CenterEye));
							Vector3 camLocal = UnityEngine.XR.InputTracking.GetLocalPosition(XRNode.CenterEye);
						#else
							mainCameraParentXform.rotation = mainCameraXform.rotation * Quaternion.Inverse(InputTracking.GetLocalRotation(VRNode.CenterEye));
							Vector3 camLocal = UnityEngine.VR.InputTracking.GetLocalPosition(VRNode.CenterEye);
						#endif
						mainCameraParentXform.position = mainCameraXform.position - camLocal.x * mainCameraParentXform.right - camLocal.y * mainCameraParentXform.up - mainCameraParentXform.forward*camLocal.z;

						#if UNITY_2017_2_OR_NEWER
							playerEyeCenterXform.position = mainCameraParentXform.TransformPoint(InputTracking.GetLocalPosition(XRNode.CenterEye));
							playerEyeCenterXform.rotation = mainCameraParentXform.rotation * InputTracking.GetLocalRotation(XRNode.CenterEye);
							playerLeftEyeXform.position = mainCameraParentXform.TransformPoint(InputTracking.GetLocalPosition(XRNode.LeftEye));
							playerRightEyeXform.position = mainCameraParentXform.TransformPoint(InputTracking.GetLocalPosition(XRNode.RightEye));
						#else
							playerEyeCenterXform.position = mainCameraParentXform.TransformPoint(InputTracking.GetLocalPosition(VRNode.CenterEye));
							playerEyeCenterXform.rotation = mainCameraParentXform.rotation * InputTracking.GetLocalRotation(VRNode.CenterEye);
							playerLeftEyeXform.position = mainCameraParentXform.TransformPoint(InputTracking.GetLocalPosition(VRNode.LeftEye));
							playerRightEyeXform.position = mainCameraParentXform.TransformPoint(InputTracking.GetLocalPosition(VRNode.RightEye));
						#endif
				}
			}
			#endif
		}



		void UpdatePlayerEyeTransformReferences()
		{
			if ( thirdPersonPlayerEyeCenter == usedThirdPersonPlayerEyeCenter )
				return;

			if ( thirdPersonPlayerEyeCenter != null )
			{
				if ( Utils.IsEqualOrDescendant(transform, thirdPersonPlayerEyeCenter) )
					Debug.LogError("Player Eye Center should be part of the player character who this character is supposed to look at, not part of this character itself!");

				playerEyeCenterXform = thirdPersonPlayerEyeCenter;
				playerLeftEyeXform = playerRightEyeXform = null;
			}
			else if ( useNativeVRSupport )
			{
				playerEyeCenterXform = createdPlayerEyeCenterGO.transform;
				playerLeftEyeXform = createdPlayerLeftEyeGO.transform;
				playerRightEyeXform = createdPlayerRightEyeGO.transform;
			}
			else if ( useVR )
			{
				GameObject ovrRigGO = GameObject.Find("OVRCameraRig");
				if ( ovrRigGO != null )
				{
					playerLeftEyeXform = Utils.FindChildInHierarchy(ovrRigGO, "LeftEyeAnchor").transform;
					playerRightEyeXform = Utils.FindChildInHierarchy(ovrRigGO, "RightEyeAnchor").transform;
					playerEyeCenterXform = Utils.FindChildInHierarchy(ovrRigGO, "CenterEyeAnchor").transform;
				}
				else
				{
					playerEyeCenterXform = FindPlayerCamera();
					playerLeftEyeXform = playerRightEyeXform = null;
				}
			}
			else
			{
				playerEyeCenterXform = FindPlayerCamera();
				playerLeftEyeXform = playerRightEyeXform = null;
			}

			usedThirdPersonPlayerEyeCenter = thirdPersonPlayerEyeCenter;
		}



		void VeryLateUpdate()
		{
			if ( false == isInitialized )
				return;
			
			UpdatePlayerEyeTransformReferences();
			
			if (useNativeVRSupport && usedThirdPersonPlayerEyeCenter == null)
				UpdateNativeVREyePositions();
				
			//*** Finished looking at current target?
			{
				if ( nextChangePOITime >= 0 && Time.time >= nextChangePOITime && eyeAndHeadAnimator.CanChangePointOfAttention() )
				{
					if ( Random.value <= lookAtPlayerRatio && IsPlayerInView() )
						LookAtPlayer(Random.Range(Mathf.Min(minLookTime, maxLookTime), Mathf.Max(minLookTime, maxLookTime)));
					else
						LookAroundIdly();
					
					return;
				}
			}
			
			
			if ( playerEyeCenterXform == null )
				return;
			
			bool shouldLookBackAtPlayer = false;
			bool shouldNoticePlayer = false;
			bool shouldLookAwayFromPlayer = false;

			Vector3 playerTargetPos = playerEyeCenterXform.position;
			float distanceToPlayer = Vector3.Distance(eyeAndHeadAnimator.GetOwnEyeCenter(), playerTargetPos);
			bool isPlayerInView = eyeAndHeadAnimator.IsInView( playerEyeCenterXform.position );
			bool isPlayerInAwarenessZone = isPlayerInView && distanceToPlayer < noticePlayerDistance;
			bool isPlayerInPersonalSpace = isPlayerInView && distanceToPlayer < personalSpaceDistance;

			//*** Awareness zone
			{
				if ( isPlayerInAwarenessZone )
				{
					if ( Time.time - timeOfLastNoticeCheck > 0.1f && state != State.LookingAtPlayer )
					{
						timeOfLastNoticeCheck = Time.time;
					
						bool isPlayerApproaching = lastDistanceToPlayer > distanceToPlayer;
						float closenessFactor01 = (noticePlayerDistance - distanceToPlayer)/noticePlayerDistance;
						float noticeProbability = Mathf.Lerp (0.1f, 0.5f, closenessFactor01);
						shouldNoticePlayer = isPlayerApproaching && timeOutsideOfAwarenessZone > 1 && Random.value < noticeProbability; 
					}
				}
				else
					timeOutsideOfAwarenessZone += Time.deltaTime;
			}


			//*** Personal space
			{
				 if ( isPlayerInPersonalSpace )
				 {
					timeInsidePersonalSpace += Time.deltaTime * Mathf.Clamp01((personalSpaceDistance - distanceToPlayer)/(0.5f * personalSpaceDistance));
					const float kMinTimeInPersonalSpaceToLookAway = 1;
					if ( timeInsidePersonalSpace >= kMinTimeInPersonalSpaceToLookAway )
						shouldLookAwayFromPlayer = true;

					if ( false == wasInPersonalSpaceLastFrame )
						OnPlayerEntersPersonalSpace.Invoke();
				 }
				 else
					timeInsidePersonalSpace = 0;

				wasInPersonalSpaceLastFrame = isPlayerInPersonalSpace;
			}


			//*** Look away from player?
			{
				if ( shouldLookAwayFromPlayer && state != State.LookingAwayFromPlayer )
				{
					LookAwayFromPlayer();

					return;
				}
			}


			//*** If the player keeps staring at us, stare back?		
			{
				if ( stareBackFactor > 0 && playerEyeCenterXform != null )
				{
					float playerLookingAtMeAngle = eyeAndHeadAnimator.GetStareAngleTargetAtMe( playerEyeCenterXform );
					bool isPlayerLookingAtMe = playerLookingAtMeAngle < 15;
		
					playerLookingAtMeTime = (isPlayerInView && isPlayerLookingAtMe	)	? Mathf.Min(10, playerLookingAtMeTime + Mathf.Cos(Mathf.Deg2Rad * playerLookingAtMeAngle) * Time.deltaTime)
																														: Mathf.Max(0, playerLookingAtMeTime - Time.deltaTime);
			
					if ( false == eyeAndHeadAnimator.IsLookingAtFace() )
					{
						if ( stareBackDeadtime > 0 )
							stareBackDeadtime -= Time.deltaTime;
						
						if (	stareBackDeadtime <= 0  &&
							Time.time - timeOfLastLookBackCheck > 0.1f &&
							playerLookingAtMeTime > minLookAtMeTimeToReact  &&
							eyeAndHeadAnimator.CanChangePointOfAttention() &&
							isPlayerLookingAtMe )
						{
							timeOfLastLookBackCheck = Time.time;
							
							float lookTimeProbability = stareBackFactor * 2 * (Mathf.Min(10, playerLookingAtMeTime) - minLookAtMeTimeToReact) / (10-minLookAtMeTimeToReact);
							shouldLookBackAtPlayer = Random.value < lookTimeProbability;
						}
					}
				}
			}

			if ( shouldLookBackAtPlayer || shouldNoticePlayer )
				LookAtPlayer(Random.Range(Mathf.Min(minLookTime, maxLookTime), Mathf.Max(minLookTime, maxLookTime)));

			lastDistanceToPlayer = distanceToPlayer;

		}


	}

}
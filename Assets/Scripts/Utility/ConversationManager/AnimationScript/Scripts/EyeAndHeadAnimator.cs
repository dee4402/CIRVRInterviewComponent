// EyeAndHeadAnimator.cs
// Tore Knabe
// Copyright 2019 ioccam@ioccam.com

// If you use FinalIK to move the head, uncomment the next line:
// #define USE_FINAL_IK

#if UNITY_4_6
	#if !UNITY_WP8 && !UNITY_WP_8_1 && !UNITY_METRO
		#define SUPPORTS_SERIALIZATION
	#endif
#else
	#if !UNITY_WP8 && !UNITY_WP_8_1 && !UNITY_WSA
		#define SUPPORTS_SERIALIZATION
	#endif
#endif

#if SUPPORTS_SERIALIZATION
	using System.IO;
	using System.Runtime.Serialization.Formatters.Binary;
#endif
using UnityEngine;
using Random = UnityEngine.Random;

namespace RealisticEyeMovements {

	[System.Serializable]
	public class EyeAndHeadAnimatorForExport
	{
		public string headBonePath;
		public float headSpeedModifier;
		public float headWeight;
		public bool useMicroSaccades;
		public bool useMacroSaccades;
		public bool kDrawSightlinesInEditor;
		public ControlData.ControlDataForExport controlData;
		public float kMinNextBlinkTime;
		public float kMaxNextBlinkTime;
		public bool eyelidsFollowEyesVertically;
		public float maxEyeHorizAngle;
		public float maxEyeHorizAngleTowardsNose;
		public float crossEyeCorrection;
		public float nervousness;
		public float limitHeadAngle;
	}


	public class EyeAndHeadAnimator : MonoBehaviour
	{
		#region fields

			const float kMaxLimitedHorizontalHeadAngle = 80;
			const float kMaxLimitedVerticalHeadAngle = 40;
			const float kMaxHorizViewAngle = 100;
			const float kMaxVertViewAngle = 60;

			public event System.Action OnCannotGetTargetIntoView;
			public event System.Action OnTargetDestroyed;
			public event System.Action OnTargetOutOfSight;
			public event System.Action OnUpdate2Finished;

			#region head
				[HideInInspector] public float headSpeedModifier = 1;
				[HideInInspector] public float headWeight = 1;
				[HideInInspector] public Transform headBoneNonMecanimXform;
				Quaternion headBoneNonMecanimFromRootQ;

				// Head jitter
				const float kHeadJitterFrequency = 0.2f;
				const float kHeadJitterAmount = 1.0f;
				Vector3 headJitterRotationComponents = new Vector3(1, 1, 0);
				const int kHeadJitterOctave = 3;
				float headJitterTime;
				Vector2[] headJitterNoiseVectors;

				enum HeadControl
				{
					None,
					Mecanim,
					FinalIK,
					Transform
				}
				HeadControl headControl = HeadControl.None;

				enum HeadTweenMethod
				{
					SmoothDamping,
					CriticalDamping
				}
				HeadTweenMethod headTweenMethod = HeadTweenMethod.SmoothDamping;

			#endregion

			public bool useMicroSaccades = true;
			public bool useMacroSaccades = true;
			public bool useHeadJitter = true;
			public bool kDrawSightlinesInEditor;
			public bool areUpdatedControlledExternally = false;

			[HideInInspector]
			public ControlData controlData = new ControlData();

			#region eye lids
				[Tooltip("Minimum seconds until next blink")]
				public float kMinNextBlinkTime = 3.0f;
				[Tooltip("Maximum seconds until next blink")]
				public float kMaxNextBlinkTime = 15.0f;
				
				[Tooltip("The blinking speed. Default is 1.")]
				[Range(0.1f, 3)]
				public float blinkSpeed = 1;
				
				[Tooltip("Whether the eyelids move up a bit when looking up and down when looking down.")]
				public bool eyelidsFollowEyesVertically = true;

				public float blink01 { get; private set; }		
	
				bool useUpperEyelids;
				bool useLowerEyelids;

				float timeOfNextBlink;
			
				enum BlinkState {
					Idle,
					Closing,
					KeepingClosed,
					Opening
				}
				BlinkState blinkState = BlinkState.Idle;
				float blinkStateTime;
				float blinkDuration;
				bool isShortBlink;

				const float kBlinkCloseTimeShort = 0.036f;
				const float kBlinkOpenTimeShort = 2 * kBlinkCloseTimeShort;
				const float kBlinkCloseTimeLong = 2 * kBlinkCloseTimeShort;
				const float kBlinkOpenTimeLong = 2 * kBlinkOpenTimeShort;
				const float kBlinkKeepingClosedTime = 0.008f;

			#endregion

			[Tooltip("Maximum horizontal eye angle (away from nose)")]
			public float maxEyeHorizAngle = 30;

			[Tooltip("Maximum horizontal eye angle towards nose")]
			public float maxEyeHorizAngleTowardsNose = 20;

			[Tooltip("Cross eye correction factor")]
			[Range(0, 5)]
			public float crossEyeCorrection = 1.0f;

			[Tooltip("The more nervous, the more often you do micro-and macrosaccades.")]
			[Range(0,10)]
			public float nervousness;

			[Tooltip("Limits the angle for the head movement")]
			[Range(0,1)]
			public float limitHeadAngle;

			public float eyeDistance { get; private set; }
			public float eyeDistanceScale { get; private set; }

			Transform leftEyeAnchor;
			Transform rightEyeAnchor;

			float leftMaxSpeedHoriz;
			float leftHorizDuration;
			float leftMaxSpeedVert;
			float leftVertDuration;
			float leftCurrentSpeedX;
			float leftCurrentSpeedY;

			float rightMaxSpeedHoriz;
			float rightHorizDuration;
			float rightMaxSpeedVert;
			float rightVertDuration;
			float rightCurrentSpeedX;
			float rightCurrentSpeedY;

			float startLeftEyeHorizDuration;
			float startLeftEyeVertDuration;
			float startLeftEyeMaxSpeedHoriz;
			float startLeftEyeMaxSpeedVert;

			float startRightEyeHorizDuration;
			float startRightEyeVertDuration;
			float startRightEyeMaxSpeedHoriz;
			float startRightEyeMaxSpeedVert;

			float timeOfEyeMovementStart;
			float timeOfHeadMovementStart;

			// Head movement with SmoothDamp method
			float headMaxSpeedHoriz;
			float headMaxSpeedVert;
			float headHorizDuration;
			float headVertDuration;
			float startHeadHorizDuration;
			float startHeadVertDuration;
			float startHeadMaxSpeedHoriz;
			float startHeadMaxSpeedVert;
			float currentHeadHorizSpeed;
			float currentHeadVertSpeed;
			float currentHeadZSpeed;

			const float kMaxHeadVelocity = 2f;
			const float kHeadOmega = 3.5f;
			CritDampTweenQuaternion critDampTween;
			Vector3 headEulerSpeed;
			Vector3 lastHeadEuler;
			float maxHeadHorizSpeedSinceSaccadeStart;
			float maxHeadVertSpeedSinceSaccadeStart;
			bool isHeadTracking;
			float headTrackingFactor = 1;

			float headLatency;
			float eyeLatency;

			float ikWeight = 1;

			Animator animator;
			#if USE_FINAL_IK
				RootMotion.FinalIK.LookAtIK lookAtIK;
				RootMotion.FinalIK.FullBodyBipedIK fbbik;
			#endif
			bool hasLateUpdateRunThisFrame;
			bool hasCheckedIdleLookTargetsThisFrame;
			bool placeNewIdleLookTargetsAtNextOpportunity;

			#region Transforms for target
				Transform currentHeadTargetPOI;
				Transform currentEyeTargetPOI;
				Transform nextHeadTargetPOI;
				Transform nextEyeTargetPOI;
				Transform currentTargetLeftEyeXform;
				Transform currentTargetRightEyeXform;
				Transform nextTargetLeftEyeXform;
				Transform nextTargetRightEyeXform;
				readonly Transform[] createdTargetXforms = new Transform[2];
				int createdTargetXformIndex;
			#endregion


			public Transform eyesRootXform { get; private set; }
			public Transform headParentXform { get; private set; }
			Transform headTargetPivotXform;
			Transform headXform;

			Quaternion leftEyeRootFromAnchorQ;
			Quaternion rightEyeRootFromAnchorQ;
			Quaternion leftAnchorFromEyeRootQ;
			Quaternion rightAnchorFromEyeRootQ;
			Vector3 currentLeftEyeLocalEuler;
			Vector3 currentRightEyeLocalEuler;
			Quaternion originalLeftEyeLocalQ;
			Quaternion originalRightEyeLocalQ;
			Quaternion lastLeftEyeLocalRotation;
			Quaternion lastRightEyeLocalQ;
			Quaternion headBoneInAvatarQ;

			Vector3 macroSaccadeTargetLocal;
			Vector3 microSaccadeTargetLocal;

			float timeOfEnteringClearingPhase;
			float timeOfLastMacroSaccade = -100;
			float timeToMicroSaccade;
			float timeToMacroSaccade;

			bool isInitialized;
			int lastFrameOfUpdate1 = -1;
			int lastFrameOfUpdate2 = -1;
		
			public enum HeadSpeed
			{
				Slow,
				Fast
			}
			HeadSpeed headSpeed = HeadSpeed.Slow;

			public enum EyeDelay
			{
				Simultaneous,
				EyesFirst,
				HeadFirst
			}

			enum LookTarget
			{
				None,
				StraightAhead,
				ClearingTargetPhase1,
				ClearingTargetPhase2,
				GeneralDirection,
				LookingAroundIdly,
				SpecificThing,
				Face
			}
			LookTarget lookTarget = LookTarget.None;

			enum FaceLookTarget
			{
				EyesCenter,
				LeftEye,
				RightEye,
				Mouth
			}
			FaceLookTarget faceLookTarget = FaceLookTarget.EyesCenter;

		#endregion



		void Awake()
		{
			Initialize();

			if ( lookTarget == LookTarget.None )
				LookAroundIdly();
		}



		public void Blink( bool isShortBlink =true)
		{
			if ( blinkState != BlinkState.Idle )
				return;

			this.isShortBlink = isShortBlink;
			blinkState = BlinkState.Closing;
			blinkStateTime = 0;
			blinkDuration = (1/blinkSpeed) * (isShortBlink ? kBlinkCloseTimeShort : kBlinkCloseTimeLong);
		}



		public bool CanGetIntoView(Vector3 point)
		{
			Vector3 targetLocalAngles = Quaternion.LookRotation( headParentXform.InverseTransformPoint( point ) ).eulerAngles;

			float x = Mathf.Abs(Utils.NormalizedDegAngle(targetLocalAngles.x));
			float y = Mathf.Abs(Utils.NormalizedDegAngle(targetLocalAngles.y));

			bool horizOk = y < (LimitHorizontalHeadAngle(kMaxLimitedHorizontalHeadAngle) + maxEyeHorizAngle + 0.2f * kMaxHorizViewAngle);

			float clampedEyeVertAngle = controlData.ClampRightVertEyeAngle(targetLocalAngles.x);
			bool vertOk = x < (LimitVerticalHeadAngle(kMaxLimitedVerticalHeadAngle) + Mathf.Abs(clampedEyeVertAngle) + 0.2f * kMaxVertViewAngle);
			
			return horizOk && vertOk;
		}



		public bool CanChangePointOfAttention()
		{
			return Time.time-timeOfLastMacroSaccade >= 2f * 1f/(1 + nervousness);
		}



		#if SUPPORTS_SERIALIZATION
			public bool CanImportFromFile(string filename)
			{
				EyeAndHeadAnimatorForExport import;
				using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
					import = (EyeAndHeadAnimatorForExport) new BinaryFormatter().Deserialize(stream);
				}

				return Utils.CanGetTransformFromPath(transform, import.headBonePath) &&
						controlData.CanImport(import.controlData, transform);
			}
		#endif


		// When looking around idly, make sure we keep looking at points that are in view based on where the head
		// looks before REM changes it, so as soon as the animation has oriented the head. So prevent REM from making
		// the character turn their head a lot when trying to keep looking at things that have moved out of view because
		// the character is walking around or dancing or so.
		void CheckIdleLookTargets()
		{
			if ( lookTarget != LookTarget.LookingAroundIdly || hasCheckedIdleLookTargetsThisFrame )
				return;

			bool currentIdleLookTargetsAreOutOfView = false;
			if ( false == placeNewIdleLookTargetsAtNextOpportunity )
			{
				Transform trans = (currentEyeTargetPOI != null) ? currentEyeTargetPOI : currentTargetLeftEyeXform;

				if ( trans != null )
				{
					Vector3 eyeTargetGlobal = trans.TransformPoint(microSaccadeTargetLocal);

					Transform referenceXform = headParentXform;
					Vector3 euler = Quaternion.FromToRotation(referenceXform.forward, eyeTargetGlobal - referenceXform.position).eulerAngles;
					const float kMaxHorizAngle = 45;
					const float kMaxVertAngle = 30;
					currentIdleLookTargetsAreOutOfView = Mathf.Abs(Utils.NormalizedDegAngle(euler.x)) > kMaxVertAngle || Mathf.Abs(Utils.NormalizedDegAngle(euler.y)) > kMaxHorizAngle;
				}
			}

			if ( placeNewIdleLookTargetsAtNextOpportunity || currentIdleLookTargetsAreOutOfView )
			{
				bool hasBoneEyelidControl = controlData.eyelidControl == ControlData.EyelidControl.Bones;
				float angleVert = Random.Range(-0.5f * (hasBoneEyelidControl ? 6f : 3f), hasBoneEyelidControl ? 6f : 4f);
				float angleHoriz = Random.Range(-10f, 10f);

				Vector3 forward = (headWeight <= 0)	? GetHeadDirection()
																		: headParentXform.forward;
				Vector3 distortedForward = Quaternion.Euler(angleVert, angleHoriz, 0) * forward;
				Vector3 point = GetOwnEyeCenter() + 2 * eyeDistanceScale * Random.Range(3.0f, 5.0f) *distortedForward;

				createdTargetXformIndex = (createdTargetXformIndex+1) % createdTargetXforms.Length;
				createdTargetXforms[createdTargetXformIndex].position = point;
				Transform poi = createdTargetXforms[createdTargetXformIndex];
				currentTargetLeftEyeXform = currentTargetRightEyeXform = null;
				nextTargetLeftEyeXform = nextTargetRightEyeXform = null;

				headLatency = 0.075f;
				nextHeadTargetPOI = poi;
				StartEyeMovement(poi, blinkIfEyesMoveEnough: false);
				placeNewIdleLookTargetsAtNextOpportunity = false;
			}

			hasCheckedIdleLookTargetsThisFrame = true;
		}



		// If eye latency is greater than zero, the head starts turning towards new target and the eyes keep looking at the old target for a while.
		// If head latency is greater than zero, the eyes look at the new target first and the head turns later.
		void CheckLatencies()
		{
			if ( eyeLatency > 0 )
			{
				eyeLatency -= Time.deltaTime;
				if ( eyeLatency <= 0 )
				{
					currentEyeTargetPOI = nextEyeTargetPOI;
					currentTargetLeftEyeXform = nextTargetLeftEyeXform;
					currentTargetRightEyeXform = nextTargetRightEyeXform;
					StartEyeMovement(currentEyeTargetPOI);
				}
			}
			else if ( headLatency > 0 )
			{
				headLatency -= Time.deltaTime;
				if ( headLatency <= 0 )
					StartHeadMovement( nextHeadTargetPOI );
			}
		}



		void CheckMacroSaccades()
		{
			if ( lookTarget == LookTarget.SpecificThing )
				return;

			if ( eyeLatency > 0 )
				return;

			timeToMacroSaccade -= Time.deltaTime;
			if ( timeToMacroSaccade <= 0 )
			{
				if ( (lookTarget == LookTarget.GeneralDirection || lookTarget == LookTarget.LookingAroundIdly) && useMacroSaccades)
				{
							const float kMacroSaccadeAngle = 10;
							bool hasBoneEyelidControl = controlData.eyelidControl == ControlData.EyelidControl.Bones;
							float angleVert = Random.Range(-kMacroSaccadeAngle * (hasBoneEyelidControl ? 0.65f : 0.3f), kMacroSaccadeAngle * (hasBoneEyelidControl ? 0.65f : 0.4f));
							float angleHoriz = Random.Range(-kMacroSaccadeAngle,kMacroSaccadeAngle);
					SetMacroSaccadeTarget( eyesRootXform.TransformPoint(	Quaternion.Euler( angleVert, angleHoriz, 0)
																												* eyesRootXform.InverseTransformPoint( GetCurrentEyeTargetPos() )));

					timeToMacroSaccade = Random.Range(5.0f, 8.0f);
					timeToMacroSaccade *= 1.0f/(1.0f + nervousness);
				}
				else if ( lookTarget == LookTarget.Face )
				{
					if ( currentEyeTargetPOI == null )
					{
						//*** Social triangle: saccade between eyes and mouth (or chest, if actor isn't looking back)
						{
							switch( faceLookTarget )
							{
								case FaceLookTarget.LeftEye:
									faceLookTarget = Random.value < 0.75f ? FaceLookTarget.RightEye : FaceLookTarget.Mouth;
									break;
								case FaceLookTarget.RightEye:
									faceLookTarget = Random.value < 0.75f ? FaceLookTarget.LeftEye : FaceLookTarget.Mouth;
									break;
								case FaceLookTarget.Mouth:
								case FaceLookTarget.EyesCenter:
									faceLookTarget = Random.value < 0.5f ? FaceLookTarget.LeftEye : FaceLookTarget.RightEye;
									break;
							}
							SetMacroSaccadeTarget( GetLookTargetPosForSocialTriangle( faceLookTarget ) );
							timeToMacroSaccade = (faceLookTarget == FaceLookTarget.Mouth)	? Random.Range(0.4f, 0.9f)
																																: Random.Range(1.0f, 3.0f);
							timeToMacroSaccade *= 1.0f/(1.0f + nervousness);
						}
					}
				}																																				
			}
		}



		void CheckMicroSaccades()
		{
			if ( false == useMicroSaccades )
				return;

			if ( eyeLatency > 0 )
				return;

			if ( lookTarget == LookTarget.GeneralDirection || lookTarget == LookTarget.SpecificThing || (lookTarget == LookTarget.Face && currentEyeTargetPOI != null) || lookTarget == LookTarget.LookingAroundIdly )
			{
				timeToMicroSaccade -= Time.deltaTime;
				if ( timeToMicroSaccade <= 0 )
				{
					const float kMicroSaccadeAngle = 3;
					bool hasBoneEyelidControl = controlData.eyelidControl == ControlData.EyelidControl.Bones;
					float angleVert = Random.Range(-kMicroSaccadeAngle * (hasBoneEyelidControl ? 0.8f : 0.5f), kMicroSaccadeAngle * (hasBoneEyelidControl ? 0.85f : 0.6f));
					float angleHoriz = Random.Range(-kMicroSaccadeAngle,kMicroSaccadeAngle);
					if ( lookTarget == LookTarget.Face )
					{
						angleVert *= 0.5f;
						angleHoriz *= 0.5f;
					}

					SetMicroSaccadeTarget ( eyesRootXform.TransformPoint(	Quaternion.Euler(angleVert, angleHoriz, 0)
																												* eyesRootXform.InverseTransformPoint( currentEyeTargetPOI.TransformPoint(macroSaccadeTargetLocal) )));
				}
			}
		}


		
		float ClampLeftHorizEyeAngle( float angle )
		{
			float normalizedAngle = Utils.NormalizedDegAngle(angle);
			bool isTowardsNose = normalizedAngle > 0;
			float maxAngle = isTowardsNose ? maxEyeHorizAngleTowardsNose : maxEyeHorizAngle;
			return Mathf.Clamp(normalizedAngle, -maxAngle, maxAngle);
		}



		float ClampRightHorizEyeAngle( float angle )
		{
			float normalizedAngle = Utils.NormalizedDegAngle(angle);
			bool isTowardsNose = normalizedAngle < 0;
			float maxAngle = isTowardsNose ? maxEyeHorizAngleTowardsNose : maxEyeHorizAngle;
			return Mathf.Clamp(normalizedAngle, -maxAngle, maxAngle);
		}



		//float ClampLeftVertEyeAngle( float angle )
		//{
		//	//if ( controlData.eyeControl == ControlData.EyeControl.MecanimEyeBones )
		//	//	return controlData.leftBoneEyeRotationLimiter.ClampAngle( angle );
		//	//else if ( controlData
		//	//return Mathf.Clamp(Utils.NormalizedDegAngle(angle), -controlData.maxEyeUpAngle, controlData.maxEyeDownAngle);
		//}



		public void ClearLookTarget()
		{
			LookAtAreaAround( GetOwnEyeCenter() + transform.forward * 1000 * eyeDistance );
			lookTarget = LookTarget.ClearingTargetPhase1;
			timeOfEnteringClearingPhase = Time.time;
		}



		void DrawSightlinesInEditor()
		{
			if ( controlData.eyeControl != ControlData.EyeControl.None )
			{
				Vector3 leftDirection = (leftEyeAnchor.parent.rotation * leftEyeAnchor.localRotation * leftAnchorFromEyeRootQ) * Vector3.forward;
				Vector3 rightDirection = (rightEyeAnchor.parent.rotation * rightEyeAnchor.localRotation * rightAnchorFromEyeRootQ) * Vector3.forward;
				Debug.DrawLine(leftEyeAnchor.position, leftEyeAnchor.position + leftDirection * 10 * eyeDistanceScale);
				Debug.DrawLine(rightEyeAnchor.position, rightEyeAnchor.position + rightDirection * 10 * eyeDistanceScale);
			}

			// Debug.DrawLine(eyesRootXform.position, eyesRootXform.position + GetOwnLookDirection() * 10  );
		}



		#if SUPPORTS_SERIALIZATION
			public void ExportToFile(string filename)
			{
				EyeAndHeadAnimatorForExport export = new EyeAndHeadAnimatorForExport
				{
					headBonePath = Utils.GetPathForTransform(transform, headBoneNonMecanimXform),
					headSpeedModifier = headSpeedModifier,
					headWeight = headWeight,
					useMicroSaccades = useMicroSaccades,
					useMacroSaccades = useMacroSaccades,
					kDrawSightlinesInEditor = kDrawSightlinesInEditor,
					controlData = controlData.GetExport(transform),
					kMaxNextBlinkTime = kMaxNextBlinkTime,
					eyelidsFollowEyesVertically = eyelidsFollowEyesVertically,
					maxEyeHorizAngle = maxEyeHorizAngle,
					maxEyeHorizAngleTowardsNose = maxEyeHorizAngleTowardsNose,
					crossEyeCorrection = crossEyeCorrection,
					nervousness = nervousness,
					limitHeadAngle = limitHeadAngle,
				};

				FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Write);
				new BinaryFormatter().Serialize(stream, export);
				stream.Close();
			}
		#endif



		Vector3 GetCurrentEyeTargetPos()
		{
			return ( currentEyeTargetPOI != null )	?	currentEyeTargetPOI.position
																	:	0.5f * ( currentTargetLeftEyeXform.position + currentTargetRightEyeXform.position );
		}



		Vector3 GetCurrentHeadTargetPos()
		{
			return ( currentHeadTargetPOI != null )	?	currentHeadTargetPOI.position
																		:	0.5f * ( currentTargetLeftEyeXform.position + currentTargetRightEyeXform.position );
		}



		public Vector3 GetHeadDirection()
		{
			return headXform.rotation * headBoneInAvatarQ * Vector3.forward;
		}



		public Vector3 GetLeftEyeDirection()
		{
			if ( leftEyeAnchor == null )
				return eyesRootXform.forward;

			return (leftEyeAnchor.parent.rotation * leftEyeAnchor.localRotation * leftAnchorFromEyeRootQ) * Vector3.forward;
		}


		Vector3 GetLookTargetPosForSocialTriangle( FaceLookTarget playerFaceLookTarget )
		{
			if ( currentTargetLeftEyeXform == null || currentTargetRightEyeXform == null )
				return currentEyeTargetPOI.position;

			Vector3 faceTargetPos = Vector3.zero;

			Vector3 eyeCenter = 0.5f * (currentTargetLeftEyeXform.position + currentTargetRightEyeXform.position);

			switch( playerFaceLookTarget )
			{
				case FaceLookTarget.EyesCenter:
					faceTargetPos = GetCurrentEyeTargetPos();
					break;
				case FaceLookTarget.LeftEye:
					faceTargetPos = Vector3.Lerp(eyeCenter, currentTargetLeftEyeXform.position, 0.75f);
					break;
				case FaceLookTarget.RightEye:
					faceTargetPos = Vector3.Lerp(eyeCenter, currentTargetRightEyeXform.position, 0.75f);
					break;
				case FaceLookTarget.Mouth:
					Vector3 eyeUp = 0.5f * (currentTargetLeftEyeXform.up + currentTargetRightEyeXform.up);
					faceTargetPos = eyeCenter - eyeUp * 0.4f * Vector3.Distance( currentTargetLeftEyeXform.position, currentTargetRightEyeXform.position );
					break;
			}

			return faceTargetPos;
		}



		public Vector3 GetOwnEyeCenter()
		{
			return eyesRootXform.position;
		}



		public Transform GetOwnEyeCenterXform()
		{
			return eyesRootXform;
		}



		Vector3 GetOwnLookDirection()
		{
			return ( leftEyeAnchor != null && rightEyeAnchor != null )	?  (Quaternion.Slerp(	leftEyeAnchor.rotation * leftAnchorFromEyeRootQ,
																																rightEyeAnchor.rotation * rightAnchorFromEyeRootQ, 0.5f)) * Vector3.forward
																								:	eyesRootXform.forward;
		}



		public Vector3 GetRightEyeDirection()
		{
			if ( rightEyeAnchor == null )
				return eyesRootXform.forward;

			return (rightEyeAnchor.parent.rotation * rightEyeAnchor.localRotation * rightAnchorFromEyeRootQ) * Vector3.forward;
		}



		public float GetStareAngleMeAtTarget( Vector3 target )
		{
			return Vector3.Angle(GetOwnLookDirection(), target - eyesRootXform.position);
		}



		public float GetStareAngleTargetAtMe( Transform targetXform )
		{
			return Vector3.Angle(targetXform.forward, GetOwnEyeCenter() - targetXform.position);
		}


	
		#if SUPPORTS_SERIALIZATION

			public void ImportFromBytes(byte[] arrBytes)
			{
					EyeAndHeadAnimatorForExport import;
					using (MemoryStream memStream = new MemoryStream()) {
						memStream.Write(arrBytes, 0, arrBytes.Length);
						memStream.Seek(0, SeekOrigin.Begin);
						import = (EyeAndHeadAnimatorForExport) new BinaryFormatter().Deserialize(memStream);
					}
					headBoneNonMecanimXform = Utils.GetTransformFromPath(transform, import.headBonePath);
					headSpeedModifier = import.headSpeedModifier;
					headWeight = import.headWeight;
					useMicroSaccades = import.useMicroSaccades;
					useMacroSaccades = import.useMacroSaccades;
					kDrawSightlinesInEditor = import.kDrawSightlinesInEditor;
					controlData.Import(import.controlData, transform);
					kMaxNextBlinkTime = import.kMaxNextBlinkTime;
					eyelidsFollowEyesVertically = import.eyelidsFollowEyesVertically;
					maxEyeHorizAngle = import.maxEyeHorizAngle;
					maxEyeHorizAngleTowardsNose = import.maxEyeHorizAngleTowardsNose;
					if ( maxEyeHorizAngleTowardsNose <= 0 )
						maxEyeHorizAngleTowardsNose = maxEyeHorizAngle;
					crossEyeCorrection = import.crossEyeCorrection;
					nervousness = import.nervousness;
					limitHeadAngle = import.limitHeadAngle;

					isInitialized = false;

					if ( controlData.NeedsSaveDefaultBlendshapeConfig() )
					{
						controlData.RestoreDefault();
						controlData.SaveDefault(this);
					}
			}



			public void ImportFromFile(string filename)
			{
				if ( false == CanImportFromFile(filename) )
				{
					Debug.LogError(name + " cannot import from file");
					return;
				}

				EyeAndHeadAnimatorForExport import;
				using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
					import = (EyeAndHeadAnimatorForExport) new BinaryFormatter().Deserialize(stream);
				}

				headBoneNonMecanimXform = Utils.GetTransformFromPath(transform, import.headBonePath);
				headSpeedModifier = import.headSpeedModifier;
				headWeight = import.headWeight;
				useMicroSaccades = import.useMicroSaccades;
				useMacroSaccades = import.useMacroSaccades;
				kDrawSightlinesInEditor = import.kDrawSightlinesInEditor;
				controlData.Import(import.controlData, transform);
				kMaxNextBlinkTime = import.kMaxNextBlinkTime;
				eyelidsFollowEyesVertically = import.eyelidsFollowEyesVertically;
				maxEyeHorizAngle = import.maxEyeHorizAngle;
				maxEyeHorizAngleTowardsNose = import.maxEyeHorizAngleTowardsNose;
				if ( maxEyeHorizAngleTowardsNose <= 0 )
					maxEyeHorizAngleTowardsNose = maxEyeHorizAngle;
				crossEyeCorrection = import.crossEyeCorrection;
				nervousness = import.nervousness;
				limitHeadAngle = import.limitHeadAngle;

				isInitialized = false;

				if ( controlData.NeedsSaveDefaultBlendshapeConfig() )
				{
					controlData.RestoreDefault();
					controlData.SaveDefault(this);
				}
			}
		#endif


		
		public void Initialize()
		{
			if ( isInitialized )
				return;

			if ( controlData == null )
				return;

			//*** Head jitter
			{
				headJitterTime = Random.value * 10;
				headJitterNoiseVectors = new Vector2[6];

		        for (var i = 0; i < 6; i++)
		        {
		            var theta = Random.value * Mathf.PI * 2;
		            headJitterNoiseVectors[i].Set(Mathf.Cos(theta), Mathf.Sin(theta));
		        }
			}


			eyeDistance = 0.064f;
			eyeDistanceScale = 1;
			animator = GetComponentInChildren<Animator>();
			#if USE_FINAL_IK
				lookAtIK = GetComponentInChildren<RootMotion.FinalIK.LookAtIK>();
				fbbik = GetComponentInChildren<RootMotion.FinalIK.FullBodyBipedIK>();

				if ( lookAtIK != null  )
				{
					headControl = HeadControl.FinalIK;
					//if ( false == areUpdatedControlledExternally )
					//{
					//	if ( fbbik != null )
					//		fbbik.solver.OnPostUpdate += Update2;
					//	else
					//		lookAtIK.solver.OnPostUpdate += Update2;
					//}
				}
			#endif

			headXform = null;
			{
				if ( headControl == HeadControl.None )
				{
					if ( animator != null && animator.GetBoneTransform(HumanBodyBones.Head) != null )
					{
						headControl = HeadControl.Mecanim;
						headXform = animator.GetBoneTransform(HumanBodyBones.Head);
					}
					else if ( headBoneNonMecanimXform != null )
					{
						headControl = HeadControl.Transform;
						headXform = headBoneNonMecanimXform;
						headBoneNonMecanimFromRootQ = Quaternion.Inverse(transform.rotation) * headBoneNonMecanimXform.rotation;
					}
				}
				else if ( headControl == HeadControl.FinalIK )
				{
					#if USE_FINAL_IK
						headXform = lookAtIK.solver.head.transform;
					#endif
				}
				if ( headXform == null )
					headXform = transform;

				headBoneInAvatarQ = Quaternion.Inverse(transform.rotation) * headXform.rotation;
			}

			controlData.CheckConsistency( animator, this );
			controlData.Initialize( transform );

			if ( createdTargetXforms[0] == null )
			{
				createdTargetXforms[0] = new GameObject(name + "_createdEyeTarget_1").transform;
				createdTargetXforms[0].gameObject.hideFlags = HideFlags.HideInHierarchy;

				DontDestroyOnLoad(createdTargetXforms[0].gameObject);
				DestroyNotifier destroyNotifer = createdTargetXforms[0].gameObject.AddComponent<DestroyNotifier>();
				destroyNotifer.OnDestroyedEvent += OnCreatedXformDestroyed;
			}

			if ( createdTargetXforms[1] == null )
			{
				createdTargetXforms[1] = new GameObject(name + "_createdEyeTarget_2").transform;
				createdTargetXforms[1].gameObject.hideFlags = HideFlags.HideInHierarchy;

				DestroyNotifier destroyNotifer = createdTargetXforms[1].gameObject.AddComponent<DestroyNotifier>();
				destroyNotifer.OnDestroyedEvent += OnCreatedXformDestroyed;
				DontDestroyOnLoad(createdTargetXforms[1].gameObject);
			}


			if ( headParentXform == null )
			{
				Transform spineXform = null;
						if ( animator != null )
						{
							spineXform = animator.GetBoneTransform(HumanBodyBones.Chest);
							if ( spineXform == null )
								spineXform = animator.GetBoneTransform(HumanBodyBones.Spine);
						}
						if ( spineXform == null )
							spineXform = transform;

				headParentXform = new GameObject(name + " head parent").transform;
				headParentXform.gameObject.hideFlags = HideFlags.HideInHierarchy;
				headParentXform.parent = spineXform;
				headParentXform.position = headXform.position;
				headParentXform.rotation = transform.rotation;
			}

			if ( headTargetPivotXform == null )
			{
				headTargetPivotXform = new GameObject(name + " head target").transform;
				headTargetPivotXform.gameObject.hideFlags = HideFlags.HideInHierarchy;
				headTargetPivotXform.parent = headParentXform;
				headTargetPivotXform.localPosition = Vector3.zero;
				headTargetPivotXform.localRotation = Quaternion.identity;

				critDampTween = new CritDampTweenQuaternion(headTargetPivotXform.localRotation, kHeadOmega, kMaxHeadVelocity);
 				lastHeadEuler = headTargetPivotXform.localEulerAngles;
			}

			//*** Eyes
			{
				if ( controlData.eyeControl == ControlData.EyeControl.MecanimEyeBones || controlData.eyeControl == ControlData.EyeControl.SelectedObjects )
				{
					if ( controlData.eyeControl == ControlData.EyeControl.MecanimEyeBones )
					{
						Transform leftEyeBoneXform = animator.GetBoneTransform(HumanBodyBones.LeftEye);
						Transform rightEyeBoneXform = animator.GetBoneTransform(HumanBodyBones.RightEye);
						leftEyeAnchor = leftEyeBoneXform;
						rightEyeAnchor = rightEyeBoneXform;
						if ( leftEyeAnchor == null )
							Debug.LogError("Left eye bone not found in Mecanim rig");
						if ( rightEyeAnchor == null )
							Debug.LogError("Right eye bone not found in Mecanim rig");
					}
					else if ( controlData.eyeControl == ControlData.EyeControl.SelectedObjects )
					{
						leftEyeAnchor = controlData.leftEye;
						rightEyeAnchor = controlData.rightEye;
					}
				}

				if ( eyesRootXform == null )
				{
					eyesRootXform = new GameObject(name + "_eyesRoot").transform;
					eyesRootXform.gameObject.hideFlags = HideFlags.HideInHierarchy;
					eyesRootXform.rotation = transform.rotation;
				}

				if ( leftEyeAnchor != null && rightEyeAnchor != null )
				{
					eyeDistance = Vector3.Distance( leftEyeAnchor.position, rightEyeAnchor.position );
					eyeDistanceScale = eyeDistance/0.064f;
					controlData.RestoreDefault(withEyelids: false);
					Quaternion inverse = Quaternion.Inverse(eyesRootXform.rotation);
					leftEyeRootFromAnchorQ = inverse * leftEyeAnchor.rotation;
					rightEyeRootFromAnchorQ = inverse * rightEyeAnchor.rotation;
					leftAnchorFromEyeRootQ = Quaternion.Inverse(leftEyeRootFromAnchorQ);
					rightAnchorFromEyeRootQ = Quaternion.Inverse(rightEyeRootFromAnchorQ);

					originalLeftEyeLocalQ = leftEyeAnchor.localRotation;
					originalRightEyeLocalQ = rightEyeAnchor.localRotation;

					eyesRootXform.position = 0.5f * (leftEyeAnchor.position + rightEyeAnchor.position);
					Transform commonAncestorXform = Utils.GetCommonAncestor( leftEyeAnchor, rightEyeAnchor );
					eyesRootXform.parent =  (commonAncestorXform != null) ? commonAncestorXform : leftEyeAnchor.parent;
					
					Vector3 right = (rightEyeAnchor.position - leftEyeAnchor.position).normalized;
					Vector3 forward = Vector3.Cross(right, transform.up);
					eyesRootXform.rotation = Quaternion.LookRotation(forward, transform.up);
				}
				else if ( animator != null )
				{
					if ( headXform != null )
					{
						eyesRootXform.position = headXform.position;
						eyesRootXform.parent = headXform;
					}
					else
					{
						eyesRootXform.position = transform.position;
						eyesRootXform.parent = transform;
					}
				}
				else
				{
					eyesRootXform.position = transform.position;
					eyesRootXform.parent = transform;
				}
			}


			//*** Eye lids
			{
				if ( controlData.eyelidControl == ControlData.EyelidControl.Bones )
				{
					if ( controlData.upperEyeLidLeft != null && controlData.upperEyeLidRight != null )
						useUpperEyelids = true;

					if ( controlData.lowerEyeLidLeft != null && controlData.lowerEyeLidRight != null )
						useLowerEyelids = true;
				}

				blink01 = 0;
				timeOfNextBlink = Time.time + Random.Range(3f, 6f);
				ikWeight = headWeight;
			}

			isInitialized = true;
		}



		public bool IsInView( Vector3 target )
		{
			if ( leftEyeAnchor == null || rightEyeAnchor == null )
			{
							Vector3 localAngles = Quaternion.LookRotation(eyesRootXform.InverseTransformDirection(target - GetOwnEyeCenter())).eulerAngles;
							float vertAngle = Utils.NormalizedDegAngle(localAngles.x);
							float horizAngle = Utils.NormalizedDegAngle(localAngles.y);
				bool seesTarget = Mathf.Abs(vertAngle) <= kMaxVertViewAngle && Mathf.Abs(horizAngle) <= kMaxHorizViewAngle;

				return seesTarget;
			}
			else
			{
							Vector3 localAnglesLeft = (leftEyeRootFromAnchorQ * Quaternion.Inverse(leftEyeAnchor.rotation) * Quaternion.LookRotation(target - leftEyeAnchor.position, leftEyeAnchor.up)).eulerAngles;
							float vertAngleLeft = Utils.NormalizedDegAngle(localAnglesLeft.x);
							float horizAngleLeft = Utils.NormalizedDegAngle(localAnglesLeft.y);
				bool leftEyeSeesTarget = Mathf.Abs(vertAngleLeft) <= kMaxVertViewAngle && Mathf.Abs(horizAngleLeft) <= kMaxHorizViewAngle;

							Vector3 localAnglesRight = (rightEyeRootFromAnchorQ * Quaternion.Inverse(rightEyeAnchor.rotation) * Quaternion.LookRotation(target - rightEyeAnchor.position, rightEyeAnchor.up)).eulerAngles;
							float vertAngleRight = Utils.NormalizedDegAngle(localAnglesRight.x);
							float horizAngleRight = Utils.NormalizedDegAngle(localAnglesRight.y);
				bool rightEyeSeesTarget = Mathf.Abs(vertAngleRight) <= kMaxVertViewAngle && Mathf.Abs(horizAngleRight) <= kMaxHorizViewAngle;

				return leftEyeSeesTarget || rightEyeSeesTarget;
			}
		}



		public bool IsLookingAtFace()
		{
			return lookTarget == LookTarget.Face;
		}
	
	
	
		void LateUpdate()
		{
			if ( Time.timeScale == 0 )
				return;

			if ( false == areUpdatedControlledExternally )
				Update1();
		}


		// The angle that the head actually takes grows sublinearly with the angle that it would take if it were looking directly at the target.
		// kMaxLimitedHorizontalHeadAngle: the maximum angle that the head can turn.
		// kMaxUnlimitedHeadAngle: the angle of the target at which kMaxLimitedHorizontalHeadAngle is reached, from then on it's just capped.
		float LimitHorizontalHeadAngle( float headAngle )
		{
			const float kMaxUnlimitedHeadAngle = 140;
			float maxLimitedHeadAngle = Mathf.Lerp(kMaxLimitedHorizontalHeadAngle, 0, limitHeadAngle);
			const float kExponent = 2f;

			headAngle = Utils.NormalizedDegAngle(headAngle);
			float absAngle = Mathf.Abs(headAngle);

			float limitedAngle = Mathf.Sign(headAngle) *
										Mathf.Min(maxLimitedHeadAngle, (absAngle - Mathf.Pow(absAngle, kExponent) * (kMaxUnlimitedHeadAngle-maxLimitedHeadAngle)/Mathf.Pow(kMaxUnlimitedHeadAngle, kExponent)));

			return limitedAngle;
		}



		float LimitVerticalHeadAngle( float headAngle )
		{
			const float kMaxUnlimitedHeadAngle = 80;
			float maxLimitedHeadAngle = Mathf.Lerp(kMaxLimitedVerticalHeadAngle, 0, limitHeadAngle);
			const float kExponent = 2f;

			headAngle = Utils.NormalizedDegAngle(headAngle);
			float absAngle = Mathf.Abs(headAngle);
			float limitedAngle = Mathf.Sign(headAngle) *
										Mathf.Min(maxLimitedHeadAngle, (absAngle - (Mathf.Pow(absAngle, kExponent)) * (kMaxUnlimitedHeadAngle-maxLimitedHeadAngle)/Mathf.Pow(kMaxUnlimitedHeadAngle, kExponent)));

			return limitedAngle;
		}



		public void LookAtFace( Transform eyeCenterXform, float headLatency=0.075f )
		{
			Initialize();

			lookTarget = LookTarget.Face;
			headSpeed = HeadSpeed.Fast;
			faceLookTarget = FaceLookTarget.EyesCenter;
			nextHeadTargetPOI = eyeCenterXform;
			this.headLatency = headLatency;
			currentTargetLeftEyeXform = currentTargetRightEyeXform = null;
			nextTargetLeftEyeXform = nextTargetRightEyeXform = null;

			StartEyeMovement( eyeCenterXform );
		}



		public void LookAtFace(	Transform leftEyeXform,
											Transform rightEyeXform,
											Transform eyesCenterXform,
											float headLatency=0.075f )
		{
			Initialize();

			lookTarget = LookTarget.Face;
			headSpeed = HeadSpeed.Fast;
			faceLookTarget = FaceLookTarget.EyesCenter;
			this.headLatency = headLatency;
			currentTargetLeftEyeXform = leftEyeXform;
			currentTargetRightEyeXform = rightEyeXform;
			nextTargetLeftEyeXform = nextTargetRightEyeXform = null;
			nextHeadTargetPOI = eyesCenterXform;

			StartEyeMovement( );
		}



		public void LookAtSpecificThing( Transform poi, float headLatency=0.075f )
		{
			Initialize();

			lookTarget = LookTarget.SpecificThing;
			headSpeed = HeadSpeed.Fast;
			this.headLatency = headLatency;
			nextHeadTargetPOI = poi;
			currentTargetLeftEyeXform = currentTargetRightEyeXform = null;
			nextTargetLeftEyeXform = nextTargetRightEyeXform = null;

			StartEyeMovement( poi );
		}



		public void LookAtSpecificThing( Vector3 point, float headLatency=0.075f )
		{
			Initialize();

			createdTargetXformIndex = (createdTargetXformIndex+1) % createdTargetXforms.Length;
			createdTargetXforms[createdTargetXformIndex].position = point;
			LookAtSpecificThing( createdTargetXforms[createdTargetXformIndex], headLatency );
		}



		public void LookAroundIdly()
		{
			Initialize();

			lookTarget = LookTarget.LookingAroundIdly;
			headSpeed = HeadSpeed.Slow;
			eyeLatency = headLatency = 0;

			placeNewIdleLookTargetsAtNextOpportunity = true;
		}



		public void LookAtAreaAround( Transform poi )
		{
			Initialize();

			lookTarget = LookTarget.GeneralDirection;
			headSpeed = HeadSpeed.Slow;
			eyeLatency = Random.Range(0.05f, 0.1f);

			nextEyeTargetPOI = poi;
			currentTargetLeftEyeXform = currentTargetRightEyeXform = null;
			nextTargetLeftEyeXform = nextTargetRightEyeXform = null;

			StartHeadMovement( poi );
		}



		public void LookAtAreaAround( Vector3 point )
		{
			Initialize();

			createdTargetXformIndex = (createdTargetXformIndex+1) % createdTargetXforms.Length;
			createdTargetXforms[createdTargetXformIndex].position = point;
			LookAtAreaAround( createdTargetXforms[createdTargetXformIndex] );
		}



		void OnAnimatorIK()
		{
			if ( headControl != HeadControl.Mecanim )
				return;

			if ( lookTarget == LookTarget.LookingAroundIdly )
			CheckIdleLookTargets();

				float targetIKWeight = (lookTarget == LookTarget.StraightAhead || lookTarget == LookTarget.ClearingTargetPhase2 || lookTarget == LookTarget.ClearingTargetPhase1 ) ? 0 : headWeight;
			ikWeight = Mathf.Lerp( ikWeight, targetIKWeight, Time.deltaTime);

			if ( ikWeight <= 0.01f )
			 	return;

			animator.SetLookAtWeight(1, 0.01f, ikWeight);
			animator.SetLookAtPosition(headTargetPivotXform.TransformPoint( eyeDistanceScale * Vector3.forward ));
		}



		void OnCreatedXformDestroyed( DestroyNotifier destroyNotifer )
		{
			Transform destroyedXform = destroyNotifer.GetComponent<Transform>();

			for (int i=0;  i<createdTargetXforms.Length; i++)
				if ( createdTargetXforms[i] == destroyedXform )
					createdTargetXforms[i] = null;
		}



		void OnDestroy()
		{
			foreach ( Transform createdXform in createdTargetXforms )
				if ( createdXform != null )
				{
					createdXform.GetComponent<DestroyNotifier>().OnDestroyedEvent -= OnCreatedXformDestroyed;
					Destroy( createdXform.gameObject );
				}

			controlData.OnDestroy();
		}



		void OnEnable()
		{
			Initialize();
		}



		void SetMacroSaccadeTarget( Vector3 targetGlobal, bool blinkIfEyesMoveEnough = true)
		{	
			macroSaccadeTargetLocal = ((currentEyeTargetPOI != null) ? currentEyeTargetPOI : currentTargetLeftEyeXform).InverseTransformPoint( targetGlobal );

			timeOfLastMacroSaccade = Time.time;

			SetMicroSaccadeTarget( targetGlobal, blinkIfEyesMoveEnough );
			timeToMicroSaccade += 0.75f;
		}



		void SetMicroSaccadeTarget( Vector3 targetGlobal, bool blinkIfEyesMoveEnough=true )
		{
			if ( controlData.eyeControl == ControlData.EyeControl.None || leftEyeAnchor == null || rightEyeAnchor == null )
				return;

			microSaccadeTargetLocal = ((currentEyeTargetPOI != null) ? currentEyeTargetPOI : currentTargetLeftEyeXform).InverseTransformPoint( targetGlobal );

			Vector3 targetLeftEyeLocalAngles = Quaternion.LookRotation(eyesRootXform.InverseTransformDirection( targetGlobal - leftEyeAnchor.position)).eulerAngles;
				targetLeftEyeLocalAngles = new Vector3(controlData.ClampLeftVertEyeAngle(targetLeftEyeLocalAngles.x),
																		ClampLeftHorizEyeAngle(targetLeftEyeLocalAngles.y),
																		targetLeftEyeLocalAngles.z);

			float leftHorizDistance = Mathf.Abs(Mathf.DeltaAngle(currentLeftEyeLocalEuler.y, targetLeftEyeLocalAngles.y));

					// From "Realistic Avatar and Head Animation Using a Neurobiological Model of Visual Attention", Itti, Dhavale, Pighin
			leftMaxSpeedHoriz = 473 * (1 - Mathf.Exp(-leftHorizDistance/7.8f));

					// From "Eyes Alive", Lee, Badler
					const float D0 = 0.025f;
					const float d = 0.00235f;
			leftHorizDuration = D0 + d * leftHorizDistance;

			float leftVertDistance = Mathf.Abs(Mathf.DeltaAngle(currentLeftEyeLocalEuler.x, targetLeftEyeLocalAngles.x));
			leftMaxSpeedVert = 473 * (1 - Mathf.Exp(-leftVertDistance/7.8f));
			leftVertDuration = D0 + d * leftVertDistance;


			Vector3 targetRightEyeLocalAngles = Quaternion.LookRotation(eyesRootXform.InverseTransformDirection( targetGlobal - rightEyeAnchor.position)).eulerAngles;
				targetRightEyeLocalAngles = new Vector3(controlData.ClampRightVertEyeAngle(targetRightEyeLocalAngles.x),
																			ClampRightHorizEyeAngle(targetRightEyeLocalAngles.y),
																			targetRightEyeLocalAngles.z);

			float rightHorizDistance = Mathf.Abs(Mathf.DeltaAngle(currentRightEyeLocalEuler.y, targetRightEyeLocalAngles.y));
			rightMaxSpeedHoriz = 473 * (1 - Mathf.Exp(-rightHorizDistance/7.8f));
			rightHorizDuration = D0 + d * rightHorizDistance;

			float rightVertDistance = Mathf.Abs(Mathf.DeltaAngle(currentRightEyeLocalEuler.x, targetRightEyeLocalAngles.x));
			rightMaxSpeedVert = 473 * (1 - Mathf.Exp(-rightVertDistance/7.8f));
			rightVertDuration = D0 + d * rightVertDistance;

			leftMaxSpeedHoriz = rightMaxSpeedHoriz = Mathf.Max( leftMaxSpeedHoriz, rightMaxSpeedHoriz );
			leftMaxSpeedVert = rightMaxSpeedVert = Mathf.Max( leftMaxSpeedVert, rightMaxSpeedVert );
			leftHorizDuration = rightHorizDuration = Mathf.Max( leftHorizDuration, rightHorizDuration );
			leftVertDuration = rightVertDuration = Mathf.Max( leftVertDuration, rightVertDuration );

			timeToMicroSaccade = Random.Range(0.8f, 1.75f);
			timeToMicroSaccade *= 1.0f/(1.0f + 0.4f * nervousness);

			//*** Blink if eyes move enough
			{
				if (blinkIfEyesMoveEnough)
					if ( useUpperEyelids || useLowerEyelids || controlData.eyelidControl == ControlData.EyelidControl.Blendshapes )
					{
						float distance = Mathf.Max(leftHorizDistance, Mathf.Max(rightHorizDistance, Mathf.Max(leftVertDistance, rightVertDistance)));
						const float kMinBlinkDistance = 25.0f;
						if ( distance >= kMinBlinkDistance )
							Blink( isShortBlink: false );
					}
			}

			//*** For letting the eyes keep tracking the target after they saccaded to it
			{
				startLeftEyeHorizDuration = leftHorizDuration;
				startLeftEyeVertDuration = leftVertDuration;
				startLeftEyeMaxSpeedHoriz = leftMaxSpeedHoriz;
				startLeftEyeMaxSpeedVert = leftMaxSpeedVert;

				startRightEyeHorizDuration = rightHorizDuration;
				startRightEyeVertDuration = rightVertDuration;
				startRightEyeMaxSpeedHoriz = rightMaxSpeedHoriz;
				startRightEyeMaxSpeedVert = rightMaxSpeedVert;

				timeOfEyeMovementStart = Time.time;
			}

		}



		void Start()
		{
			#if USE_FINAL_IK
				if ( headControl == HeadControl.FinalIK  && false == areUpdatedControlledExternally )
				{
					if ( fbbik != null )
						fbbik.solver.OnPostUpdate += Update2;
					else
						lookAtIK.solver.OnPostUpdate += Update2;
				}
			#endif
		}



		void StartEyeMovement( Transform targetXform=null, bool blinkIfEyesMoveEnough = true)
		{
			eyeLatency = 0;
			currentEyeTargetPOI = targetXform;
			nextEyeTargetPOI = null;
			nextTargetLeftEyeXform = nextTargetRightEyeXform = null;

			SetMacroSaccadeTarget ( GetCurrentEyeTargetPos(), blinkIfEyesMoveEnough );
			timeToMacroSaccade = Random.Range(1.5f, 2.5f);
			timeToMacroSaccade *= 1.0f/(1.0f + nervousness);

			if ( currentHeadTargetPOI == null )
				currentHeadTargetPOI = currentEyeTargetPOI;
		}



		void StartHeadMovement( Transform targetXform=null )
		{
			headLatency = 0;
			currentHeadTargetPOI = targetXform;
			nextHeadTargetPOI = null;

			Vector3 localAngles = headTargetPivotXform.localEulerAngles;
			Vector3 targetLocalAngles = Quaternion.LookRotation( headParentXform.InverseTransformPoint( GetCurrentHeadTargetPos() ), headParentXform.up ).eulerAngles;
						targetLocalAngles = new Vector3(	LimitVerticalHeadAngle(targetLocalAngles.x),
																			LimitHorizontalHeadAngle(targetLocalAngles.y),
																			0 );

			float horizDistance = Mathf.Abs(Mathf.DeltaAngle(localAngles.y, targetLocalAngles.y));
			float vertDistance = Mathf.Abs(Mathf.DeltaAngle(localAngles.x, targetLocalAngles.x));
			bool isQuickMove = headSpeed == HeadSpeed.Fast;

			if ( headTweenMethod == HeadTweenMethod.SmoothDamping )
			{
				const float d1fast = 0.38746871f;
				const float d2fast = 0.00741433f;
				const float d1slow = 0.58208538f;
				const float d2slow = 0.01056395f;
				float d1 = isQuickMove ? d1fast : d1slow;
				float d2 = isQuickMove ? d2fast : d2slow;
				headHorizDuration = d1 + d2 * horizDistance;
				headVertDuration = d1 + d2 * vertDistance;

				const float m1fast = 33.42039746f;
				const float m2fast = 2.58679992f;
				const float m1slow = 19.79938085f;
				const float m2slow = 1.6078972f;
				float m1 = isQuickMove ? m1fast : m1slow;
				float m2 = isQuickMove ? m2fast : m2slow;
				headMaxSpeedHoriz = m1 + m2 * horizDistance;
				headMaxSpeedVert = m1 + m2 * vertDistance;

				float mod = (isQuickMove ? 1.3f : 1.0f) * headSpeedModifier;
				headMaxSpeedHoriz *= mod;
				headMaxSpeedVert *= mod;
				headHorizDuration /= mod;
				headVertDuration /= mod;

				startHeadHorizDuration = headHorizDuration;
				startHeadVertDuration = headVertDuration;
				startHeadMaxSpeedHoriz = headMaxSpeedHoriz;
				startHeadMaxSpeedVert = headMaxSpeedVert;
			}
			else if ( headTweenMethod == HeadTweenMethod.CriticalDamping )
			{
				isHeadTracking = false;
				headTrackingFactor = 1;
			}

			timeOfHeadMovementStart = Time.time;

			maxHeadHorizSpeedSinceSaccadeStart = maxHeadVertSpeedSinceSaccadeStart = 0;

			if ( currentEyeTargetPOI == null && currentTargetLeftEyeXform == null )
				currentEyeTargetPOI = currentHeadTargetPOI;
		}




		void Update()
		{
			hasLateUpdateRunThisFrame = false;
			hasCheckedIdleLookTargetsThisFrame = false;

			if ( false == isInitialized )
				return;

			CheckLatencies();
		}


				// If using FinalIK, this is supposed to be called before
				// the head is oriented, because it sets the head target.
		public void Update1()
		{
			if ( Time.frameCount == lastFrameOfUpdate1 )
			{
				Debug.LogWarning("Update1 is suppsed to be called only once per frame");
				return;
			}
			lastFrameOfUpdate1 = Time.frameCount;
			
			hasLateUpdateRunThisFrame = true;

			if ( false == isInitialized )
				return;

			if ( lookTarget == LookTarget.StraightAhead )
				return;
			else if ( lookTarget == LookTarget.LookingAroundIdly )
			CheckIdleLookTargets();

			if ( currentHeadTargetPOI == null && currentTargetLeftEyeXform == null )
			{
				if ( OnTargetDestroyed != null )
					OnTargetDestroyed();

				return;
			}


			#if USE_FINAL_IK
				if ( headControl == HeadControl.FinalIK )
				{
					float targetIKWeight = (lookTarget == LookTarget.StraightAhead || lookTarget == LookTarget.ClearingTargetPhase2 ||lookTarget == LookTarget.ClearingTargetPhase1 ) ? 0 : headWeight;
					ikWeight = Mathf.Lerp( ikWeight, targetIKWeight, Time.deltaTime);
					lookAtIK.solver.IKPositionWeight = ikWeight;
					lookAtIK.solver.IKPosition = headTargetPivotXform.TransformPoint( eyeDistanceScale * Vector3.forward );
				}
			#endif

			if ( headControl == HeadControl.Transform )
			{
				float targetIKWeight = (lookTarget == LookTarget.StraightAhead || lookTarget == LookTarget.ClearingTargetPhase2 ||lookTarget == LookTarget.ClearingTargetPhase1 ) ? 0 : headWeight;
				ikWeight = Mathf.Lerp( ikWeight, targetIKWeight, Time.deltaTime);
				Vector3 globalLookTarget = headTargetPivotXform.TransformPoint(eyeDistanceScale * Vector3.forward);
				Quaternion lookQ = Quaternion.LookRotation(globalLookTarget - headBoneNonMecanimXform.position, transform.up);
				Quaternion lerpedQ = Quaternion.Slerp(Quaternion.identity, lookQ, ikWeight);
				headBoneNonMecanimXform.rotation = lerpedQ * headBoneNonMecanimFromRootQ;
			}

			Transform trans = (currentEyeTargetPOI != null) ? currentEyeTargetPOI : currentTargetLeftEyeXform;
			if (trans != null && OnCannotGetTargetIntoView != null && false == CanGetIntoView(trans.TransformPoint(macroSaccadeTargetLocal)) && eyeLatency <= 0)
				OnCannotGetTargetIntoView();

			UpdateHeadMovement();

			if ( headControl != HeadControl.FinalIK && false == areUpdatedControlledExternally )
				Update2();
		}



				// If using FinalIK, this is supposed to be called after the head is oriented,
				// because it moves the eyes from the head orientation to their final look target
		public void Update2()
		{
			if ( Time.frameCount == lastFrameOfUpdate2 )
			{
				Debug.LogWarning("Update2 is suppsed to be called only once per frame");
				return;
			}
			lastFrameOfUpdate2 = Time.frameCount;
			
			if ( false == isInitialized || false == enabled )
				return;

			if ( lookTarget == LookTarget.StraightAhead )
				return;

			CheckMicroSaccades();
			CheckMacroSaccades();

			if ( headControl == HeadControl.FinalIK && false == hasLateUpdateRunThisFrame )
				Debug.LogError("Since the last update of Realistic Eye Movements (REM), FinalIK scripts must run after REM scripts. Please remove the REM scripts from the script execution order list or move them to run before FinalIK scripts. ");

			if ( controlData.eyeControl != ControlData.EyeControl.None )
				UpdateEyeMovement();
			UpdateBlinking();
			UpdateEyelids();

			if ( kDrawSightlinesInEditor )
				DrawSightlinesInEditor();

			if ( OnUpdate2Finished != null )
				OnUpdate2Finished();
		}



		void UpdateBlinking()
		{
			if ( blinkState != BlinkState.Idle )
			{
				blinkStateTime += Time.deltaTime;

				if ( blinkStateTime >= blinkDuration )
				{
					blinkStateTime = 0;

					if ( blinkState == BlinkState.Closing )
					{
						if ( isShortBlink )
						{
							blinkState = BlinkState.Opening;
							blinkDuration = (1/blinkSpeed) * (isShortBlink ? kBlinkOpenTimeShort : kBlinkOpenTimeLong);
							blink01 = 1;
						}
						else
						{
							blinkState = BlinkState.KeepingClosed;
							blinkDuration = (1/blinkSpeed) * kBlinkKeepingClosedTime;
							blink01 = 1;
						}
					}
					else if ( blinkState == BlinkState.KeepingClosed )
					{
						blinkState = BlinkState.Opening;
						blinkDuration = (1/blinkSpeed) * (isShortBlink ? kBlinkOpenTimeShort : kBlinkOpenTimeLong);
					}
					else if ( blinkState == BlinkState.Opening )
					{
						blinkState = BlinkState.Idle;
						float minTime = Mathf.Max( 0.1f, Mathf.Min(kMinNextBlinkTime, kMaxNextBlinkTime));
						float maxTime = Mathf.Max( 0.1f, Mathf.Max(kMinNextBlinkTime, kMaxNextBlinkTime));
						timeOfNextBlink = Time.time + Random.Range( minTime, maxTime);
						blink01 = 0;
					}
				}
				else
					blink01 = Utils.EaseSineIn(	blinkStateTime,
																blinkState == BlinkState.Closing ? 0 : 1,
																blinkState == BlinkState.Closing ? 1 : -1,
																blinkDuration );
			}
		
			if ( Time.time >= timeOfNextBlink && blinkState == BlinkState.Idle )
				Blink();
		}



		void UpdateEyelids()
		{
			if ( controlData.eyelidControl != ControlData.EyelidControl.None )
				controlData.UpdateEyelids( currentLeftEyeLocalEuler.x, currentRightEyeLocalEuler.x, blink01, eyelidsFollowEyesVertically );
		}



		void UpdateEyeMovement()
		{
			if ( lookTarget == LookTarget.ClearingTargetPhase2 )
			{
				if ( Time.time - timeOfEnteringClearingPhase >= 1 )
					lookTarget = LookTarget.StraightAhead;
				else
				{
					leftEyeAnchor.localRotation = lastLeftEyeLocalRotation = Quaternion.Slerp(lastLeftEyeLocalRotation, originalLeftEyeLocalQ, Time.deltaTime);
					rightEyeAnchor.localRotation = lastRightEyeLocalQ = Quaternion.Slerp(lastRightEyeLocalQ, originalRightEyeLocalQ, Time.deltaTime);
				}

				return;
			}

			if ( lookTarget == LookTarget.ClearingTargetPhase1 )
			{
				if ( Time.time - timeOfEnteringClearingPhase >= 2 )
				{
					lookTarget = LookTarget.ClearingTargetPhase2;
					timeOfEnteringClearingPhase = Time.time;
				}
			}
		
			bool isLookingAtFace = lookTarget == LookTarget.Face;
			bool shouldDoSocialTriangle =		isLookingAtFace &&
															faceLookTarget != FaceLookTarget.EyesCenter;
			Transform trans = (currentEyeTargetPOI != null) ? currentEyeTargetPOI : currentTargetLeftEyeXform;

			if ( trans == null )
				return;

			Vector3 eyeTargetGlobal = shouldDoSocialTriangle	? GetLookTargetPosForSocialTriangle( faceLookTarget )
																						: trans.TransformPoint(microSaccadeTargetLocal);

			//*** Prevent cross-eyes
			{
				Vector3 ownEyeCenter = GetOwnEyeCenter();
				Vector3 eyeCenterToTarget = eyeTargetGlobal - ownEyeCenter;
				float distance = eyeCenterToTarget.magnitude / eyeDistanceScale;
				float corrDistMax = isLookingAtFace ? 2f : 0.6f;
				float corrDistMin = isLookingAtFace ? 1.5f : 0.2f;
						
				if ( distance < corrDistMax )
				{
					float modifiedDistance = corrDistMin + distance * (corrDistMax-corrDistMin)/corrDistMax;
					modifiedDistance = crossEyeCorrection * (modifiedDistance-distance) + distance;
					eyeTargetGlobal = ownEyeCenter + eyeDistanceScale * modifiedDistance * (eyeCenterToTarget/distance);
				}
			}

			//*** After the eyes saccaded to the new POI, adjust eye duration and speed so they keep tracking the target quickly enough.
			{
				const float kEyeDurationForTracking = 0.005f;
				const float kEyeMaxSpeedForTracking = 600;

				float timeSinceLeftEyeHorizInitiatedMovementStop = Time.time-(timeOfEyeMovementStart + 1.5f * startLeftEyeHorizDuration);
				if ( timeSinceLeftEyeHorizInitiatedMovementStop > 0 )
				{
					leftHorizDuration = kEyeDurationForTracking + startLeftEyeHorizDuration/(1 + timeSinceLeftEyeHorizInitiatedMovementStop);
					leftMaxSpeedHoriz = kEyeMaxSpeedForTracking - startLeftEyeMaxSpeedHoriz/(1 + timeSinceLeftEyeHorizInitiatedMovementStop);
				}

				float timeSinceLeftEyeVertInitiatedMovementStop = Time.time-(timeOfEyeMovementStart + 1.5f * startLeftEyeVertDuration);
				if ( timeSinceLeftEyeVertInitiatedMovementStop > 0 )
				{
					leftVertDuration = kEyeDurationForTracking + startLeftEyeVertDuration/(1 + timeSinceLeftEyeVertInitiatedMovementStop);
					leftMaxSpeedVert = kEyeMaxSpeedForTracking - startLeftEyeMaxSpeedVert/(1 + timeSinceLeftEyeVertInitiatedMovementStop);
				}

				float timeSinceRightEyeHorizInitiatedMovementStop = Time.time-(timeOfEyeMovementStart + 1.5f * startRightEyeHorizDuration);
				if ( timeSinceRightEyeHorizInitiatedMovementStop > 0 )
				{
					rightHorizDuration = kEyeDurationForTracking + startRightEyeHorizDuration/(1 + timeSinceRightEyeHorizInitiatedMovementStop);
					rightMaxSpeedHoriz = kEyeMaxSpeedForTracking - startRightEyeMaxSpeedHoriz/(1 + timeSinceRightEyeHorizInitiatedMovementStop);
				}

				float timeSinceRightEyeVertInitiatedMovementStop = Time.time-(timeOfEyeMovementStart + 1.5f * startRightEyeVertDuration);
				if ( timeSinceRightEyeVertInitiatedMovementStop > 0 )
				{
					rightVertDuration = kEyeDurationForTracking + startRightEyeVertDuration/(1 + timeSinceRightEyeVertInitiatedMovementStop);
					rightMaxSpeedVert = kEyeMaxSpeedForTracking - startRightEyeMaxSpeedVert/(1 + timeSinceRightEyeVertInitiatedMovementStop);
				}
			}


					Vector3 desiredLeftEyeTargetAngles = Quaternion.LookRotation(eyesRootXform.InverseTransformDirection( eyeTargetGlobal - leftEyeAnchor.position )).eulerAngles;
					Vector3 leftEyeTargetAngles = new Vector3(controlData.ClampLeftVertEyeAngle(desiredLeftEyeTargetAngles.x),
																		ClampLeftHorizEyeAngle(desiredLeftEyeTargetAngles.y),
																		0);
					float deltaTime = Mathf.Max(0.0001f, Time.deltaTime);
					float headMaxSpeedHoriz = 4*maxHeadHorizSpeedSinceSaccadeStart * Mathf.Sign(headEulerSpeed.y);
					float headMaxSpeedVert = 4*maxHeadVertSpeedSinceSaccadeStart * Mathf.Sign(headEulerSpeed.x);

			currentLeftEyeLocalEuler = new Vector3(	controlData.ClampLeftVertEyeAngle(Mathf.SmoothDampAngle(	currentLeftEyeLocalEuler.x,
																																			leftEyeTargetAngles.x,
																																			ref leftCurrentSpeedX,
																																			leftVertDuration,
																																			Mathf.Max(headMaxSpeedVert, leftMaxSpeedVert),
																																			deltaTime)),
																		ClampLeftHorizEyeAngle(Mathf.SmoothDampAngle(	currentLeftEyeLocalEuler.y,
																																					leftEyeTargetAngles.y,
																																					ref leftCurrentSpeedY,
																																					leftHorizDuration,
																																					Mathf.Max(headMaxSpeedHoriz, leftMaxSpeedHoriz),
																																					deltaTime)),
																		leftEyeTargetAngles.z);

			leftEyeAnchor.localRotation = Quaternion.Inverse(leftEyeAnchor.parent.rotation) * eyesRootXform.rotation * Quaternion.Euler( currentLeftEyeLocalEuler ) * leftEyeRootFromAnchorQ;

					Vector3 desiredRightEyeTargetAngles = Quaternion.LookRotation(eyesRootXform.InverseTransformDirection( eyeTargetGlobal - rightEyeAnchor.position)).eulerAngles;
					Vector3 rightEyeTargetAngles = new Vector3(	controlData.ClampRightVertEyeAngle(desiredRightEyeTargetAngles.x),
																			ClampRightHorizEyeAngle(desiredRightEyeTargetAngles.y),
																			0);
			currentRightEyeLocalEuler= new Vector3( controlData.ClampRightVertEyeAngle(Mathf.SmoothDampAngle(	currentRightEyeLocalEuler.x,
																																			rightEyeTargetAngles.x,
																																			ref rightCurrentSpeedX,
																																			rightVertDuration,
																																			Mathf.Max(headMaxSpeedVert, rightMaxSpeedVert),
																																			deltaTime)),
																		ClampRightHorizEyeAngle(Mathf.SmoothDampAngle(currentRightEyeLocalEuler.y,
																																					rightEyeTargetAngles.y,
																																					ref rightCurrentSpeedY,
																																					rightHorizDuration,
																																					Mathf.Max(headMaxSpeedHoriz, rightMaxSpeedHoriz),
																																					deltaTime)),
																		rightEyeTargetAngles.z);

			rightEyeAnchor.localRotation = Quaternion.Inverse(rightEyeAnchor.parent.rotation) * eyesRootXform.rotation * Quaternion.Euler( currentRightEyeLocalEuler ) * rightEyeRootFromAnchorQ;

			lastLeftEyeLocalRotation = leftEyeAnchor.localRotation;
			lastRightEyeLocalQ = rightEyeAnchor.localRotation;

			//		const float kMaxAngleDiff = 5;
			//bool isTargetOutOfView =	Mathf.DeltaAngle(desiredLeftEyeTargetAngles.x, leftEyeTargetAngles.x) >= kMaxAngleDiff ||
			//										Mathf.DeltaAngle(desiredLeftEyeTargetAngles.y, leftEyeTargetAngles.y) >= kMaxAngleDiff ||
			//										Mathf.DeltaAngle(desiredRightEyeTargetAngles.x, rightEyeTargetAngles.x) >= kMaxAngleDiff ||
			//										Mathf.DeltaAngle(desiredRightEyeTargetAngles.y, rightEyeTargetAngles.y) >= kMaxAngleDiff;

			//if (isTargetOutOfView && OnTargetOutOfSight != null)
			//	OnTargetOutOfSight();
		}



		void UpdateHeadMovement()
		{
			if ( headControl == HeadControl.None )
				return;

			if ( ikWeight <= 0 )
				return;

			Vector3 localAngles = headTargetPivotXform.localEulerAngles;
			Vector3 targetLocalAngles = Quaternion.LookRotation( headParentXform.InverseTransformPoint( GetCurrentHeadTargetPos() ) ).eulerAngles;

			//*** Head jitter
			{
				if (useHeadJitter)
				{
					headJitterTime += Time.deltaTime * kHeadJitterFrequency;

					if (kHeadJitterAmount != 0.0f)
					{
						var r = new Vector3(
							Utils.Fbm(headJitterNoiseVectors[3] * headJitterTime, kHeadJitterOctave),
							Utils.Fbm(headJitterNoiseVectors[4] * headJitterTime, kHeadJitterOctave),
							Utils.Fbm(headJitterNoiseVectors[5] * headJitterTime, kHeadJitterOctave)
						);
						r = Vector3.Scale(r, headJitterRotationComponents) * kHeadJitterAmount * 2;
						targetLocalAngles += r;
					}
				}
			}

			targetLocalAngles = new Vector3(LimitVerticalHeadAngle(targetLocalAngles.x),
																LimitHorizontalHeadAngle(targetLocalAngles.y),
																0);

			//*** After the head moved to the new POI, adjust head duration so the head keeps tracking the target quickly enough.
			{
				if (headTweenMethod == HeadTweenMethod.SmoothDamping)
				{
					const float kHeadDurationForTracking = 0.1f;
					const float kHeadMaxSpeedForTracking = 150;
					float timeSinceHorizInitiatedHeadMovementStop = Time.time - (timeOfHeadMovementStart + 1.5f * startHeadHorizDuration);
					if (timeSinceHorizInitiatedHeadMovementStop > 0)
					{
						headHorizDuration = kHeadDurationForTracking + startHeadHorizDuration / (1 + timeSinceHorizInitiatedHeadMovementStop);
						headMaxSpeedHoriz = Mathf.Max(startHeadMaxSpeedHoriz, kHeadMaxSpeedForTracking) - startHeadMaxSpeedHoriz / (1 + timeSinceHorizInitiatedHeadMovementStop);
					}
					float timeSinceVertInitiatedHeadMovementStop = Time.time - (timeOfHeadMovementStart + 1.5f * startHeadVertDuration);
					if (timeSinceVertInitiatedHeadMovementStop > 0)
					{
						headVertDuration = kHeadDurationForTracking + startHeadVertDuration / (1 + timeSinceVertInitiatedHeadMovementStop);
						headMaxSpeedVert = Mathf.Max(startHeadMaxSpeedVert, kHeadMaxSpeedForTracking) - startHeadMaxSpeedVert / (1 + timeSinceVertInitiatedHeadMovementStop);
					}
				}
				else if ( headTweenMethod == HeadTweenMethod.CriticalDamping )
				{
				const float kMinAngleToStartTracking = 2;
				if ( false == isHeadTracking )
				{
					Vector3 tweenEuler = critDampTween.rotation.eulerAngles;
					isHeadTracking =	Mathf.Abs(Mathf.DeltaAngle(tweenEuler.x, targetLocalAngles.x)) < kMinAngleToStartTracking &&
												Mathf.Abs(Mathf.DeltaAngle(tweenEuler.y, targetLocalAngles.y)) < kMinAngleToStartTracking;
				}

				float headSpeedFactor = (headSpeed == HeadSpeed.Slow) ? 0.5f : 1.0f;
				float targetHeadTrackingFactor = isHeadTracking ? 5 : 1;
				headTrackingFactor = Mathf.Lerp(headTrackingFactor, targetHeadTrackingFactor, Time.deltaTime * 3);
				critDampTween.omega = headSpeedFactor * headSpeedModifier * headTrackingFactor * kHeadOmega;
			}
			}

			float deltaTime = Mathf.Max(0.0001f, Time.deltaTime);

			if ( headTweenMethod == HeadTweenMethod.SmoothDamping )
			{
				headTargetPivotXform.localEulerAngles = new Vector3( Mathf.SmoothDampAngle(	localAngles.x,
																																		targetLocalAngles.x,
																																		ref currentHeadVertSpeed,
																																		headVertDuration,
																																		headMaxSpeedVert,
																																		deltaTime),
																								Mathf.SmoothDampAngle(	localAngles.y,
																																		targetLocalAngles.y,
																																		ref currentHeadHorizSpeed,
																																		headHorizDuration,
																																		headMaxSpeedHoriz,
																																		deltaTime),
																								Mathf.SmoothDampAngle(	localAngles.z,
																																		targetLocalAngles.z,
																																		ref currentHeadZSpeed,
																																		headHorizDuration,
																																		headMaxSpeedHoriz,
																																		deltaTime));
			}
			else if (headTweenMethod == HeadTweenMethod.CriticalDamping)
			{
				Quaternion targetLocalRotation = Quaternion.Euler(targetLocalAngles);
				critDampTween.Step(targetLocalRotation);

				headTargetPivotXform.localEulerAngles = critDampTween.rotation.eulerAngles;
			}

			headEulerSpeed = (headTargetPivotXform.localEulerAngles - lastHeadEuler)/deltaTime;
			lastHeadEuler = headTargetPivotXform.localEulerAngles;
			maxHeadHorizSpeedSinceSaccadeStart = Mathf.Max(maxHeadHorizSpeedSinceSaccadeStart, Mathf.Abs(headEulerSpeed.y));
			maxHeadVertSpeedSinceSaccadeStart = Mathf.Max(maxHeadHorizSpeedSinceSaccadeStart, Mathf.Abs(headEulerSpeed.x));
		}

	}

}
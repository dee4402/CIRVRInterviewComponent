using System;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility.Events;
using Tobii.Gaming;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (Rigidbody))]
    [RequireComponent(typeof (CapsuleCollider))]
    public class RigidbodyFirstPersonController : MonoBehaviour
    {
        [Serializable]
        public class MovementSettings
        {
            public float ForwardSpeed = 8.0f;   // Speed when walking forward
            public float BackwardSpeed = 4.0f;  // Speed when walking backwards
            public float StrafeSpeed = 4.0f;    // Speed when walking sideways
            public float RunMultiplier = 2.0f;   // Speed when sprinting
	        public KeyCode RunKey = KeyCode.LeftShift;
            public float JumpForce = 30f;
            public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
            [HideInInspector] public float CurrentTargetSpeed = 8f;

#if !MOBILE_INPUT
            private bool m_Running;
#endif

            public void UpdateDesiredTargetSpeed(Vector2 input)
            {
	            if (input == Vector2.zero) return;
				if (input.x > 0 || input.x < 0)
				{
					//strafe
					CurrentTargetSpeed = StrafeSpeed;
				}
				if (input.y < 0)
				{
					//backwards
					CurrentTargetSpeed = BackwardSpeed;
				}
				if (input.y > 0)
				{
					//forwards
					//handled last as if strafing and moving forward at the same time forwards speed should take precedence
					CurrentTargetSpeed = ForwardSpeed;
				}
#if !MOBILE_INPUT
	            if (Input.GetKey(RunKey))
	            {
		            CurrentTargetSpeed *= RunMultiplier;
		            m_Running = true;
	            }
	            else
	            {
		            m_Running = false;
	            }
#endif
            }

#if !MOBILE_INPUT
            public bool Running
            {
                get { return m_Running; }
            }
#endif
        }


        [Serializable]
        public class AdvancedSettings
        {
            public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
            public float stickToGroundHelperDistance = 0.5f; // stops the character
            public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
            public bool airControl; // can the user control the direction that is being moved in the air
            [Tooltip("set it to 0.1 or more if you get stuck in wall")]
            public float shellOffset; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
        }


        public Camera standingCam;
        public Camera seatedCam;
        public Camera rightSide;
        public Canvas overlay;
        public MovementSettings movementSettings = new MovementSettings();
        public MouseLook mouseLook = new MouseLook(); //TODO: IF USING FOVE, MUST DYNAMICALLY DISABLE
        public AdvancedSettings advancedSettings = new AdvancedSettings();

        private bool seated = false;
        private float differenceX = 10.0f;
        private float differenceZ = 10.0f;

        private Rigidbody m_RigidBody;
        private CapsuleCollider m_Capsule;
        private float m_YRotation;
        private Vector3 m_GroundContactNormal;
        private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;
        private bool m_Seated = false;
        private float m_standCamVert;
        private float m_sitCamVert;
        private float m_camVertOffset = 0.5F;
        private Text m_sitStandLabel;
        private Vector3 m_seatedPositionLocked; //JOSH 06.25.2019
        private string m_txtSeated = "Press the spacebar to stand.";
        private string m_txtStanding = "Press the spacebar to have a seat.";
        public bool canMove = true;

        public float speedH = 2.0f;
        public float speedV = 2.0f;
        private float yaw = 180.0f;
        private float pitch = 0.0f;
        private KeyCode[] konamiKeyCode = { KeyCode.UpArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow,
                                        KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.B, KeyCode.A, KeyCode.Return};
        private bool tiny = false;
        private int k = 0;
        private bool hitTrigger = false;



        public Vector3 Velocity
        {
            get { return m_RigidBody.velocity; }
        }

        public bool Grounded
        {
            get { return m_IsGrounded; }
        }

        public bool Jumping
        {
            get { return m_Jumping; }
        }

        public bool Running
        {
            get
            {
 #if !MOBILE_INPUT
				return movementSettings.Running;
#else
	            return false;
#endif
            }
        }


        private void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();
            // mouseLook.Init (transform, standingCam.transform); //TODO: IF USING FOVE, MUST DYNAMICALLY DISABLE
            // m_standCamVert = standingCam.transform.position.y;
            // m_sitCamVert = m_standCamVert + m_camVertOffset;
            //m_sitStandLabel = GameObject.Find("Canvas/SitStandLabel").GetComponent<Text>();
            //m_sitStandLabel.text = m_txtStanding;
            m_seatedPositionLocked = new Vector3(229.0739F, -183.3779F, 272.4285F);
            seatedCam.rect = new Rect(0f, 0f, .70f, 1f);
            rightSide.rect = new Rect(0.70f, 0f, .30f, 1f);

            EventSystem.current.RegisterListener<PlayerBeginInterview>(OnInterviewBegin);
        }

        //void OnGUI()
        //{
        //    if (Input.GetKeyDown(KeyCode.UpArrow))
        //    {
        //        m_camVertOffset += 0.1F;
        //    }
        //    else if (Input.GetKeyDown(KeyCode.DownArrow) )
        //    {
        //        m_camVertOffset -= 0.1F;
        //    }
        //    Debug.Log("===> New vert cam offset = " + m_camVertOffset.ToString());
        //    cam.transform.position = m_standCamPosition + new Vector3(0, m_camVertOffset, 0);
        //}

        private void Update()
        {
            if(standingCam == null)
            {
                standingCam = gameObject.GetComponentInChildren<Camera>();
                //mouseLook.Init (transform, standingCam.transform); //TODO: IF USING FOVE, MUST DYNAMICALLY DISABLE
                m_standCamVert = standingCam.transform.position.y;
                m_sitCamVert = m_standCamVert + m_camVertOffset;
            }
           

            if (Input.GetKeyDown(konamiKeyCode[k]))//KeyCode.(konamiCode[k])))
            {
                if(konamiKeyCode[k].ToString() == "Return")
                {
                    konamiCode();
                    k = 0;
                }
                k++;
            }
            
        }

        private void Sit()
        {

            Vector3 currCamPos = standingCam.transform.position;
            standingCam.transform.position = new Vector3(currCamPos.x,currCamPos.y - m_camVertOffset, currCamPos.z);
            standingCam.enabled = false;
            seatedCam.enabled = true;
            rightSide.enabled = true;
            //Michael Breen 7/15/19: canvas changes render to camera to ensure ui elements do not cover right side
            overlay.renderMode = RenderMode.ScreenSpaceCamera;
            overlay.worldCamera = seatedCam;
            overlay.planeDistance = .5f;
            standingCam.gameObject.GetComponent<AudioListener>().enabled = false;
            seatedCam.gameObject.GetComponent<AudioListener>().enabled = true;
            TobiiAPI.SetCurrentUserViewPointCamera(seatedCam);
        }
        
        private void Stand()
        {
            Vector3 currCamPos = standingCam.transform.position;
            standingCam.transform.position = new Vector3(currCamPos.x, currCamPos.y + m_camVertOffset, currCamPos.z);
            standingCam.enabled = true;
            //Michael Breen 7/15/19: canvas changes render to overlay
            overlay.renderMode = RenderMode.ScreenSpaceOverlay;
            seatedCam.enabled = false;
            rightSide.enabled = false;
            standingCam.gameObject.GetComponent<AudioListener>().enabled = true;
            seatedCam.gameObject.GetComponent<AudioListener>().enabled = false;
            TobiiAPI.SetCurrentUserViewPointCamera(standingCam);
        }
        
        private void FixedUpdate()
        {
            GroundCheck();
            Vector2 input = m_Seated ? Vector2.zero : GetInput();

            if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || m_IsGrounded))
            {
                // always move along the camera forward as it is the direction that it being aimed at
                Vector3 desiredMove = standingCam.transform.forward*input.y + standingCam.transform.right*input.x;
                desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

                desiredMove.x = desiredMove.x*movementSettings.CurrentTargetSpeed;
                desiredMove.z = desiredMove.z*movementSettings.CurrentTargetSpeed;
                desiredMove.y = desiredMove.y*movementSettings.CurrentTargetSpeed;
                if (m_RigidBody.velocity.sqrMagnitude <
                    (movementSettings.CurrentTargetSpeed*movementSettings.CurrentTargetSpeed))
                {
                    m_RigidBody.AddForce(desiredMove*SlopeMultiplier(), ForceMode.Impulse);
                }
            }

            if (m_IsGrounded)
            {
                m_RigidBody.drag = 5f;

                if (m_Jump)
                {
                    m_RigidBody.drag = 0f;
                    m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
                    m_RigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                    m_Jumping = true;
                }

                if (!m_Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && m_RigidBody.velocity.magnitude < 1f)
                {
                    m_RigidBody.Sleep();
                }
            }
            else
            {
                m_RigidBody.drag = 0f;
                if (m_PreviouslyGrounded && !m_Jumping)
                {
                    StickToGroundHelper();
                }
            }
            m_Jump = false;
        }


        private float SlopeMultiplier()
        {
            float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
            return movementSettings.SlopeCurveModifier.Evaluate(angle);
        }

        void OnTriggerEnter(Collider col)
        {
            var GO = gameObject.GetComponentsInChildren<Transform>();
            //Has the user sat down
            // if (col.name == "InterviewTrigger")
            // {
            //     canMove = false;
            // }
            //Has the user entered the room
            if (col.name.Contains("Doorway"))
            {                
                //I disable the doorway collider to stop her from greeting every time they enter the room, temp fix : Michael
                col.enabled = false;
                //If the user is wearing a fove then don't allow movement
                // if(standingCam.name.Contains("Fove"))
                // {
                    canMove = false;
                //}
                foreach(Transform x in GO)
                {
                    if(x.gameObject.activeSelf)
                    {
                        EventSystem.current.FireEvent(new PlayerEnteredInterviewRoom(""));
                        break;
                    }
                }
                
            }
        }


        private void StickToGroundHelper()
        {
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   ((m_Capsule.height/2f) - m_Capsule.radius) +
                                   advancedSettings.stickToGroundHelperDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
                {
                    m_RigidBody.velocity = Vector3.ProjectOnPlane(m_RigidBody.velocity, hitInfo.normal);
                }
            }
        }


        private Vector2 GetInput()
        {
            if (!canMove)
                return Vector2.zero;
            Vector2 input = new Vector2
                {
                    x = CrossPlatformInputManager.GetAxis("Horizontal"),
                    y = CrossPlatformInputManager.GetAxis("Vertical")
                };
			movementSettings.UpdateDesiredTargetSpeed(input);
            return input;
        }


        private void RotateView()
        {
            //avoids the mouse looking if the game is effectively paused
            if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;
            if (seated == true) return;

            differenceX = standingCam.transform.position.x - seatedCam.transform.position.x;
            differenceZ = standingCam.transform.position.z - seatedCam.transform.position.z;

            if (hitTrigger || !canMove)
            {
                seated = true;
            }
            else { seated = false; }

            // get the rotation before it's changed
            float oldYRotation = transform.eulerAngles.y;

            //if (!m_Seated) //JOSH: 03.12.2019
            {
                mouseLook.LookRotation(transform, standingCam.transform);
            }//TODO: IF USING FOVE, MUST DYNAMICALLY DISABLE
             //Michael: 6/26/2019

            //Determines where the camera should rotate to on the x and y axis
            yaw += speedH * Input.GetAxis("Mouse X");
            pitch -= speedV * Input.GetAxis("Mouse Y");
            //Doesn't let the camera turn all of the way around on the x axis
            pitch = Mathf.Clamp(pitch, -60f, 90f);
            //Performs the transformation, must be in update 
            standingCam.transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
            

            if (m_IsGrounded || advancedSettings.airControl)
            {
                // Rotate the rigidbody velocity to match the new direction that the character is looking
                Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
                m_RigidBody.velocity = velRotation*m_RigidBody.velocity;
            }
        }

        /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
        private void GroundCheck()
        {
            m_PreviouslyGrounded = m_IsGrounded;
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   ((m_Capsule.height/2f) - m_Capsule.radius) + advancedSettings.groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_IsGrounded = true;
                m_GroundContactNormal = hitInfo.normal;
            }
            else
            {
                m_IsGrounded = false;
                m_GroundContactNormal = Vector3.up;
            }
            if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping)
            {
                m_Jumping = false;
            }
        }
        private void konamiCode()
        {
            //Vector3 y = new Vector3(1f, -.72f, 1f);
            if (tiny == false)
            {
                standingCam.transform.position = new Vector3(standingCam.transform.position.x, standingCam.transform.position.y - 1.3f, standingCam.transform.position.z);
                //transform.position = new Vector3(transform.position.x, transform.position.y - 0.786f, transform.position.z);
                transform.localScale = new Vector3(1f, .15f, 1f);
                m_Capsule.radius = .11f;
                tiny = true;
                
            }
            else if(tiny == true)
            {
                standingCam.transform.position = new Vector3(standingCam.transform.position.x, standingCam.transform.position.y + 1.3f, standingCam.transform.position.z);
                //transform.position = new Vector3(transform.position.x, transform.position.y + 0.786f, transform.position.z);
                transform.localScale = new Vector3(1f, 1f, 1f);
                m_Capsule.radius = 1f;
                tiny = false;
            }
        }
        public void OnInterviewBegin(PlayerBeginInterview e)
        {
            hitTrigger = true;
        }
    }
}

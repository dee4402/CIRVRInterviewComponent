using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using UnityStandardAssets.Utility.Events;
using Valve.VR.Extras;
using UnityEngine.UI;
using System.Linq;


public class TouchpadMove : MonoBehaviour
{
   private Vector2 trackpad;
    private Vector3 moveDirection;
    private int GroundCount;
    private CapsuleCollider CapCollider;

    public SteamVR_Input_Sources MovementHand;//Set Hand To Get Input From
    public SteamVR_Input_Sources Interact;// = SteamVR_Input_Sources.Any;
    public SteamVR_Input_Sources SnapLeft;
    public SteamVR_Input_Sources SnapRight;
    public SteamVR_Action_Vector2 TrackpadAction;
    public SteamVR_Action_Boolean TriggerAction;
    public SteamVR_Action_Boolean SnapLeftAction;
    public SteamVR_Action_Boolean SnapRightAction;
    public SteamVR_LaserPointer laserPointer;

    public float MovementSpeed;
    public float Deadzone;//the Deadzone of the trackpad. used to prevent unwanted walking.
    public GameObject Head;
    private Rigidbody RBody;
    public GameObject AxisHand;//Hand Controller GameObject
    public PhysicMaterial NoFrictionMaterial;
    public PhysicMaterial FrictionMaterial;
    private Transform RigHolder, camHolder;
    private bool interviewStarted, toChair, toBoard, noMove, whiteboardStarted, dialogBegan;
    private float assumedHeadHeight = 1.5f;

    public Dictionary<string, Transform> anchors = new Dictionary<string, Transform>();
    //184.5 and 185

    void OnEnable ()
    {
        //UnityEngine.XR.InputTracking.disablePositionalTracking = true;
        //Adds actions for the trigger
        if (TriggerAction != null)
        {
            TriggerAction.AddOnStateDownListener(TriggerDown, Interact);
        }
        if(SnapLeftAction != null && SnapRightAction != null)
        {
            SnapLeftAction.AddOnStateDownListener(TurnLeft, SnapLeft);
            SnapRightAction.AddOnStateDownListener(TurnRight, SnapRight);
        }
    }
    private void Start()
    {
        //Grabs all needed Components and transforms
        RigHolder = gameObject.GetComponentInParent<Transform>();
        camHolder = gameObject.GetComponentsInChildren<Transform>().Where(x => x.name.Contains("Holder")).Single();
        
        laserPointer = gameObject.GetComponentInChildren<SteamVR_LaserPointer>();
        laserPointer.PointerIn += PointerInside;
        laserPointer.PointerOut += PointerOutside;
        laserPointer.PointerClick += PointerClick;
        
        CapCollider = GetComponent<CapsuleCollider>();
        var allLocations = GameObject.FindGameObjectsWithTag("Anchor");
        foreach(GameObject x in allLocations)
        {
            anchors.Add(x.name, x.transform);
        }
        foreach(KeyValuePair<string, Transform> x in anchors)
        {
            if(!x.Key.Contains("Base"))// || !x.Key.Contains("Right"))
            {
                anchors[x.Key].position = new Vector3(anchors["InterviewViewAnchorBase"].position.x + anchors[x.Key].localPosition.x, 
                                                      anchors["InterviewViewAnchorBase"].position.y + anchors[x.Key].localPosition.y,
                                                      anchors["InterviewViewAnchorBase"].position.z + anchors[x.Key].localPosition.z);
            }
        }
        RBody = GetComponent<Rigidbody>();

        EventSystem.current.RegisterListener<EndWhiteBoardSection>(EndWBQ);
        EventSystem.current.RegisterListener<BeginWhiteBoard>(BeginWB);
        EventSystem.current.RegisterListener<BeginDialog>(BeginDialog);
    }

    void Update()
    {
        //Forces the player to maintain a specific height while standing and sitting
        MaintainCamDimensions(assumedHeadHeight);

        //Doorway trigger hit and vive trigger hit then fade to black and fade back after wards
        if(toChair)
        {   if(!interviewStarted)
                {
                    StartCoroutine(WaitToFade("interviewView", 1f));
                    assumedHeadHeight = 1.2f;
                    toChair = false;
                }
        }
        else if(toBoard)
        {
            if(!whiteboardStarted)
            {
                StartCoroutine(WaitToFade("whiteboardView", 0.5f));
                assumedHeadHeight = 1.75f;
                toBoard = false;
            }
        }

        //Stops movement once the user hits the doorway trigger
        if(noMove == false)
        {
            updateInput();
            updateCollider();

            moveDirection = Quaternion.AngleAxis(Angle(trackpad) + AxisHand.transform.localRotation.eulerAngles.y, Vector3.up) * transform.forward;//get the angle of the touch and correct it for the rotation of the controller
            Vector3 velocity = new Vector3(0,0,0);
            if (trackpad.magnitude > Deadzone)
            {//make sure the touch isn't in the deadzone and we aren't going to fast.
                CapCollider.material = NoFrictionMaterial;
                velocity = moveDirection;
                RBody.AddForce(velocity.x*MovementSpeed - RBody.velocity.x, 0, velocity.z*MovementSpeed - RBody.velocity.z, ForceMode.VelocityChange);
            }
            
            else if(GroundCount > 0)
            {
                CapCollider.material = FrictionMaterial;
            }
        }
        else
        {
            CapCollider.material = FrictionMaterial;
        }

    }

    private void MaintainCamDimensions(float recommendedHeadHeight = 1.75f)
    {
        float factor = 0f;
        var heightDiff = (Head.transform.localPosition.y + camHolder.localPosition.y) - recommendedHeadHeight;
        if((Head.transform.localPosition.y + camHolder.transform.localPosition.y) > recommendedHeadHeight)
        {
            Debug.Log($"assumed height {recommendedHeadHeight}");
            if(heightDiff >= .1f)
            {
                factor = camHolder.transform.localPosition.y - .1f;
            }
            else
            {
                factor = camHolder.transform.localPosition.y - .0001f;
            }
        }
        else if ((Head.transform.localPosition.y + camHolder.transform.localPosition.y) < recommendedHeadHeight)
        {
            Debug.Log($"assumed height in else if {recommendedHeadHeight}");
            if(heightDiff <= -.1f)
            {
                factor = camHolder.transform.localPosition.y + .1f;
            }
            else
            {
                factor = camHolder.transform.localPosition.y + .0001f;
            }
        }
        //if(!toChair) Why did I do this? Comment your stuff idiot
        {
            camHolder.transform.localPosition = new Vector3(-1 * Head.transform.localPosition.x, 
                                                            factor                               , 
                                                            -1 * Head.transform.localPosition.z);
        }
    }

    private void TriggerDown(SteamVR_Action_Boolean TriggerAction, SteamVR_Input_Sources Interact)
    {
        //Submit responses for when vive controllers are being used, send event from conversation manager for bool
        if(dialogBegan) {
            EventSystem.current.FireEvent(new CatchViveTriggerHit("User has hit trigger to submit response to a question.", true));
        }
    }

    private void BeginDialog(BeginDialog e) {
        dialogBegan = e.dialogBegan;
        e.dialogBegan = false;
    }

    //User can use the trackpad to snap left and right
    private void TurnLeft(SteamVR_Action_Boolean SnapLeftAction, SteamVR_Input_Sources SnapLeft)
    {
        RigHolder.eulerAngles = new Vector3(0, RigHolder.eulerAngles.y - 25, 0);
    }

    private void TurnRight(SteamVR_Action_Boolean SnapRightAction, SteamVR_Input_Sources SnapRight)
    {
        RigHolder.eulerAngles = new Vector3(0, RigHolder.eulerAngles.y + 25, 0);
    }

    //Activates the laser pointer when pointed at whiteboard
    // * Have them activate on UI * Michael
    public void PointerInside(object sender, PointerEventArgs e)
    {
        Debug.Log(e.target.transform.root.name.Contains("Whiteboard"));
        if(e.target.name.Contains("Whiteboard") || e.target.name.Contains("Button") || e.target.name.Contains("Toggle") || e.target.name.Contains("Scale") ||
            e.target.transform.root.name.Contains("Whiteboard"))
        {
            laserPointer.thickness = .002f;
        }
        // if(e.target.name.Contains("Toggle"))
        // {
        //     ColorBlock temp = e.target.GetComponent<Toggle>().colors;
        //     temp.normalColor = Color.grey;
        //     e.target.GetComponent<Button>().colors = temp;
        // }
        if(e.target.name.Contains("Button"))
        {
            ColorBlock temp = e.target.GetComponent<Button>().colors;
            temp.normalColor = Color.grey;
            e.target.GetComponent<Button>().colors = temp;
        }
    }

    //Turns of laser when it leaves the whiteboard
    public void PointerOutside(object sender, PointerEventArgs e)
    {
        if(!e.target.name.Contains("Whiteboard") || !e.target.name.Contains("Button") || !e.target.name.Contains("Toggle") || !e.target.name.Contains("Scale"))
        {
            laserPointer.thickness = 0;
        }
        // if(e.target.name.Contains("Toggle"))
        // {
        //     ColorBlock temp = e.target.GetComponent<Toggle>().colors;
        //     temp.normalColor = Color.white;
        //     e.target.GetComponent<Button>().colors = temp;
        // }
        if(e.target.name.Contains("Button"))
        {
            ColorBlock temp = e.target.GetComponent<Button>().colors;
            temp.normalColor = Color.white;
            e.target.GetComponent<Button>().colors = temp;
            
        }
    }

    //Activates toggles and buttons when laser pointer is pointed at them
    public void PointerClick(object sender, PointerEventArgs e)
    {
        Debug.Log($"e name {e.target.name} | {e.target.gameObject.name}");
        if(e.target.name.Contains("Toggle"))
        {
            if(e.target.GetComponent<Toggle>().isOn == false)
            {
                e.target.gameObject.GetComponent<Toggle>().isOn = true;
            }
            else
            {
                e.target.gameObject.GetComponent<Toggle>().isOn = false;
            }
        }
        if(e.target.name.Contains("Button"))
        {
            e.target.GetComponent<Button>().onClick.Invoke();
        }
        if(e.target.name.Contains("For Vive")) {
            int lastIndex = System.Int32.Parse(e.target.name[(e.target.name.Length - 1)].ToString());
            e.target.gameObject.GetComponentInParent<Slider>().value = lastIndex;
        }
    }

    //Uses SteamVR to fade
    private IEnumerator WaitToFade(string view, float timeBeforeStart)
    {
        yield return new WaitForSeconds(timeBeforeStart);
        SteamVR_Fade.View(Color.black, 0.05f);
        yield return new WaitForSeconds(0.7f);
        RBody.position = anchors[view].position;
        yield return new WaitForSeconds(1);
        SteamVR_Fade.View(Color.clear, 1);
        if(view.Contains("interview"))
        {
            interviewStarted = true;
        }
        else if(view.Contains("whiteboard"))
        {
            whiteboardStarted = true;
        }
    }

    //Calculates Angle for the rigidbody
    public static float Angle(Vector2 p_vector2)
    {
        if (p_vector2.x < 0)
        {
            return 360 - (Mathf.Atan2(p_vector2.x, p_vector2.y) * Mathf.Rad2Deg * -1);
        }
        else
        {
            return Mathf.Atan2(p_vector2.x, p_vector2.y) * Mathf.Rad2Deg;
        }
    }

    //Moves the collider 
    private void updateCollider()
    {
        CapCollider.height = Head.transform.localPosition.y;
        CapCollider.center = new Vector3(0, Head.transform.localPosition.y / 2, 0);
    }

    //Gets the input from the movement trackpad
    private void updateInput()
    {
        trackpad = TrackpadAction.GetAxis(MovementHand);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GroundCount++;
    }
    private void OnCollisionExit(Collision collision)
    {
        GroundCount--;
    }
    //These events move between the whiteboard
    private void BeginWB(BeginWhiteBoard e)
    {
        Debug.Log("Fade to whiteboard ");
        toChair = false;
        toBoard = true;
    }
    private void EndWBQ(EndWhiteBoardSection e)
    {
        StartCoroutine(WaitToFade("interviewView", 0.5f));
        assumedHeadHeight = 1.2f;
        toChair = true;
        toBoard = false;
        whiteboardStarted = false;
    }    
    //Stops movement and moves player to chair with trigger click
    void OnTriggerEnter(Collider col)
    {
        if (col.name.Contains("Doorway"))
        {
            toChair = true;
            noMove = true;
        }  
    }
}

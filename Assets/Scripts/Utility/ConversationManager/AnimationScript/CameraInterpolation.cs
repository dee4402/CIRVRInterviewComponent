using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility.Events;

public class CameraInterpolation : MonoBehaviour
{
    public Transform[] views;
    public float transitionSpeed;
    Transform currentView;
    private Camera cam;
    private bool beginMove = false;
    private float wait = 0f;
    
    GameObject body;
    private bool allowMove = true, inChair = false, inter = false, seatedHap = false, hitTrigger = false, moveToWhiteBoard = false;
    private Vector3 currentAngle;
    private float differenceY = 10f;
    private float delay = 0f;
    

    // Start is called before the first frame update
    void Start()
    {
        cam = transform.GetComponent<Camera>();
        currentView = cam.transform;
       // cam.fieldOfView = 45f;
        hitTrigger = true;

        body = GameObject.FindGameObjectWithTag("Player");
        //EventSystem.current.RegisterListener<BeginWhiteBoard>(onBeginWhiteboard);
        //EventSystem.current.RegisterListener<PlayerEnteredInterviewRoom>(PlayerEnteredInterviewRoom);
    }
    void Update()
    {
        if (hitTrigger )
        {
            currentView = views[0];
            allowMove = false;
            inChair = true;
            inter = true;
            seatedHap = true;
            transitionSpeed = 1f;
            cam.fieldOfView = 45f;
            //StartCoroutine(SmoothFOV(45f));
            //hitTrigger = false;
        }



        if (!allowMove) {
            wait += Time.deltaTime;
        }

        //differenceY = views[2].position.y - transform.position.y;
        if (allowMove == true && differenceY < .1 && seatedHap == true) {
            inter = false;
        }
        differenceY = views[0].position.y - transform.position.y;
        
        if (inter)
        {
            //if (delay < 1.5f)
               // body.transform.position = Vector3.Lerp(body.transform.position, views[0].position, Time.deltaTime * transitionSpeed);
            //else
            //{
                transform.position = Vector3.Lerp(transform.position, currentView.position,  transitionSpeed);
                //Code for rotating
                currentAngle = new Vector3(
                    Mathf.LerpAngle(transform.rotation.eulerAngles.x, currentView.transform.rotation.eulerAngles.x,  transitionSpeed),
                    Mathf.LerpAngle(transform.rotation.eulerAngles.y, currentView.transform.rotation.eulerAngles.y,  transitionSpeed),
                    Mathf.LerpAngle(transform.rotation.eulerAngles.z, currentView.transform.rotation.eulerAngles.z,  transitionSpeed));
                    transform.eulerAngles = currentAngle;
            //}
            //delay += Time.deltaTime;
        }

    }
    public void PlayerEnteredInterviewRoom(PlayerEnteredInterviewRoom e) {
        //hitTrigger = true;

    }

    public void onBeginWhiteboard(BeginWhiteBoard e)
    {
        moveToWhiteBoard = !moveToWhiteBoard;
    }
    private IEnumerator SmoothFOV(float desiredFOV)
    {
        cam.fieldOfView = desiredFOV;
        //float change = .25f;
        yield return new WaitForSeconds(.01f);
        /*if(cam.fieldOfView < desiredFOV)
        {
            cam.fieldOfView += change;
        }
        else if(cam.fieldOfView > desiredFOV)
        {
            cam.fieldOfView -= change;
        }
        else
        {
            yield break;
        }*/
        //StartCoroutine(SmoothFOV(desiredFOV));
    }
}

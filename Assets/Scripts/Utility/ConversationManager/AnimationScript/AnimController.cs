/*Author: Michael Breen + whoever touches this next
 * Script name: AnimController.cs //Will most likely become an interviewer specific anim controller script
 * This is the animation controller script currently for the interviewer, possibly rename to interviewer controller
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityStandardAssets.Utility.Config;
using RealisticEyeMovements;
using System.Text.RegularExpressions;

public static class MyArrayExtensions
{
    public static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
    {
        for (var i = 0; i < (float)array.Length / size; i++)
        {
            yield return array.Skip(i * size).Take(size);
        }
    }
}

public class AnimController : MonoBehaviour
{

    private System.Random random;
    private Dictionary<string, Animator> anims = new Dictionary<string, Animator>();
    //Hand animation times
    private Dictionary<string, List<Tuple<int, float>>> handAnimTimes = new Dictionary<string, List<Tuple<int, float>>>();
    
    //All of the deictic lists
    private List<Tuple<int, float>> personSpecificDeicticList = new List<Tuple<int, float>>();  //you, he, it, his, her, theirs
    private List<Tuple<int, float>> personGeneralDeicticList = new List<Tuple<int, float>>();   //him, her, it, they, them, their

    private List<Tuple<int, float>> selfSpecificDeicticList = new List<Tuple<int, float>>(); // me, my, mine, i
    private List<Tuple<int, float>> selfGeneralDeicticList = new List<Tuple<int, float>>();  // we, our, us

    private List<Tuple<int, float>> nomDeicticList = new List<Tuple<int, float>>();
    private List<Tuple<int, float>> presentDeicticList = new List<Tuple<int, float>>();
    private List<Tuple<int, float>> pastDeicticList = new List<Tuple<int, float>>();
    private Animator interviewer;

    //Used for blending faces so that they can smile
    private SkinnedMeshRenderer face;
    private SkinnedMeshRenderer eyed;
    private SkinnedMeshRenderer bodied;
    private Mesh faceMesh;
    private Material holdMat;
    private LookTargetController eyeAndHead;

    // Use this for initialization
    void Start()
    {
        List<string> addedItems = new List<string>();
        AnimationClip[] clips;

        //Grabs components related to new FacePunch body
        face = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>().Where(r => r.tag == "Face").FirstOrDefault();
        faceMesh = GetComponentsInChildren<SkinnedMeshRenderer>().Where(r=> r.tag == "Face").FirstOrDefault().sharedMesh;
        eyed = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>().Where(r => r.tag == "Eye").FirstOrDefault();
        bodied = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>().Where(r => r.tag == "Body").FirstOrDefault();
        holdMat = eyed.material;
        eyeAndHead = GetComponent<LookTargetController>();

        StartCoroutine(Blink());

        random = new System.Random();
        Animator[] animators = FindObjectsOfType<Animator>();
        //Finds animators of scene
        foreach (Animator anim in animators)
        {
            anims.Add(anim.name, anim);
        }
        interviewer = anims[ConfigInfo.envSettings.interviewer];

        //Finds clips related to hand gestures
        clips = anims[ConfigInfo.envSettings.interviewer].runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {       

            var clipArr = clip.name.Split('_');
            if (clipArr.Length > 1 && clipArr[1] == "gesture")
            {
                int clipNum = Int32.Parse(clip.name[clip.name.Length - 1].ToString());
                var holder = new Tuple<int, float>(clipNum, clip.length);
                string caseCheck = $"{clipArr[2]}_{clipArr[3]}";
                switch (caseCheck)
                {
                    case "person_specific":
                        personSpecificDeicticList.Add(holder);
                        break;
                    case "self_specific":
                        selfSpecificDeicticList.Add(holder);
                        break;
                    case "person_general":
                        personGeneralDeicticList.Add(holder);
                        break;
                    case "self_general":
                        selfGeneralDeicticList.Add(holder);
                        break;
                    case "nominative_deictic":
                        nomDeicticList.Add(holder);
                        break;
                    case "present_deictic":
                        presentDeicticList.Add(holder);
                        break;
                    case "past_deictic":
                        pastDeicticList.Add(holder);
                        break;
                    default:
                        break;
                }
            }
        }
        handAnimTimes.Add("person_specific_deictic", personSpecificDeicticList);
        handAnimTimes.Add("self_specific_deictic", selfSpecificDeicticList);
        handAnimTimes.Add("person_general_deictic", personGeneralDeicticList);
        handAnimTimes.Add("self_general_deictic", selfGeneralDeicticList);

        handAnimTimes.Add("nominative_deictic", nomDeicticList);
        handAnimTimes.Add("present_deictic", presentDeicticList);
        handAnimTimes.Add("past_deictic", pastDeicticList);
    }

    IEnumerator Blink()
    {
        yield return new WaitForSeconds(.1f);
        eyed.material.color = Color.white;
        eyed.material = holdMat;
        yield return new WaitForSeconds(GetRandomTimeInterval(4, 9));
        eyed.material = bodied.material;
        eyed.material.color = Color.grey;
        StartCoroutine(Blink());
    }

    /// <summary>
    /// Play Lip and Stop Lip control lip sync for audio based on decibel levels
    /// </summary>
    /// <param name="goName"></param>
    /// <returns>Returns name of speaker</returns>
    public string PlayLipMvmtAnim(string goName)
    {
        if (anims.ContainsKey(goName))
        {
            anims[goName].SetTrigger("interviewerSpeechBegin");
            anims[goName].ResetTrigger("interviewerSpeechEnd");
            return goName;
        }
        return "";
    }

    public void StopLipAnim(string goName)
    {
        if (anims.ContainsKey(goName))
        {
            anims[goName].SetTrigger("interviewerSpeechEnd");
            anims[goName].ResetTrigger("interviewerSpeechBegin");
        }
    }

    public void GestureToWhiteboard()
    {
        anims[ConfigInfo.envSettings.interviewer].SetTrigger("WBQ");
    }

    /// <summary>
    /// Stop and Start Interruption controls what animations the interrupted are to do 
    /// </summary>
    /// <param name="goName"></param>
    public void StopDuringInterruption(string goName, string sourceOfInterruption)
    {
        if (anims.ContainsKey(goName))
        {
            anims[goName].SetBool("Interruption", true);
            anims[goName].SetTrigger(sourceOfInterruption);
        }
    }
    public void StartAfterInterruption(string goName)
    {
        if (anims.ContainsKey(goName))
        {
            anims[goName].GetComponent<LookTargetController>().LookAtPlayer(1);
            anims[goName].SetBool("Interruption", false);
        }
    }
    
    public void HandleGazeInterruption(string avatar = "")
    {
        var POI = GameObject.Find(avatar);
        if (!avatar.Contains("Interviewer"))
        {
            if(POI.TryGetComponent(out AudioSource audioSource))
            {
                eyeAndHead.LookAtPoiDirectly(audioSource.transform.position, -1);
            }    
            else 
            {
                eyeAndHead.LookAtPoiDirectly(POI.GetComponentInChildren<AudioSource>().transform.position, -1);
            }
        }
    }

    /// <summary>
    /// talking parameter is set to true or false depending on what animation she should be performing
    /// </summary>
    /// <param name="goName"></param>
    public void StartTalking(string goName)
    {
        if (anims.ContainsKey(goName))
        {
            anims[goName].SetBool("attentive", false);
            anims[goName].SetBool("talking", true);
        }
    }

    public void StopTalking(string goName)
    {
        if (anims.ContainsKey(goName))
        {
            anims[goName].SetBool("attentive", true);
            anims[goName].SetBool("talking", false);
        }
    }

    /// <summary>
    /// Gets random int to be used in getting a random animation
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="randIndex"></param>
    /// <returns>Random integer</returns>
    private int GetRandomInt(int startIndex, int endIndex)
    {
        if (startIndex != endIndex) {
            //int randomInt = random.Next(startIndex, endIndex);
            return random.Next(startIndex, endIndex);
        }
        else
        {
            return startIndex;
        }
    }

    /// <summary>
    /// Gets random time intervals for playing animations at random times
    /// </summary>
    /// <returns>Random time value</returns>
    private float GetRandomTimeInterval(int low = 0, int high = 1)
    {
        float ran = random.Next(low, high);
        ran /= 3f;
        return ran;
    }

    

    /// <summary>
    /// Splits the sentence into parts and time of dialog into parts
    /// </summary>
    /// <param name="audioLength"></param>
    /// <param name="curSentence"></param>
    public void SplitDialog(float audioLength, string curSentence) 
    {
        string lowerCase = curSentence.ToLower();
        //split sentence at spaces so to get all words 
        string[] strArray = lowerCase.Split(' ');
        //Returns an array of size n : Split(n)
        var splitArr = strArray.Split(3).ToArray();
        //get float that will be used to determine when a word is spoken
        float timePartition = audioLength / splitArr.Length;
        DecideGestures(splitArr, timePartition);
    } 

    /// <summary>
    /// Decides and stores what types of gestures to play dependent on the word  
    /// </summary>
    /// <param name="splitArr"></param>
    /// <param name="timePartition"></param>
    private void DecideGestures(IEnumerable<string>[] splitArr, float timePartition)
    {
        //gesture information
        List<float> gestureTimes = new List<float>();
        List<string> gestureTypes = new List<string>();
        //loop that searches for deictic words and assigns correct type and determines time to play anim
        for (int i = 0; i < splitArr.Count(); i++)
        {
            for (int k = 0; k < splitArr.ElementAt(i).Count(); k++)
            {
                try
                {
                    //get the word we working with and remove any punctuation
                    string word = splitArr.ElementAt(i).ElementAt(k);
                    
                    word = $" {word} ";
                    word = Regex.Replace(word, @"[;,!,.,?,\,]", "");

                    //you, your, you're
                    if (word.Contains(" you") || word.Contains(" let's ")) //Add users name
                    {
                        gestureTimes.Add(i * timePartition);
                        gestureTypes.Add("person_specific_deictic");
                        i++;
                        break;
                    }
                    //him, her, it, they, them, their
                    else if (word.Contains(" him ") || word.Contains(" they "))
                    {
                        gestureTimes.Add(i * timePartition);
                        gestureTypes.Add("person_general_deictic");
                        i++;
                        break;
                    }
                    // me, my, mine, i
                    else if (word.Contains(" i ") || word.Contains(" i'") || word.Contains(" my ") || word.Contains(" mine "))
                    {
                        gestureTimes.Add(i * timePartition);
                        gestureTypes.Add("self_specific_deictic");
                        i++;
                        break;
                    }
                    // we, our, us
                    else if (word.Contains(" we ") || word.Contains(" our ") || word.Contains(" us "))
                    {
                        gestureTimes.Add(i * timePartition);
                        gestureTypes.Add("self_general_deictic");
                        i++;
                        break;
                    }
                    else if(word.Contains(" before "))
                    {
                        gestureTimes.Add(i * timePartition);
                        gestureTypes.Add("past_deictic");
                        i++;
                        break;
                    }
                    else if(word.Contains(" unity ") || word.Contains(" unreal ") || word.ToLower().Contains(" microsoft ") || word.ToLower().Contains(" apple "))
                    {
                        gestureTimes.Add(i * timePartition);
                        gestureTypes.Add("nominative_deictic");
                        i++;
                        break;
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Debug.Log($"e content {e.Message}");
                }

            }
        }
        //calls the function that will determine the gesture to play
        DetermineTimeToPlay(gestureTimes, gestureTypes);
    }

    /// <summary>
    /// Determines the start and end time for gestures, which are also set here
    /// </summary>
    /// <param name="gTimes"></param>
    /// <param name="gTypes"></param>
    private void DetermineTimeToPlay(List<float> gTimes, List<string> gTypes)
    {
        //resets anim parameter that marks the end of the curr anim
        interviewer.SetFloat("timeOfAnim", 0f);
        //Two lists that are to be passed to the IEnum later, one contains endTimes and the other the gesture to play
        List<float> endTimes = new List<float>();
        List<string> gestureToPlay = new List<string>();
        //random for random gesture, get the length for end time, and the actual gesture
        int random = 0;
        float gestureLen = 0f;
        string gesture = "";
        //for loop decides the gesture to play through random choice and stores all applicable info
        for (int i = 0; i < gTypes.Count; i++)
        {
            random = GetRandomInt(0, handAnimTimes[gTypes[i]].Count);
            gestureLen = handAnimTimes[gTypes[i]].ElementAt(random).Item2;
            gesture = handAnimTimes[gTypes[i]].ElementAt(random).Item1.ToString();

            endTimes.Add(gTimes[i] + (gestureLen));
            gestureToPlay.Add(gesture);
        }
        //Starts all of the coroutines for Gesture function
        for(int i = 0; i < gestureToPlay.Count; i++)
        {
            StartCoroutine(Gesture(gTimes[i], gestureToPlay[i], gTypes[i], endTimes[i]));
        }
    }

    /// <summary>
    /// Plays gestures at specific times of word occurrence in sentence based on 
    /// what type of deictic animation it is: self, person, nominative, time based
    /// </summary>
    /// <param name="playTime"></param>
    /// <param name="gestureToPlay"></param>
    /// <param name="types"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    IEnumerator Gesture(float playTime, string gestureToPlay, string types, float endTime)
    {
        //yield once gesture starts
        yield return new WaitForSeconds(playTime);
        //if curr gesture overlaps next, and it's not the first then set bool and get out
        if (interviewer.GetFloat("timeOfAnim") + 1f > playTime && playTime != 0f)
        {
            interviewer.SetFloat("timeOfAnim", (endTime));
            interviewer.SetBool("isGoing", true);
            yield break;
        }
        //else, set up appropriate params
        else
        {
            interviewer.SetFloat("timeOfAnim", (endTime));
            interviewer.SetTrigger(types);
            interviewer.SetTrigger(gestureToPlay);
            interviewer.SetBool("isGoing", false);
        }
    }

    /// <summary>
    /// Assigns a listening animation to play
    /// </summary>
    public void ListeningAnim()
    {
        eyeAndHead.LookAtPlayer(10);
    }

    /// <summary>
    /// Changes the face dependant upon passed attitude
    /// Current options : neutral, happy, listening, scowl, extremehappy(this ones more of a joke)
    /// TODO : create in parent controller class
    /// </summary>
    /// <param name="attitude"></param>
    public void ChangeAttitude(string attitude)
    {
        string attitudeLow = attitude.ToLower();

        int smileInd = 0;
        int closedInd = 0;
        int openInd = 0;
        //Because each face model blend shape has different indexes for the same blends we need to fix that 
        try
        {
            smileInd = GetBlendIndexes(new string[] { faceMesh.GetBlendShapeName(0), faceMesh.GetBlendShapeName(1), faceMesh.GetBlendShapeName(2) }, "smile");
            closedInd = GetBlendIndexes(new string[] { faceMesh.GetBlendShapeName(0), faceMesh.GetBlendShapeName(1), faceMesh.GetBlendShapeName(2) }, "closed");
            openInd = GetBlendIndexes(new string[] { faceMesh.GetBlendShapeName(0), faceMesh.GetBlendShapeName(1), faceMesh.GetBlendShapeName(2) }, "open");
        }
        catch (ArgumentOutOfRangeException e) { Debug.Log($"Blend index out of range {e.Message}"); }
        //We need the original blend weights and a new array for what the new ones will be
        float[] arr = new float[] { face.GetBlendShapeWeight(0), face.GetBlendShapeWeight(1), face.GetBlendShapeWeight(2) };
        float[] newArray = new float[] { 0f, 0f, 0f };
        //Determine's what attitude to switch the face to and changes weights appropriately
        //I chose weights based on looking at her avatar and seeing what she looked like
        switch (attitudeLow)
        {
            case "neutral":
                newArray[smileInd] = 0;
                newArray[closedInd] = 52;
                newArray[openInd] = 0;
                ControlMouthBlends(newArray, arr, 2f);
                break;
            case "happy":
                newArray[smileInd] = 40;
                newArray[closedInd] = 84;
                newArray[openInd] = 6;
                ControlMouthBlends(newArray, arr, 2f);
                break;
            case "listening":
                newArray[smileInd] = 6;
                newArray[closedInd] = 100;
                newArray[openInd] = 0;
                ControlMouthBlends(newArray, arr, 2f);
                break;
            case "scowl":
                ControlMouthBlends(new float[] { 0, 0, 0 }, arr, .1f);
                break;
            case "extremehappy":
                ControlMouthBlends(new float[] { 100, 100, 100 }, arr, .1f);
                break;
        }
    }

    /// <summary>
    /// Determines the index of each blend shape associated with the faces of the 
    /// new face punch avatars ; create in parent controller class, might not be necessary
    /// </summary>
    /// <param name="strArr"></param>
    /// <param name="mouthShape"></param>
    /// <returns> index of smile/open/closed </returns>
    private int GetBlendIndexes(string[] strArr, string mouthShape)
    {   
        if(mouthShape == "smile")
        {
            for(int i = 0; i < 3; i++)
            {   //finds index for the smile blend
                if (strArr[i].Contains("_Smile"))
                {
                    return i;
                }
            }
        }
        if (mouthShape == "open")
        {   //finds index for the open mouth blend
            for (int i = 0; i < 3; i++)
            {
                if (strArr[i].Contains("_Open"))
                {
                    return i;
                }
            }
        }
        if (mouthShape == "closed")
        {   //finds index for the closed mouth blend
            for (int i = 0; i < 3; i++)
            {
                if (strArr[i].Contains("_Closed"))
                {
                    return i;
                }
            }
        }
        return 99;
    }
    /// <summary>
    /// Controls float values assigned to facial blends of currently attached head
    /// create in parent controller class
    /// </summary>
    /// <param name="blends"></param>
    /// <param name="blendSpeed"></param>
    private void ControlMouthBlends(float[] blends = null, float[] curBlends = null, float blendSpeed = .1f)
    {
        if (blends != null && curBlends != null)
        {
            //Changes each blend 
            for (int i = 0; i < blends.Length; i++)
            {
                //elif that continues to change curr blend until it reaches correct value
                //TODO : blend changes are immediately instead of gradual, looks bad
                if (curBlends[i] < blends[i])
                {
                    StartCoroutine(NewControlMouthBlends(i, blends, curBlends, blendSpeed));
                    //face.SetBlendShapeWeight(i, curBlends[i]);
                    curBlends[i] += blendSpeed;
                }
                else if (curBlends[i] > blends[i])
                {
                    StartCoroutine(NewControlMouthBlends(i, blends, curBlends, blendSpeed));
                    //face.SetBlendShapeWeight(i, curBlends[i]);
                    curBlends[i] -= blendSpeed;
                }
            }
        }
        else
        {
            Debug.Log($"Blends == null");
        }
    }

    /// <summary>
    /// IEnumerator that makes the change of mouth look nicer
    /// </summary>
    /// <param name="blendNum"></param>
    /// <param name="blends"></param>
    /// <param name="curBlends"></param>
    /// <param name="blendSpeed"></param>
    /// <returns></returns>
    IEnumerator NewControlMouthBlends (int blendNum, float[] blends, float[] curBlends, float blendSpeed = .1f)
    {
        //Wait .1 seconds to change the blend weight
        yield return new WaitForSeconds(.1f);
        face.SetBlendShapeWeight(blendNum, curBlends[blendNum]);
        ControlMouthBlends(blends, curBlends, blendSpeed);
    }

    ///<summary>
    ///Handles the avatars face based on the stress value of the user
    ///<summar>
    ///<param name="stressVal"></param>
    ///<returns></returns>
    public void HandleAvatarFace(float stressVal = 0f, float responseLength = 0f)
    {
        if(stressVal > 0f){
            if(stressVal >= .65f)
            {
                ChangeAttitude("happy");
            }
            else if(stressVal >= .35f && stressVal < .65f)
            {
                ChangeAttitude("listening");
            }
            else
            {
                ChangeAttitude("neutral");
            }
        }
        else{
            //Debug.Log($"stress value is ZERO");
        }
    }
}
using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.Utility.Config;
using UnityStandardAssets.Utility.Events;
using System.Diagnostics;
using UnityEngine.Networking;
using System.Linq;

namespace Cirvr.ConversationManager
{
    /// <summary>
    /// This class facilitates interaction between the Bot, 
    /// the DialogSet, and the Conversation context
    /// </summary>
    public class ConversationManager : MonoBehaviour
    {
        const int DB_THRESHOLD = -40;
        const int GATE_DB_THRESHOLD = -80;
        const int VOLUME_SAMPLE_BUF_SIZE = 1024;
        // We can also add the wait times here

        private bool isPaused = false;
        //private Interruptions iClass;
        private object threadLocker = new object();
        
        private static InterviewerBot InterviewerBot;
        SpeechConfig m_speechConfig;
        SpeechConfig synthSpeechConfig;
        //SpeechRecognizer m_recognizer;
        //SpeechSynthesizer m_synthesizer;
        GameObject InterviewerAvatar;

        Dictionary<String, AudioSource> sources;
        AnimController InterviewerAnimator;

        // Inspector variables
        public string lastUtterance;
        //public Text textText, userText;
        public TMPro.TextMeshProUGUI subtitlesText;
        //public Image imageImage;

        private bool questionRepeated = false;
        private bool questionWasSkipped = false;
        //User Interruption vars
        private int interruptionCount = 0;
        private string userInterruptionText = String.Empty;

        private static ConversationContext Context;
        private static ConversationManager _instance;
        public static ConversationManager Instance { get { return _instance; } }

        private string fullString = "Timestamp, Question Number, Question, Response Length, Interruptions \n";
        float responseLength = 0;
        bool questionFlag, userTalkedTooLong;
        private int index = 0;
        public int repeatCount;
        public int timeToRepeat = 8;
        private string oldSrc;
        private int altIndex = 0;

        private bool userTalks = false;
        private bool userInterrupts = false;
        private int questionNum = 1;

        private float interruptionTimeToWait = 2f;

        private System.Diagnostics.Stopwatch stopWatch;
        private System.Diagnostics.Stopwatch userInterruptionStopWatch;
        private System.Diagnostics.Stopwatch repeatStopWatch;
        private System.Diagnostics.Stopwatch questionSectionTimer;

        // ienumerators
        private IEnumerator interruptionWaiting;
        private IEnumerator answerWaiting;
        private IEnumerator interruptionStartWaiting;
        private IEnumerator waitForRecognized;
        private IEnumerator waitForQuestionFT;
        private IEnumerator waitForQuestionT;

        private bool waitFRIsRunning = false;
        private bool waitForQuestionIsRunning = false;

    
        private bool waitingForAnswer = false;
      
        private bool gotAnswer = false;
        private bool recognizedAnswer = false;
        private bool playInterruption = false;
        private float temp = 1f;
        private Image recordingIndicator;
        public TMPro.TextMeshProUGUI sphereText;
        private GameObject sphereIndicatorForFove;
        private bool gotQuestion, waitingForQuestion;        
        private bool ViveTriggerHit = false;

        private bool askQuestion = false;
        private bool askedQuestion = false;
        private string questionText = "";
        private bool recognizedQuestion = false;
        private Stopwatch questionStopWatch;

        public TMPro.TextMeshProUGUI instructionText;

        //Dictionairy to keep track of interview content D<DialogID, D<ColumnHeaderForCSV, content>>
        private List<string[]> recentInteraction = new List<string[]>();

        private Stopwatch timeToStartAfterInterruption;

        private string date = DateTime.Now.ToString().Replace("/", "-");

        System.Random randomNum = new System.Random();


        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        }

        private void Start()
        {
            // Make a stopwatch for tracking response lengths
            stopWatch = new Stopwatch();
            userInterruptionStopWatch = new Stopwatch();
            repeatStopWatch = new Stopwatch();
            questionSectionTimer = new Stopwatch();
            questionStopWatch = new Stopwatch();
            timeToStartAfterInterruption = new Stopwatch();
            timeToStartAfterInterruption.Start();

            // Disable image
            //imageImage.enabled = false;

            //Circle that changes color when user hits space
            recordingIndicator = GameObject.Find("RecordingIndicator").GetComponent<Image>();
            recordingIndicator.gameObject.SetActive(false);

            sphereIndicatorForFove = GameObject.Find("VRRecordingIndicator");
            sphereIndicatorForFove.transform.parent.gameObject.SetActive(false);

            
            // Get refs to our singletons
            InterviewerBot = InterviewerBot.Instance;
            Context = ConversationContext.Instance;

            // Get reference to our main animator
            InterviewerAvatar = GameObject.Find(ConfigInfo.envSettings.interviewer);
            InterviewerAnimator = InterviewerAvatar.GetComponent<AnimController>();

            // Initialize stuff
            m_speechConfig = SpeechConfig.FromSubscription("ADD PRIMARY KEY HERE ", "ADD LOCATION NAME HERE (E.g. westus2)");
            synthSpeechConfig = SpeechConfig.FromSubscription("ADD PRIMARY KEY HERE", "ADD LOCATION NAME HERE ");
            synthSpeechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff24Khz16BitMonoPcm);
           // m_recognizer = new SpeechRecognizer(m_speechConfig);


            RegisterListeners();
            AddDialogsFromJson();
            InitAudioSources();

           
        }

        private void Update()
        {            
            //Spins the recording sphere
            if(sphereIndicatorForFove.activeSelf)
            {
                sphereIndicatorForFove.transform.Rotate(new Vector3(0, 30 * Time.deltaTime, 0), Space.Self);
            }

            
            
            
            
            //Handles face, for pilot will use a specific value
            InterviewerAnimator.HandleAvatarFace(0.5f);

            // If they've been silent for x seconds, and we got a recognized event, fire the gotAnswer event
            // originally stopWatch.Elapsed.TotalSeconds >= timeToWait 
            // Now if the user hits the submit key and have talked then analyze response, 
            // or if the user takes too long to begin or finish answering 
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0) || ViveTriggerHit) && (waitingForAnswer || waitForQuestionIsRunning)  && userTalks
                && !Context.GetCurrentDialog().questionSection)
            {
                if(!waitFRIsRunning) {
                    UnityEngine.Debug.Log($"waitForRecognized has been started");
                    waitForRecognized = WaitForAzureResponseToProcess();
                    StartCoroutine(waitForRecognized);
                    waitFRIsRunning = true;
                    if(waitForQuestionIsRunning) {
                        UnityEngine.Debug.Log($"waitForQuestion has been stopped");
                        StopCoroutine(waitForQuestionFT);
                        waitForQuestionIsRunning = false;
                    }
                }

                //Strictly for resetting bool that is captured when the user hits one of the triggers on the vive controller
                ViveTriggerHit = false;
            }

            //If the user talks too long we want to address this
            if(userTalks && Context.GetCurrentDialog().timeLimit > 0 && stopWatch.Elapsed.TotalSeconds > Context.GetCurrentDialog().timeLimit && !Context.GetCurrentDialog().questionSection)
            {
                string noCommasLastUtterance = lastUtterance.Replace(",", " ");
              
                userTalks = false;
                userTalkedTooLong = true;
                recognizedAnswer = false;
                stopWatch.Reset();
            }

            //If the user does not talk initially the interviewer will repeat the questions up to 3 times, afterwards she will move on
            if(repeatStopWatch.Elapsed.Seconds > timeToRepeat && Context.GetCurrentDialog().requireResponse == true && !userTalks && !recognizedAnswer && !Context.GetCurrentDialog().questionSection)
            {
                string noCommasLastUtterance = lastUtterance.Replace(",", " ");
               
                gotAnswer = true;
                //I'm not too sure about changing the lastUtterance, consider a different plan
                //New idea, new function in Context to change the next id for skip question; possibly also one for repeating
                if(repeatCount < 2)
                {
                    lastUtterance = "repeat question";
                    repeatCount++;
                    timeToRepeat -= 1;
                }
                else
                {
                    repeatCount = 0;
                    lastUtterance += " can we skip this";
                }
                repeatStopWatch.Restart();
            }

           

            //Waits for user to ask a question during question section
            if(waitingForQuestion && Input.GetKeyDown(KeyCode.Space)) {
                string noCommasLastUtterance = lastUtterance.Replace(",", " ");
                
                ActivateRecordingIndicator(true);
                gotQuestion = true;
                stopWatch.Reset();
            }

            //If the user hits either ctrl key start a mini question section
            if(InterviewerBot.dialogIsSet && Context.GetCurrentDialog().whiteboardType == null) 
            {
                if((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))) {
                    //Start asking
                    if(!askQuestion) {
                        InterviewerAnimator.StopDuringInterruption(ConfigInfo.envSettings.interviewer, "");
                        if(waitingForAnswer) {
                            waitingForAnswer = false;
                            repeatStopWatch.Stop();
                        }
                        askQuestion = true;
                        
                        StopTalking();
                        if(!waitForQuestionIsRunning) {
                            UnityEngine.Debug.Log($"Wait for question is not running | co routine starts here");
                            StartCoroutine(waitForQuestionFT);
                            waitForQuestionIsRunning = true;
                        }
                        
                        if(waitFRIsRunning) {
                            UnityEngine.Debug.Log($"Stop the waitForRecognized Coroutine");
                            StopCoroutine(waitForRecognized);
                            waitFRIsRunning = false;
                        }

                        // StartCoroutine(WaitForQuestion(false, true));
                        stopWatch.Stop();
                        questionStopWatch.Restart();
                        InterviewerAnimator.StopLipAnim(ConfigInfo.envSettings.interviewer);
                    }
                    //Submit it
                    else {
                        InterviewerAnimator.StartAfterInterruption(ConfigInfo.envSettings.interviewer);
                        askQuestion = false;
                        string noCommasLastUtterance = questionText.Replace(",", " ");
                        if(!(noCommasLastUtterance.Length > 0)) {
                            noCommasLastUtterance = $"THE USER DID NOT ASK ANYTHING";
                        }
                       
                        ActivateRecordingIndicator(false);
                        
                        stopWatch.Reset();
                        questionStopWatch.Stop();
                        askedQuestion = true;
                    }
                }
            }
            
            //Grab audio source and check for null
            string src;
            if ((src = getPlayingSource()) != null)
            {
                if (oldSrc != src && oldSrc != null)
                {
                    InterviewerAnimator.StopLipAnim(oldSrc);
                }
                if (getVolume(src) > DB_THRESHOLD)
                {
                    oldSrc = InterviewerAnimator.PlayLipMvmtAnim(src);
                }
                else if (getVolume(src) <= DB_THRESHOLD)
                {
                    InterviewerAnimator.StopLipAnim(src);
                }
            }
            
            if(!SettingsInfo.subtitles) {
                subtitlesText.enabled = false;
            }
            else {
                subtitlesText.enabled = true;
            }
        }

      
        //IEnumerator that is started once the user submits an answer, this 
        //IE will then wait for a recognized answer to arrive from Azure, at which point
        //the magic happens
        private IEnumerator WaitForAzureResponseToProcess() {
            yield return new WaitUntil(() => recognizedAnswer && userTalks);
            responseLength = (float)stopWatch.Elapsed.TotalSeconds;
            userTalks = false;
            recognizedAnswer = false;
            lock (threadLocker)
            {
                gotAnswer = true;
            }

            string noCommasLastUtterance = lastUtterance.Replace(",", " ");
           
            //If the response is too short prod or move on depending on the question
            //This block skips the prodding questions if applicable
            UnityEngine.Debug.Log($"response length {responseLength}");
            if(Context.GetCurrentDialog().requireResponse == true && (responseLength > (Context.GetCurrentDialog().timeLimit / 3) && lastUtterance.Split(' ').Length >= 3))
            {   
                UnityEngine.Debug.Log($"required response {Context.GetCurrentDialog().requireResponse == true} responseLength {responseLength > (Context.GetCurrentDialog().timeLimit / 10)} utteranceLength {lastUtterance.Split(' ').Length >= 3}");
                //If it has a childMap replace the ids in the childmap
                if(Context.GetCurrentDialog().childMap != null && Context.GetCurrentDialog().nextTopicId != null)
                {
                    Context.GetCurrentDialog().childMap.EQUAL_TO = Context.GetCurrentDialog().childMap.GREATER_THAN = Context.GetCurrentDialog().nextTopicId;
                }
                //Otherwise replace the nextDialogId
                else if(Context.GetCurrentDialog().nextTopicId != null)
                {
                    Context.GetCurrentDialog().NextDialogID = Context.GetCurrentDialog().nextTopicId;
                }
                else
                    UnityEngine.Debug.Log($"Short answer but we don't care");
            }
            stopWatch.Reset();
            waitFRIsRunning = false;
        }

        

        /// <summary>
        /// Gets the volume of an audio src specified by an ID
        /// </summary>
        /// <param name="src">The source whose volume level to check</param>
        /// <returns></returns>
        private float getVolume(string src)
        {
            float[] samples = new float[VOLUME_SAMPLE_BUF_SIZE];

            // fill array with samples
            sources[src].GetOutputData(samples, 0);

            // sum squared samples
            float sum = 0;
            for (int i = 0; i < VOLUME_SAMPLE_BUF_SIZE; i++)
            {
                sum += samples[i] * samples[i];
            }

            // rms = square root of average
            float rmsValue = Mathf.Sqrt(sum / 1024);

            // calculate dB
            float dbValue = 20 * Mathf.Log10(rmsValue / 0.1f);

            if (dbValue < GATE_DB_THRESHOLD)
            {
                // Infinite ratio
                return Single.MinValue;
            }
            return dbValue;
        }

        /// <summary>
        /// Gets current source attached to npc that is playing audio
        /// </summary>
        /// <returns></returns>
        private string getPlayingSource()
        {
            foreach (KeyValuePair<string, AudioSource> nameSrc in sources)
            {
                if (nameSrc.Value.isPlaying && (nameSrc.Value.gameObject.tag == "Npc" || nameSrc.Value.gameObject.tag == "InterruptSource"))
                {
                    return nameSrc.Key;
                }
            }

            return null;
        }

        private void ActivateRecordingIndicator(bool activate, string inputText = "Action Button") {
            TMPro.TextMeshProUGUI indicatorText = new TMPro.TextMeshProUGUI();
            string text = $"Press {inputText} to submit";
            if(SettingsInfo.VR == "none") {
                recordingIndicator.gameObject.SetActive(activate);
                indicatorText = recordingIndicator.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            }
            else {
                sphereIndicatorForFove.transform.parent.gameObject.SetActive(activate);
                indicatorText = sphereIndicatorForFove.transform.parent.gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();//sphereText;
            }

            indicatorText.text = text;
        }

        /// <summary>
        /// Collect all audio sources in our scene into a dictionary to reference throughout the conversation.
        /// </summary>
        private void InitAudioSources()
        {
            sources = new Dictionary<string, AudioSource>();
            AudioSource[] srcs = FindObjectsOfType<AudioSource>();
            foreach (AudioSource src in srcs)
            {
                if (!sources.ContainsKey(src.name))
                {
                    sources.Add(src.name, src);
                }
            }
        }

        /// <summary>
        /// Registers functions to premade events
        /// </summary>
        private void RegisterListeners()
        {
            EventSystem.current.RegisterListener<PlayerBeginInterview>(OnInterviewBegin);
            EventSystem.current.RegisterListener<BeginDialog>(OnBeginDialog);
            EventSystem.current.RegisterListener<EndDialog>(OnEndDialog);
            
            
        }

       

        private void StopTalking()
        {
            // stop the current audio source and anim
            string src = getPlayingSource();
            if (src != null)
            {
                sources[src].Stop();
            }
            InterviewerAnimator.StopLipAnim(ConfigInfo.envSettings.interviewer);
        }

       
        

        public void OnEndDialog(EndDialog e)
        {
            waitingForAnswer = true;

            InterviewerAnimator.StopTalking(ConfigInfo.envSettings.interviewer);
            interruptionCount = 0;
            Dialog currentDialog = Context.GetCurrentDialog();

            subtitlesText.text = "";

            //starts timer stuff for handling pauses in sentences
            repeatStopWatch.Start();
            // If this dialog requires a response, wait for an answer
  
        }

       
        

        // Long running coroutine that continuously waits for answers
        // Waits for user to begin talking to begin listening animation, 
        // then waits for user to finish talking to fire azure event
        

        public void OnBeginDialog(BeginDialog e)
        {
            InterviewerAnimator.StartTalking(ConfigInfo.envSettings.interviewer);
            Dialog currentDialog = Context.GetCurrentDialog();
            repeatStopWatch.Reset();
            timeToRepeat = 8;

            // Reset utterances
            lastUtterance = String.Empty;
            userInterruptionText = String.Empty;

          
           
            
            // Handle the case where the interviewee ask for question repeat
            if (!questionRepeated)
            {
                altIndex = 0;
            }
            else
            {
                if (currentDialog.alternates.Count - 1 > altIndex)
                {
                    altIndex++;
                }
                else
                {
                    altIndex = 0;
                }
                questionRepeated = false;
            }
            UnityEngine.Debug.Log($"{recentInteraction[0][0]} | {currentDialog.DialogID}");
            
        }

        

        // Wait for end of question by waiting for the audioclip to finish
        public IEnumerator WaitForAudioToFinish(Dialog dialog, string srcName, int clipInstanceId, System.Action cb)
        {

            // Yield onto the callback only when the audio is no longer playing, 
            // not paused, 
            // the stopped audio clip is the same one passed in,
            // and the audioclip actually finished playing
            yield return new WaitUntil(() => sources[srcName].clip.length - .1f <= sources[srcName].time);

            yield return new WaitUntil (() => {
                return !sources[srcName].isPlaying &&
                !isPaused &&
                sources[srcName].clip.GetInstanceID() == clipInstanceId;
                
            });
            UnityEngine.Debug.Log($"is pausing breaking here");
            if (dialog.Interruptee != null){
                //do nothing 
                UnityEngine.Debug.Log("Python Interruption not available.");
            }
            else
            {
                //waitingForInterruption = true;
            }

            cb();
        }

        public void OnInterviewBegin(PlayerBeginInterview e)
        {
           // instructionText.gameObject.SetActive(true);
            Context.SwitchDialogSet("mainDialogSet");
            //imageImage.enabled = true;

            // This will retrieve and process the first dialog text and move us to the next question
            Ask(InterviewerBot.GetFirstQuestion(Context));
        }

      

        public async void getAudioClipFromText(string text, string voice, Action<AudioClip> cb)
        {
            SSMLSettings settings = new SSMLSettings(text, voice);
            string ssmlText = SSMLEngine.RenderSSML(settings, 0.5f);
        
            if(subtitlesText != null && SettingsInfo.subtitles)
            {
                subtitlesText.text = text;
            }
            else if(subtitlesText != null && !SettingsInfo.subtitles)
            {
                subtitlesText.text = "";
            }
            using (SpeechSynthesizer newSynth = new SpeechSynthesizer(synthSpeechConfig, null))
            {
                var result = await newSynth.SpeakSsmlAsync(ssmlText);

                // Checks result.
                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    var sampleCount = result.AudioData.Length / 2;
                    var audioData = new float[sampleCount];

                    for (var i = 0; i < sampleCount; ++i)
                    {
                        audioData[i] = (short)(result.AudioData[i * 2 + 1] << 8 | result.AudioData[i * 2]) / 32768.0F;
                    }

                    // The default output audio format is 16K 16bit mono
                    var audioClip = AudioClip.Create("SynthAudio", sampleCount, 1, 24000, false);
                    audioClip.SetData(audioData, 0);

                    cb(audioClip);
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                    UnityEngine.Debug.Log($"{cancellation.Reason}, {cancellation.ErrorDetails}, {cancellation.ErrorCode}");
                }
            }
        }


        //When Finish button is hit, display next question or get out of whiteboard if necessary
         public void EndWBQ(EndWhiteBoardSection sting)
         {
            //Ask(InterviewerBot.GetResponse(Context));
            //instructionText.gameObject.SetActive(true);
         }

        public void Ask(string text)
        {
            // recordingIndicator.gameObject.SetActive(false);
            // sphereIndicatorForFove.transform.parent.gameObject.SetActive(false);
            ActivateRecordingIndicator(false);
            //for adding questions to the csv file
            questionFlag = false;

            // @todo Use the requires response flag to determine instead of checking for question mark
            if (text.Trim().EndsWith("?"))
            {
                questionFlag = true;
            }

            // @todo Escape commas instead of just deleting them. e.g. text.Replace("\",\"", string.Empty); 
            string questionText = text.Replace(",", string.Empty);
            fullString += GetCurrentTime() + ", " + questionNum++ + ", " + questionText;

            // If this isn't a question, fill out the response length and interruptions with placeholders
            if (!questionFlag)
            {
                fullString += ", -----, " + interruptionCount + "\n";
            }

            // If we have dialogtext
            if (Context.GetCurrentDialog().DialogText != null)
            {   
                // Play Audio, Fire the begin dialog event to kick off other stuff
                PlayAudio(text, Context.GetCurrentDialog().Interruptee);
                EventSystem.current.FireEvent(new BeginDialog("The interviewer began a question.", Context.GetCurrentDialog()));
            }
            else if (text == Context.GetCurrentDialog().whiteboardType)
            {
               
                //instructionText.gameObject.SetActive(false);
                waitingForAnswer = false;
            }
            else
            {
                UnityEngine.Debug.Log($"Dialog question config error");
                // throw some dialog question config error
            }
        }

        public string GetCurrentTime()
        {
            var n = DateTime.Now;
            return n.Hour + ":" + n.Minute + ":" + n.Second;
        }

        private void FillQuestionCSV()
        {
            string path = Path.Combine(Application.streamingAssetsPath, "Data", "Events");//  "Assets/Data/Events/";
            string name = DateTime.Now.ToString();
            name = name.Replace("/", "-");
            name = name.Replace(":", "_");
            path += name + "_Events.csv";
            try
            {
                File.WriteAllText(path, fullString);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("Exception trying to write question csv file: " + e);
            }
        }

        /// <summary>
        /// Read in dialog sets and questions from JSON file
        /// </summary>
        private void AddDialogsFromJson()
        {
            TopLvlArrList JsonArrs = new TopLvlArrList();
            FileHandler.GrabJSON(ref JsonArrs, "Dialog-jw.json");

            // Add in dialog sets we just read in
            Context.AddDialogSet(new DialogSet("mainDialogSet", "000", JsonArrs.interviewerDialogs, ConfigInfo.envSettings.interviewer));
        }

        // @todo Interviewee being interrupted shouldn't depend on a length of time after the dialog starts,
        // it should depend on a length of time after they start responding
        public void PlayAudio(string text, string interrupt)
        {
            Dialog currDialog = Context.GetCurrentDialog();

            getAudioClipFromText(text, "", clip =>
            {
                if (clip == null)
                {
                    // throw
                }

                // Play our dialog audio
                sources[ConfigInfo.envSettings.interviewer].clip = clip;
                sources[ConfigInfo.envSettings.interviewer].Play();
                //Determine if the user is not stressed enough for an interruption
                //Play audio normally if no interruption is needed
                if(!playInterruption)
                {                
                    UnityEngine.Debug.Log($"QUESTION: {text}");
                    //Record interview event at start of dialog
                   
                    InterviewerAnimator.SplitDialog(clip.length, text);
                    StartCoroutine(WaitForAudioToFinish(
                    Context.GetCurrentDialog(),
                    ConfigInfo.envSettings.interviewer,
                    clip.GetInstanceID(),
                    () => EventSystem.current.FireEvent(new EndDialog("The interviewer finished a question.", currDialog))
                    ));
                }
                //other wise start interruption
                else
                {
                    UnityEngine.Debug.Log($"INTERRUPTION");
                    StartCoroutine(WaitForUserToTalkForInterruption(clip.length));                    
                }
            });
        }

        //This IEnumerator starts the interruption based on when the user starts to talk
        private IEnumerator WaitForUserToTalkForInterruption(float clipLength) {
            yield return new WaitForSeconds(clipLength);
            ActivateRecordingIndicator(true);
            playInterruption = false;
            yield return new WaitForSeconds(2f);
            ActivateRecordingIndicator(false);
        }

        // Generic way to line up callbacks off certain conditions?
        /* 
         * Call like this:
         * StartCoroutine(sequence( new (YieldInstruction, System.Action) [] { 
         *     (new WaitForSeconds(5), () => UnityEngine.Debug.Log("Waited 5 seconds")), 
         *     (new WaitForSeconds(4), () => UnityEngine.Debug.Log("4 seconds")), 
         *     (null, () => UnityEngine.Debug.Log("This executed immediately on the next frame.")) 
         * } )); 
        */
        private IEnumerator sequence(params (YieldInstruction yi, System.Action cb)[] instructions)
        {
            foreach (var instr in instructions)
            {
                yield return instr.yi;
                instr.cb();
            }
        }
        
        

        private IEnumerator WaitThenExecute(float time, System.Action cb)
        {
            yield return new WaitForSeconds(time);
            cb();
        }

        //If the interviewer repeats a question handle a couple of things such as setting the alternate text and 
        //making sure the interruption doesn't happen every single time.
        private void HandleRepeating(QuestionWasRepeated o)
        {
            if (Context.GetCurrentDialog().Interruptee != null)
            {
                Context.GetCurrentDialog().Interruptee = null;
            }

            questionRepeated = true;
            EventSystem.current.FireEvent(new CatchAlternate("setting alternate", altIndex));
        }

        private void HandleQuestionSkip(QuestionWasSkipped o) {
            questionWasSkipped = true;
        
            if(recentInteraction[recentInteraction.Count - 1][3] == "user") {
                try {
                    recentInteraction[recentInteraction.Count - 2][7] = "true";
                }
                catch(ArgumentOutOfRangeException e) {
                    UnityEngine.Debug.Log("Argument Out of Range Exception has been thrown for the recentInteraction List");
                    UnityEngine.Debug.Log(e.Data);
                }
            }
            else {
                recentInteraction[recentInteraction.Count - 1][7] = "true";
            }
        }


        private void OnDestroy()
        {
            //m_recognizer.Dispose();
           
        }


        private string CurrentTime()
        {
            return DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK");
        }

}

    

    // @todo Abstract these to an audio manager class, they don't belong here
    public class AudioInfo
    {
        public AudioClip Clip { get; set; }
        public AudioSource Src { get; set; }
        // we can add more params like volume, SSML stuff, etc...

        public AudioInfo(AudioSource src, AudioClip clip)
        {
            this.Src = src;
            this.Clip = clip;
        }
    }

    [Serializable]
    public class AudioFX
    {
        public string id;
        public string path;
        public string source;
        public AudioClip clip;
    }
}

using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Collections;
using UnityEngine.Networking;
using System.Text;

namespace Cirvr.ConversationManager
{
    public class TextToSpeechClient : MonoBehaviour
    {
        public AudioSource source;
        public string token;

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(GetAzureSpeechToken());
        }

        // Update is called once per frame
        void Update()
        {
            if (token != null)
            {
                UnityEngine.Debug.Log("test");
                StartCoroutine(GetAudioFilePostRequest("test 123 test"));
                token = null;
            }
        }

        private IEnumerator GetAudioFilePostRequest(string text)
        {

            // Define the URL we're interacting with
            string url = "https://westus.tts.speech.microsoft.com/cognitiveservices/v1";

            // Make our dl handler
            DownloadHandlerAudioClip handler = new DownloadHandlerAudioClip(url, AudioType.WAV);
            handler.compressed = true;
            handler.streamAudio = true;

            // Make our ul handler
            string ssmlText = @"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>
        <voice name='Microsoft Server Speech Text to Speech Voice (en-US, JessaRUS)'>" + text + "</voice></speak>";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(ssmlText);
            UploadHandlerRaw uploadHandler = new UploadHandlerRaw(bytes);

            // Make our request
            var www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST, handler, uploadHandler);
            www.SetRequestHeader("Content-type", "application/ssml+xml");
            www.SetRequestHeader("Authorization", token);
            www.SetRequestHeader("X-Microsoft-OutputFormat", "riff-24khz-16bit-mono-pcm");
            www.SetRequestHeader("User-Agent", "DynamicAudioTest");

            www.SendWebRequest();
            yield return new WaitUntil(() => www.downloadedBytes >= 1000);

            if (www.isNetworkError || www.isHttpError)
            {
                UnityEngine.Debug.Log(www.error);
            }
            else
            {
                UnityEngine.Debug.Log(www.downloadedBytes);
                source.clip = handler.audioClip;
                source.Play();
            }

        }

        private IEnumerator GetAzureSpeechToken()
        {
            UnityWebRequest www = UnityWebRequest.Post("https://westus.api.cognitive.microsoft.com/sts/v1.0/issueToken", new List<IMultipartFormSection>());

            www.SetRequestHeader("Content-type", "application/x-www-form-urlencoded");
            www.SetRequestHeader("Ocp-Apim-Subscription-Key", "dc54916ff0d94e8980b8dca7d19d6a48");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                UnityEngine.Debug.Log(www.error);
            }
            else
            {
                token = www.downloadHandler.text;
            }
        }
    }
}

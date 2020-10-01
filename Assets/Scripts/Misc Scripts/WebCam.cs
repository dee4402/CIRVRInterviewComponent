using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System;

//for capturing images from the webcam during the interview
public class WebCam : MonoBehaviour
{
    public WebCamTexture webCamTexture;
    public RawImage image;
    public int photoNum = 0;
    public string PHOTO_DIR_PATH;
    public string CSV_DIR_PATH;
    public FaceResponse faceResponse = null;
    public FileStream currentCSVFile;
    public bool finished = false;
    public const string dataHeaders = "Index, TimeStamp, FaceID, FaceRectangle - top, FaceRectangle - left, FaceRectangle - width, FaceRectangle - height"
            + ", Smile, HeadPitch, HeadRoll, HeadYaw, Gender, Age, Glasses, Anger, Contempt, Disgust, Fear, Happiness, Neutral, Sadness, Surprise\n";
    public int picsTaken;
    private string timeOfPic;
    public bool isPaused;

    void Start()
    {
        image = GameObject.Find("WebCamDisplayImage").GetComponent<RawImage>();//(RawImage)FindObjectOfType(typeof(RawImage)); //display the webcam feed to the raw image
        webCamTexture = new WebCamTexture();
        image.texture = webCamTexture;
        webCamTexture.Play(); //this throws an exception if no webcam is found
        PHOTO_DIR_PATH = $"{Application.persistentDataPath}/photos"; 

        //stores csvs in appdata folder for now, will eventually be stored to "Assets/Data/CSV files"
        CSV_DIR_PATH = Path.Combine(Application.streamingAssetsPath, "Data", "CSV files"); 

        Directory.CreateDirectory(PHOTO_DIR_PATH);
        Directory.CreateDirectory(CSV_DIR_PATH);
        CreateCSVFile();
        picsTaken = 0;
        isPaused = false;
        StartCoroutine(TakePhoto());
    }

    //creates the csv file to be written to
    async public void CreateCSVFile()
    {
        AsyncCallback callBack = new AsyncCallback(FinishFileOp);
        System.Text.Encoding.UTF8.GetBytes(dataHeaders);
         
        await Task.Run(() =>
        {
            string name = "" + DateTime.Now.ToString();
            name = name.Replace("/", "-");
            name = name.Replace(":", "_");
            name += "_WebCam";
            currentCSVFile = File.Create($"{CSV_DIR_PATH}/{name}.csv");
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(dataHeaders);
            int maxBytes = System.Text.Encoding.UTF8.GetMaxByteCount(dataHeaders.Length);

            // Add header
            currentCSVFile.Write(bytes, 0, bytes.Length);
        });
    }

    static void FinishFileOp(IAsyncResult result)
    {
        Debug.Log("Fin");
    }

    private string GetCurrentTime()
    {
        return DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK");
    }
     
    IEnumerator TakePhoto()  // Start this Coroutine on some button click
    {
        for (int i = 0; i < 100000000000; ++i) //should never hit end of loop, only stops when scene changes
        {  
            yield return new WaitForSeconds(3.5f);
            // For some reason we need to wait to frame end
            yield return new WaitForEndOfFrame();

            if (!isPaused) //don't do anything when the interview is paused
            {
                //Debug.Log("Picture: " + ++picsTaken);

                // Actually get the 2d texture representation
                Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
                photo.SetPixels(webCamTexture.GetPixels());
                photo.Apply();

                //encode it
                byte[] bytes = photo.EncodeToPNG();
                timeOfPic = GetCurrentTime();

                // Assemble the URI for the REST API Call.
                const string uri = "https://westus.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,headPose,glasses,smile,emotion";

                // Make the request object
                UnityWebRequest unityWebRequest = UnityWebRequest.Post(uri, new List<IMultipartFormSection>());

                // Request headers.
                unityWebRequest.SetRequestHeader("Content-Type", "application/octet-stream");
                unityWebRequest.SetRequestHeader("Ocp-Apim-Subscription-Key", "5ae7520060014146a0760d82584c1722");

                UploadHandlerRaw uploadHandler = new UploadHandlerRaw(bytes);
                unityWebRequest.uploadHandler = uploadHandler;

                // We don't really need this to finish in order to move on
                string photoPath = @PHOTO_DIR_PATH + $"\\{photoNum}.png";
                SavePhoto(bytes, photoPath);
                photoNum += 1;

                yield return unityWebRequest.SendWebRequest();
                bool emptyPic = false;
                string csvRec = "";
                try
                {
                    // This is hacky b/c unity json supported arrays as root level json elements            
                    faceResponse = JsonUtility.FromJson<FaceResponseWrapper>("{ \"faceResponses\":" + unityWebRequest.downloadHandler.text + "}").faceResponses[0];
                }
                catch (Exception e) //for when no face is detected
                {
                    csvRec = picsTaken + ", " + timeOfPic; //when no face is detected, fill the row with dashes
                    for (int count = 0; count < 20; ++count)
                        csvRec += ", -";

                    csvRec += "\n";
                    emptyPic = true;
                }
                finished = true;

                if (faceResponse.error != null && currentCSVFile != null)
                {
                    if (!emptyPic)
                    {
                        picsTaken += 1;
                        AsyncCallback callBack = new AsyncCallback(FinishFileOp);
                        csvRec = faceResponse.toCSVRecord(picsTaken, timeOfPic);
                    }
                    //write the data to the csv file and then delete the no longer needed photo
                    byte[] recordBuffer = System.Text.Encoding.UTF8.GetBytes(csvRec);
                    int maxBytes = System.Text.Encoding.UTF8.GetMaxByteCount(csvRec.Length);
                    currentCSVFile.Write(recordBuffer, 0, recordBuffer.Length);
                    File.Delete(photoPath);
                }
            }
            yield return null;
        }
    }
    
    //creates/saves the photo
    private async void SavePhoto(byte[] bytes, string path)
    {
        await Task.Run(() =>
        {
            // persist the PNG
            File.WriteAllBytes(path, bytes);
        });
    }

    [System.Serializable]
    public class FaceResponseWrapper
    {
        public List<FaceResponse> faceResponses;
    }

    [System.Serializable]
    public class FaceResponse
    {
        public string faceId;
        public FaceRectange faceRectange;
        public FaceAttributes faceAttributes;
        public FaceError error;

        //creates the string of data to be written to the csv file
        public string toCSVRecord(int index, string time)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(index.ToString());
            sb.Append(',');
            sb.Append(time);
            sb.Append(',');

            sb.Append(faceId);
            sb.Append(',');

            sb.Append(faceRectange.top.ToString());
            sb.Append(',');
            sb.Append(faceRectange.left.ToString());
            sb.Append(',');
            sb.Append(faceRectange.width.ToString());
            sb.Append(',');
            sb.Append(faceRectange.height.ToString());
            sb.Append(',');

            sb.Append(faceAttributes.smile.ToString());
            sb.Append(',');
            sb.Append(faceAttributes.headPose.pitch.ToString());
            sb.Append(',');
            sb.Append(faceAttributes.headPose.roll.ToString());
            sb.Append(',');
            sb.Append(faceAttributes.headPose.yaw.ToString());
            sb.Append(',');

            sb.Append(faceAttributes.gender);
            sb.Append(',');
            sb.Append(faceAttributes.age.ToString());
            sb.Append(',');
            sb.Append(faceAttributes.glasses);
            sb.Append(',');

            sb.Append(faceAttributes.emotion.anger.ToString());
            sb.Append(',');
            sb.Append(faceAttributes.emotion.contempt.ToString());
            sb.Append(',');
            sb.Append(faceAttributes.emotion.disgust.ToString());
            sb.Append(',');
            sb.Append(faceAttributes.emotion.fear.ToString());
            sb.Append(',');
            sb.Append(faceAttributes.emotion.happiness.ToString());
            sb.Append(',');
            sb.Append(faceAttributes.emotion.neutral.ToString());
            sb.Append(',');
            sb.Append(faceAttributes.emotion.sadness.ToString());
            sb.Append(',');
            sb.Append(faceAttributes.emotion.surprise.ToString());

            sb.Append('\n');

            return sb.ToString();
        }
    }

    [System.Serializable]
    public class FaceError
    {
        public int statusCode;
        public string message;
    }

    [System.Serializable]
    public class FaceRectange
    {
        public int top;
        public int left;
        public int width;
        public int height;
    }

    [System.Serializable]
    public class FaceAttributes
    {
        public float smile;
        public HeadPose headPose;
        public string gender;
        public float age;
        public string glasses;
        public Emotion emotion;
    }

    [System.Serializable]
    public class HeadPose
    {
        public float pitch;
        public float roll;
        public float yaw;
    }

    [System.Serializable]
    public class Emotion
    {
        public float anger;
        public float contempt;
        public float disgust;
        public float fear;
        public float happiness;
        public float neutral;
        public float sadness;
        public float surprise;
    }

    //makes sure every picture gets deleted, no need for them once they are used
    private void ClearRemainingPictures()
    {
        var files = Directory.GetFiles(PHOTO_DIR_PATH);
        if (files.Length != 0)
        {
            foreach (var i in files)
            {
                File.Delete(i);
            }
        }
    }

    private void OnDestroy()
    {
        currentCSVFile.Close();
        ClearRemainingPictures();
        webCamTexture.Stop();
    }
}

using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class FOVE3DCursor : MonoBehaviour
{
    public FoveInterfaceBase foveInterface;
    public LayerMask layerMask;
    public Camera mainCamera;
	private string dateAcquired;
	private string path;
    public GameObject cloner;
    
    private void Start() 
    {
		dateAcquired = "" + System.DateTime.Now.ToString();
	
		dateAcquired = dateAcquired.Replace("/", "-");
		dateAcquired = dateAcquired.Replace(":", "_");
        //Path to data location, name is dynamically set with Date
		path = Application.streamingAssetsPath + "/Data/EyeTracker files/" + "FoveEyes_" + dateAcquired + ".csv";

        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    //Keeps track of the objects being looked at by the user
    void Update()
    {
        //If the Fove cam is being used
        if(mainCamera.isActiveAndEnabled)
        {
            //Gets the gaze rays from the foveInterface class, then creates Unity rays for each eye
            FoveInterfaceBase.EyeRays rays = foveInterface.GetGazeRays();
            Ray leftEye = rays.left;
            Ray rightEye = rays.right;
            // I create a center ray so that we don't need each eye
            Ray centerRay = new Ray(mainCamera.transform.position, new Vector3((leftEye.direction.x + rightEye.direction.x) / 2, (leftEye.direction.y + rightEye.direction.y) / 2, leftEye.direction.z));
            RaycastHit hit = new RaycastHit();
            var temp = new Ray(centerRay.origin, centerRay.direction);
            // This sends out the ray up to a certain distance or if it lands upon a layer
            if (Physics.Raycast(centerRay, out hit, 1000f, layerMask))
            {
                transform.position = hit.point;
            }

            var test = foveInterface.CheckEyesClosed();
            bool eyesClosed = false;
            Debug.Log($"test {test}");
            if(test.ToString() == "Both") {
                eyesClosed = true;
            }
            
            // if(Physics.Raycast(temp, out hit, 10000f, layerMask)) {
            //     Instantiate(cloner, hit.point, new Quaternion(0,0,0,0));
            // }
            //Logs when the user looks at certain objects
            //LogGaze(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z, hit.transform.gameObject.name);
            LogGaze(centerRay.direction.x, centerRay.direction.y, centerRay.direction.z, centerRay.origin, eyesClosed, hit.transform.gameObject.name);
        }
    }

    //Returns date 
    private string CurrentTime()
    {
        return DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK");
    }

    //Logs gaze position of object being raycast
    private void LogGaze(float posX, float posY, float posZ, Vector3 origin, bool eyesClosed, string gameObject)
    {	
		if (!File.Exists(path))
        {
			//Writing using streamwriter
			using (StreamWriter sw = File.CreateText(path))
			{
				sw.WriteLine("ts,x direction pos,y direction pos,z direction pos, x origin, y origin, z origin, eyes closed, viewedObjects");
				WriteData(sw, posX, posY, posZ, origin.x, origin.y, origin.z, eyesClosed, gameObject);
			}
        }
        else
        {
			//Writing using streamwriter
			using (StreamWriter sw = File.AppendText(path))
			{
				WriteData(sw, posX, posY, posZ, origin.x, origin.y, origin.z, eyesClosed, gameObject);
			}
        }
    }

    //This writes each line of the csv, might be able to make this generic with an array of strings, where each value is the object to be written
	private void WriteData(StreamWriter sw, float xD, float yD, float zD, float xO, float yO, float zO, bool eyesClosed, string objectName)
	{
		sw.WriteLine(CurrentTime() + "," 
                                   + xD + "," 
                                   + yD + ","
                                   + zD + ","
                                   + xO + ","
                                   + yO + ","
                                   + zO + ","
                                   + eyesClosed + ","
                                   + objectName
                        );

		sw.Flush();
	}
}

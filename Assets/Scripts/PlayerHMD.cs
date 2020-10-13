using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
   Author : Michael Breen
   This script stores static variables from the main menu buttons to decide
   what form of visual input the user will be using (Fove , Vive, or no VR)
   Once the device is chosen then all gameobjects that are not going to be used are deactivated
 */

public class PlayerHMD : MonoBehaviour
{
    public GameObject fove, vive, none;

    //These are attached to buttons so as to set the SettingInfo VR property
    public void SetNOVR()
    {
        SettingsInfo.VR = "none";
    }

   /* public void SetFOVE()
    {
        SettingsInfo.VR = "fove";
    }

    public void SetVive()
    {
        SettingsInfo.VR = "vive";
    }*/

    private void Start() 
    {  
        if(gameObject.activeSelf)
        {
            //If all GO's are null then something is wrong, this should probably be a quit instead.
            if(/*fove == null && vive == null && */none == null)
            {
                return;
            }
            
            //Switch case that sets the appropriate GO's to inactive 
            switch(SettingsInfo.VR)
            {
                case "main":
                fove.SetActive(false);
                vive.SetActive(false);
                break;
               /* case "vive":
                none.SetActive(false);
                fove.SetActive(false);
                fove.GetComponentInParent<Transform>().gameObject.SetActive(false);
                // TobiiXR.Start(GameObject.Find("TobiiXR Initializer").GetComponent<TobiiXR_Initializer>().Settings);
                break;
                case "fove":
                none.SetActive(false);
                vive.SetActive(false);
                break;
                default :
                fove.SetActive(false);
                vive.SetActive(false);
                break;
                */
            }
        }
    }
}

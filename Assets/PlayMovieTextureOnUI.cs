using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

 public class PlayMovieTextureOnUI : MonoBehaviour 
 {
     public RawImage rawimage;
     void Start () 
     {
         rawimage = gameObject.GetComponent<RawImage>();
         WebCamTexture webcamTexture = new WebCamTexture();
         rawimage.texture = webcamTexture;
         rawimage.material.mainTexture = webcamTexture;
         webcamTexture.Play();
     }
 }

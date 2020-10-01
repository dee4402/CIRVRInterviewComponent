using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StessIndexColor : MonoBehaviour
{
    private Image m_Image;
    private Slider m_Slider; 
     
    void Start()
    {
        m_Image = GameObject.Find("Canvas/stressIndex/Fill Area/Fill").GetComponent<Image>();
        m_Slider = GameObject.Find("Canvas/stressIndex").GetComponent<Slider>();
    }
    
    void Update()
    {
        m_Image.color = m_Slider.value > .5F ? Color.red : Color.green;
        
    }
}

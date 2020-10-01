using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MasterVolumeSlider : MonoBehaviour
{
    public Slider masterSlider;

    void Start()
    {
        masterSlider = transform.GetComponent<Slider>();
    }

    void Update()
    {
        AudioListener.volume = masterSlider.value;
    }
}

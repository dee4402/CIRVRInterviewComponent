using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundFXVolumeSlider : MonoBehaviour
{
    public Slider soundFXVolume;
    public AudioSource source;

    // Update is called once per frame
    void Update()
    {
        source.volume = soundFXVolume.value;
    }
}

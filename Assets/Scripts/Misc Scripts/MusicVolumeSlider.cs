using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicVolumeSlider : MonoBehaviour
{
    public Slider musicVolume;
    public AudioSource source;

    // Update is called once per frame
    void Update()
    {
        source.volume = musicVolume.value;
    }
}

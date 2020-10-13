using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* Messy script that sets the initial settings */

public class SetInitialUserSettings : MonoBehaviour
{
    public Slider masterSlider, voiceSlider, ambientSlider, brightnessSlider, qualitySlider, subtitleSize;
    public Toggle fullscreen, subtitles;
   

    void Start()
    {
        SetInitialValues();
    }

    //Set's all of the initial values of each UI element
    public void SetInitialValues()
    {
        fullscreen.isOn = SettingsInfo.fullscreen;
        RenderSettings.ambientIntensity = SettingsInfo.brightness;
        subtitleSize.value = SettingsInfo.subtitlesSize;
        brightnessSlider.value = SettingsInfo.brightness;
        masterSlider.value = SettingsInfo.masterVolume;
        voiceSlider.value = SettingsInfo.characterVolume;
        ambientSlider.value = SettingsInfo.environmentVolume;
        subtitles.isOn = SettingsInfo.subtitles;
        qualitySlider.value = SettingsInfo.qualityLevel;
        QualitySettings.SetQualityLevel(SettingsInfo.qualityLevel);
       

        SetResolutionBasedOnINI();
    }

    public void SetResolutionBasedOnINI()
    {
        int width, height;
        var widthAndHeight = SettingsInfo.resolution.Split('x');
        int.TryParse(widthAndHeight[0], out width);
        int.TryParse(widthAndHeight[1], out height);
        Screen.SetResolution(width, height, SettingsInfo.fullscreen);
    }
}

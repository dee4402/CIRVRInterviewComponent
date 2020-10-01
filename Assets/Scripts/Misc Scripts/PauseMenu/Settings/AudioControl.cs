using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class AudioControl : MonoBehaviour
{
    /* Vars */

    //Audio mixer controls the volume of the scene
    public AudioMixer audioMixer;

    //UI elements set in inspector
    public Slider masterSlider, voiceSlider, ambientSlider;

    //Back button handles turning on the settings selection page
    private Button backButton;

    //Double check is the prompt that appears when user changes settings and attempts to leave without saving
    public RectTransform doubleCheck;

    public Transform settingsSelection;
    public TMPro.TMP_Dropdown audioLanguage;

    public AudioSource masterSource, ambientSource, voiceSource;

    private void Start() 
    {
        //Sets non inspector objects, initial volume via audioMixer and UI listeners 
        //doubleCheck = gameObject.GetComponentsInChildren<RectTransform>().Where(x => x.name.Contains("DoubleCheck")).Single();
        backButton = gameObject.GetComponentsInChildren<Button>().Where(x => x.name.Contains("Back")).Single();


        backButton.onClick.AddListener(delegate {HandleBackButton();});
        audioLanguage.onValueChanged.AddListener(delegate {SetAudioLanguage(audioLanguage.options);});
        audioMixer.SetFloat("VoiceVolume", SettingsInfo.characterVolume);
        audioMixer.SetFloat("MasterVolume", SettingsInfo.masterVolume);
        audioMixer.SetFloat("AmbientVolume", SettingsInfo.environmentVolume);
    }

    //Everytime the object is enabled we match ui values to the static values 
    private void OnEnable() {
        SettingsInfo.writtenToINI = true;
        masterSlider.value = SettingsInfo.masterVolume;
        ambientSlider.value = SettingsInfo.environmentVolume;
        voiceSlider.value = SettingsInfo.characterVolume;
        GoThroughDropdown(audioLanguage, SettingsInfo.spokenLanguage);
    }

    private void SetAudioLanguage(List<TMP_Dropdown.OptionData> temp)
    {
        SettingsInfo.writtenToINI = false;
        //change the default language here
    }

    //Reset settings calls function from static class and resets ingame settings to previous rendition
    public void ResetSettings()
    {
        SettingsInfo.ResetValues();
        audioMixer.SetFloat("VoiceVolume", SettingsInfo.characterVolume);
        audioMixer.SetFloat("MasterVolume", SettingsInfo.masterVolume);
        audioMixer.SetFloat("AmbientVolume", SettingsInfo.environmentVolume);
    }

    //Reset to default calls function from static class and resets settings via default ini, also sets new UI values
    public void ResetToDefault()
    {
        SettingsInfo.ResetToDefault("audio");
        audioMixer.SetFloat("VoiceVolume", SettingsInfo.characterVolume);
        audioMixer.SetFloat("MasterVolume", SettingsInfo.masterVolume);
        audioMixer.SetFloat("AmbientVolume", SettingsInfo.environmentVolume);
        voiceSlider.value = SettingsInfo.characterVolume;
        masterSlider.value = SettingsInfo.masterVolume;
        ambientSlider.value = SettingsInfo.environmentVolume;
    }

    //If the user has not saved after changing settings then prompt them, otherwise move on
    private void HandleBackButton()
    {
        if(SettingsInfo.writtenToINI)
        {
            gameObject.SetActive(false);
            settingsSelection.gameObject.SetActive(true);

        }
        else
        {
            doubleCheck.gameObject.SetActive(true);
        }
    }

    //Listeners for volume changes
    public void setVoiceVolume()
    {
        // If statement used to play an audio clip for testing volume
        // Known bugs : You can hear a small snippet of it on start up and when leave
        // sometimes whe leaving befor it finishes the clip will play again on opening the page
        if(!voiceSource.isPlaying)
        {
            voiceSource.Play();
        }
        SettingsInfo.writtenToINI = false;
        audioMixer.SetFloat("VoiceVolume", voiceSlider.value);
        SettingsInfo.characterVolume = (int)voiceSlider.value;
    }
    public void setMasterVolume()
    {
        //If statement used to play an audio clip for testing volume
        // Known bugs : You can hear a small snippet of it on start up and when leave
        // sometimes whe leaving befor it finishes the clip will play again on opening the page
        if(!masterSource.isPlaying)
        {
            masterSource.Play();
        }
        SettingsInfo.writtenToINI = false;
        audioMixer.SetFloat("MasterVolume", masterSlider.value);
        SettingsInfo.masterVolume = (int)masterSlider.value;
    }
    public void setAmbientVolume()
    {
        //If statement used to play an audio clip for testing volume
        // Known bugs : You can hear a small snippet of it on start up and when leave
        // sometimes whe leaving befor it finishes the clip will play again on opening the page
        if(!ambientSource.isPlaying)
        {
            ambientSource.Play();
        }
        SettingsInfo.writtenToINI = false;
        audioMixer.SetFloat("AmbientVolume", ambientSlider.value);
        SettingsInfo.environmentVolume = (int)ambientSlider.value;
    }

    //Generalize this again
    private void GoThroughDropdown(TMPro.TMP_Dropdown dropdownToParse, string comparitor)
    {
        for(int i = 0; i < dropdownToParse.options.Count(); i++)
        {
            if(dropdownToParse.options[i].text == comparitor)
            {
                dropdownToParse.value = i;
            }
        }
    }
}

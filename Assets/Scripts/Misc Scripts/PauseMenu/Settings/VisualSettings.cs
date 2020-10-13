using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/*
    This script handles the visual settings options
*/

public class VisualSettings : MonoBehaviour
{
    //UI variables
    public Toggle fullscreen;
    public Slider brightness;
    private Button backButton;
    public Transform settingsSelection;
    public Slider qualitySlider;

    //Double check is the element that pops up if the user leaves before saving any changes
    public RectTransform doubleCheck;

    // Start is called before the first frame update
    void Start()
    {
        /*Sets initial values and listeners*/

        //Finds the two objects needed to handle the double check element
        //TODO: this is used for every page, so this could be made into one function instead of each having their own
        //doubleCheck = gameObject.GetComponentsInChildren<RectTransform>().Where(x => x.name.Contains("DoubleCheck")).Single();
        backButton = gameObject.GetComponentsInChildren<Button>().Where(x => x.name.Contains("Back")).Single();

        //Listeners for each UI element
        backButton.onClick.AddListener(delegate {HandleBackButton();});
        fullscreen.onValueChanged.AddListener(delegate {ToggleValueChanged(fullscreen.isOn);});
        brightness.onValueChanged.AddListener(delegate {ChangeBrightness();});
        qualitySlider.onValueChanged.AddListener(delegate {ChangeQuality();});
    }

    //Changes the quality based on slider value
    public void ChangeQuality()
    {
        SettingsInfo.writtenToINI = false;
        //Quality settings is set up in Unity as an array where each element is a level of quality
        //So a slider with int options is used to set quality
        //TODO: for more transparency a dropdown could be used with the same effect
        QualitySettings.SetQualityLevel((int)qualitySlider.value);
        SettingsInfo.qualityLevel = (int)qualitySlider.value;
    }

    //OnEnable values are set via static values
    //This is so everytime the user opens up the page the options are visually what they actually are
    private void OnEnable() {
        SettingsInfo.writtenToINI = true;
        fullscreen.isOn = SettingsInfo.fullscreen;
        brightness.value = SettingsInfo.brightness;
        qualitySlider.value = SettingsInfo.qualityLevel;
    }

    // TODO: this function can be made generic for dropdowns in other pages
    // This function is used to check the values in a dropdown based on the 
    // names of each option and what option you want to check for
   

    //Function called from static class to reset game settings via current ini settings
    public void ResetSettings()
    {
        //Static function that is called to reset the static values
        SettingsInfo.ResetValues();
        //Resets the visual settings based on the original values
        RenderSettings.ambientIntensity = SettingsInfo.brightness;
        Screen.fullScreen = SettingsInfo.fullscreen;
        SetResolutionBasedOnINI();
    }


    //Function called from static class to reset game settings via default settings
    public void ResetToDefault()
    {
        SettingsInfo.writtenToINI = false;
        //Resets the settings in static class to default
        SettingsInfo.ResetToDefault("video");
        
        RenderSettings.ambientIntensity = SettingsInfo.brightness;
        Screen.fullScreen = SettingsInfo.fullscreen;
        fullscreen.isOn = SettingsInfo.fullscreen;
        brightness.value = SettingsInfo.brightness;
        SetResolutionBasedOnINI();
    }

    //Resolution is set via ini value
    public void SetResolutionBasedOnINI()
    {
        SettingsInfo.writtenToINI = false;
        //Sets the Resolution
        //Todo: this can be combined with the other resolution setting function
        int width, height;
        var widthAndHeight = SettingsInfo.resolution.Split('x');
        int.TryParse(widthAndHeight[0], out width);
        int.TryParse(widthAndHeight[1], out height);
        Screen.SetResolution(width, height, SettingsInfo.fullscreen);
    }

    //If the user attempts to back without saving changes they are prompted, otherwise move on
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

    //Toggle listner for fullscreen 
    private void ToggleValueChanged(bool value)
    {
        SettingsInfo.writtenToINI = false;
        SettingsInfo.fullscreen = value;
        Screen.fullScreen = value;
    }
    
    //Brightness slider is used to set ambient intensity, pseudo brightness based on light intensity
    private void ChangeBrightness()
    {
        SettingsInfo.writtenToINI = false;
        SettingsInfo.brightness = brightness.value;
        RenderSettings.ambientIntensity = brightness.value;
    }

   
}

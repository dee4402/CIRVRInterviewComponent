using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class GameplayControl : MonoBehaviour
{
    public Slider subtitleSize;
    public TMPro.TextMeshProUGUI subtitlesDisplayText, subtitlesText;
    public Toggle subtitles;
    private Button backButton;
    public RectTransform doubleCheck;
    public Transform settingsSelection;
    public TMPro.TMP_Dropdown subtitleLanguage;

    // Start is called before the first frame update
    void Start()
    {
        //doubleCheck = gameObject.GetComponentsInChildren<RectTransform>().Where(x => x.name.Contains("DoubleCheck")).Single();
        backButton = gameObject.GetComponentsInChildren<Button>().Where(x => x.name.Contains("Back")).Single();

        backButton.onClick.AddListener(delegate {HandleBackButton();});
        subtitleLanguage.onValueChanged.AddListener(delegate {SetSubtitleLanguage(subtitleLanguage.options);});
        subtitles.onValueChanged.AddListener(delegate {ToggleValueChanged(subtitles.isOn);});
        subtitleSize.onValueChanged.AddListener(delegate {SubtitlesSize();});
        
        subtitles.isOn = SettingsInfo.subtitles;
        subtitleSize.value = SettingsInfo.subtitlesSize;
        Debug.Log($"start being called");
    }
    //Everytime the object is enabled we match ui values to the static values 
    private void OnEnable() {
        SettingsInfo.writtenToINI = true;
        subtitles.isOn = SettingsInfo.subtitles;
        subtitleSize.value = SettingsInfo.subtitlesSize;
        subtitlesDisplayText.fontSize = SettingsInfo.subtitlesSize;
        GoThroughDropdown(subtitleLanguage, SettingsInfo.writtenLanguage);
    }

    private void SetSubtitleLanguage(List<TMP_Dropdown.OptionData> temp)
    {
        SettingsInfo.writtenToINI = false;
        //set subtitle language
    }

    //Listener for subtitles toggle
    private void ToggleValueChanged(bool value)
    {
        Debug.Log($"value changed");
        SettingsInfo.writtenToINI = false;
        SettingsInfo.subtitles = value;
    }

    //Changes the size of both the testing subtitles and the actual subtitles
    private void SubtitlesSize()
    {
        SettingsInfo.writtenToINI = false;
        subtitlesDisplayText.fontSize = subtitleSize.value;
        if(SettingsInfo.subtitles && subtitlesText != null)
        {
            subtitlesText.fontSize = subtitleSize.value;
        }
        SettingsInfo.subtitlesSize = subtitleSize.value;
    }

    //Reset settings calls function from static class and resets ingame settings to previous rendition
    public void ResetSettings()
    {
        SettingsInfo.ResetValues();
        subtitles.isOn = SettingsInfo.subtitles;
        subtitleSize.value = SettingsInfo.subtitlesSize;
    }

    //Reset to default calls function from static class and resets settings via default ini, also sets new UI values
    public void ResetToDefault()
    {
        SettingsInfo.writtenToINI = false;
        SettingsInfo.ResetToDefault("gameplay");
        subtitles.isOn = SettingsInfo.subtitles;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadInterview : MonoBehaviour
{
    public void LoadScene()
    {
        SettingsInfo.sceneName = "Interview";
        SceneManager.LoadScene("LoadingScene", LoadSceneMode.Single);
    }
}

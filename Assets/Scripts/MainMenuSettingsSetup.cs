using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* I was having problems with main menu settings pages all starting on top */

public class MainMenuSettingsSetup : MonoBehaviour
{

    GameObject[] menuOptions;

    // Start is called before the first frame update
    void Start()
    {
        menuOptions = GameObject.FindGameObjectsWithTag("Paused");
        StartCoroutine(WaitOne());
    }

    private IEnumerator WaitOne()
    {
        yield return new WaitForSeconds(.01f);
        HideSettings();
    }

    public void HideSettings()
    {
        foreach (GameObject g in menuOptions)
        {
            g.SetActive(false);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMethods : MonoBehaviour
{
    //Function used for save button
    public void SetIni()
    {
        SettingsInfo.WriteToINI();
        SettingsInfo.writtenToINI = true;
    }
}

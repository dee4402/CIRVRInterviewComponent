using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script strictly for allowing the user to move mouse in Main Menu

public class EnableMouseCursor : MonoBehaviour
{
    private void OnEnable() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}

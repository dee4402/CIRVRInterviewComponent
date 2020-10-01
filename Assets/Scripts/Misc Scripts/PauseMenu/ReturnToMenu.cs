using UnityEngine;
using UnityStandardAssets.Utility.Events;

public class ReturnToMenu : MonoBehaviour
{

    public void ExitInterviews()
    {
        EventSystem.current.FireEvent(new PlayerEndInterview("Exit interview from pause menu"));
    }

}

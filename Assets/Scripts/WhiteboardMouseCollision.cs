using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Utility.Events;


namespace Cirvr.ConversationManager
{
    public class WhiteboardMouseCollision : MonoBehaviour
    {
        private bool clickedOnce = false;
        private Toggle currToggle;
        private bool waitedSeconds = true;
        private float timer;

        private void Start() 
        {
            currToggle = this.gameObject.GetComponent<Toggle>();
            EventSystem.current.RegisterListener<WBQAnswered>(WhiteBoardQuestionAnswered);
        }
        private void Update() 
        {
            if(timer >= .5f)
            {
                waitedSeconds = true;
                timer = 0f;
            }
            else
            {
                timer += Time.deltaTime;
            }

        }
        private void OnTriggerEnter(Collider other) 
        {
            gameObject.GetComponentInChildren<Image>().color = Color.grey;
        }
        private void OnTriggerStay(Collider other) 
        {       
            if(gameObject.name.Contains("Toggle") && Input.GetMouseButtonDown(0) && waitedSeconds)
            {
                currToggle.isOn = !clickedOnce;
                clickedOnce = !clickedOnce;
                waitedSeconds = false;
            }
            if(gameObject.name.Contains("Button") && Input.GetMouseButtonDown(0) && waitedSeconds)
            {
                waitedSeconds = false;
                clickedOnce = false;
               
            }
        }
        private void OnTriggerExit(Collider other)
        {
            gameObject.GetComponentInChildren<Image>().color = Color.white;
        }

        private void WhiteBoardQuestionAnswered(WBQAnswered e)
        {
            clickedOnce = false;
        }
    }
}

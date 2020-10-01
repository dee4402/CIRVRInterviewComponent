using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatBotUI : MonoBehaviour
{
    private Text m_chatBotText;
    private InputField m_userInputText;

	void Start ()
    {
        //cache componenets
        m_chatBotText = GameObject.Find("Canvas/Text_ChatbotResponse").GetComponent<Text>();
        m_userInputText = GameObject.Find("Canvas/Text_UserInput").GetComponent<InputField>();

        //set initial text values
        m_chatBotText.text = "DEEKSHA, you can set chatbot text using this variable. See ChatBotUI.cs script.";
    }

    public void SubmitUserQuery()
    {
        //DEEKSHA: This function is called when the user hits the "SUBMIT" button.
        //
        // YOU CAN ADD YOUR AZURE CALLS HERE

        //reset user input field
        m_userInputText.text = "";
    }

    public void RecordUserQuery()
    {
        //DEEKSHA: This function is called when the user hits the "RECORD" button.
        //
        // YOU CAN ADD YOUR AZURE CALLS HERE
        //
        // NOTE: Doesn't actually record audio yet, but I can help with that
    }
}

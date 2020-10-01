using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility.Events;
using UnityEngine.UI; //TEMP added by Josh on 03.10.2019

public class ClientSocketManager : MonoBehaviour
{
    private ServerSocket m_socket;
    private static string m_msgToSend = "";
    private static string m_recvData = "";
    private enum ClientSocketState {NOT_CONNECTED,IDLE,WAITING_FOR_RESPONSE};
    private ClientSocketState m_clientSocketState = ClientSocketState.NOT_CONNECTED;
    private bool m_beginConnect = false;
    private int m_sendMsgCount = -1;
    
	void Start ()
    {
        //m_beginConnect = true;
    }

    void Update ()
    {
        //this will need re-factoring after the demo
        if( m_clientSocketState == ClientSocketState.NOT_CONNECTED )
        {
            if(m_beginConnect)
            {
                m_socket = new ServerSocket("127.0.0.1", 19001);
                m_socket.Start();

                m_clientSocketState = ClientSocketState.WAITING_FOR_RESPONSE; //uncomment to recv first then send
                //m_clientSocketState = ClientSocketState.IDLE; //uncomment to send first then recv
            }
        }
        else if(m_clientSocketState == ClientSocketState.IDLE)
        {
            if (m_msgToSend != "")
            {
                Debug.Log("Sending message to Azure App " + m_msgToSend);
                m_socket.Send(m_msgToSend);
                m_msgToSend = "";

                m_clientSocketState = ClientSocketState.WAITING_FOR_RESPONSE;
            }
        }
        else if( m_clientSocketState == ClientSocketState.WAITING_FOR_RESPONSE )
        {
            if( m_socket.PollAndRecv(ref m_recvData) )
            {
                //this is the point at which listeners should be notified of a message received event
                Debug.Log("Received message from Azure App " + m_recvData);
                //EventSystem.current.FireEvent(new ReceivedAzureMessage("Received message from Azure App.", m_recvData));
                m_clientSocketState = ClientSocketState.IDLE;

                //TEMP added by Josh on 03.10.2019
                //GameObject.Find("Canvas/textHUD").GetComponent<Text>().text = "\"" + m_recvData + "\"";
            }
        }

	}

    private void OnGUI()
    {
        if(Input.GetKeyDown(KeyCode.S))
        {
            SetSendMsg("TEST MESSAGE FROM UNITY " + (++m_sendMsgCount).ToString());
        }
    }

    public void BeginConnect()
    {
        //m_beginConnect = true;
    }

    public static void SetSendMsg(string msg)
    {
        m_msgToSend = msg;
    }
}

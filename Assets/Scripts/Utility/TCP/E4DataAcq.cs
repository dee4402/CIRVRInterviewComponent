using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Utility.Events;
public class E4DataAcq : MonoBehaviour
{
    private ServerSocket m_socket;
    private string m_ipAddr = "127.0.0.1";
    private int m_port = 19002;
    private enum SocketState {NOT_STARTED, SENDRECV, CLOSED};
    private SocketState m_socketState = SocketState.NOT_STARTED;
    private string m_recvData = "No affect predictions received.";
    private bool m_beginConnect = false;
    private Text m_affectLabel;
    
	void Start ()
    {
        //m_affectLabel = GameObject.Find("Canvas/AffectiveStateLabel").GetComponent<Text>();
        this.UpdateAffectLabelUI();
    }

    public void BeginConnect()
    {
        m_beginConnect = true;
    }
	
	void Update ()
    {
		if( m_socketState == SocketState.NOT_STARTED )
        {
            if(m_beginConnect)
            {
                m_socket = new ServerSocket(m_ipAddr, m_port);
                Debug.Log($"m_socket null {m_socket == null}");
                m_socket.Start();
                m_socketState = SocketState.SENDRECV;
            }
        }
        else if (m_socketState == SocketState.SENDRECV)
        {
            if(m_socket.PollAndRecv(ref m_recvData))
            {
                Debug.Log(">>> Received data from E4: " + m_recvData);
                this.UpdateAffectLabelUI();

                m_socket.Send("ACK");
            }
        }
        else if (m_socketState == SocketState.CLOSED)
        {

        }
    }

    private void UpdateAffectLabelUI()
    {
        //m_affectLabel.text = m_recvData;
    }
}

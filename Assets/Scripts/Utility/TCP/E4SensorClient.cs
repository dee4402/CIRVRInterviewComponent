using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class E4SensorClient
{
    private const int BUFFER_SIZE = 2000;
    private string m_ipAddr;
    private int m_port;
    private IPEndPoint m_ipEndPoint;
    private Socket m_socket;
    
    public E4SensorClient(string ipAddr, int port)
    {
        m_ipAddr = ipAddr;
        m_port = port;

        this.Init();
    }
     
    public bool Init()
    {
        //initialize endpoint and socket
        bool success;
        m_ipEndPoint = new IPEndPoint(IPAddress.Parse(m_ipAddr), m_port);
        try {
            m_socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            success = true;
        }
        catch (SocketException e) {
            Debug.Log("Failed to initialize m_socket. " + e.ToString());
            success = false;
        }
        return success;
    }

    public bool ConnectToServer()
    {
        bool success;
        try {
            m_socket.Connect(m_ipEndPoint);
            success = m_socket.Connected;
        }
        catch (SocketException e) {
            System.Console.WriteLine("Failed to connect to the server. Closing m_socket. " + e.ToString());
            m_socket.Close();
            success = false;
        }

        return success;
    }

    public bool BlockingRecv(ref string recvData)
    {
        byte[] rxBuffer = new byte[BUFFER_SIZE];
        bool success;
        try {
            m_socket.Receive(rxBuffer);
            recvData = ASCIIEncoding.ASCII.GetString(rxBuffer);
            success = true;
        }
        catch (SocketException e) {
            System.Console.WriteLine("Failed to read data on buffer. Closing socket. " + e.ToString());
            success = false;
        }

        return success;
    }

    public bool PollAndRecv(ref string recvData)
    {
        bool dataReady = false;
        if (m_socket.Poll(10, SelectMode.SelectRead))
        {
            try {
                dataReady = this.BlockingRecv(ref recvData);
            }
            catch(SocketException e) {
                System.Console.WriteLine("Failed to read data on buffer. Closing socket. " + e.ToString());
            }
        }

        return dataReady;

    }

    public void Send(string sendData)
    {
        try {
            byte[] txBuffer = Encoding.ASCII.GetBytes(sendData);
            m_socket.Send(txBuffer);
        }
        catch (SocketException e) {
            System.Console.WriteLine("Failed to send data to the server. Closing socket. " + e.ToString());
            m_socket.Close();
        }
    }

}

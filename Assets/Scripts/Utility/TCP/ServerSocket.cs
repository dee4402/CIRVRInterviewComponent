using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;


public class ServerSocket
{
    private const int BUFFER_SIZE = 2000;
    private string m_ipAddr;
    private int m_port;
    private IPEndPoint m_ipEndpoint;
    private Socket m_socket;
    private Socket m_client;

    public ServerSocket(string ipAddr, int port)
    {
        m_ipAddr = ipAddr;
        m_port = port;

        //bind the socket if initialization is successful
        if(this.Init())
            this.Bind();
    }

    private bool Init()
    {
        //initialize endpoint and socket
        bool success;
        m_ipEndpoint = new IPEndPoint(IPAddress.Parse(m_ipAddr), m_port);
        try {
            m_socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            success = true;
        }
        catch (System.Exception e) {
            System.Console.WriteLine("Unable to initialize m_socket. " + e.ToString());
            success = false;
        }

        return success;
    }

    private void Bind()
    {
        try {
            m_socket.Bind(m_ipEndpoint);
        }
        catch (System.Exception e) {
            m_socket.Close();
            System.Console.WriteLine("Failed to bind m_socket. Closing socket. " + e.ToString());
        }
    }

    public bool Start()
    {
        bool success;
        try {
            m_socket.Listen(1);
            m_client = m_socket.Accept();
            success = m_client.Connected;
        }
        catch(SocketException e) {
            System.Console.WriteLine("Failed to accept connection. Closing socket. " + e.ToString());
            success = false;
        }

        return success;
    }

    public void Close()
    {
        try {
            m_client.Shutdown(SocketShutdown.Both);
            m_client.Close();
            m_socket.Close();
        }
        catch (SocketException e) {
            System.Console.WriteLine("Error occured while closing socket: " + e.ToString());
        }
    }

    public bool BlockingRecv(ref string recvData)
    {
        byte[] rxBuffer = new byte[BUFFER_SIZE]; //JOSH: adjust size as needed; consider audio file size --> send audio file path instead?
        bool success;
        try {
            m_client.Receive(rxBuffer);
            recvData = ASCIIEncoding.ASCII.GetString(rxBuffer); //JOSH: this assumes ASCII encoding; may actually be UTF8 etc.
            recvData = recvData.Substring(0,recvData.IndexOf("$__$"));
            success = true;
        }
        catch(SocketException e) {
            System.Console.WriteLine("Failed to read data on buffer. Closing socket. " + e.ToString());
            success = false;
        }

        return success;
    }

    public bool PollAndRecv(ref string recvData)
    {
        bool dataReady = false;
        if(m_client.Poll(10, SelectMode.SelectRead))
        {
            try {
                dataReady = this.BlockingRecv(ref recvData);
            }
            catch(SocketException e) {
                System.Console.WriteLine("Failed to read data on buffer. Closing socket. " + e.ToString());
                dataReady = false;
            }
        }

        return dataReady;
    }

    public void Send(string sendData)
    {
        try {
            byte[] txBuffer = Encoding.ASCII.GetBytes(sendData+"$__$");
            m_client.Send(txBuffer);
        }
        catch(SocketException e) {
            System.Console.WriteLine("Failed to send data to client. Closing socket. " + e.ToString());
        }
    }
}

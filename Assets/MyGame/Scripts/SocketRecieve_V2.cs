using System;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class SocketRecieve_V2 : MonoBehaviour
{
    public Text summaryText;
    public TcpClient client;
    StreamReader reader;
    public string message;

    void Start()
    {
        Application.targetFrameRate = 30;

        
    }

    public void TryConnect(int port)
    {
        try
        {
            TcpClient testClient = new TcpClient();
            testClient.Connect("127.0.0.1", port);

            client = testClient;
            reader = new StreamReader(client.GetStream());
            Debug.Log("Connected on port " + port);
        }
        catch (Exception)
        {
            // Anderen Port versuchen, debug ist in anderem Codeblock
        }
    }

    void Update()
    {
        if (client != null && client.Connected)
        {
            if (client.Available > 0)
            {
                message = reader.ReadLine();
            }
            
            if (!string.IsNullOrEmpty(message))
            {
                Debug.Log("Message from Python revceived");
                if (summaryText != null)
                    summaryText.text = message;
            }
        }
    }



    void OnApplicationQuit()
    {
        reader?.Close();
        client?.Close();
    }


    public void CloseConnection()
    {
        try
        {
            if (reader != null)
            {
                reader.Close();
                reader = null;
            }

            if (client != null)
            {
                client.Close();
                client = null;
            }

            Debug.Log("Socket connection closed manually.");

            if (summaryText == null) return;
            summaryText.text = "Ready for next connection!";
        }
        catch (Exception e)
        {
            Debug.LogError("Error while closing connection: " + e.Message);
        }
    }

}

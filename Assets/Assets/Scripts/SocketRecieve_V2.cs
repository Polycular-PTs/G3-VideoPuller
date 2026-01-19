using System;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class SocketRecieve_V2 : MonoBehaviour
{
    public Text summaryText;
    TcpClient client;
    StreamReader reader;

    void Start()
    {
        Application.targetFrameRate = 60;

        TryConnect(5005);   // Zuerst versuchen auf port 5005, also mit der Object Detection eine verbindung herzustellen

        if (client == null || !client.Connected)
        {
            Debug.Log("Trying fallback port 5006...");
            TryConnect(5006);   // Dann versuchen auf port 5006, also mit der Action Detection eine verbindung herzustellen
        }

        if (client != null && client.Connected)
        {
            Debug.Log("Connected to Python");
        }
        else
        {
            Debug.LogError("Could not connect on ports 5005 or 5006.");
        }

        
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
            string msg = reader.ReadLine();
            if (!string.IsNullOrEmpty(msg))
            {
                Debug.Log("From Python: " + msg);
                if (summaryText != null)
                    summaryText.text = msg;
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
            
            summaryText.text = "Ready for next connection!";
        }
        catch (Exception e)
        {
            Debug.LogError("Error while closing connection: " + e.Message);
        }
    }

}

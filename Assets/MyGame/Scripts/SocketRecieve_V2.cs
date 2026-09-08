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
    [SerializeField]
    Button DebugButton;
    [SerializeField]
    Button CameraSkipButton;

    // STATIC: Der Status bleibt global für das ganze Spiel erhalten, 
    // egal wie oft du dich neu verbindest oder Skripte wechselst.
    private static bool isDebugMode = false;

    void Start()
    {
        Application.targetFrameRate = 60;
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

            // --- DER FIX ---
            // Sobald die Verbindung steht, schießen wir sofort den aktuellen Status rüber!
            StreamWriter writer = new StreamWriter(client.GetStream());
            writer.AutoFlush = true;
            writer.WriteLine(isDebugMode ? "DEBUG:ON" : "DEBUG:OFF");

        }
        catch (Exception)
        {
            // Anderen Port versuchen
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
                // Optional: Die ständigen Empfangs-Logs auskommentieren, falls sie die Konsole zuspammen
                // Debug.Log("Message from Python received"); 

                if (summaryText != null)
                    summaryText.text = "I currently see " + message;
                else
                {
                    Debug.Log("summary Text is null");
                }
            }
        }
    }

    public void SendCaptureCommand(string fileName)
    {
        if (client != null && client.Connected)
        {
            try
            {
                StreamWriter writer = new StreamWriter(client.GetStream());
                writer.AutoFlush = true;
                writer.WriteLine($"CAPTURE:{fileName}");
                Debug.Log($"[Socket] Requested Python capture: {fileName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Socket] Failed to send capture command: {e.Message}");
            }
        }
    }

    public void SwitchDebugMode()
    {
        // Toggle den Modus (gilt jetzt global wegen 'static')
        isDebugMode = !isDebugMode;
        Debug.Log($"[Socket] Internal debug state switched to: {isDebugMode}");

        // Nur senden, wenn wir GERADE verbunden sind. 
        // Wenn nicht, wird es sowieso beim nächsten TryConnect() gesendet!
        if (client != null && client.Connected)
        {
            try
            {
                StreamWriter writer = new StreamWriter(client.GetStream());
                writer.AutoFlush = true;
                writer.WriteLine(isDebugMode ? "DEBUG:ON" : "DEBUG:OFF");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Socket] Failed to send debug command: {e.Message}");
            }
        }

        if (isDebugMode == true)
        {

            DebugButton.GetComponentInChildren<Text>().enabled = true;
            DebugButton.GetComponent<Image>().color = new Color(255, 0, 233, 255);
            
            CameraSkipButton.GetComponent<Image>().enabled = true;
            CameraSkipButton.GetComponent<Button>().enabled = true;
            CameraSkipButton.GetComponentInChildren<Text>().enabled = true;
            DebugSkipButton.skipCameraRequirement = false;
            CameraSkipButton.GetComponent<DebugSkipButton>().UpdateButtonVisuals();

        }

        else if (isDebugMode == false)
        {
            DebugButton.GetComponentInChildren<Text>().enabled = false;
            DebugButton.GetComponent<Image>().color = new Color(255, 0, 233, 0);
            DebugSkipButton.skipCameraRequirement = false;
            CameraSkipButton.GetComponent<Image>().enabled = false;
            CameraSkipButton.GetComponent<Button>().enabled = false;
            CameraSkipButton.GetComponentInChildren<Text>().enabled = false;
        }
    }

    void OnApplicationQuit()
    {
        reader?.Close();
        client?.Close();
    }

    private void OnDisable()
    {
        CloseConnection();
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

            message = "";
            Debug.Log("Socket connection closed manually.");

            if (summaryText != null)
            {
                summaryText.text = "I'm not ready to see yet.";
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error while closing connection: " + e.Message);
        }
    }
}
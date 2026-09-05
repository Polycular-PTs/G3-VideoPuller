using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChestOpen : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] Text currentGoalText;
    [SerializeField] int stage;
    [SerializeField] float requiredSpeed;
    [SerializeField] string[] cameraReqs; //Simon
    [SerializeField] int[] detectionUsed; //Simon
    [SerializeField] GameObject nextChest;

    const float OPEN_THRESHOLD_DEGREES = 170f; //Simon
    const float CLOSE_THRESHOLD_DEGREES = 10f; //Simon
    const int CONNECTION_DELAY_STAGE1 = 5;
    const int CONNECTION_DELAY_OTHER_STAGES = 4;
    const float GAME_RESTART_DELAY = 20f;
    public bool openedTruly;
    public string camMessage; //Simon

    VideoManager videoMan;
    LevelManager levelMan;
    SocketRecieve_V2 detectionMan; //Simon
    KurbelRotation kurbelRotation;

    private void Start()
    {
        //Gabriel
        videoMan = GameObject.FindGameObjectWithTag("VideoManager").GetComponent<VideoManager>();
        levelMan = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();



        //Simon
        detectionMan = GameObject.FindGameObjectWithTag("DetectionManager").GetComponent<SocketRecieve_V2>();
        kurbelRotation = GameObject.FindGameObjectWithTag("KurbelRotMan").GetComponent<KurbelRotation>();
        cameraReqs[0] = "waving"; cameraReqs[1] = "sad"; cameraReqs[2] = "2x_bottle"; cameraReqs[3] = "jumping"; cameraReqs[4] = "chair"; cameraReqs[5] = "keyboard";
        detectionUsed[0] = 5006; detectionUsed[1] = 5005; detectionUsed[2] = 5005; detectionUsed[3] = 5006; detectionUsed[4] = 5005; detectionUsed[5] = 5005;
        
        currentGoalText.text = cameraReqs[stage];
        if (stage == 0)
        {
            StartCoroutine(ConnectNextStage(stage, CONNECTION_DELAY_STAGE1));
        }

        else
        {
            StartCoroutine(ConnectNextStage(stage, CONNECTION_DELAY_OTHER_STAGES));
        }
    }

    void Update() //wird noch die kurbel
    {
        slider.value = kurbelRotation.currentAngle;
        OpenTheChest(slider.value); // Slider ist temporär
    }


    

    void OpenTheChest(float rotationDegrees)
    {
        gameObject.transform.rotation = Quaternion.Euler(rotationDegrees, 0, 0);

        if (rotationDegrees >= OPEN_THRESHOLD_DEGREES && !openedTruly //Gabriel
            && detectionMan.message.Contains(cameraReqs[stage])) //Simon
        {
            Debug.Log($"Playing video for stage: {stage}");
            videoMan.PlayVideo(stage);
            openedTruly = true;
        }
        else if (rotationDegrees < CLOSE_THRESHOLD_DEGREES && openedTruly == true)
        {
            detectionMan.CloseConnection(); //Simon
            videoMan.StopVideo();
            kurbelRotation.currentAngle = 0f;

            if (nextChest != null)
            {
                Debug.Log(nextChest);
                levelMan.giveResetInfo(gameObject.transform.parent.gameObject, nextChest);
                GetComponentInParent<Animator>().Play("Move");
                Debug.Log("closedTruly");
            }
            else
            {
                Debug.Log("Fertig");
                StartCoroutine(RestartGameWithDelay(GAME_RESTART_DELAY));
            }
            openedTruly = false;
        }
    }


    // Simon
    IEnumerator ConnectNextStage(int stage, int delay)
    {
        // 1. Initial wait before the first connection attempt
        yield return new WaitForSeconds(delay);

        int targetPort = detectionUsed[stage];

        // 2. Loop every 1 second until detectionMan is connected
        while (detectionMan.client == null || !detectionMan.client.Connected)
        {
            Debug.Log($"Trying to connect to Port {targetPort}...");
            detectionMan.TryConnect(targetPort);

            // Wait 1 second before checking/trying again
            yield return new WaitForSeconds(1f);
        }

        Debug.Log($"Successfully connected to Port {targetPort} for stage {stage}!");
        // The coroutine stops automatically here once connected
    }


    IEnumerator RestartGameWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log($"Reloading scene in {delay} seconds...");
        levelMan.restartGameInstant();
    }
}

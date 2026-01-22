using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
    const int CONNECTION_DELAY_STAGE1 = 28;
    const int EXTRA_DELAY_STAGE1 = 10;
    const int CONNECTION_DELAY_OTHER_STAGES = 4;
    public bool openedTruly;
    public string camMessage; //Simon

    VideoManager videoMan;
    LevelManager levelMan;
    SocketRecieve_V2 detectionMan; //Simon
    KurbelRotation kurbelRotation;
    CountdownManager countdownMan;

    private void Start()
    {
        //Gabriel
        videoMan = GameObject.FindGameObjectWithTag("VideoManager").GetComponent<VideoManager>();
        levelMan = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();



        //Simon
        detectionMan = GameObject.FindGameObjectWithTag("DetectionManager").GetComponent<SocketRecieve_V2>();
        kurbelRotation = GameObject.FindGameObjectWithTag("KurbelRotMan").GetComponent<KurbelRotation>();
        countdownMan = GameObject.FindGameObjectWithTag("CountdownManager").GetComponent<CountdownManager>();
        cameraReqs[0] = "waving"; cameraReqs[1] = "angry"; cameraReqs[2] = "2x_bottle"; cameraReqs[3] = "jumping"; cameraReqs[4] = "3x_person";
        detectionUsed[0] = 5006; detectionUsed[1] = 5005; detectionUsed[2] = 5005; detectionUsed[3] = 5006; detectionUsed[4] = 5005;
        
        currentGoalText.text = cameraReqs[stage];
        if (stage == 0)
        {
            StartCoroutine(ConnectNextStage(stage, CONNECTION_DELAY_STAGE1));
            countdownMan.EstimatedConnectionTime = CONNECTION_DELAY_STAGE1 + EXTRA_DELAY_STAGE1;
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
                levelMan.giveResetInfo(gameObject, nextChest);
                GetComponentInParent<Animator>().Play("Move");
                Debug.Log("closedTruly");
            }
            else
            {
                Debug.Log("Fertig");
            }
            openedTruly = false;
        }
    }


    //Simon
    IEnumerator ConnectNextStage(int stage, int delay) 
    {
        yield return new WaitForSeconds(delay);
        Debug.Log($"Trying To connect to Port {detectionUsed[stage]}");
        detectionMan.TryConnect(detectionUsed[stage]);
    }

}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChestOpen : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] Image sliderFillImage;
    [SerializeField] Text currentGoalText;
    [SerializeField] int stage;
    [SerializeField] float requiredSpeed;
    [SerializeField] string[] cameraReqs;
    [SerializeField] int[] detectionUsed;
    [SerializeField] GameObject nextChest;
    [SerializeField] GameObject endScreenPanel;
    [SerializeField] DebugSkipButton debugMan;

    [Header("Colors & Pulse")]
    [SerializeField] Color defaultSliderColor = Color.black;
    [SerializeField] Color completedSliderColor = Color.green;
    [SerializeField] float pulseSpeed = 4f;
    [SerializeField] float pulseMagnitude = 0.08f;

    const float OPEN_THRESHOLD_DEGREES = 170f;
    const float CLOSE_THRESHOLD_DEGREES = 10f;
    const int CONNECTION_DELAY_STAGE1 = 5;
    const int CONNECTION_DELAY_OTHER_STAGES = 4;
    const float GAME_RESTART_DELAY = 40f;
    const float DELAY_REACTION = 4.5f;

    public bool openedTruly;
    public string camMessage;

    VideoManager videoMan;
    LevelManager levelMan;
    SocketRecieve_V2 detectionMan;
    KurbelRotation kurbelRotation;
    Vector3 baseTextScale;

    private void Start()
    {
        videoMan = GameObject.FindGameObjectWithTag("VideoManager").GetComponent<VideoManager>();
        levelMan = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
        detectionMan = GameObject.FindGameObjectWithTag("DetectionManager").GetComponent<SocketRecieve_V2>();
        kurbelRotation = GameObject.FindGameObjectWithTag("KurbelRotMan").GetComponent<KurbelRotation>();
        debugMan = GameObject.FindGameObjectWithTag("DebugMan").GetComponent<DebugSkipButton>();

        // Fill-Image des Sliders automatisch holen, falls nicht im Inspector zugewiesen
        if (sliderFillImage == null && slider != null && slider.fillRect != null)
        {
            sliderFillImage = slider.fillRect.GetComponent<Image>();
        }

        if (sliderFillImage != null)
        {
            sliderFillImage.color = defaultSliderColor;
        }

        if (currentGoalText != null)
        {
            baseTextScale = currentGoalText.transform.localScale;
        }

        cameraReqs[0] = "waving"; cameraReqs[1] = "happy"; cameraReqs[2] = "2x_bottle"; cameraReqs[3] = "jumping"; cameraReqs[4] = "3x_person"; cameraReqs[5] = "book";
        detectionUsed[0] = 5006; detectionUsed[1] = 5005; detectionUsed[2] = 5005; detectionUsed[3] = 5006; detectionUsed[4] = 5005; detectionUsed[5] = 5005;

        UpdateGoalText(0f);

        if (stage == 0)
        {
            StartCoroutine(ConnectNextStage(stage, CONNECTION_DELAY_STAGE1));
        }
        else
        {
            StartCoroutine(ConnectNextStage(stage, CONNECTION_DELAY_OTHER_STAGES));
        }
    }

    void Update()
    {
        slider.value = kurbelRotation.currentAngle;
        OpenTheChest(slider.value);

        UpdateGoalText(slider.value);
        UpdateSliderVisuals(slider.value);
        AnimateTextPulse();
    }

    void UpdateSliderVisuals(float rotationDegrees)
    {
        if (sliderFillImage == null) return;

        // Wenn fertig gekurbelt -> Grün, sonst Schwarz
        if (rotationDegrees >= OPEN_THRESHOLD_DEGREES)
        {
            sliderFillImage.color = completedSliderColor;
        }
        else
        {
            sliderFillImage.color = defaultSliderColor;
        }
    }

    void UpdateGoalText(float rotationDegrees)
    {
        if (currentGoalText == null) return;

        // 1. Wenn das Video aktiv abgespielt wird
        if (openedTruly)
        {
            currentGoalText.text = "Enjoy the video on the hologram!";
        }
        // 2. Wenn noch gekurbelt werden muss
        else if (rotationDegrees < OPEN_THRESHOLD_DEGREES)
        {
            currentGoalText.text = "Use the crank to open the chest!";
        }
        // 3. Wenn die Kurbel oben ist und die Pose/das Objekt verlangt wird
        else
        {
            currentGoalText.text = $"Show the camera {cameraReqs[stage]}";
        }
    }

    void AnimateTextPulse()
    {
        if (currentGoalText == null) return;

        // Während das Video läuft: Pulsieren anhalten und Text-Skalierung fixieren
        if (openedTruly)
        {
            currentGoalText.transform.localScale = baseTextScale;
            return;
        }

        // Weiches Pulsieren im Gameplay
        float scaleFactor = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseMagnitude;
        currentGoalText.transform.localScale = baseTextScale * scaleFactor;
    }

    void OpenTheChest(float rotationDegrees)
    {
        bool cameraConditionMet = DebugSkipButton.skipCameraRequirement || detectionMan.message.Contains(cameraReqs[stage]);
        gameObject.transform.rotation = Quaternion.Euler(rotationDegrees, 0, 0);

        if (rotationDegrees >= OPEN_THRESHOLD_DEGREES && !openedTruly
            && cameraConditionMet)
        {
            if (nextChest != null)
            {
                Debug.Log($"Playing video for stage: {stage}");
                videoMan.PlayVideo(stage);
                openedTruly = true;

                detectionMan.SendCaptureCommand($"stage_{stage}_action.jpg");
                StartCoroutine(CaptureReelReaction(DELAY_REACTION));
            }
            else
            {
                detectionMan.SendCaptureCommand($"stage_{stage}_action.jpg");
                Debug.Log("Fertig. Spiel startet in 30 Sekunden neu");

                if (endScreenPanel != null)
                {
                    endScreenPanel.SetActive(true);
                }

                StartCoroutine(RestartGameWithDelay(GAME_RESTART_DELAY));
            }
        }
        else if (rotationDegrees < CLOSE_THRESHOLD_DEGREES && openedTruly == true)
        {
            detectionMan.CloseConnection();
            videoMan.StopVideo();
            kurbelRotation.currentAngle = 0f;
            kurbelRotation.isLocked = false;

            if (nextChest != null)
            {
                Debug.Log(nextChest);
                levelMan.giveResetInfo(gameObject.transform.parent.gameObject, nextChest);
                GetComponentInParent<Animator>().Play("Move");
                Debug.Log("closedTruly");
            }
            else
            {
                Debug.LogError("next chest unassigned");
            }
            openedTruly = false;
        }
    }

    IEnumerator CaptureReelReaction(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (openedTruly)
        {
            detectionMan.SendCaptureCommand($"stage_{stage}_reel.jpg");
        }
    }

    IEnumerator ConnectNextStage(int stage, int delay)
    {
        yield return new WaitForSeconds(delay);

        int targetPort = detectionUsed[stage];

        while (detectionMan.client == null || !detectionMan.client.Connected)
        {
            Debug.Log($"Trying to connect to Port {targetPort}...");
            detectionMan.TryConnect(targetPort);
            yield return new WaitForSeconds(1f);
        }

        Debug.Log($"Successfully connected to Port {targetPort} for stage {stage}!");
    }

    IEnumerator RestartGameWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log($"Reloading scene in {delay} seconds...");
        levelMan.restartGameInstant();
    }
}
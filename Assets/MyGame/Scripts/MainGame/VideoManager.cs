using UnityEngine;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    [SerializeField] Animator videoAnim;
    [SerializeField] Animator levelSwapper;
    [SerializeField] VideoPlayer video;
    [SerializeField] VideoClip[] possibleVideo;

    private const string VIDEO_APP = "videoApp";
    private const string VIDEO_DAPP = "videoDapp";

    private KurbelRotation kurbelRotation;

    void Awake()
    {
        // Find the crank rotation manager
        GameObject kurbelObj = GameObject.FindGameObjectWithTag("KurbelRotMan");
        if (kurbelObj != null)
        {
            kurbelRotation = kurbelObj.GetComponent<KurbelRotation>();
        }
    }

    void OnEnable()
    {
        if (video != null)
        {
            video.loopPointReached += OnVideoEnd;
        }
    }

    void OnDisable()
    {
        if (video != null)
        {
            video.loopPointReached -= OnVideoEnd;
        }
    }

    public void PlayVideo(int stage)
    {
        if (stage < 0 || stage >= possibleVideo.Length)
        {
            Debug.LogError($"[VideoManager] Invalid stage index: {stage}");
            return;
        }

        video.isLooping = false; // Ensure looping is disabled so it reaches the end
        video.Stop();
        video.clip = possibleVideo[stage];
        video.Play();

        videoAnim.SetBool(VIDEO_DAPP, false);
        videoAnim.SetBool(VIDEO_APP, true);
    }

    public void StopVideo()
    {
        videoAnim.SetBool(VIDEO_DAPP, true);
        videoAnim.SetBool(VIDEO_APP, false);
        video.Stop();
    }

    private void OnVideoEnd(VideoPlayer source)
    {
        Debug.Log("[VideoManager] Video finished playing. Resetting crank angle to 0.");

        // Reset the crank angle to 0
        if (kurbelRotation != null)
        {
            kurbelRotation.currentAngle = 0f;
        }
    }
}
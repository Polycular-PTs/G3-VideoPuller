using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    [SerializeField] Animator videoAnim;
    [SerializeField] Animator levelSwapper;
    [SerializeField] VideoPlayer video;
    [SerializeField] VideoClip[] possibleVideo;


    private const string VIDEO_APP = "videoApp";
    private const string VIDEO_DAPP = "videoDapp";



    public void PlayVideo(int stage)
    {
        video.clip = possibleVideo[stage];
        videoAnim.SetBool(VIDEO_DAPP, false);
        videoAnim.SetBool(VIDEO_APP, true);
    }
    public void StopVideo()
    {
        videoAnim.SetBool(VIDEO_DAPP, true);
        videoAnim.SetBool(VIDEO_APP, false);
    }
    

}

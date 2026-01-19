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



    public void PlayVideo(int stage)
    {
        video.clip = possibleVideo[stage];
        videoAnim.SetBool("videoDapp", false);
        videoAnim.SetBool("videoApp", true);
    }
    public void StopVideo()
    {
        videoAnim.SetBool("videoDapp", true);
        videoAnim.SetBool("videoApp", false);
    }
    
    public IEnumerator chestsActiveness(GameObject self, GameObject nextChest)
    {
        nextChest.SetActive(true);
        yield return new WaitForSeconds(1);
        self.SetActive(false);
    }
}

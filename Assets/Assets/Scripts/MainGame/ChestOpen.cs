using UnityEngine;
using UnityEngine.UI;


public class ChestOpen : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] int stage;
    [SerializeField] float requiredSpeed;
    [SerializeField] string cameraReqs;

    bool openedTruly;

    VideoManager videoMan;

    private void Start()
    {
        videoMan = GameObject.FindGameObjectWithTag("VideoManager").GetComponent<VideoManager>();
    }

    void Update() //wird noch die kurbel
    {
        //requiredSpeed dann mit Kurbel vergleichen.
        OpenTheChest(slider.value); // Slider ist temporär
    }


    // DIE METHODE MUSS NOCH MIT DER KAMERA COMPARISSON VERBUNDEN WERDEN
    void StartCamComparrisson(string camOutput)
    {
        if (camOutput.Contains(cameraReqs))
        {
            OpenTheChest(slider.value); // Slider íst temporär
        }
    }
    

    void OpenTheChest(float degreePerTick)
    {
        float rotTranslate = degreePerTick;
        gameObject.transform.rotation = Quaternion.Euler(degreePerTick, 0, 0);

        if (rotTranslate >= 170f)
        {
            videoMan.PlayVideo(stage);
            openedTruly = true;
            
        }
        else if (rotTranslate < 10f && openedTruly == true)
        {

            videoMan.StopVideo();
            GetComponentInParent<Animator>().SetTrigger("nextLevel");
            
            openedTruly =false;
        }
    }

}

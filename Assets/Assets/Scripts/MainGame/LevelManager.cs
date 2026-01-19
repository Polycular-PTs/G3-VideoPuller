using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private bool resetTheChests;
    private bool waitForReset;

    public GameObject currentChestTemp;
    public GameObject nextChestTemp;

    public void RoundPosition()
    {
        int value = Mathf.RoundToInt(transform.position.x);
        print("value: " + value);
        transform.position = new Vector3(value,0,0);
    }

    public void giveResetInfo(GameObject currentChest, GameObject nextChest)
    {
        currentChestTemp = currentChest;
        nextChestTemp = nextChest;
    }

    public void chestsActiveness()
    {

        nextChestTemp.SetActive(true);
        currentChestTemp.SetActive(false);

    }
}

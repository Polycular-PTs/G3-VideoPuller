using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public void RoundPosition()
    {
        int value = Mathf.RoundToInt(transform.position.x);
        print("value: " + value);
        transform.position = new Vector3(value,0,0);
    }

}

using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class EndScreenGallery : MonoBehaviour
{
    [System.Serializable]
    public struct StageDisplay
    {
        public RawImage actionImage;
        //public RawImage reelImage;
    }

    [SerializeField] StageDisplay[] stageDisplays;

    void OnEnable()
    {
        LoadAllCaptures();
    }

    public void LoadAllCaptures()
    {
        string captureDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "captures"));
        Debug.Log($"Loading images from {captureDir}");

        for (int i = 0; i < stageDisplays.Length; i++)
        {
            string actionPath = Path.Combine(captureDir, $"stage_{i}_action.jpg");
            //string reelPath = Path.Combine(captureDir, $"stage_{i}_reel.jpg");

            if (File.Exists(actionPath))
            {
                stageDisplays[i].actionImage.texture = LoadTextureFromFile(actionPath);
            }

            //if (File.Exists(reelPath))
            //{
                //stageDisplays[i].reelImage.texture = LoadTextureFromFile(reelPath);
            //}
        }
    }

    private Texture2D LoadTextureFromFile(string filePath)
    {
        byte[] bytes = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(bytes); // Auto-resizes texture to match file dimensions
        return texture;
    }
}
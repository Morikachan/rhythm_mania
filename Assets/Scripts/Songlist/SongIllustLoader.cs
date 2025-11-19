using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SongIllustLoader : MonoBehaviour
{
    public static SongIllustLoader instance { get; private set; }

    private const string SONG_ILLUST_PATH = @"C:\xampp\htdocs\rhythm_mania\Assets\UI\song_illust\";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    public void LoadSongIllustration(Image targetImage, string fileName)
    {
        if (targetImage == null)
        {
            Debug.LogError("Target Image component is null. Cannot load illustration.");
            return;
        }

        string fullPath = Path.Combine(SONG_ILLUST_PATH, fileName);

        if (File.Exists(fullPath))
        {
            Sprite newSprite = LoadIllustFromFile(fullPath);
            if (newSprite != null)
            {
                targetImage.sprite = newSprite;
            }
            else
            {
                Debug.LogError($"Failed to convert image to Sprite for path: {fullPath}");
            }
        }
        else
        {
            Debug.LogError($"Card illustration file not found at path: {fullPath}");
        }
    }

    private Sprite LoadIllustFromFile(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);

        if (texture.LoadImage(fileData))
        {
            return Sprite.Create(
                texture,
                new Rect(0.0f, 0.0f, texture.width, texture.height),
                Vector2.zero,
                100f
            );
        }
        return null;
    }
}   
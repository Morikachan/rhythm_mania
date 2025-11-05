using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class CardLoader : MonoBehaviour
{
    public static CardLoader Instance { get; private set; }

    private const string BASE_PATH = @"C:\xampp\htdocs\rhythm_mania\Assets\Cards\card_illust\";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    public void LoadCardIllustration(int cardId, Image targetImage)
    {
        if (targetImage == null)
        {
            Debug.LogError("Target Image component is null. Cannot load illustration.");
            return;
        }

        string fileName = $"card_{cardId}.jpg";
        string fullPath = Path.Combine(BASE_PATH, fileName);

        if (File.Exists(fullPath))
        {
            Sprite newSprite = LoadSpriteFromFile(fullPath);
            if (newSprite != null)
            {
                targetImage.sprite = newSprite;
                //targetImage.SetNativeSize();
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

    private Sprite LoadSpriteFromFile(string filePath)
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
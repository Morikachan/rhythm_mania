using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class PlayerCardIllustLoader : MonoBehaviour
{
    public static PlayerCardIllustLoader instance { get; private set; }

    private const string GAME_ICON_PATH = @"C:\xampp\htdocs\rhythm_mania\Assets\Cards\card_game\";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    public void LoadPlayerIllustration(Image targetImage, string fileName, Action onLoaded = null)
    {
        if (targetImage == null)
        {
            Debug.LogError("Target Image component is null. Cannot load illustration.");
            return;
        }

        string fullPath = Path.Combine(GAME_ICON_PATH, fileName);

        if (File.Exists(fullPath))
        {
            Sprite newSprite = LoadIllustFromFile(fullPath);
            if (newSprite != null)
            {
                targetImage.sprite = newSprite;
                if (onLoaded != null)
                {
                    onLoaded.Invoke();
                }
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

//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.Networking;
//using System.Collections;
//using System;

//public class PlayerCardIllustLoader : MonoBehaviour
//{
//    public static PlayerCardIllustLoader instance { get; private set; }

//    private const string GAME_ICON_PATH = "http://153.126.183.193/student/k248010/ps_game/src/cards/card_game/";

//    private void Awake()
//    {
//        if (instance != null && instance != this)
//        {
//            Destroy(this.gameObject);
//            return;
//        }
//        instance = this;
//        DontDestroyOnLoad(this.gameObject);
//    }

//    public void LoadPlayerIllustration(Image targetImage, string fileName, Action onLoaded = null)
//    {
//        if (targetImage == null)
//        {
//            Debug.LogError("Target Image is null.");
//            return;
//        }

//        string fullUrl = GAME_ICON_PATH + fileName;

//        if (gameObject.activeInHierarchy)
//        {
//            StartCoroutine(DownloadImage(targetImage, fullUrl, onLoaded));
//        }
//    }

//    private IEnumerator DownloadImage(Image targetImage, string url, Action onLoaded)
//    {
//        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
//        {
//            yield return uwr.SendWebRequest();

//            if (uwr.result == UnityWebRequest.Result.Success)
//            {
//                if (targetImage == null) yield break;

//                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

//                Sprite newSprite = Sprite.Create(
//                    texture,
//                    new Rect(0.0f, 0.0f, texture.width, texture.height),
//                    Vector2.zero,
//                    100f
//                );

//                targetImage.sprite = newSprite;

//                onLoaded?.Invoke();
//            }
//            else
//            {
//                Debug.LogError($"Loading Error (PlayerCard): {uwr.error} | URL: {url}");
//            }
//        }
//    }
//}
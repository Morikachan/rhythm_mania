//using UnityEngine;
//using UnityEngine.UI;
//using System.IO;

//public class SongIllustLoader : MonoBehaviour
//{
//    public static SongIllustLoader instance { get; private set; }

//    private const string SONG_ILLUST_PATH = @"C:\xampp\htdocs\rhythm_mania\Assets\UI\song_illust\";

//    private void Awake()
//    {
//        if (instance != null && instance != this)
//        {
//            Destroy(this.gameObject);
//            return;
//        }

//        instance = this;
//    }

//    public void LoadSongIllustration(Image targetImage, string fileName)
//    {
//        if (targetImage == null)
//        {
//            Debug.LogError("Target Image component is null. Cannot load illustration.");
//            return;
//        }

//        string fullPath = Path.Combine(SONG_ILLUST_PATH, fileName);

//        if (File.Exists(fullPath))
//        {
//            Sprite newSprite = LoadIllustFromFile(fullPath);
//            if (newSprite != null)
//            {
//                targetImage.sprite = newSprite;
//            }
//            else
//            {
//                Debug.LogError($"Failed to convert image to Sprite for path: {fullPath}");
//            }
//        }
//        else
//        {
//            Debug.LogError($"Card illustration file not found at path: {fullPath}");
//        }
//    }

//    private Sprite LoadIllustFromFile(string filePath)
//    {
//        byte[] fileData = File.ReadAllBytes(filePath);
//        Texture2D texture = new Texture2D(2, 2);

//        if (texture.LoadImage(fileData))
//        {
//            return Sprite.Create(
//                texture,
//                new Rect(0.0f, 0.0f, texture.width, texture.height),
//                Vector2.zero,
//                100f
//            );
//        }
//        return null;
//    }
//}   


using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System;

public class SongIllustLoader : MonoBehaviour {
    public static SongIllustLoader instance { get; private set; }

    private const string SONG_ILLUST_PATH = "http://153.126.183.193/student/k248010/ps_game/src/cards/song_illust/";

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
    }

    public void LoadSongIllustration(Image targetImage, string fileName, Action onLoaded = null)
    {
        if(targetImage == null)
        {
            Debug.LogError("Target Image is null.");
            return;
        }

        string fullUrl = SONG_ILLUST_PATH + fileName;

        StartCoroutine(DownloadImage(targetImage, fullUrl, onLoaded));
    }

    private IEnumerator DownloadImage(Image targetImage, string url, Action onLoaded)
    {
        using(UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if(uwr.result == UnityWebRequest.Result.Success)
            {
                if(targetImage == null) yield break;

                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

                Sprite newSprite = Sprite.Create(
                    texture,
                    new Rect(0.0f, 0.0f, texture.width, texture.height),
                    Vector2.zero,
                    100f
                );

                targetImage.sprite = newSprite;

                onLoaded?.Invoke();
            }
            else
            {
                Debug.LogError($"Loading Error (Song): {uwr.error} | URL: {url}");
            }
        }
    }
}
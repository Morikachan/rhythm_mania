using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System;

public class CardLoaderOnline : MonoBehaviour
{
    public static CardLoaderOnline Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public void LoadCardIllustration(Image targetImage, string basePath, string fileName, Action onLoaded = null)
    {
        if (targetImage == null) return;

        string fullUrl = basePath + fileName;
        StartCoroutine(DownloadImage(targetImage, fullUrl, onLoaded));
    }

    private IEnumerator DownloadImage(Image targetImage, string url, Action onLoaded)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                targetImage.sprite = newSprite;

                if (onLoaded != null)
                {
                    onLoaded.Invoke();
                }
            }
            else
            {
                Debug.LogError("„O„Š„y„q„{„p „x„p„s„‚„…„x„{„y: " + uwr.error + " URL: " + url);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LanguageSwitcher : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI buttonText;
    private int language;

    void Start()
    {
        language = PlayerPrefs.GetInt("language", 0);
        UpdateVisuals();
    }

    public void ToggleLanguage()
    {
        language = (language == 0) ? 1 : 0;

        PlayerPrefs.SetInt("language", language);

        SceneManager.LoadScene("Title");
    }

    private void UpdateVisuals()
    {
        if(buttonText != null)
        {
            buttonText.text = (language == 0) ? "EN" : "JP";
        }
    }
}
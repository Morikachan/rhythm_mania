using TMPro;
using UnityEngine;

public class LanguageText : MonoBehaviour {
    public int language;
    public string[] text;

    [Header("Font Settings")]
    [SerializeField] private TMP_FontAsset enFont; // LiberationSans
    [SerializeField] private TMP_FontAsset jpFont; // NotoSansJP
    [SerializeField] public int jpFontSize = 24;

    private TextMeshProUGUI textLine;

    void Start()
    {
        textLine = GetComponent<TextMeshProUGUI>();

        language = PlayerPrefs.GetInt("language", 0);

        if(text != null && language < text.Length)
        {
            textLine.text = text[language];
        }

        ApplyFont();
    }

    private void ApplyFont()
    {
        if(textLine == null) return;

        textLine.font = (language == 0) ? enFont : jpFont;

        if(language == 1)
        {
            textLine.fontSize = jpFontSize;
            textLine.fontStyle = FontStyles.Bold;
        }
    }
}
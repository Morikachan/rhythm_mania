using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SongPanelController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI songNameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI bpmText;
    public Image backgroundImage;

    [HideInInspector] public SongListManager.Song songData;

    private SongListManager manager;

    private Color defaultColor = Color.white;
    private Color selectedColor = new Color(1.0f, 0.537f, 0.690f);

    private void Awake()
    {
        backgroundImage = GetComponent<Image>();

        if (backgroundImage != null)
        {
            // Now it's safe to use the reference
            defaultColor = backgroundImage.color;
        }

        Button button = GetComponent<Button>();

        if (button != null)
        {
            // Assign the OnClick listener
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnPanelClicked);
        }
    }

    // Create panel
    public void Setup(SongListManager.Song song, SongListManager listManager)
    {
        songData = song;
        manager = listManager;

        songNameText.text = song.song_name;
        levelText.text = song.song_level.ToString();
        bpmText.text = song.song_bpm.ToString();

        SetSelected(false);
    }

    // Button on the panel is clicked
    private void OnPanelClicked()
    {
        // When clicked, manager handles selection and deselection
        manager.SelectSong(this, songData);
    }

    public void SetSelected(bool isSelected)
    {
        if (backgroundImage == null) return;

        if (isSelected)
        {
            backgroundImage.color = selectedColor;
            print("Selected");
        }
        else
        {
            backgroundImage.color = defaultColor;
        }
    }
}

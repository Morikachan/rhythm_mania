using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultiHPBarUI : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI hpText;

    void Update()
    {
        UpdateHP();
    }

    void UpdateHP()
    {
        var gm = MultiGameManager.instance;
        if (gm == null) return;

        if (!gm.players.ContainsKey(gm.localActor))
            return;

        var data = gm.players[gm.localActor];

        slider.maxValue = data.maxHP;
        slider.value = data.hp;

        if (hpText != null)
            hpText.text = data.hp.ToString();
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HPBarUI : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI userHpText;

    void Start()
    {
        slider.maxValue = HPManager.instance.maxHP;
        slider.value = HPManager.instance.maxHP;
        UpdateHP();
    }

    public void UpdateHP()
    {
        int hp = HPManager.instance.currentHP;

        slider.value = hp;

        if(userHpText != null)
            userHpText.text = $"{hp}";
    }
}

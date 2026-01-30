using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ProfileSettingsManager : MonoBehaviour
{
    public CardSelectPopup cardPopup;
    public Button changeCardButton;
    public CardInventoryService inventoryService;

    private void Start()
    {
        changeCardButton.onClick.AddListener(OnChangeCardClicked);
    }
    public void OnChangeCardClicked()
    {
        cardPopup.Open(inventoryService.AllCards);
    }
}

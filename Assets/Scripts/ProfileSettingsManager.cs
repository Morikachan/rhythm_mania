using UnityEngine;

public class ProfileSettingsManager : MonoBehaviour
{
    public CardSelectPopup cardPopup;
    public CardInventoryService inventoryService;

    public void OnChangeCardClicked()
    {
        cardPopup.Open(inventoryService.AllCards);
    }
}

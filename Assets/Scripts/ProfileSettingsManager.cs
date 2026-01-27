using System.Collections.Generic;
using UnityEngine;

public class ProfileSettingsManager : MonoBehaviour
{
    public CardSelectPopup cardPopup;
    public List<CardData> allCards;

    public void OnChangeCardClicked()
    {
        cardPopup.Open(allCards);
    }
}

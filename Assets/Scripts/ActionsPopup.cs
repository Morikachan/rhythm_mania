using UnityEngine;
using UnityEngine.UI;

public class ActionsPopup : MonoBehaviour
{
    public GameObject popup;

    public void OnClickOpenPopup()
    {
        popup.SetActive(true);
    }
    public void OnClickClosePopup()
    {
        popup.SetActive(false);
    }
}
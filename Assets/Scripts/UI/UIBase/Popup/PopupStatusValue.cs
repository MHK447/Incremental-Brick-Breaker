using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

[UIPath("UI/Popup/PopupStatusValue")]
public class PopupStatusValue : UIBase
{
    [SerializeField]
    private TextMeshProUGUI StatusValueText;

    [SerializeField]
    private Image StatusImg;

    private Coroutine hideCoroutine;


    public void SetStatusValue(int value)
    {
        // If this popup is already shown, reset animation and auto-hide timer.
        if (hideCoroutine != null)
        {
            GameRoot.Instance.StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        ProjectUtility.SetActiveCheck(this.gameObject, true);

        var symbol = value > 0 ? "+" : "-";

        StatusValueText.text = $"{symbol}{value}";

        StatusValueText.color = value > 0 ? Color.green : Color.red;

        StatusImg.sprite = value > 0 ? AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Icon_StattusUp")
        : AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Icon_StattusDown");

        // Restart show animation from the beginning.
        ShowImediately();


        hideCoroutine = GameRoot.Instance.WaitTimeAndCallback(3f, () =>
        {
            hideCoroutine = null;
            Hide();
            ProjectUtility.SetActiveCheck(this.gameObject, false);
        });
    }

}
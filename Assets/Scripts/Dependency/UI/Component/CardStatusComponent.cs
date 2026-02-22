using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class CardStatusComponent : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI StatusNameText;

    [SerializeField]
    private TextMeshProUGUI StatusValueText;

    [SerializeField]
    private TextMeshProUGUI PlusValueText;

    [SerializeField]
    private Image StatusImg;


    public void Set(int cardidx, int statustype, int grade = 1)
    {
        StatusValueText.text = GameRoot.Instance.CardSystem.GetStatusValue(cardidx, (UnitStatusType)statustype, grade).ToString("F1");

        var td = Tables.Instance.GetTable<CardInfo>().GetData(cardidx);

        if (td == null) return;

        var findidx = td.card_upgrade_type.FindIndex(x => x == (int)statustype);

        StatusImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, $"Common_Icon_Status_{statustype}");

        StatusNameText.text = Tables.Instance.GetTable<Localize>().GetString($"str_unit_status_type_{(int)statustype}");



        switch (statustype)
        {
            case (int)UnitStatusType.Attack:
                StatusValueText.text = GameRoot.Instance.CardSystem.GetStatusValue(cardidx, (UnitStatusType)statustype, grade).ToString();

                if (PlusValueText != null)
                    PlusValueText.text = $"+{td.card_upgrade_increase[findidx]}";
                break;
            case (int)UnitStatusType.Hp:
                StatusValueText.text = GameRoot.Instance.CardSystem.GetStatusValue(cardidx, (UnitStatusType)statustype, grade).ToString();

                if (PlusValueText != null)
                    PlusValueText.text = $"+{td.card_upgrade_increase[findidx]}";
                break;
            case (int)UnitStatusType.AttackSpeed:
                StatusValueText.text = $"{GameRoot.Instance.CardSystem.GetStatusValue(cardidx, (UnitStatusType)statustype, grade).ToString("F1")}";

                if (PlusValueText != null)
                    PlusValueText.text = $"+{td.card_upgrade_increase[findidx]}";
                break;
            case (int)UnitStatusType.AttackRange:
                StatusValueText.text = $"{GameRoot.Instance.CardSystem.GetStatusValue(cardidx, (UnitStatusType)statustype, grade).ToString("F1")}";

                if (PlusValueText != null)
                    PlusValueText.text = $"+{td.card_upgrade_increase[findidx] / 10}";
                break;
            case (int)UnitStatusType.Shield:
                StatusValueText.text = GameRoot.Instance.CardSystem.GetStatusValue(cardidx, (UnitStatusType)statustype, grade).ToString();

                if (PlusValueText != null)
                    PlusValueText.text = $"+{td.card_upgrade_increase[findidx]}";
                break;
            case (int)UnitStatusType.SILVERCOIN:
                StatusValueText.text = GameRoot.Instance.CardSystem.GetStatusValue(cardidx, (UnitStatusType)statustype, grade).ToString();

                if (PlusValueText != null)
                    PlusValueText.text = $"+{td.card_upgrade_increase[findidx]}";
                break;
            case (int)UnitStatusType.HpHeal:
                StatusValueText.text = GameRoot.Instance.CardSystem.GetStatusValue(cardidx, (UnitStatusType)statustype, grade).ToString();

                if (PlusValueText != null)
                    PlusValueText.text = $"+{td.card_upgrade_increase[findidx]}";
                break;
        }

    }

}


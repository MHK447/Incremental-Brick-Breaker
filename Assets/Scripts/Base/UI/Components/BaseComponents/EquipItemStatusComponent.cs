using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class EquipItemStatusComponent : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI PlusDescText;


    [SerializeField]
    private TextMeshProUGUI BaseValueText;

    [SerializeField]
    private TextMeshProUGUI StatusNameText;

    [SerializeField]
    private Image StatusImg;


    public void Set(int abilitytype , int basevalue , int plusvalue)
    {
        StatusImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, $"Common_Icon_Status_{abilitytype}");

        StatusNameText.text = Tables.Instance.GetTable<Localize>().GetString($"str_equipment_ability_desc_{abilitytype}");

        BaseValueText.text = basevalue.ToString();
        PlusDescText.text = $"+{plusvalue}";

    }


}


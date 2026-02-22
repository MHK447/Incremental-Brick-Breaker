using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

[UIPath("UI/Popup/PopupMergeWeaponInfo", true)]
public class PopupMergeWeaponInfo : UIBase
{
    [SerializeField]
    private TextMeshProUGUI WeaponNameText;

    [SerializeField]
    private TextMeshProUGUI WeaponDescText;

    [SerializeField]
    private TextMeshProUGUI WeaponLevelText;

    [SerializeField]
    private Image EquipImg;

    [SerializeField]
    private List<CardStatusComponent> CardStatusComponentList = new List<CardStatusComponent>();

    private TileWeaponComponent TileWeapon;

    private int EquipIdx = 0;


    public void Set(TileWeaponComponent tileweapon)
    {
        EquipIdx = tileweapon.EquipIdx;

        TileWeapon = tileweapon;

        var td = Tables.Instance.GetTable<EquipInfo>().GetData(EquipIdx);

        if(td != null)
        {
            EquipImg.sprite = td.idx == 2 ?
             AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, $"Common_Card_Icon_2_{tileweapon.Grade}")
             : AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_CommonWeapon, $"Common_Weapon_Type_{tileweapon.EquipIdx}_{tileweapon.Grade}");
            

            EquipImg.SetNativeSize();

            WeaponNameText.text = Tables.Instance.GetTable<Localize>().GetString($"card_name_{EquipIdx}");
            WeaponLevelText.text = $"Lv.{tileweapon.Grade}";
            WeaponDescText.text = Tables.Instance.GetTable<Localize>().GetFormat($"str_equipinfo_desc_{EquipIdx}" , (td.cooltime * 0.01f).ToString());


            SetStatus();
        }
    }



    public void SetStatus()
    {
        var td = Tables.Instance.GetTable<CardInfo>().GetData(EquipIdx);

        if(td == null )return;

        for(int i = 0; i < CardStatusComponentList.Count; i++)
        {
            ProjectUtility.SetActiveCheck(CardStatusComponentList[i].gameObject, false);
        }

        for(int i = 0; i < td.card_upgrade_type.Count; i++)
        {
            var statusType = td.card_upgrade_type[i];
            var statusValue = GameRoot.Instance.UnitSystem.GetUnitStatus(EquipIdx, (UnitStatusType)statusType);

            CardStatusComponentList[i].Set(EquipIdx, statusType, TileWeapon.Grade);
            ProjectUtility.SetActiveCheck(CardStatusComponentList[i].gameObject, true);
        }
    }
}


using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class RevivalComponent : MonoBehaviour
{
    [SerializeField]
    private AdsButton AdsBtn;

    [SerializeField]
    private Image WeaponImg;

    [SerializeField]
    private TextMeshProUGUI WeaponNameText;

    [SerializeField]
    private TextMeshProUGUI TotalDmgText;

    private int WeaponIdx = 0;


    void Awake()
    {
        AdsBtn.AddListener(TpMaxProp.AdRewardType.Revival, OnClickRevival);
    }


    public void Set(int weaponidx)
    {
        WeaponIdx = weaponidx;

        var td = Tables.Instance.GetTable<UnitInfo>().GetData(weaponidx);

        WeaponImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Upgrade, $"Upgrade_icon_{weaponidx}");
        //WeaponNameText.text = Tables.Instance.GetTable<Localize>().GetString(td.name);
    }
    


    public void OnClickRevival()
    {
        GameRoot.Instance.UISystem.GetUI<PopupRevival>().Hide();
        //GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.PlayerUnitGroup.RevivalUnit(WeaponIdx);
    }

}


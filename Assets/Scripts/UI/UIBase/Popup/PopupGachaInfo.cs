using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[UIPath("UI/Popup/PopupGachaInfo")]
public class PopupGachaInfo : UIBase
{
    [SerializeField]
    private List<GachaInfoComponent> GachaInfoComponentList = new List<GachaInfoComponent>();


    [SerializeField]
    private Button UpgradeBtn;



    protected override void Awake()
    {
        base.Awake();

        UpgradeBtn.onClick.AddListener(OnUpgradeBtnClick);
    }



    public void Init()
    {
        var tdlist = Tables.Instance.GetTable<EquipmentGachaInfo>().DataList.ToList();

        foreach(var gachaInfoComponent in GachaInfoComponentList)
        {
            ProjectUtility.SetActiveCheck(gachaInfoComponent.gameObject, false);
        }


        foreach(var td in tdlist)
        {
            ProjectUtility.SetActiveCheck(GachaInfoComponentList[td.level - 1].gameObject, true);
            GachaInfoComponentList[td.level - 1].Set(td.level);
        }
    }

    void OnUpgradeBtnClick()
    {
        

    }
}


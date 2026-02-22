using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UniRx;
using TMPro;

public class HeroItemComponent : MonoBehaviour
{
    [SerializeField]
    private GameObject LockObj;

    [SerializeField]
    private GameObject EquipItemRoot;

    [SerializeField]
    private HeroItemImg HeroItemImg;

    [SerializeField]
    private EquipAttributeComponent EquipAttributeComponent;

    [SerializeField]
    private GameObject EquipLineRoot;

    [SerializeField]
    private Button HeroEquipBtn;

    [SerializeField]
    private TextMeshProUGUI ItemLevelText;

    private int HeroItemIdx = 0;

    private HeroItemData HeroItemData = null;
    private CompositeDisposable disposables = new CompositeDisposable();

    void Awake()
    {
        HeroEquipBtn.onClick.AddListener(OnClickHeroEquip);
    }


    public void Set(int itemidx, HeroItemData heroitemdata = null)
    {
        HeroItemIdx = itemidx;

        var td = Tables.Instance.GetTable<HeroItemInfo>().GetData(itemidx);

        ProjectUtility.SetActiveCheck(LockObj, heroitemdata == null);

        ProjectUtility.SetActiveCheck(EquipItemRoot, false);

        int herograde = heroitemdata == null ? 1 : heroitemdata.Grade;

        EquipAttributeComponent.Set(herograde, td.item_equip_type);

        if (heroitemdata != null)
        {
            HeroItemData = heroitemdata;

            var equipdata = GameRoot.Instance.HeroSystem.FindHeroEquipItemData(td.item_equip_type);

            disposables.Clear();

            if (equipdata != null)
            {
                ItemLevelText.text = Tables.Instance.GetTable<Localize>().GetFormat("str_level_desc", equipdata.Level.Value);

                equipdata.Level.Subscribe(x =>
                           {
                               ItemLevelText.text = Tables.Instance.GetTable<Localize>().GetFormat("str_level_desc", x);
                           }).AddTo(disposables);
            }

            HeroItemData.Isequip.Subscribe(x => HeroImgSet()).AddTo(disposables);
        }
    }

    public void HeroImgSet()
    {
        if (HeroItemData != null)
        {
            ProjectUtility.SetActiveCheck(EquipItemRoot, true);

            HeroItemImg.Set(HeroItemData);

            var heroequipdata = GameRoot.Instance.UserData.Herogroudata.Equipheroitems.Find(x => x.Heroitemdata == HeroItemData);

            bool isequip = HeroItemData != null ? HeroItemData.Isequip.Value : false;

            ProjectUtility.SetActiveCheck(EquipLineRoot, isequip);
        }
    }


    public void OnClickHeroEquip()
    {
        if (LockObj.gameObject.activeSelf)
        {
            return;
        }

        GameRoot.Instance.UISystem.OpenUI<PopupEquipItemManage>(popup => popup.Init(HeroItemData));
    }

    void OnDestroy()
    {
        disposables.Clear();
    }

    void OnDisable()
    {
        disposables.Clear();
    }
}


using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UniRx;
using TMPro;

public class EquipItemComponent : MonoBehaviour
{
    [SerializeField]
    private Image ItemImg;


    [SerializeField]
    private Image EquipGradeBgImg;

    [SerializeField]
    private GameObject NoneEquipFrameRoot;

    [SerializeField]
    private GameObject EquipFrameRoot;

    [SerializeField]
    private EquipAttributeComponent AttributeComponent;

    [SerializeField]
    public HeroEquipType HeroEquipType;

    [SerializeField]
    private Button InfoBtn;

    [SerializeField]
    private Image AttributeImg;

    [SerializeField]
    private TextMeshProUGUI LevelText;

    [SerializeField]
    private GameObject HeroUpgradeReddot;

    private HeroEquipItemData findHeroEquipItemData;

    private CompositeDisposable disposables = new CompositeDisposable();

    void Awake()
    {
        InfoBtn.onClick.AddListener(OnClickInfo);
    }

    public void Init()
    {
        findHeroEquipItemData = GameRoot.Instance.HeroSystem.FindHeroEquipItemData((int)HeroEquipType);

        disposables.Clear();

        findHeroEquipItemData.Isequip.Subscribe(x => UpdateInfo()).AddTo(disposables);


        findHeroEquipItemData.Level.Subscribe(x =>
        {
            LevelText.text = Tables.Instance.GetTable<Localize>().GetFormat("str_training_level", x);
        }).AddTo(disposables);

        findHeroEquipItemData.Level.Subscribe(x => {
            ReddotCheck();
        }).AddTo(disposables);
        
        GameRoot.Instance.UserData.Money.Subscribe(x =>
        {
            ReddotCheck();
        }).AddTo(disposables);


        //findHeroEquipItemData.Level
    }

    public void ReddotCheck()
    {
        ProjectUtility.SetActiveCheck(HeroUpgradeReddot, false);

        if (findHeroEquipItemData.Heroitemdata == null) return;

        var td = Tables.Instance.GetTable<HeroItemInfo>().GetData(findHeroEquipItemData.Heroitemdata.Heroitemidx);

        if(td == null) return;

        var finditemdata = GameRoot.Instance.ItemSystem.GetItemData((int)ItemSystem.ItemType.EquipUpgradeItem, td.item_equip_type);

        var allfinditemdata = GameRoot.Instance.ItemSystem.GetItemData((int)ItemSystem.ItemType.EquipUpgradeItem, 0);

        var UpgradeGoldCost = GameRoot.Instance.HeroSystem.equip_item_level_gold_cost * findHeroEquipItemData.Level.Value;
        var UpgradeMaterialCount = GameRoot.Instance.HeroSystem.equip_item_level_count * findHeroEquipItemData.Level.Value;

        var curupgradecountvalue = allfinditemdata.Itemcnt.Value + finditemdata.Itemcnt.Value;

        ProjectUtility.SetActiveCheck(HeroUpgradeReddot, UpgradeGoldCost <= GameRoot.Instance.UserData.Money.Value && UpgradeMaterialCount <= curupgradecountvalue);
    }


    public void UpdateInfo()
    {
        findHeroEquipItemData = GameRoot.Instance.HeroSystem.FindHeroEquipItemData((int)HeroEquipType);

        if (!findHeroEquipItemData.Isequip.Value)
        {
            ProjectUtility.SetActiveCheck(NoneEquipFrameRoot, true);
            ProjectUtility.SetActiveCheck(EquipFrameRoot, false);
            return;
        }

        ProjectUtility.SetActiveCheck(NoneEquipFrameRoot, false);
        ProjectUtility.SetActiveCheck(EquipFrameRoot, true);


        AttributeImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common,
        $"Common_Icon_AttributeItem_{findHeroEquipItemData.Heroitemdata.Heroitemidx}");


        LevelText.text = Tables.Instance.GetTable<Localize>().GetFormat("str_training_level", findHeroEquipItemData.Level);

        EquipItem();
    }

    public void OnClickInfo()
    {
        if (findHeroEquipItemData.Heroitemdata == null) return;

        var td = Tables.Instance.GetTable<HeroItemInfo>().GetData(findHeroEquipItemData.Heroitemdata.Heroitemidx);
        if (td != null)
        {
            GameRoot.Instance.UISystem.OpenUI<PopupEquipItemManage>(popup => popup.Init(findHeroEquipItemData.Heroitemdata), null);
        }
    }

    public void EquipItem()
    {
        if (findHeroEquipItemData == null) return;

        var td = Tables.Instance.GetTable<HeroItemInfo>().GetData(findHeroEquipItemData.Heroitemdata.Heroitemidx);
        if (td != null)
        {
            EquipGradeBgImg.color = Config.Instance.GetImageColor($"item_grade_color_{findHeroEquipItemData.Heroitemdata.Grade}");
            AttributeComponent.Set(findHeroEquipItemData.Heroitemdata.Grade, findHeroEquipItemData.Heroitemtype);

            ItemImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_EquipItem, td.item_img);
        }

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


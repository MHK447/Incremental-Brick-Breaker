using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

[UIPath("UI/Popup/PopupEquipItemManage")]
public class PopupEquipItemManage : CommonUIBase
{
    [SerializeField]
    private TextMeshProUGUI EquipItemNameText;

    [SerializeField]
    private TextMeshProUGUI EquipGradeText;

    [SerializeField]
    private Image ItemImg;

    [SerializeField]
    private Image BgImg;

    [SerializeField]
    private EquipAttributeComponent EquipAttributeComponent;

    [SerializeField]
    private TextMeshProUGUI LevelText;

    [SerializeField]
    private TextMeshProUGUI ItemSetEffectText;

    [SerializeField]
    private List<TextMeshProUGUI> ItemLevelEffectTextList = new List<TextMeshProUGUI>();

    [SerializeField]
    private List<GameObject> LockImgList = new List<GameObject>();

    [SerializeField]
    private List<GameObject> UnLockImgList = new List<GameObject>();

    [SerializeField]
    private Button UnEquipBtn;

    [SerializeField]
    private Button EquipBtn;

    [SerializeField]
    private Button UpgradeBtn;

    [SerializeField]
    private List<EquipItemStatusComponent> EquipItemStatusComponentList = new List<EquipItemStatusComponent>();

    [SerializeField]
    private GameObject SetLockDimObj;

    [SerializeField]
    private List<GameObject> UpgradeEffectList = new List<GameObject>();

    [SerializeField]
    private Image EquimentImg;

    [SerializeField]
    private Image EquipmentMaterialImg;



    private HeroItemData HeroItemData = null;

    private int EquipItemIdx = 0;


    //Upgrade Info

    private int UpgradeGoldCost = 0;

    private int UpgradeMaterialCount = 0;

    [SerializeField]
    private TextMeshProUGUI UpgradeGoldCostText;

    [SerializeField]
    private TextMeshProUGUI UpgradeMaterialCountText;


    protected override void Awake()
    {
        base.Awake();

        EquipBtn.onClick.AddListener(OnClickEquip);
        UpgradeBtn.onClick.AddListener(OnClickUpgrade);
        UnEquipBtn.onClick.AddListener(OnClickUnEquip);
    }

    public void Init(HeroItemData heroitemdata)
    {
        HeroItemData = heroitemdata;
        Set(HeroItemData);

        for (int i = 0; i < UpgradeEffectList.Count; i++)
        {
            ProjectUtility.SetActiveCheck(UpgradeEffectList[i], false);
        }
    }



    public void Set(HeroItemData heroitemdata)
    {
        HeroItemData = heroitemdata;

        EquipItemIdx = HeroItemData.Heroitemidx;

        var td = Tables.Instance.GetTable<HeroItemInfo>().GetData(EquipItemIdx);

        if (td != null)
        {
            var finddata = GameRoot.Instance.HeroSystem.FindHeroEquipItemData(td.item_equip_type);

            var level = finddata.Level.Value;

            BgImg.color = Config.Instance.GetImageColor($"item_grade_color_{HeroItemData.Grade}");
            ItemImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_EquipItem, td.item_img);
            EquipItemNameText.text = Tables.Instance.GetTable<Localize>().GetString(td.name);

            LevelText.text = Tables.Instance.GetTable<Localize>().GetFormat("str_level_desc", level);

            EquipmentMaterialImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_EquipItem, $"Equip_Upgrade_Item_{td.item_equip_type}");

            EquipGradeText.text = Tables.Instance.GetTable<Localize>().GetString($"item_grade_name_{HeroItemData.Grade}");

            EquipAttributeComponent.Set(HeroItemData.Grade, td.item_equip_type);

            ProjectUtility.SetActiveCheck(UnEquipBtn.gameObject, HeroItemData.Isequip.Value);
            ProjectUtility.SetActiveCheck(EquipBtn.gameObject, !HeroItemData.Isequip.Value);

            EquimentImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, $"Common_Icon_AttributeItem_{td.item_equip_type}");



            for (int i = 0; i < LockImgList.Count; ++i)
            {
                ProjectUtility.SetActiveCheck(UnLockImgList[i].gameObject, i + 1 <= HeroItemData.Grade);
                ProjectUtility.SetActiveCheck(LockImgList[i].gameObject, i + 1 > HeroItemData.Grade);
            }

            for (int i = 0; i < ItemLevelEffectTextList.Count; i++)
            {
                var textColor = ItemLevelEffectTextList[i].color;
                textColor.a = i + 1 <= HeroItemData.Grade ? 1f : 0.5f;

                ItemLevelEffectTextList[i].color = textColor;
                ItemLevelEffectTextList[i].text = Tables.Instance.GetTable<Localize>().
                GetFormat($"item_grade_abilitytype_desc_{td.item_grade_ability_type[i]}", td.item_grade_ability_value[i]);
            }


            ProjectUtility.SetActiveCheck(SetLockDimObj, GameRoot.Instance.HeroSystem.GetCurSetEquipCheck() != td.item_set_type);


            foreach (var status in EquipItemStatusComponentList)
            {
                ProjectUtility.SetActiveCheck(status.gameObject, false);
            }

            for (int i = 0; i < td.item_ability_type.Count; i++)
            {
                var buffvalue = GameRoot.Instance.HeroSystem.GetHeroItemStatusValue(HeroItemData, td.item_ability_type[i]);

                EquipItemStatusComponentList[i].Set(td.item_ability_type[i], buffvalue, td.levelup_increase_value[i]);
                ProjectUtility.SetActiveCheck(EquipItemStatusComponentList[i].gameObject, true);
            }


            SetUpgradeInfo();
        }
    }



    public void OnClickEquip()
    {
        var td = Tables.Instance.GetTable<HeroItemInfo>().GetData(EquipItemIdx);
        if (td != null)
        {
            var finddata = GameRoot.Instance.HeroSystem.FindHeroEquipItemData(td.item_equip_type);

            if (finddata != null)
            {
                if (finddata.Heroitemdata != null)
                {
                    finddata.Heroitemdata.Isequip.Value = false;
                }

                finddata.Heroitemdata = HeroItemData;
                finddata.Isequip.Value = false;
                finddata.Isequip.Value = true;
                finddata.Heroitemdata.Isequip.Value = true;

                Hide();
            }

        }
    }

    public void OnClickUnEquip()
    {
        var td = Tables.Instance.GetTable<HeroItemInfo>().GetData(EquipItemIdx);
        if (td != null)
        {
            HeroItemData.Isequip.Value = false;
            var finddata = GameRoot.Instance.HeroSystem.FindHeroEquipItemData(td.item_equip_type);
            if (finddata != null)
            {
                finddata.Isequip.Value = false;
                finddata.Heroitemdata = null;
            }
            Hide();
        }
    }


    public void OnClickUpgrade()
    {
        if (IsUpgradeCheck)
        {
            var td = Tables.Instance.GetTable<HeroItemInfo>().GetData(HeroItemData.Heroitemidx);

            if (td != null)
            {
                var finditemdata = GameRoot.Instance.ItemSystem.GetItemData((int)ItemSystem.ItemType.EquipUpgradeItem, td.item_equip_type);
                var findallitemdata = GameRoot.Instance.ItemSystem.GetItemData((int)ItemSystem.ItemType.EquipUpgradeItem, 0);


                if (finditemdata != null)
                {

                    if (finditemdata.Itemcnt.Value >= UpgradeMaterialCount)
                    {
                        finditemdata.Itemcnt.Value -= UpgradeMaterialCount;
                    }
                    else 
                    {
                        var diff = UpgradeMaterialCount - finditemdata.Itemcnt.Value;

                        findallitemdata.Itemcnt.Value -= diff;
                        finditemdata.Itemcnt.Value = 0;
                    }


                    SoundPlayer.Instance.PlaySound("effect_contents_open");

                    GameRoot.Instance.UserData.Money.Value -= UpgradeGoldCost;
                }
            }


            var finddata = GameRoot.Instance.HeroSystem.FindHeroEquipItemData(td.item_equip_type);
            if (finddata != null)
            {
                for (int i = 0; i < UpgradeEffectList.Count; i++)
                {
                    ProjectUtility.SetActiveCheck(UpgradeEffectList[i], false);
                    ProjectUtility.SetActiveCheck(UpgradeEffectList[i], true);
                }

                finddata.Level.Value += 1;


                GameRoot.Instance.UserData.Save();

                Set(HeroItemData);
            }
        }
        else
        {
            if(GameRoot.Instance.UserData.Money.Value < UpgradeGoldCost)
            {
                GameRoot.Instance.UISystem.OpenUI<PopupGoldCoinInsufficent>();
            }
        }
    }


    private bool IsUpgradeCheck = false;


    public void SetUpgradeInfo()
    {
        var td = Tables.Instance.GetTable<HeroItemInfo>().GetData(HeroItemData.Heroitemidx);

        if (td != null)
        {
            var equipmentitemdata = GameRoot.Instance.UserData.Herogroudata.Equipheroitems.Find(x => x.Heroitemtype == td.item_equip_type);

            if (equipmentitemdata != null)
            {
                UpgradeGoldCost = GameRoot.Instance.HeroSystem.equip_item_level_gold_cost * equipmentitemdata.Level.Value;
                UpgradeMaterialCount = GameRoot.Instance.HeroSystem.equip_item_level_count * equipmentitemdata.Level.Value;

                var finditemdata = GameRoot.Instance.ItemSystem.GetItemData((int)ItemSystem.ItemType.EquipUpgradeItem, td.item_equip_type);

                var allfinditemdata = GameRoot.Instance.ItemSystem.GetItemData((int)ItemSystem.ItemType.EquipUpgradeItem, 0);

                var curupgradecountvalue = allfinditemdata.Itemcnt.Value + finditemdata.Itemcnt.Value;

                UpgradeGoldCostText.text = UpgradeGoldCost.ToString();
                UpgradeMaterialCountText.text = UpgradeMaterialCount.ToString();


                UpgradeGoldCostText.color = UpgradeGoldCost <= GameRoot.Instance.UserData.Money.Value ? Color.white : Color.red;
                UpgradeMaterialCountText.color = UpgradeMaterialCount <= curupgradecountvalue ? Color.white : Color.red;


                IsUpgradeCheck = UpgradeGoldCost <= GameRoot.Instance.UserData.Money.Value && UpgradeMaterialCount <= curupgradecountvalue;

                var settd = Tables.Instance.GetTable<HeroItemSet>().GetData(td.item_set_type);

                if (settd != null)
                {
                    ItemSetEffectText.text = Tables.Instance.GetTable<Localize>().GetFormat($"set_name_desc_{td.item_set_type}", settd.set_ability_value);
                }
            }
        }
    }
}


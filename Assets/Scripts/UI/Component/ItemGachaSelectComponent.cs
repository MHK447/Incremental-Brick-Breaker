using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class ItemGachaSelectComponent : MonoBehaviour
{
    [SerializeField]
    private SelectItemComponent EquipSelectItemComponent;

    [SerializeField]
    private SelectItemComponent GachaSelectItemComponent;

    [SerializeField]
    private Button EquipBtn;

    [SerializeField]
    private Button SellBtn;

    private EquipItemData EquipItemData;

    private EquipItemData GachaItemData;

    void Awake()
    {
        EquipBtn.onClick.AddListener(OnEquipBtnClick);
        SellBtn.onClick.AddListener(OnSellBtnClick);
    }

    public void Init(EquipItemData equipItemData, EquipItemData gachaItemData)
    {
        EquipItemData = equipItemData;
        GachaItemData = gachaItemData;

        EquipSelectItemComponent.Set(equipItemData, gachaItemData);
        GachaSelectItemComponent.Set(gachaItemData, equipItemData);
    }

    void OnEquipBtnClick()
    {
        GameRoot.Instance.EquipmentSystem.EquipItem(GachaItemData);

        // 장착 데이터가 바뀌는 즉시 EquipmentComponent UI도 갱신합니다.
        var parentGroup = GetComponentInParent<EquipmnentUpgradeGroup>();
        parentGroup?.RefreshEquipmentComponents();

        Init(GachaItemData , EquipItemData);
        
        GameRoot.Instance.UISystem.OpenUI<PopupStatusValue>(popup => popup.SetStatusValue(5));
    }

    void OnSellBtnClick()
    {
        Hide();
    }


    public void Hide()
    {
        ProjectUtility.SetActiveCheck(this.gameObject, false);
    }
}


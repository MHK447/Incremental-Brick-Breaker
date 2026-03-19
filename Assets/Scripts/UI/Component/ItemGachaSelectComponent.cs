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

        Init(GachaItemData , EquipItemData);
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


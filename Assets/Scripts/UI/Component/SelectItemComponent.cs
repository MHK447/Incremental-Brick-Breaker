using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;


public class SelectItemComponent : MonoBehaviour
{
    [SerializeField]
    private ItemInfoComponent ItemComponent;

    [SerializeField]
    private TextMeshProUGUI ItemNameText;

    [SerializeField]
    private TextMeshProUGUI ItemValueText;

    [SerializeField]
    private Image ArrowImg;

    private EquipItemData CurrentItemData;
    private EquipItemData AfterItemData;


    public void Set(EquipItemData currentitemData, EquipItemData afteritemData)
    {
        CurrentItemData = currentitemData;
        AfterItemData = afteritemData;

        var td = Tables.Instance.GetTable<EquipItemInfo>().GetData(new KeyValuePair<int, int>(currentitemData.Equipitemtype, currentitemData.Equipitemidx));

        if (td != null)
        {
            var getitemvalue = GameRoot.Instance.EquipmentSystem.GetItemValue(currentitemData.Equipitemtype, currentitemData.Equipitemidx, currentitemData.Grade, currentitemData.Level);

            ItemComponent.Set(currentitemData.Equipitemtype, currentitemData.Equipitemidx, currentitemData.Grade, currentitemData.Level);
            ItemNameText.text = Tables.Instance.GetTable<Localize>().GetString(td.item_name);
            ItemValueText.text = Tables.Instance.GetTable<Localize>().GetFormat(td.item_desc, getitemvalue);

            ArrowCheck();
        }
    }


    public void EquipItemSet(EquipItemData equipitemdata)
    {
        ProjectUtility.SetActiveCheck(this.gameObject, true);

        ItemComponent.Set(equipitemdata.Equipitemtype, equipitemdata.Equipitemidx, equipitemdata.Grade, equipitemdata.Level);

        var td = Tables.Instance.GetTable<EquipItemInfo>().GetData(new KeyValuePair<int, int>(equipitemdata.Equipitemtype, equipitemdata.Equipitemidx));

        if (td != null)
        {   
            var getitemvalue = GameRoot.Instance.EquipmentSystem.GetItemValue(equipitemdata.Equipitemtype, equipitemdata.Equipitemidx, equipitemdata.Grade, equipitemdata.Level);

            ItemNameText.text = Tables.Instance.GetTable<Localize>().GetString(td.item_name);
            ItemValueText.text = Tables.Instance.GetTable<Localize>().GetFormat(td.item_desc, getitemvalue);
            ProjectUtility.SetActiveCheck(ArrowImg.gameObject, false);
        }
    }


    public void ArrowCheck()
    {
        var beforevalue = GameRoot.Instance.EquipmentSystem.GetItemValue(CurrentItemData.Equipitemtype, CurrentItemData.Equipitemidx, CurrentItemData.Grade, CurrentItemData.Level);
        var aftervalue = GameRoot.Instance.EquipmentSystem.GetItemValue(AfterItemData.Equipitemtype, AfterItemData.Equipitemidx, AfterItemData.Grade, AfterItemData.Level);

        ArrowImg.color = beforevalue > aftervalue ? Color.green : Color.red;
        ArrowImg.transform.localScale = beforevalue > aftervalue ? Vector3.one * -1 : Vector3.one;

        ProjectUtility.SetActiveCheck(ArrowImg.gameObject, beforevalue != aftervalue);
    }
}


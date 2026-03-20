using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class EquipmentComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private CarParts CardPartsType;

    [SerializeField]
    private Image BgImg;

    [SerializeField]
    private Image PartsImg;

    [SerializeField]
    private TextMeshProUGUI LevelText;

    [SerializeField]
    private GameObject LockRoot;

    [SerializeField]
    private GameObject ActiveRoot;

    [SerializeField]
    private EquipmnentUpgradeGroup EquipmnentUpgradeGroup;

    private EquipItemData CurrentEquipItemData;

    void Awake()
    {
        if (EquipmnentUpgradeGroup == null)
        {
            EquipmnentUpgradeGroup = GetComponentInParent<EquipmnentUpgradeGroup>();
        }
    }

    public void Init()
    {
        var finddata = GameRoot.Instance.UserData.Playerequipdata.FindEquipItemData((int)CardPartsType);
        CurrentEquipItemData = finddata;

        if (finddata != null)
        {
            var td = Tables.Instance.GetTable<EquipItemInfo>().GetData(
                new KeyValuePair<int, int>(finddata.Equipitemtype, finddata.Equipitemidx));

            BgImg.color = Config.Instance.GetImageColor($"Bg_Grade_Color_{finddata.Grade}");
            LevelText.text = $"Lv.{finddata.Level}";
            PartsImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_InGame, td.image);
        }

        ProjectUtility.SetActiveCheck(LockRoot, finddata == null);
        ProjectUtility.SetActiveCheck(ActiveRoot, finddata != null);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CurrentEquipItemData == null || EquipmnentUpgradeGroup == null) return;

        var hoverPos = transform.position + new Vector3(0f, 50f, 0f);
        EquipmnentUpgradeGroup.EquipItemSet(CurrentEquipItemData, true, hoverPos);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (EquipmnentUpgradeGroup == null) return;

        EquipmnentUpgradeGroup.EquipItemSet(CurrentEquipItemData, false);
    }
}


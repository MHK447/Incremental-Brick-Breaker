using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class EquipUpgradeComponent : MonoBehaviour
{
    [SerializeField]
    private EquipAttributeComponent AttributeComponent;

    [SerializeField]
    private Image BgImg;

    [SerializeField]
    private Image ItemImg;



    public void Set(int itemidx, int grade)
    {
        var td = Tables.Instance.GetTable<HeroItemInfo>().GetData(itemidx);

        if (td != null)
        {
            BgImg.color = Config.Instance.GetImageColor($"item_grade_color_{grade}");

            AttributeComponent.Set(grade, td.item_equip_type);

            ItemImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_EquipItem, td.item_img);

        }
    }
}


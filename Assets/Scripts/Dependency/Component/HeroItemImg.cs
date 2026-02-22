using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class HeroItemImg : MonoBehaviour
{
    [SerializeField]
    private Image ItemImg;

    [SerializeField]
    private Image EquipGradeBgImg;


    public void Set(HeroItemData euqipitemdata)
    {
        var td = Tables.Instance.GetTable<HeroItemInfo>().GetData(euqipitemdata.Heroitemidx);
        if (td != null)
        {
            EquipGradeBgImg.color = Config.Instance.GetImageColor($"item_grade_color_{euqipitemdata.Grade}");
            ItemImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_EquipItem, td.item_img);
        }
    }
}


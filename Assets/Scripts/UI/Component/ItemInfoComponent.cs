using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class ItemInfoComponent : MonoBehaviour
{
    [SerializeField]
    private Image ItemImg;

    [SerializeField]
    private Image BgImg;

    [SerializeField]
    private TextMeshProUGUI LevelText;


    public void Set(int itemtype , int itemidx , int grade , int level)
    {
        var td = Tables.Instance.GetTable<EquipItemInfo>().GetData(new KeyValuePair<int, int>(itemtype, itemidx));

        if (td != null)
        {
            ItemImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_InGame, td.image);
            BgImg.color = Config.Instance.GetImageColor($"Bg_Grade_Color_{grade}");
            LevelText.text = $"Lv.{level}";
        }
    }
}


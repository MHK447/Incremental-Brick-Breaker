using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class EquipmentComponent : MonoBehaviour
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




    public void Init()
    {
        var finddata = GameRoot.Instance.UserData.Playerequipdata.FindEquipItemData((int)CardPartsType);

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
}


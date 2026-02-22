using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class EquipAttributeComponent : MonoBehaviour
{
    [SerializeField]
    private Image GradeBgImg;

    [SerializeField]
    private Image ItemTypeImg;

    public void Set(int grade , int itemtype)
    {
        GradeBgImg.color = Config.Instance.GetImageColor($"item_grade_color_{grade}");

        ItemTypeImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, $"Common_Icon_AttributeItem_{itemtype}");

    }
}


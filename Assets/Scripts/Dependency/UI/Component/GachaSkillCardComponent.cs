using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;
using BanpoFri;
using System.Linq;

public class GachaSkillCardComponent : MonoBehaviour
{
    [SerializeField]
    private Image CardImg;
    [SerializeField]
    private GameObject NewObj;

    [SerializeField]
    private Image BgImg;

    [SerializeField]
    private TextMeshProUGUI CardNameText;

    private int CardIdx = 0;
   
    public void Set(int cardidx , bool isnew)
    {
        CardIdx = cardidx;

        var cardtd = Tables.Instance.GetTable<CardInfo>().GetData(cardidx);

        if(cardtd != null)
        {
            CardImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, cardtd.icon);

            BgImg.sprite = cardtd.card_type == 2 ?  AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Card_Frame_Weapon")
             : AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Card_Frame_Weapon");

            ProjectUtility.SetActiveCheck(NewObj, isnew);

            CardNameText.text = Tables.Instance.GetTable<Localize>().GetString(cardtd.card_name);
        }
    }

}

